using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TheGrid : MonoBehaviour
{
    public LevelData levelData;
    private Dictionary<Vector2Int, TileEntity> entities = new();

    public static TheGrid Instance { get; private set; }

    public Grid grid;
    public Vector2 TileSize => new Vector2(grid.cellSize.x, grid.cellSize.y);

    public void RegisterAtPosition(TileEntity entity, Vector2Int position)
    {
        if (!entities.ContainsKey(position))
        {
            if (entities.ContainsValue(entity))
            {
                Vector2Int existingPosition = entities.FirstOrDefault(pair => pair.Value == entity).Key;
                Debug.Log($"Removing {entity.gameObject.name} from {existingPosition}");
                entities.Remove(existingPosition);
            }
            entities.Add(position, entity);
        }
    }

    public void RegisterAtPosition(TileEntityData entity, Vector2Int position)
    {
    }

    public TileEntity CheckGridPosition(Vector2Int position)
    {
        TileEntity entity;
        return (entities.TryGetValue(position, out entity) ? entity : null);
    }

    void Awake()
    {
        Instance = this;
    }
}
