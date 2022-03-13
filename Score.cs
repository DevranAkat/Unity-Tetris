using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    public int totalScore { get; private set; }
    [SerializeField] TextMeshPro  boardScore;
    [SerializeField] GameObject  leaderBoard;

    public void AddPoints(int clearedLines, int currentLevel)
    {   
        float multiplier = GetMultiplier(currentLevel);

        var scoreArray = new int[] {0, 100, 220, 360, 520};
        var score = scoreArray[clearedLines];

        var scoree = (multiplier * (float)score);
        this.totalScore += (int)scoree;

        UpdateBoardScore();
    }

    public float GetMultiplier(int currentLevel)
    {
        var multiplierArray = new float[] {1f, 1f, 1.1f, 1.3f , 1.65f, 2f, 2.5f, 3.1f, 3.7f};

        currentLevel = currentLevel > 8 ?  8 : currentLevel; 
        return multiplierArray[currentLevel];
    }

    public void UpdateBoardScore()
    {
        boardScore.text = "Score: " + this.totalScore.ToString();
    }

    public void UpdateHiscore()
    {
        var score = totalScore;
        var thirdHiscore = PlayerPrefs.GetInt("HiscoreThird");

        if (score > thirdHiscore)
        {
            setNewHiscore(score);
        }
    }

    public void setNewHiscore(int newHiscore)
    {
        var hiscoreFirst = PlayerPrefs.GetInt("HiscoreFirst");
        var hiscoreSecond = PlayerPrefs.GetInt("HiscoreSecond");
        var hiscoreThird = PlayerPrefs.GetInt("HiscoreThird");

        if(newHiscore >= hiscoreFirst){
            PlayerPrefs.SetInt("HiscoreFirst", newHiscore);
            PlayerPrefs.SetInt("HiscoreSecond", hiscoreFirst);
            PlayerPrefs.SetInt("HiscoreThird", hiscoreSecond);
        }
        else if(newHiscore >= hiscoreSecond){
            PlayerPrefs.SetInt("HiscoreSecond", newHiscore);
            PlayerPrefs.SetInt("HiscoreThird", hiscoreSecond);
        }
        else if(newHiscore > hiscoreThird){
            PlayerPrefs.SetInt("HiscoreThird", newHiscore);
        }
    }

    public void GetHiScore()
    {
        var hiscoreFirst = PlayerPrefs.GetInt("HiscoreFirst");
        var hiscoreSecond = PlayerPrefs.GetInt("HiscoreSecond");
        var hiscoreThird = PlayerPrefs.GetInt("HiscoreThird");

        var hiScorePanel = leaderBoard.GetComponent<TextMeshProUGUI>();
        hiScorePanel.text = "1. " + hiscoreFirst + "\n" + 
                            "2. " + hiscoreSecond + "\n" + 
                            "3. " + hiscoreThird;
    }
}
