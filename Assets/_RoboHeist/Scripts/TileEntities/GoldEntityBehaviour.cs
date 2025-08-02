using UnityEngine;

public class GoldEntityBehaviour : MoveableEntityBehaviour
{
    public GoldEntityData goldEntityData;

    protected override TileEntityData GetTileEntityData()
    {
        return goldEntityData;
    }
}
