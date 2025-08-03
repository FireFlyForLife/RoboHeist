using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The result of an instruction, did it succeed? Did it succeed with additional data?
/// </summary>
public class InstructionResult
{
    public List<TileEntityData> CollisionObjects = null;
    public bool WasBlocked = false;
}

/// <summary>
/// A single instruction for a robot
/// </summary>
[Serializable]
public abstract class Instruction
{
    public abstract string CommandShortForm { get; }
    public abstract string CommandLongForm { get; }
    public abstract InstructionResult Execute(RobotEntityBehaviour robot);
}

[Serializable]
public class NoOp : Instruction
{
    public override string CommandShortForm => "NOP";
    public override string CommandLongForm => "Wait";

    public override InstructionResult Execute(RobotEntityBehaviour robot)
    {
        return new InstructionResult();
    }
}

[Serializable]
public class MoveForward : Instruction
{
    public override string CommandShortForm => "FWD";
    public override string CommandLongForm => "Move Forward";


    public override InstructionResult Execute(RobotEntityBehaviour robot)
    {
        if (!robot.robotEntityData.CanMove())
        {
            return new InstructionResult();
        }

        if (robot.robotEntityData.isLifting)
        {
            return ExecuteWhileLifting(robot);
        }
        else
        {
            return ExecuteWhileNotLifting(robot);
        }
    }

    private InstructionResult ExecuteWhileNotLifting(RobotEntityBehaviour robot)
    {
        var dir = robot.robotEntityData.direction.AsVec2();
        Vector2Int positionInFrontOfBot = robot.robotEntityData.position + dir;

        var instructionResult = new InstructionResult();
        instructionResult.CollisionObjects = TheGrid.Instance.CheckGridPosition(positionInFrontOfBot).ToList();
        if (instructionResult.CollisionObjects.Count > 0)
        {
            instructionResult.WasBlocked = false;
            foreach (var entity in instructionResult.CollisionObjects)
            {
                instructionResult.WasBlocked |= (entity.IsSolid() && !entity.CanBePushed(dir));
            }
        }

        if (!instructionResult.WasBlocked)
        {
            robot.robotEntityData.position = robot.robotEntityData.position + dir;

            // Push whatever is in our way...
            foreach (var entity in instructionResult.CollisionObjects.Where(e => e.IsSolid()))
            {
                entity.Push(robot.robotEntityData, dir);
            }
        }

        return instructionResult;
    }

    private InstructionResult ExecuteWhileLifting(RobotEntityBehaviour robot)
    {
        var dir = robot.robotEntityData.direction.AsVec2();

        // If we're lifting we're now a two block entity...
        Vector2Int positionInFrontOfBot = robot.robotEntityData.position + dir + dir;

        var instructionResult = new InstructionResult();
        instructionResult.CollisionObjects = TheGrid.Instance.CheckGridPosition(positionInFrontOfBot).ToList();
        if (instructionResult.CollisionObjects.Count > 0)
        {
            instructionResult.WasBlocked = false;
            foreach (var obj in instructionResult.CollisionObjects)
            {
                instructionResult.WasBlocked |= (obj.IsSolid() && !obj.CanBePushed(dir));
            }
        }

        if (!instructionResult.WasBlocked)
        {
            robot.robotEntityData.position = robot.robotEntityData.position + dir;

            // Push the lifted objects...
            foreach (var lifted in robot.robotEntityData.lifedEntities)
            {
                lifted.Push(robot.robotEntityData, dir);
            }

            // Push whatever is in our way...
            foreach (var entity in instructionResult.CollisionObjects.Where(e => !e.IsSolid()))
            {
                entity.Push(robot.robotEntityData, dir);
            }
        }
        else
        {
            DropEntities(robot);
        }

        return instructionResult;
    }

