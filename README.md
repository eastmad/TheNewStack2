# Making a Sonnet


- Clone and run the solution TheNewStack2 in Visual Studio or an equivalent. 
- I set the target framework as net6.0 in the Shakespeare.scsproj; there are no important dependencies.
  I used Visual Studio Version 17
- Build and run the project

Note that the corpus text can be altered or replaced, but try to keep the format of space separators the eame.

The constants to play with in the Program.cs code are:

    const short _MAXWORDS_ = 25;

This just controls the number of words in the new sonnet.

    const short _DEPTH_ONE_BIAS_ = 5;
    const short _DEPTH_TWO_BIAS_ = 2;
     
This effects the weight set of the word next to, or one away from the target word.    
        
    const short MINIMUMNUMBERCANDIDATES = 5;
    
When finding candidates for the next word, the bigger the candidate list the wider the possible outcome, combined with enough random noise 
        
    const short RANDOM_NOISE = 10;

The highr the number, the more likely stats based on occurences will be overthrown
        
        
    const short _REPEATABLEWORDLENGTH_ = 2;