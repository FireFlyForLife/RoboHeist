using System;
using UnityEngine;


public class InstructionResult
{
    public GameObject CollisionObject = null;
}


/// <summary>
/// A single instruction for a robot
/// </summary>
[Serializable]
public abstract class Instruction
{
    public abstract string CommandShortForm { get; }
    public abstract string CommandLongForm { get; }
    public abstract InstructionResult Execute(Robot robot);
}

[Serializable]
public class NoOp : Instruction
{
    public override string CommandShortForm => "NOP";
    public override string CommandLongForm => "Wait";

    public override InstructionResult Execute(Robot robot)
    {
        return new InstructionResult();
    }
}

[Serializable]
public class MoveForward : Instruction
{
    public override string CommandShortForm => "FWD";
    public override string CommandLongForm => "Move Forward";

    public override InstructionResult Execute(Robot robot)
    {
        var dir = robot.direction.AsVec2();
        robot.position += dir;
        return new InstructionResult();
    }
}

[Serializable]
public class TurnLeft : Instruction
{
    public override string CommandShortForm => "LFT";
    public override string CommandLongForm => "Turn Left";

    public override InstructionResult Execute(Robot robot)
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

    public override InstructionResult Execute(Robot robot)
    {
        robot.direction = robot.direction.TurnedRight();
        return new InstructionResult();
    }
}
