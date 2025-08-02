using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TheGrid : MonoBehaviour
{
    public LevelData levelData;
    private Dictionary<TileEntityData, Vector2Int> entities = new();

    public static TheGrid Instance { get; private set; }

    public Grid grid;
    public Vector2 TileSize => new Vector2(grid.cellSize.x, grid.cellSize.y);

    public Vector3 CalculateWorldPosition(Vector2Int position)
    {
        return new Vector3((float)position.x * -TileSize.x, 0.0f, (float)position.y * TileSize.y);
    }

    public Quaternion CalculateWorldRotation(Direction direction)
    {
        return Quaternion.AngleAxis((int)direction * 90.0f + 180.0f, Vector3.up);
    }

    public void RegisterAtPosition(TileEntityData entity, Vector2Int position)
    {
        entities[entity] = position;
    }

    public IEnumerable<TileEntityData> CheckGridPosition(Vector2Int position)
    {
        return entities.Where(pair => pair.Value == position).Select(pair => pair.Key);
    }

    void Awake()
    {
        Instance = this;
    }
}
