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

    public void EnsureSize()
    {
        if (TileEntities == null || Dimensions.y * Dimensions.x != TileEntities.Length)
        {
            TileEntities = new GridLocation[Dimensions.y * Dimensions.x];
            Array.Fill(TileEntities, new GridLocation());
        }
    }
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [SerializeField]
    LevelGridData LevelGrid;

    private void OnValidate()
    {
        LevelGrid.EnsureSize();
    }
}
