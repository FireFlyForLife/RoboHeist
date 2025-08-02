using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class RobotEntityBehaviour : MoveableEntityBehaviour
{
    public RobotEntityData robotEntityData;

    public InstructionQueue instructionQueue;
    public RobotState currentState = RobotState.Idle;
    public UInt16 basePointer = 0x3181;

    private Coroutine instructionRunner = null;

    public RobotState CurrentState
    {
        get => currentState;
        set
        {
            if (currentState == value)
            {
                return;
            }

            StartRobotBehaviour(value);
            currentState = value;
        }
    }

    public IEnumerator<Instruction> Instructions { get; set; }

    protected override TileEntityData GetTileEntityData()
    {
        return robotEntityData;
    }

    protected override void Start()
    {
        base.Start();

        instructionQueue = new InstructionQueue(robotEntityData.robotConfig.InstructionQueue);
        currentState = robotEntityData.startingState;
        StartRobotBehaviour(currentState);
    }

    private void StartRobotBehaviour(RobotState newState)
    {
        // Stop old running state
        if (instructionRunner != null)
        {
            StopCoroutine(instructionRunner);
        }

        // Update state so new runner has only sees the new state
        currentState = newState;

        // Start new runner
        if (newState == RobotState.Running)
        {
            instructionRunner = StartCoroutine(ExecuteInstructions());
        }
        else if (newState == RobotState.Error && robotEntityData.robotConfig.ErrorHandler != null)
        {
            instructionRunner = StartCoroutine(robotEntityData.robotConfig.ErrorHandler.ExecuteErrorInstructions(robotEntityData));
        }
    }

    private IEnumerator ExecuteInstructions()
    {
        Instructions = instructionQueue.GetNextInstruction(true);
        while (currentState != RobotState.Idle && Instructions.MoveNext())
        {
            var instructionResult = Instructions.Current.Execute(robotEntityData);
            if (robotEntityData.robotConfig.ErrorHandler != null && robotEntityData.robotConfig.ErrorHandler.DetectErrorState(robotEntityData, instructionResult))
            {
                CurrentState = RobotState.Error;
            }
            yield return new WaitForSeconds(robotEntityData.executionDelay);
        }

        CurrentState = RobotState.Idle;
    }

    private void OnValidate()
    {
        StartRobotBehaviour(currentState);
    }
}
