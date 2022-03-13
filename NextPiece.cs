using UnityEngine;
using UnityEngine.Tilemaps;

public class NextPiece : MonoBehaviour
{
    public TetrominoData data { get; private set; }
    public Tilemap tilemap { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set;}

    public void Initialize(Board board, TetrominoData data, int placement)
    {
        this.position = GetPositionOfPlacement(placement);
        this.data = data;
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.cells = new Vector3Int[4];

        if (this.cells == null) {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < this.cells.Length; i++) {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
        Clear(placement);
        Set();
    }
    
    public void Clear(int placement)
    {
        var nextPlacement = placement == 1 ? 8 : 3;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Vector3Int tilePosition = new Vector3Int(6+i,nextPlacement-j);
                this.tilemap.SetTile(tilePosition, null);
            }
        }
    }

    private void Set()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, data.tile);
        }
    }

    private Vector3Int GetPositionOfPlacement(int placement)
    {
        return placement == 1 ? new Vector3Int(7,7) : new Vector3Int(7,2) ;
    }

}