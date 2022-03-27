using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 2f;
    public float moveDelay = 0.1f;
    public float lockDelay = 0.5f;

    public double screenLeftSide = Screen.width*0.333;
    public double screenRightSide = Screen.width - (Screen.width*0.333);
    public double screenBottomSide = Screen.height/5; 

    private float stepTime;
    private float moveTime;
    private float lockTime;
    private float levelSpeed;


    public void Initialize(Board board, Vector3Int position, TetrominoData data, float speedLevel)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;

        this.stepTime = Time.time + this.stepDelay;
        this.moveTime = Time.time + this.moveDelay;
        this.lockTime = 0f;
        this.levelSpeed = speedLevel;

        if (this.cells == null) {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < this.cells.Length; i++) {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        this.board.ClearPieceFromBoard(this);
        this.lockTime += Time.deltaTime;

        //TODO: Remove, just used to test on PC
        if (Input.GetButtonDown("Fire1"))
        {
            Vector3 mousePos = Input.mousePosition;

            if (mousePos.y < screenBottomSide)
            {
                Move(Vector2Int.down);
            }
            else if (mousePos.x <= screenLeftSide)
            {
                Move(Vector2Int.left);
            }
            else if (mousePos.x >= screenRightSide)
            {
                Move(Vector2Int.right);
            }
            else if(mousePos.x > screenLeftSide && mousePos.x < screenRightSide)
            {
                Rotate(1);
            }
        }

        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            if (touch.position.y < screenBottomSide)
            {
                Move(Vector2Int.down);
            }
            else if (touch.position.x <= screenLeftSide)
            {
                Move(Vector2Int.left);
            }
            else if (touch.position.x >= screenRightSide)
            {
                Move(Vector2Int.right);
            }
            else if(touch.position.x > screenLeftSide && touch.position.x < screenRightSide)
            {
                Rotate(1);
            }
        } 

        //TODO: Add swipe-control


        //TODO: Remove this part, PC input
        if(Input.GetKeyDown(KeyCode.RightArrow)){
            Move(Vector2Int.right);
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow)){
            Move(Vector2Int.left);
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow)){
            Move(Vector2Int.down);
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow)){
            Rotate(1);
        }
        else if(Input.GetKeyDown(KeyCode.Space)){
            HardDrop();
        }

        // Allow the player to hold movement keys but only after a move delay
        // so it does not move too fast
        if (Time.time > this.moveTime) {
            HandleMoveInputs();
        }

        // Advance the piece to the next row every x seconds
        if (Time.time >= this.stepTime - levelSpeed) {
            Step();
        }

        this.board.SetPieceOnBoard(this);
    }

     private void HandleMoveInputs()
    {
        // Soft drop movement
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (Move(Vector2Int.down)) {
                // Update the step time to prevent double movement
                this.stepTime = Time.time + this.stepDelay;
            }
        }

        // Left/right movement
        if (Input.GetKey(KeyCode.LeftArrow)) {
            Move(Vector2Int.left);
        } else if (Input.GetKey(KeyCode.RightArrow)) {
            Move(Vector2Int.right);
        }
    }

     private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;

        // Step down to the next row
        Move(Vector2Int.down);

        // Once the piece has been inactive for too long it becomes locked
        if (this.lockTime >= this.lockDelay) {
            Lock();
        }
    }
    
    private void HardDrop()
    {
        while(Move(Vector2Int.down)){
            continue;
        }

        Lock();
    }

    private void Lock()
    {
        this.board.SetPieceOnBoard(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = this.board.IsValidPosition(this, newPosition);

        if (valid)
        {
            this.position = newPosition;
            this.moveTime = Time.time + this.moveDelay;
            this.lockTime = 0f;
        }
        return valid;
    }

    private void Rotate(int direction)
    {
        // Store the current rotation in case the rotation fails
        // and we need to revert
        int originalRotation = this.rotationIndex;

        // Rotate all of the cells using a rotation matrix
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        // Revert the rotation if the wall kick tests fail
        if (!TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min) {
            return max - (min - input) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        // Rotate all of the cells using the rotation matrix
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];

            int x, y;

            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    // "I" and "O" are rotated from an offset center point
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0) {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];

            if (Move(translation)) {
                return true;
            }
        }

        return false;
    }
}
