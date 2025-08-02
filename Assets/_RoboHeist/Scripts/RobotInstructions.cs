using System;
using UnityEngine;

/// <summary>
/// The result of an instruction, did it succeed? Did it succeed with additional data?
/// </summary>
public class InstructionResult
{
    public TileEntityData CollisionObject = null;
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
    public abstract InstructionResult Execute(RobotEntityData robot);
}

[Serializable]
public class NoOp : Instruction
{
    public override string CommandShortForm => "NOP";
    public override string CommandLongForm => "Wait";

    public override InstructionResult Execute(RobotEntityData robot)
    {
        return new InstructionResult();
    }
}

[Serializable]
public class MoveForward : Instruction
{
    public override string CommandShortForm => "FWD";
    public override string CommandLongForm => "Move Forward";


    public override InstructionResult Execute(RobotEntityData robot)
    {
        var dir = robot.direction.AsVec2();

        var instructionResult = new InstructionResult();

        instructionResult.CollisionObject = TheGrid.Instance.CheckGridPosition(robot.position + dir);
        if (instructionResult.CollisionObject != null)
        {
            instructionResult.WasBlocked = !instructionResult.CollisionObject.Push(robot, dir);
        }
        else
        {
            robot.position += dir;
        }

        return instructionResult;
    }
}

[Serializable]
public class TurnLeft : Instruction
{
    public override string CommandShortForm => "LFT";
    public override string CommandLongForm => "Turn Left";


    public override InstructionResult Execute(RobotEntityData robot)
    {
        robot.direction = robot.direction.TurnedLeft();
        return new InstructionResult();
    }
}

[Serializable]
public class TurnRight : Instruction
{
    public override string CommandShortForm => "RGT";
    public override string CommandLongForm => "Turn Right";


    public override InstructionResult Execute(RobotEntityData robot)
    {
        robot.direction = robot.direction.TurnedRight();
        return new InstructionResult();
    }
}
