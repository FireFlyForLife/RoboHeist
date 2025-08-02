using UnityEngine;

public class DoorEntityBehaviour : TileEntityBehaviour
{
    public DoorEntityData doorEntityData;

    protected override TileEntityData GetTileEntityData()
    {
        return doorEntityData;
    }
}
