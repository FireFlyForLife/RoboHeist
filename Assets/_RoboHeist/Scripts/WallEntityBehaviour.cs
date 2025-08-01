using UnityEngine;

public class WallEntityBehaviour : TileEntityBehaviour
{
    public WallEntityData wallEntityData;

    protected override TileEntityData GetTileEntityData()
    {
        return wallEntityData;
    }
}
