using UnityEngine;

public class SimpleDoorEntityBehaviour : TileEntityBehaviour
{
    public SimpleDoorEntityData doorEntityData;
    public Animator doorAnimator;

    private bool wasOpen = false;

    protected override TileEntityData GetTileEntityData()
    {
        return doorEntityData;
    }

    private void Update()
    {
        bool isOpen = doorEntityData.IsOpen();
        if (isOpen && !wasOpen)
        {
            doorAnimator.Play("Open");
        }
        else if (wasOpen && !isOpen)
        {
            doorAnimator.Play("Close");
        }
        wasOpen = isOpen;
    }
}
