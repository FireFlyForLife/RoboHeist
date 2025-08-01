using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Robot : MoveableTileEntity
{
    public RobotConfig config;
    public InstructionQueue instructionQueue;
    public float executionDelay = 1.0f; // In seconds

    [SerializeField]
    private RobotState currentState = RobotState.Idle;

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

    protected override void Start()
    {
        base.Start();
        //instructionQueue = new Instruction[baseInstructions.InstructionQueue.Length];
        //baseInstructions.InstructionQueue.CopyTo(instructionQueue, 0);

        instructionQueue = new InstructionQueue(config.InstructionQueue);
        StartRobotBehaviour(currentState);
    }

    private void StartRobotBehaviour(RobotState newState)
    {
        if (instructionRunner != null)
        {
            StopCoroutine(instructionRunner);
        }
        if (newState == RobotState.Running)
        {
            instructionRunner = StartCoroutine(ExecuteInstructions());
        }
        else if (newState == RobotState.Error && config.ErrorHandler != null)
        {
            instructionRunner = StartCoroutine(config.ErrorHandler.ExecuteErrorInstructions(this));
        }
    }

    private IEnumerator ExecuteInstructions()
    {
        Instructions = instructionQueue.GetNextInstruction(true);
        while (currentState != RobotState.Idle && Instructions.MoveNext())
        {
            var instructionResult = Instructions.Current.Execute(this);
            if (config.ErrorHandler != null && config.ErrorHandler.DetectErrorState(this, instructionResult))
            {
                currentState = RobotState.Error;
            }
            yield return new WaitForSeconds(executionDelay);
        }

        CurrentState = RobotState.Idle;
    }
}
