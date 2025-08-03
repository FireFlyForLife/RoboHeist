using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TreasureTargetEntityBehaviour : TileEntityBehaviour
{
    public TreasureTargetEntityData treasureTargetEntityData;

    protected override TileEntityData GetTileEntityData()
    {
        return treasureTargetEntityData;
    }

    void Update()
    {
        var results = TheGrid.Instance.CheckGridPosition(treasureTargetEntityData.position);
        foreach (var result in results)
        {
            if (result is GoldEntityData)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }
}
