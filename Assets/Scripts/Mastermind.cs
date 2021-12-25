using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Mastermind : MonoBehaviour
{
    #region
    // UI
    [SerializeField]
    private InputField codemakerInputField;
    [SerializeField]
    private Text aiGuessText;
    [SerializeField]
    private Text roundNumText;
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private Text aiStatusText;
    #endregion


    [SerializeField]
    private bool allowRepeat = false;

    public bool AllowRepeat
    {
        get
        {
            return allowRepeat;
        }
    }

    [SerializeField]
    private int numOfPegs = 4;
    [SerializeField]
    private int maxRounds = 9;
    [SerializeField]
    private CodebreakerAI codebreaker;
    [SerializeField]
    private int numOfColors = 6;

    public int NumOfColors
    {
        get
        {
            return numOfColors;
        }
    }

    [SerializeField]
    private char blackKey = 'B';
    [SerializeField]
    private char whiteKey = 'W';
    [SerializeField]
    private char wrongKey = '.';

    private string defaultScore = "";
    private string winScore = "";

    [SerializeField]
    private string currentAnswer = "2212";
    private bool shouldTerminate = false;
    private bool startGuessing = false;

    private float playWait = 1.0f;
    private float playTimer = 0.0f;


    public string CalculateScore(string guess, string answer)
    {
        // Evaluate current state based on number of Blacks and Whites
        string score = "";

        string wrongGuess = "";
        string wrongAnswer = "";

        for(int i = 0; i < numOfPegs; i++)
        {
            if(guess[i] == answer[i])
            {
                score += blackKey;
            }
            else
            {
                wrongGuess += guess[i];
                wrongAnswer += answer[i];
            }
        }

        char[] wrongAnswerPegs = wrongAnswer.ToCharArray();
        foreach (char c in wrongGuess)
        {
            // indexOf returns the zero-based index position of value if that character is found, or -1 if it is not
            int index = Array.IndexOf(wrongAnswerPegs, c);

            if (index != -1)
            {

                // Remove c to avoid in duplicate checks
                wrongAnswerPegs[index] = '-';
                 //wrongPeg.Remove(index);

                 // Append W/Cows to score
                 score += whiteKey;
            }
        }

        for (int i = score.Length; i < numOfPegs; ++i)
        {
            // Append '.'/wrong peg to score
            score += wrongKey;
        }
        return score;
    }

   

    void PlayGuess()
    {
        aiStatusText.text = "I am guessing....Ummm....";
        codebreaker.roundNum++;
        codebreaker.currentGuess = codebreaker.GuessAnswer();
        Debug.Log("Current Guess : " + codebreaker.currentGuess);
        codebreaker.currentScore = CalculateScore(codebreaker.currentGuess, currentAnswer);
        Debug.Log("Current Score : " + codebreaker.currentScore);
        
        if (CheckWin())
        {
            aiStatusText.text = "Did I crack your code? " + codebreaker.currentGuess + " ;)\nGive me another!";
            Debug.Log("AI wins!");
            // Terminate algorithm
            shouldTerminate = true;
            startGuessing = false;
        }
        else if(codebreaker.roundNum > maxRounds)
        {
            aiStatusText.text = "I lost!!";
            Debug.Log("AI woses :|");
            // Terminate algorithm
            shouldTerminate = true;
            startGuessing = false;
        }
    }

    bool CheckWin()
    {
        return codebreaker.currentScore == winScore;
    }

    bool isGuessScoreMatching(string guess, string answer, string score)
    {
        return CalculateScore(guess, answer) == score;
    }


    public List<string> MatchingAnswers(string currentGuess, string score, List<string> guesses)
    {
        List<string> possibleAnswers = new List<string>();

        foreach(string checkGuess in guesses)
        {
            if(isGuessScoreMatching(currentGuess, checkGuess, score))
            {
                possibleAnswers.Add(checkGuess);
            }
        }

        return possibleAnswers;
    }

    private void StartCalculations()
    {
        
        shouldTerminate = false;
        startGuessing = true;
        codebreaker.ResetAI();
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (!codebreaker)
        {
            codebreaker = FindObjectOfType<CodebreakerAI>();
        }

        codebreaker.Init(this);
        aiStatusText.text = "....Idle....";
        for (int i = 0; i < numOfPegs; ++i)
        {
            defaultScore += wrongKey;
            winScore += blackKey;
        }
        //Debug.Log("Value of " + codebreaker.initialGuess + " " + currentAnswer + " is " + codebreaker.scoreTable[codebreaker.initialGuess + currentAnswer]);
        codemakerInputField.characterLimit = numOfPegs;
        codemakerInputField.characterValidation = InputField.CharacterValidation.Decimal;
    }

    // Update is called once per frame
    private void Update()
    {

        if (Input.GetKeyUp(KeyCode.Return) && !startGuessing) // Could be made into an enumerated state machine
        {
            if(codemakerInputField && codemakerInputField.text.Length == numOfPegs)
            {
                bool isNotAllowed = false;

                foreach(char c in codemakerInputField.text)
                {
                    int charNum = int.Parse(c.ToString());
                    if (charNum < 1 || charNum > 6)
                    {
                        isNotAllowed = true;
                        break;
                    }
                }

                if (!isNotAllowed)
                {
                    aiStatusText.text = "Calculations Initiated";
                    currentAnswer = codemakerInputField.text;
                    StartCalculations();
                }
                else
                {
                    aiStatusText.text = codemakerInputField.text + "?\nThat's cheating!\nNot allowed";
                }
            }
            
        }

        if (!shouldTerminate && startGuessing)
        {
            playTimer += Time.deltaTime;
            if(playTimer >= playWait)
            {
                Debug.Log("Round : " + codebreaker.roundNum);

                playTimer = 0.0f;
                PlayGuess();
                if (aiGuessText)
                {
                    aiGuessText.text = "My guess : " + codebreaker.currentGuess;
                }
                if(roundNumText)
                {
                    roundNumText.text = "Round : " + codebreaker.roundNum;
                }
                if(scoreText)
                {
                    scoreText.text = "Score : " + codebreaker.currentScore;
                }
            }
        }
    }
}
