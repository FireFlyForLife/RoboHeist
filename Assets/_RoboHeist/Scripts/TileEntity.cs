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

public class TileEntity : MonoBehaviour
{
    // Variables
    public Direction direction = Direction.Up;
    public Vector2Int position;

    public virtual bool CanBePushed(Vector2Int direction)
    {
        return false;
    }

    public virtual bool Push(TileEntity pusher, Vector2Int direction)
    {
        // By default all entities are static and immovable.
        return false;
    }

    // Messages
    protected virtual void Start()
    {
        // One time position set for static entities.
        Vector2 gridSize = TheGrid.Instance.TileSize;
        TheGrid.Instance.RegisterAtPosition(this, position);
        transform.localPosition = new Vector3((float)position.x * gridSize.x, 0.0f, (float)position.y * gridSize.y);
        transform.localRotation = Quaternion.AngleAxis((int)direction * 90.0f, Vector3.up);
    }
}

public class MoveableTileEntity : TileEntity
{
    public override bool CanBePushed(Vector2Int direction)
    {
        return TheGrid.Instance.CheckGridPosition(position + direction) == null;
    }

    public override bool Push(TileEntity pusher, Vector2Int direction)
    {
        if (CanBePushed(direction))
        {
            pusher.position += direction;
            position += direction;
            return true;
        }
        return false;
    }

    protected virtual void Update()
    {
        // TODO: interpolate

        // Update visual transform from grid position and direction
        Vector2 gridSize = TheGrid.Instance.TileSize;
        TheGrid.Instance.RegisterAtPosition(this, position);
        transform.localPosition = new Vector3((float)position.x * gridSize.x, 0.0f, (float)position.y * gridSize.y);
        transform.localRotation = Quaternion.AngleAxis((int)direction * 90.0f, Vector3.up);
    }
}
