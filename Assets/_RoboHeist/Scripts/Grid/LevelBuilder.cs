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
    public GameObject shortWallPrefab = null;
    public GameObject simpleDoorPrefab = null;
    public GameObject goldPrefab = null;
    public GameObject treasureTargetPrefab = null;

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
                    //grid.RegisterAtPosition(tileData.TileEntity, position);
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
            SimpleDoorEntityData d => MakeInstanceFromTileData(d, position),
            GoldEntityData g => MakeInstanceFromTileData(g, position),
            TreasureTargetEntityData t => MakeInstanceFromTileData(t, position),
            RobotEntityData r => MakeInstanceFromTileData(r, position),
            _ => null,
        };

        return entity;
    }

    private GameObject MakeInstanceFromTileData(WallEntityData entityData, Vector2Int position)
    {
        GameObject instance;
        if (!entityData.IsShort)
        {
            instance = Instantiate(wallPrefab, grid.CalculateWorldPosition(position), Quaternion.identity, transform);

        }
        else
        {
            instance = Instantiate(shortWallPrefab, grid.CalculateWorldPosition(position), Quaternion.identity, transform);
        }
        var web = instance?.GetComponent<WallEntityBehaviour>();
        web.wallEntityData = (WallEntityData)entityData.Clone();
        web.wallEntityData.position = position;
        web.EnsurePositionAndRotation(grid);

        return instance;
    }

    private GameObject MakeInstanceFromTileData(SimpleDoorEntityData entityData, Vector2Int position)
    {
        GameObject instance = Instantiate(simpleDoorPrefab, grid.CalculateWorldPosition(position), Quaternion.identity, transform);
        var deb = instance?.GetComponent<SimpleDoorEntityBehaviour>();
        deb.doorEntityData = (SimpleDoorEntityData)entityData.Clone();
        deb.doorEntityData.position = position;
        deb.EnsurePositionAndRotation(grid);
        return instance;
    }

    private GameObject MakeInstanceFromTileData(GoldEntityData entityData, Vector2Int position)
    {
        GameObject instance = Instantiate(goldPrefab, grid.CalculateWorldPosition(position), Quaternion.identity, transform);
        var geb = instance?.GetComponent<GoldEntityBehaviour>();
        geb.goldEntityData = (GoldEntityData)entityData.Clone();
        geb.goldEntityData.position = position;
        geb.EnsurePositionAndRotation(grid);
        return instance;
    }

    private GameObject MakeInstanceFromTileData(TreasureTargetEntityData entityData, Vector2Int position)
    {
        GameObject instance = Instantiate(treasureTargetPrefab, grid.CalculateWorldPosition(position), Quaternion.identity, transform);
        var tteb = instance?.GetComponent<TreasureTargetEntityBehaviour>();
        tteb.treasureTargetEntityData = (TreasureTargetEntityData)entityData.Clone();
        tteb.treasureTargetEntityData.position = position;
        tteb.EnsurePositionAndRotation(grid);
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
