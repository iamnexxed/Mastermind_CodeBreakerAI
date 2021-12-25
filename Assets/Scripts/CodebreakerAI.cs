using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CodebreakerAI : MonoBehaviour
{
    // Start with initial guess 1122 (Knuth gives examples showing that this algorithm using other first guesses such as 1123, 1234 does not win in five tries on every code)
    // This was found by bruteforcing each guess
    public string initialGuess = "1122";
    public string currentGuess = "";
    public int roundNum = 0;
    public string currentScore = "";

    public List<string> currentPossibleAnswerSet;

    public List<Guess> possibleScores;
    
    private List<string> allPossibleAnswers;
    private int maxNumOfColors = 0;
    private Mastermind mindScript;
   
    public struct GuessInfo
    {
        public int worstCaseProbability;
        public bool isImpossible;
        public string actualGuess;

        public GuessInfo(int getProbability, bool getImpossibility, string getGuess)
        {
            worstCaseProbability = getProbability;
            isImpossible = getImpossibility;
            actualGuess = getGuess;
        }
    }

    // Could be actually mapped onto a hash table for faster lookups
    // example: table[guess][answer] = score
    public class Guess
    {
        public string guess;
        public List<AnswerScorePair> answerScorePair;
       
        public void ConnectNewAnswerScorePair(List<AnswerScorePair> newAnswerScorePair)
        {
            answerScorePair = newAnswerScorePair;
        }

        public Guess(string getGuess)
        {
            guess = getGuess;
            answerScorePair = new List<AnswerScorePair>();
        }

       public void ClearAnswerScoreList()
        {
            answerScorePair.Clear();
        }

        public void AddPair(AnswerScorePair getPair)
        {
            answerScorePair.Add(getPair);
        }
    }

    public struct AnswerScorePair
    {
        public string answer;
        public string score;

        public AnswerScorePair(string getAnswer, string getScore)
        {
            answer = getAnswer;
            score = getScore;
        }
    }


    // Init function creates a list of all possible answers considering the number of colors and the number of pegs
    public void Init(Mastermind getMindScript)
    {
        maxNumOfColors = getMindScript.NumOfColors;
        mindScript = getMindScript;
        allPossibleAnswers = new List<string>();

        // Calculate guesses with duplicates for 6 different colors
        // TODO: Create a generic loop that calculates all possible guesses with less hardcoding
        for (int i = 1; i <= maxNumOfColors; ++i)
        {
            for (int j = 1; j <= maxNumOfColors; ++j)
            {
                if (!getMindScript.AllowRepeat && (i == j))
                    continue;
                for (int k = 1; k <= maxNumOfColors; ++k)
                {
                    if (!getMindScript.AllowRepeat && (j == k || i == k))
                        continue;
                    for (int l = 1; l <= maxNumOfColors; ++l)
                    {
                        if (!getMindScript.AllowRepeat && (k == l || j == l || i == l))
                            continue;

                        string temp = i.ToString() + j.ToString() + k.ToString() + l.ToString();
                        allPossibleAnswers.Add(temp);
                    }
                }
            }
        }
        ResetAI();
    }

    // Reset All AI Statistics
    public void ResetAI()
    {
        currentPossibleAnswerSet = new List<string>(allPossibleAnswers);
        roundNum = 0;
        possibleScores = CreateScoreTable();
    }

    // Generate list containing guess:answer->score 
    // Each guess has a list of answerscore pair
    // score in answer score pair is calculated based on how each answer compares with guess to generate a score
    private List<Guess> CreateScoreTable()
    {
        List<Guess> table = new List<Guess>();
        foreach (string guess in allPossibleAnswers)
        {
            Guess newGuess = new Guess(guess);
            foreach (string answer in allPossibleAnswers)
            {
                AnswerScorePair pair = new AnswerScorePair(answer, mindScript.CalculateScore(guess, answer));

                newGuess.AddPair(pair);
            }
            table.Add(newGuess);
        }
        Debug.Log("Created " + table.Count + " entries in the score table");
        return table;
        
    }

    // Actual core body for guessing
    public string GuessAnswer()
    {
        //roundNum++;
        if (roundNum > 1)
        {
            //remove from currentPossibleAnswerSet any code that would not give the same response if it(the guess) were the code
            currentPossibleAnswerSet = mindScript.MatchingAnswers(currentGuess, currentScore, currentPossibleAnswerSet);

            Debug.Log("Current Possible Answer Set has count: " + currentPossibleAnswerSet.Count);

            // Create a list that will hold all possible guesses
            // The list also needs to contain guess probability 
            // TODO: dont populate guesses with used ones
            List<GuessInfo> guesses = new List<GuessInfo>();


            foreach(var newGuess in possibleScores)
            {
                List<AnswerScorePair> newScoreByAnswer = new List<AnswerScorePair>();

                foreach(var newPair in newGuess.answerScorePair)
                {
                    // Check if the score and the answer pair is already present in the recent calculated answer set
                    if(currentPossibleAnswerSet.Contains(newPair.answer))
                    {
                        // If it is add it to the new list of answer pair in the current check guess
                        newScoreByAnswer.Add(newPair);
                    }
                    // Otherwise the answer is guaranteed to be invalid
                }

                // Remove previous answer data
                newGuess.ClearAnswerScoreList();

                // Update the list with new answers and scores for the respective guess
                newGuess.ConnectNewAnswerScorePair(newScoreByAnswer);

                // Create a dictionary of scores and their count
                Dictionary<string, int> possibilityPerScore = CreateScoreCounter(newGuess.answerScorePair);

                // Apply minimax algorithm to determine the best possible guess in order reduce the possible answer set

                // Calculate the max of the possibilites 
                int maxProbability = CalulateMax(possibilityPerScore);
                bool isImpossible = !currentPossibleAnswerSet.Contains(newGuess.guess);
                GuessInfo guessInfo = new GuessInfo(maxProbability, isImpossible, newGuess.guess);
                guesses.Add(guessInfo);

            }
            // Find the minimum probable guess which has least probability to appear as an answer which could further eliminate
            // a majority of impossible answers
            return FindMinPossibleGuess(guesses);
        }
        else
        {
            return initialGuess;
        }
    }

    // Returns a dictionary containing score as the key and the number of time the score is found as a value  
    private Dictionary<string, int> CreateScoreCounter(List<AnswerScorePair> list)
    {
        Dictionary<string, int> scoreCounter = new Dictionary<string, int>();
        foreach(var item in list)
        {
            if(scoreCounter.ContainsKey(item.score))
            {
                scoreCounter[item.score]++;
            }
            else
            {
                scoreCounter.Add(item.score, 1);
            }
        }
        return scoreCounter;
    }

    // Returns the maximum "value" of all the values from the key:value pairs in the dictionary
    private int CalulateMax(Dictionary<string, int> dict)
    {
        int max = -99999999;
        foreach(var item in dict)
        {
            if(item.Value > max)
            {
                max = item.Value;
            }
        }
        return max;
    }
    
    // Returns a guess in the form of a string
    // Calculates a minumum valued guess based on the minimum probability of each guessInfo in the guesses list
    // The guesses with minimum values are then checked to see if they exist within the list of possible answers
    // if it exists it should be the final guess otherwise the guess found can help eliminate all the guesses which
    // scenario of appearing as an answer
    private string FindMinPossibleGuess(List<GuessInfo> guesses)
    {
        GuessInfo minGuessInfo = new GuessInfo();
        int min = 99999999;
        foreach (var guess in guesses)
        {
            if(guess.worstCaseProbability < min)
            {
                minGuessInfo = guess;
                min = guess.worstCaseProbability;
            }
        }

        // If the guess was already present in the possible answers list it has a higher precedence to get the correct answer
        foreach (var guess in guesses)
        {
            if(guess.worstCaseProbability == min)
            {
                
                if(guess.isImpossible == false)
                {
                   return guess.actualGuess;
                }
                
            }
        }

        return minGuessInfo.actualGuess;
    }
}
