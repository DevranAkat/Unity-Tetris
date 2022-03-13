using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public NextPiece nextPiece { get; private set; }
    public NextPiece nextPiece2 { get; private set; }
    public Score Score { get; private set; }
    public LevelManager LevelManager { get; private set; }
    public TetrominoData[] tetrominos;
    public Vector3Int spawnPosition;
    public Queue<TetrominoData> queue { get; private set; }
    public GameObject gameoverPanel;
    
    public Vector2Int boardSize = new Vector2Int(10, 20);
    private bool gameOver = false;
    
    public RectInt Bounds {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();
        this.nextPiece = GetComponentInChildren<NextPiece>();
        this.nextPiece2 = GetComponentInChildren<NextPiece>();
        this.Score = GetComponentInChildren<Score>();
        this.LevelManager = GetComponentInChildren<LevelManager>();
        this.queue = new Queue<TetrominoData>();

        for (int i = 0; i < this.tetrominos.Length; i++)
        {
            this.tetrominos[i].Initialize();
        } 
    }

    public void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            AddRandomPiece();
        }
        
        SpawnPiece();
    }

    public void AddRandomPiece()
    {
        var random = Random.Range(0, this.tetrominos.Length);
        var tetromino = this.tetrominos[random];
        this.queue.Enqueue(tetromino);
    }

    public void SpawnPiece()
    {
        if (gameOver)
        {
            return;
        }

        AddRandomPiece();     

        var newPiece = this.queue.Dequeue();
        var peekNextPiece = this.queue.Peek();
        var peekSecondNextPiece = this.queue.ElementAt(1);

        this.activePiece.Initialize(this, this.spawnPosition, newPiece, LevelManager.levelSpeed);
        this.nextPiece.Initialize(this, peekNextPiece, 1);
        this.nextPiece2.Initialize(this, peekSecondNextPiece, 2);

        if (!IsValidPosition(this.activePiece, this.spawnPosition)) {
            GameOver();
        } else {
            SetPieceOnBoard(this.activePiece);
        }
    }

    public void SetPieceOnBoard(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);    
        }
    }

    public void ClearPieceFromBoard(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);    
        }
    }

    public void ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;
        var clearedLines = 0;
        // Clear from bottom to top
        while (row < bounds.yMax)
        {
            // Only advance to the next row if the current is not cleared
            // because the tiles above will fall down when a row is cleared
            if (IsLineFull(row)) {
                LineClear(row);
                clearedLines++;
            } else {
                row++;
            }
        }
        Score.AddPoints(clearedLines, LevelManager.level);
        LevelManager.UpdateLevel(clearedLines);
    }

        public void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        // Clear all tiles in the row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        // Shift every row above down one
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }

            row++;
        }
    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // The line is not full if a tile is missing
            if (!this.tilemap.HasTile(position)) {
                return false;
            }
        }

        return true;
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        // The position is only valid if every cell is valid
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            // An out of bounds tile is invalid
            if (!bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }

            // A tile already occupies the position, thus invalid
            if (this.tilemap.HasTile(tilePosition)) {
                return false;
            }
        }

        return true;
    }

    public void GameOver()
    {
        gameOver = true;

        // this.tilemap.ClearAllTiles();

        if (gameoverPanel != null)
        {
            Score.UpdateHiscore();
            gameoverPanel.SetActive (true);
        }
        // Do anything else you want on game over here..
    }

    public void NewGame()
    {
        SceneManager.LoadScene("Game"); 
    }

}
