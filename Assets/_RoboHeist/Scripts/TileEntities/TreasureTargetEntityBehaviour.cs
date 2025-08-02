using UnityEngine;

public class TreasureTargetEntityBehaviour : TileEntityBehaviour
{
    public TreasureTargetEntityData treasureTargetEntityData;

    protected override TileEntityData GetTileEntityData()
    {
        return treasureTargetEntityData;
    }
}
