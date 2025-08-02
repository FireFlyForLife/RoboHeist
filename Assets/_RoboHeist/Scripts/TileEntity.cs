using System;
using UnityEngine;

public enum Direction : int
{
    Up, Right, Down, Left
}

public static class DirectionExtensions
{
    public static Direction TurnedLeft(this Direction direction)
    {
        var dirValue = (int)direction;
        var rotatedValue = (dirValue - 1) % 4;
        return (Direction)rotatedValue;
    }

    public static Direction TurnedRight(this Direction direction)
    {
        var dirValue = (int)direction;
        var rotatedValue = (dirValue + 1) % 4;
        return (Direction)rotatedValue;
    }

    public static Direction TurnedAround(this Direction direction)
    {
        var dirValue = (int)direction;
        var rotatedValue = (dirValue + 2) % 4;
        return (Direction)rotatedValue;
    }

    public static Vector2Int AsVec2(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return new Vector2Int(0, 1);
            case Direction.Right: return new Vector2Int(1, 0);
            case Direction.Down: return new Vector2Int(0, -1);
            case Direction.Left: return new Vector2Int(-1, 0);
            default: return new Vector2Int(0, 0);
        }
    }
}

[Serializable]
public abstract class TileEntityData : ICloneable
{
    // Variables
    public Direction direction = Direction.Up;
    public Vector2Int position;

    public abstract object Clone();

    public virtual bool CanBePushed(Vector2Int direction)
    {
        return false;
    }

    public virtual bool Push(TileEntityData pusher, Vector2Int direction)
    {
        // By default all entities are static and immovable.
        return false;
    }

    protected virtual void CloneImpl(TileEntityData clone)
    {
        clone.direction = direction;
        clone.position = position;
    }
}

[Serializable]
public abstract class MoveableEntityData : TileEntityData
{
    public override bool CanBePushed(Vector2Int direction)
    {
        return TheGrid.Instance.CheckGridPosition(position + direction) == null;
    }

    public override bool Push(TileEntityData pusher, Vector2Int direction)
    {
        if (CanBePushed(direction))
        {
            pusher.position += direction;
            position += direction;
            return true;
        }
        return false;
    }
}

[Serializable]
public class RobotEntityData : MoveableEntityData
{
    public RobotConfig robotConfig;
    public float executionDelay = 1.0f; // In seconds
    public RobotState startingState = RobotState.Idle;

    public override object Clone()
    {
        RobotEntityData clone = new RobotEntityData();
        CloneImpl(clone);
        return clone;
    }

    protected override void CloneImpl(TileEntityData clone)
    {
        base.CloneImpl(clone);
        var typedClone = (RobotEntityData)clone;
        typedClone.robotConfig = robotConfig;
        typedClone.executionDelay = executionDelay;
        typedClone.startingState = startingState;
    }
}

[Serializable]
public class WallEntityData : TileEntityData
{
    public override object Clone()
    {
        WallEntityData clone = new WallEntityData();
        CloneImpl(clone);
        return clone;
    }

    protected override void CloneImpl(TileEntityData clone)
    {
        base.CloneImpl(clone);
    }
}



public abstract class TileEntityBehaviour : MonoBehaviour
{
    protected abstract TileEntityData GetTileEntityData();

    public void EnsurePositionAndRotation(TheGrid grid)
    {
        var entityData = GetTileEntityData();
        grid.RegisterAtPosition(entityData, entityData.position);
        transform.localPosition = grid.CalculateWorldPosition(entityData.position);//new Vector3((float)entityData.position.x * gridSize.x, 0.0f, (float)entityData.position.y * gridSize.y);
        transform.localRotation = grid.CalculateWorldRotation(entityData.direction);//Quaternion.AngleAxis((int)entityData.direction * 90.0f, Vector3.up);
    }

    // Messages
    protected virtual void Start()
    {
        // One time position set for static entities.
        EnsurePositionAndRotation(TheGrid.Instance);
    }
}

public abstract class MoveableEntityBehaviour : TileEntityBehaviour
{
    protected virtual void Update()
    {
        // TODO: interpolate

        // One time position set for static entities.
        EnsurePositionAndRotation(TheGrid.Instance);
    }
}

//public class RobotEntityBehaviour : TileEntityBehaviour
//{
//    [SerializeReference, SubclassSelector]
//    public new RobotEntityData entityData;

//    private Coroutine instructionRunner = null;

//    public RobotState CurrentState
//    {
//        get => entityData.currentState;
//        set
//        {
//            if (entityData.currentState == value)
//            {
//                return;
//            }

//            StartRobotBehaviour(value);
//            entityData.currentState = value;
//        }
//    }

//    public IEnumerator<Instruction> Instructions { get; set; }
//}

//public class TileEntity : MonoBehaviour
//{
//    // Variables
//    public Direction direction = Direction.Up;
//    public Vector2Int position;

//    public virtual bool CanBePushed(Vector2Int direction)
//    {
//        return false;
//    }

//    public virtual bool Push(TileEntity pusher, Vector2Int direction)
//    {
//        // By default all entities are static and immovable.
//        return false;
//    }

//    public virtual bool IsSolid()
//    {
//        return true;
//    }

//    // Messages
//    protected virtual void Start()
//    {
//        // One time position set for static entities.
//        Vector2 gridSize = TheGrid.Instance.TileSize;
//        TheGrid.Instance.RegisterAtPosition(this, position);
//        transform.localPosition = new Vector3((float)position.x * gridSize.x, 0.0f, (float)position.y * gridSize.y);
//        transform.localRotation = Quaternion.AngleAxis((int)direction * 90.0f, Vector3.up);
//    }
//}

//[Serializable]
//public class MoveableTileEntity : TileEntity
//{
//    public override bool CanBePushed(Vector2Int direction)
//    {
//        var tile = TheGrid.Instance.CheckGridPosition(position + direction);
//        return tile == null || !tile.IsSolid();
//    }

//    public override bool Push(TileEntity pusher, Vector2Int direction)
//    {
//        if (CanBePushed(direction))
//        {
//            pusher.position += direction;
//            position += direction;
//            return true;
//        }
//        return false;
//    }

//    protected virtual void Update()
//    {
//        // TODO: interpolate

//        // Update visual transform from grid position and direction
//        Vector2 gridSize = TheGrid.Instance.TileSize;
//        TheGrid.Instance.RegisterAtPosition(this, position);
//        transform.localPosition = new Vector3((float)position.x * gridSize.x, 0.0f, (float)position.y * gridSize.y);
//        transform.localRotation = Quaternion.AngleAxis((int)direction * 90.0f, Vector3.up);
//    }
//}
