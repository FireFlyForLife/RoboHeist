using System;
using UnityEngine;

[Serializable]
public class GridLocation
{
    [SerializeReference]
    public TileEntityData TileEntity = null;
    public string floor = "tarmac";
}

[Serializable]
public class LevelGridData
{
    [SerializeField]
    public Vector2Int Dimensions;

    [SerializeField]
    public GridLocation[] TileEntities;

    public GridLocation GetAt(Vector2Int position)
    {
        return TileEntities[position.y * Dimensions.x + position.x];
    }

    public void EnsureSize()
    {
        if (TileEntities == null || Dimensions.y * Dimensions.x != TileEntities.Length)
        {
            TileEntities = new GridLocation[Dimensions.y * Dimensions.x];
            for (int i = 0; i < TileEntities.Length; ++i)
            {
                TileEntities[i] = new GridLocation();
            }
        }
    }
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [SerializeField]
    public LevelGridData LevelGrid;

    private void OnValidate()
    {
        LevelGrid.EnsureSize();

        var levelBuilder = FindFirstObjectByType<LevelBuilder>();
        levelBuilder.InstantiateLevel(LevelGrid);
    }
}
