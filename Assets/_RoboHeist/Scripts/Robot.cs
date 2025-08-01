using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A single instruction for a robot
/// </summary>
[Serializable]
public abstract class Instruction
{
    public string Label = "Instruction";
    public Sprite Sprite = null;

    public abstract void Execute(Robot robot);

}

public class MoveForward : Instruction
{
    public MoveForward()
    {
        Label = "Move Forward";
    }

    public override void Execute(Robot robot)
    {
        var dir = robot.direction.AsVec2();
        robot.position += dir;
    }
}

public class TurnLeft : Instruction
{
    public TurnLeft()
    {
        Label = "Turn Left";
    }

    public override void Execute(Robot robot)
    {
        robot.direction = robot.direction.TurnedLeft();
    }
}

public class TurnRight : Instruction
{
    public TurnRight()
    {
        Label = "Turn Right";
    }

    public override void Execute(Robot robot)
    {
        robot.direction = robot.direction.TurnedRight();
    }
}



public class Robot : TileEntity
{
    public List<Instruction> instructionList = new();
    public float executionDelay = 1.0f; // In seconds

    private float lastExecution = float.MinValue;
    private int instructionPointer = 0;

    protected override void Start()
    {
        base.Start();

        // TODO: Make this customizable
        instructionList.Add(new MoveForward());
        instructionList.Add(new TurnRight());
        instructionList.Add(new MoveForward());
        instructionList.Add(new TurnRight());
        instructionList.Add(new MoveForward());
        instructionList.Add(new TurnRight());
        instructionList.Add(new MoveForward());
        instructionList.Add(new TurnRight());
    }

    protected override void Update()
    {
        base.Update();

        if (lastExecution + executionDelay < Time.time)
        {
            lastExecution = Time.time;

            Instruction instruction = instructionList[instructionPointer];
            instructionPointer = (instructionPointer + 1) % instructionList.Count;

            instruction.Execute(this);
        }
    }
}
