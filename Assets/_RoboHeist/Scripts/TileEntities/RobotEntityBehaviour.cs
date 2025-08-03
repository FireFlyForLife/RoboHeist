using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public struct HistoricalRobotTransform
{
    public float time;
    public Vector2Int pos;
    public Direction direction;
    public InstructionQueue instructionQueueState;
}

[SelectionBase]
public class RobotEntityBehaviour : MoveableEntityBehaviour
{
    // Global loaded pool of robots
    public static List<RobotEntityBehaviour> allRobots = new();

    public RobotEntityData robotEntityData;

    public InstructionQueue instructionQueue;
    public RobotState currentState = RobotState.Idle;
    public UInt16 basePointer = 0x3181;

    public List<HistoricalRobotTransform> transformHistory = new();

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

        allRobots.Add(this);

        instructionQueue = new InstructionQueue(robotEntityData.robotConfig.InstructionQueue);

        transformHistory.Add(new HistoricalRobotTransform
        {
            time = -1.0f,
            pos = GetTileEntityData().position,
            direction = GetTileEntityData().direction,
            instructionQueueState = instructionQueue.CloneViaSerialization()
        });

        currentState = robotEntityData.startingState;
        StartRobotBehaviour(currentState);
    }

    void OnDestroy()
    {
        allRobots.Remove(this);
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

            transformHistory.Add(new HistoricalRobotTransform
            {
                time = TimelineUIController.TheTime,
                pos = GetTileEntityData().position,
                direction = GetTileEntityData().direction,
                instructionQueueState = instructionQueue.CloneViaSerialization()
            });

            foreach (var aaaa in transformHistory)
                Debug.Log($"{instructionQueue.GetInstructionPointer()}  {aaaa.instructionQueueState.GetInstructionPointer()} and {String.Join(',', aaaa.instructionQueueState.GetAllInstructions().ToArray().GetEnumerator())}");

            yield return new WaitForSeconds(robotEntityData.executionDelay);
        }

        CurrentState = RobotState.Idle;
    }

    private void OnValidate()
    {
        StartRobotBehaviour(currentState);
    }
}
