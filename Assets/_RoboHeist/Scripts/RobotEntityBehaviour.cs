using UnityEngine;

public class RobotEntityBehaviour : MoveableEntityBehaviour
{
    public RobotEntityData robotEntityData;

    protected override TileEntityData GetTileEntityData()
    {
        return robotEntityData;
    }
}
