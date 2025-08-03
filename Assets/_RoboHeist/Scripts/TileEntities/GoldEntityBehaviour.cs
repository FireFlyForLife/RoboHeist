using System.Collections.Generic;
using UnityEngine;

public class GoldEntityBehaviour : MoveableEntityBehaviour
{
    public static List<GoldEntityBehaviour> allGold = new();

    public List<HistoricalRobotTransform> historicalTransforms = new();
    private Vector2Int lastPos;
    public GoldEntityData goldEntityData;

    protected override void Start()
    {
        base.Start();
        allGold.Add(this);
        lastPos = goldEntityData.position;
        historicalTransforms.Add(new HistoricalRobotTransform
        {
            time = -1.0f,
            pos = lastPos,
            direction = goldEntityData.direction
        });
    }

    private void OnDestroy()
    {
        allGold.Remove(this);
    }

    protected override void Update()
    {
        base.Update();
        if (lastPos != goldEntityData.position)
        {
            historicalTransforms.Add(new HistoricalRobotTransform
            {
                time = TimelineUIController.TheTime,
                pos = goldEntityData.position,
                direction = goldEntityData.direction
            });
            lastPos = goldEntityData.position;
        }
    }

    protected override TileEntityData GetTileEntityData()
    {
        return goldEntityData;
    }
}
