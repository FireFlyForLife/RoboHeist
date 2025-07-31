using UnityEngine;
using UnityEngine.Tilemaps;

public class TheGrid : MonoBehaviour
{
    public static TheGrid Instance { get; private set; }

    public Grid grid;
    public Vector2 TileSize => new Vector2(grid.cellSize.x, grid.cellSize.y);

    void Start()
    {
        Instance = this;
        grid = GetComponent<Grid>();
    }
}
