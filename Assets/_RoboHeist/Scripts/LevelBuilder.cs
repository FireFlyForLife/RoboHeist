using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RobotPrefabPair
{
    public RobotConfig Config;
    public GameObject Prefab;
}

[ExecuteAlways]
public class LevelBuilder : MonoBehaviour
{
    public TheGrid grid = null;
    public List<RobotPrefabPair> robotPrefabs = new();
    public GameObject wallPrefab = null;

    private LevelGridData levelGridData = null;
    private List<GameObject> createdInstances = new();

    public void InstantiateLevel(LevelGridData levelGridData)
    {
        this.levelGridData = levelGridData;
    }

    private void Start()
    {
        InstantiateLevel(grid.levelData.LevelGrid);
    }

    private void Update()
    {
        if (levelGridData == null)
        {
            return;
        }

        if (createdInstances.Count > 0)
        {
            DestroyInstances();
        }

        Vector2Int position = Vector2Int.zero;
        for (position.y = 0; position.y < levelGridData.Dimensions.y; ++position.y)
        {
            for (position.x = 0; position.x < levelGridData.Dimensions.x; ++position.x)
            {
                var tileData = levelGridData.GetAt(position);
                if (tileData == null || tileData.TileEntity == null)
                {
                    continue;
                }

                var instance = MakeInstanceFromTileData(tileData.TileEntity, position);
                if (instance != null)
                {
                    instance.hideFlags = HideFlags.DontSaveInEditor;
                    grid.RegisterAtPosition(tileData.TileEntity, position);
                    createdInstances.Add(instance);
                }
            }
        }
        levelGridData = null;
    }

    private GameObject MakeInstanceFromTileData(TileEntityData entityData, Vector2Int position)
    {
        var entity = entityData switch
        {
            WallEntityData w => MakeInstanceFromTileData(w, position),
            RobotEntityData r => MakeInstanceFromTileData(r, position),
            _ => null,
        };

        return entity;
    }

    private GameObject MakeInstanceFromTileData(WallEntityData entityData, Vector2Int position)
    {
        GameObject instance = Instantiate(wallPrefab, grid.CalculateWorldPosition(position), Quaternion.identity, transform);
        var web = instance?.GetComponent<WallEntityBehaviour>();
        web.wallEntityData = (WallEntityData)entityData.Clone();
        web.wallEntityData.position = position;
        web.EnsurePositionAndRotation(grid);
        return instance;
    }

    private GameObject MakeInstanceFromTileData(RobotEntityData entityData, Vector2Int position)
    {
        var prefab = robotPrefabs.FirstOrDefault(pair => pair.Config?.name == entityData.robotConfig?.name);
        GameObject instance = null;
        if (prefab != null)
        {
            instance = Instantiate(prefab.Prefab, grid.CalculateWorldPosition(position), grid.CalculateWorldRotation(entityData.direction), transform);
            var reb = instance?.GetComponent<RobotEntityBehaviour>();
            reb.robotEntityData = (RobotEntityData)entityData.Clone();
            reb.robotEntityData.position = position;
            reb.EnsurePositionAndRotation(grid);
        }
        return instance;
    }

    private void DestroyInstances()
    {
        foreach (var instance in createdInstances)
        {
            DestroyImmediate(instance);
        }
        createdInstances.Clear();
    }
}
