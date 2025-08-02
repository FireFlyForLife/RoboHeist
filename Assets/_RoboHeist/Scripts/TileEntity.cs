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
            case Direction.Up: return new Vector2Int(0, -1);
            case Direction.Right: return new Vector2Int(1, 0);
            case Direction.Down: return new Vector2Int(0, 1);
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

    public virtual bool IsSolid()
    {
        return true;
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

            TheGrid.Instance.RegisterAtPosition(pusher, pusher.position);
            TheGrid.Instance.RegisterAtPosition(this, position);
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

[Serializable]
public class TreasureTargetEntityData : TileEntityData
{
    public override object Clone()
    {
        TreasureTargetEntityData clone = new TreasureTargetEntityData();
        CloneImpl(clone);
        return clone;
    }

    protected override void CloneImpl(TileEntityData clone)
    {
        base.CloneImpl(clone);
    }

    public override bool IsSolid()
    {
        return false;
    }
}

[Serializable]
public class GoldEntityData : MoveableEntityData
{
    public override object Clone()
    {
        GoldEntityData clone = new GoldEntityData();
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
        transform.localPosition = grid.CalculateWorldPosition(entityData.position);
        transform.localRotation = grid.CalculateWorldRotation(entityData.direction);
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