    private void DropEntities(RobotEntityBehaviour robot)
    {
        Debug.Log($"Dropping {robot.robotEntityData.lifedEntities.Count} items...");
        foreach (var moveable in robot.robotEntityData.lifedEntities)
        {
            moveable.SetCanMove(true);
        }

        robot.robotEntityData.isLifting = false;
        robot.robotEntityData.lifedEntities.Clear();
    }
}

[Serializable]
public class MoveForwardOrLift : MoveForward
{
    public override string CommandShortForm => "FOL";
    public override string CommandLongForm => "Move Forward Or Lift";


    public override InstructionResult Execute(RobotEntityBehaviour robot)
    {
        var instructionResult = base.Execute(robot);
        if (instructionResult.WasBlocked && robot.robotEntityData.robotConfig.CanLift)
        {
            instructionResult.WasBlocked = !LiftEntities(robot, instructionResult.CollisionObjects);
        }

        return instructionResult;
    }

    private bool LiftEntities(RobotEntityBehaviour robot, List<TileEntityData> entities)
    {
        if (entities.Any(entity => entity.IsSolid() && entity is not MoveableEntityData))
        {
            return false;
        }
        else
        {
            robot.robotEntityData.isLifting = true;
            robot.robotEntityData.lifedEntities = entities.Where(entity => entity is MoveableEntityData).Cast<MoveableEntityData>().ToList();

            Debug.Log($"Lifting {robot.robotEntityData.lifedEntities.Count} items...");
            foreach (var moveable in robot.robotEntityData.lifedEntities)
            {
                moveable.SetCanMove(false);
            }
            return true;
        }
    }
}

public abstract class FaceDirectionInstruction : Instruction
{
    protected InstructionResult ExecuteWhileNotLifting(RobotEntityBehaviour robot, Direction direction)
    {
        robot.robotEntityData.direction = direction;
        return new InstructionResult() { WasBlocked = !robot.robotEntityData.CanMove() };
    }

    protected InstructionResult ExecuteWhileLifting(RobotEntityBehaviour robot, Direction direction)
    {
        var dir = direction.AsVec2();

        // If we're lifting we're now a two block entity...
        Vector2Int positionInFrontOfBot = robot.robotEntityData.position + dir;

        var instructionResult = new InstructionResult();
        instructionResult.CollisionObjects = TheGrid.Instance.CheckGridPosition(positionInFrontOfBot).ToList();
        if (instructionResult.CollisionObjects.Count > 0)
        {
            instructionResult.WasBlocked = false;
            foreach (var obj in instructionResult.CollisionObjects)
            {
                instructionResult.WasBlocked |= (obj.IsSolid() /*&& !obj.CanBePushed(dir)*/);
            }
        }

        if (!instructionResult.WasBlocked)
        {
            robot.robotEntityData.direction = direction;

            // Push the lifted objects...
            foreach (var lifted in robot.robotEntityData.lifedEntities)
            {
                lifted.position = robot.robotEntityData.position + dir;
            }
        }
        else
        {
            Debug.Log("Blocked!");
        }

        return instructionResult;
    }
}

[Serializable]
public class TurnLeft : FaceDirectionInstruction
{
    public override string CommandShortForm => "LFT";
    public override string CommandLongForm => "Turn Left";


    public override InstructionResult Execute(RobotEntityBehaviour robot)
    {
        var direction = robot.robotEntityData.direction.TurnedLeft();
        if (robot.robotEntityData.isLifting)
        {
            return ExecuteWhileLifting(robot, direction);
        }
        else
        {
            return ExecuteWhileNotLifting(robot, direction);
        }
    }
}

[Serializable]
public class TurnRight : FaceDirectionInstruction
{
    public override string CommandShortForm => "RGT";
    public override string CommandLongForm => "Turn Right";


    public override InstructionResult Execute(RobotEntityBehaviour robot)
    {
        var direction = robot.robotEntityData.direction.TurnedRight();
        if (robot.robotEntityData.isLifting)
        {
            return ExecuteWhileLifting(robot, direction);
        }
        else
        {
            return ExecuteWhileNotLifting(robot, direction);
        }
    }
}
