using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public int level { get; private set; }
    public int totalClearedLines { get; private set; }
    public float levelSpeed { get; private set; }
    [SerializeField] TextMeshPro  boardLevel;

    public void UpdateLevel(int clearedLines)
    {   
        this.totalClearedLines += clearedLines;
        this.level = (this.totalClearedLines / 10) + 1;

        UpdateBoardLevel();
        UpdateLevelSpeed();

    }

    public void UpdateLevelSpeed()
    {
        var level = this.level > 8 ?  8 : this.level; 

        var levelSpeeds = new float[] {0f, 0f, 0.2f, 0.35f, 0.6f, 0.75f, 0.85f, 0.9f,0.95f,};
        this.levelSpeed = levelSpeeds[level];
    }

     public void UpdateBoardLevel()
    {
        boardLevel.text = "Level: " + this.level.ToString();
    }
}
