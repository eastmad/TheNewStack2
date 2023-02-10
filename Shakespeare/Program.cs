using System;
using System.Collections;
using System.Collections.Generic;

namespace SonnetMaker
{

    public class CorpusCheck
    {
        const short MAXWORDS = 25;

        public static void Main(string[] args)
        {
            PrepareReferences();

            //Sonnet Loop
            List<string> newwords = new List<string>();

            //Starting words
            MakeSonnetWord ms = new MakeSonnetWord("The", "intelligence,");
            newwords.Add(ms.firstword);
            newwords.Add(ms.secondword);

            for (short i = 0; i < MAXWORDS; i++)
            {
                ForwardReference fr =  ms.EvaluteBestReference(newwords);
                newwords.Add(fr.referencetoken);
                ms = new MakeSonnetWord(ms.secondword, fr.referencetoken);

            }

            Console.Write($"\nNew sonnet! \n\n {string.Join(" ", newwords)}");
        }

        static void PrepareReferences()
        {
            string[]? tokens = BackEnd.FileServices.ReadCorpus();
            Console.WriteLine("Total corpus words: " + tokens?.Length);
            Console.WriteLine("First word: " + tokens?.First());
            Console.WriteLine("Last word: " + tokens?.Last());

            for (int i = 2; i < tokens?.Length; i++)
            {
                string currentToken = tokens[i];
                ForwardReference.Add(tokens[i - 2], ForwardReference.Depth.Two, currentToken);
                ForwardReference.Add(tokens[i - 1], ForwardReference.Depth.One, currentToken);
            }

            Console.WriteLine("Entries: " + ForwardReference.Size());
            string example_check = "The";
            Console.WriteLine($"\n\nExample check for '{example_check}' - " + ForwardReference.ShowEntry(example_check));
        }
    }

    public class MakeSonnetWord
    {
        public readonly string firstword;
        public readonly string secondword;

        const short DEPTH_ONE_BIAS = 5;
        const short DEPTH_TWO_BIAS = 2;
        const short MINIMUMNUMBERCANDIDATES = 5;
        const short RANDOM_NOISE = 10;
        const short REPEATABLEWORDLENGTH = 2;

        public MakeSonnetWord(string first, string second)
        {
            firstword = first;
            secondword = second;
        }

        public ForwardReference EvaluteBestReference(List<string> usedtokens)
        {
            List<ForwardReference> frs = new List<ForwardReference>();

            BestAtDepth(secondword, ForwardReference.Depth.One).ForEach(fr => frs.Add(fr));

            BestAtDepth(firstword, ForwardReference.Depth.Two).ForEach(fr => frs.Add(fr));

            int highestscore = 0;
            ForwardReference winner = new ForwardReference(ForwardReference.Depth.One, "None");

            Random rnd = new Random();
            foreach (ForwardReference fr in frs)
            {
                //Change if you add an extra depth
                short bias = (fr.depth == ForwardReference.Depth.One)? DEPTH_ONE_BIAS : DEPTH_TWO_BIAS;
         
                if (!usedtokens.Contains(fr.referencetoken) || fr.referencetoken.Length <= REPEATABLEWORDLENGTH)
                {
                    int noise = rnd.Next(RANDOM_NOISE);
                    if ((fr.occurences * (bias + noise) ) > highestscore)
                    {
                        //Console.WriteLine($"{fr.referencetoken} wins with {fr.occurences * (bias + noise)}");
                        highestscore = fr.occurences * (bias + noise);
                        winner = fr;
                    }
                }

            }

            //Console.WriteLine($"Winner from {frs.Count} entries is '{winner.referencetoken}'");

            return winner;
        }

        // We will make sure there is a minimum of 5
        private List<ForwardReference> BestAtDepth(string token, ForwardReference.Depth depth)
        {
            List<ForwardReference> candidates = new List<ForwardReference>();

            short highestoccurence = 0;

            
            foreach (KeyValuePair<string, ForwardReference> kvp in ForwardReference.FindReferences(token))
            {
                ForwardReference fr = kvp.Value;
                if (fr.depth == depth && fr.occurences > highestoccurence)
                    highestoccurence = fr.occurences;
            }

            do
            {
                foreach (KeyValuePair<string, ForwardReference> kvp in ForwardReference.FindReferences(token))
                {
                    ForwardReference fr = kvp.Value;
                    if (fr.depth == depth && fr.occurences == highestoccurence)
                        candidates.Add(fr);
                }

                highestoccurence -= 1;

            }
            while (candidates.Count < MINIMUMNUMBERCANDIDATES);

            return candidates;
        }
    }


    public class ForwardReference
    {
        private static Dictionary<string, Dictionary<string, ForwardReference>> references = new Dictionary<string, Dictionary<string, ForwardReference>>();

        public enum Depth {One = 1, Two = 2}

        public Depth depth;
        public short occurences;
        public string referencetoken;

        public ForwardReference(Depth depth, string referencetoken)
        {
            this.depth = depth;
            this.referencetoken = referencetoken;
            occurences = 1;
        }

        public override string ToString()
        {
            return "{" + referencetoken + " " + depth + " X " + occurences + "} ";
        }

        public static Dictionary<string, ForwardReference> FindReferences(string token)
        {
            try
            {
                return references[token]; 
            }
            catch (KeyNotFoundException)
            {
                throw new Exception("The token " + token + " is not in corpus.");
            }
        }

        public static string ShowEntry(string token)
        {
            string desc = "";

            if (references.ContainsKey(token))
            {
                foreach (KeyValuePair<string,ForwardReference> fr in references[token])
                    desc += fr.Value;
            }
            else desc = "No reference";

            return desc;
        }

        public static int Size()
        {
            return references.Count();
        }

        public static void Add(string token, Depth depth, string referencetoken)
        {
            if (references.ContainsKey(token))
            {
                if (references[token].ContainsKey(referencetoken))
                    references[token][referencetoken].occurences += 1;
                else references[token][referencetoken] = new ForwardReference(depth, referencetoken);
            }
            else
            {
                references[token] = new Dictionary<string, ForwardReference>();
                references[token].Add(referencetoken, new ForwardReference(depth, referencetoken));
            }
        }
    }
}




namespace BackEnd
{
    public class FileServices
    {
        static string BASEDIRECTORY = Path.Combine(Directory.GetCurrentDirectory(),"..","..","..");
        const string IDENTITYFILENAME = "shakespearecorpus.txt";

        public static string[]? ReadCorpus()
        {
            return ReadFromFile(Path.Combine(BASEDIRECTORY, IDENTITYFILENAME));
        }

        private static string[]? ReadFromFile(string filename)
        {
            string[]? words = null;

            filename = Path.Combine(BASEDIRECTORY, filename);

            if (File.Exists(filename))
            {
                string thestring = File.ReadAllText(filename);
                words = thestring.Split(' ');
               
            }
            else Console.WriteLine("No existing file found for " + filename);

            return words;
        }
    }
}
