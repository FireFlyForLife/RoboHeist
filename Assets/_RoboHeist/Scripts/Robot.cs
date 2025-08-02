using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[Serializable]
//public class Robot : MoveableTileEntity
//{
//    public RobotConfig config;
//    public InstructionQueue instructionQueue;
//    public float executionDelay = 1.0f; // In seconds
//    public UInt16 basePointer = 0x3181;

//    [SerializeField]
//    private RobotState currentState = RobotState.Idle;

//    private Coroutine instructionRunner = null;

//    public RobotState CurrentState
//    {
//        get => currentState;
//        set
//        {
//            if (currentState == value)
//            {
//                return;
//            }

//            StartRobotBehaviour(value);
//            currentState = value;
//        }
//    }

//    public IEnumerator<Instruction> Instructions { get; set; }

//    protected override void Start()
//    {
//        base.Start();
//        //instructionQueue = new Instruction[baseInstructions.InstructionQueue.Length];
//        //baseInstructions.InstructionQueue.CopyTo(instructionQueue, 0);

//        instructionQueue = new InstructionQueue(config.InstructionQueue);
//        StartRobotBehaviour(currentState);
//    }

//    private void StartRobotBehaviour(RobotState newState)
//    {
//        // Stop old running state
//        if (instructionRunner != null)
//        {
//            StopCoroutine(instructionRunner);
//        }

//        // Update state so new runner has only sees the new state
//        currentState = newState;

//        // Start new runner
//        if (newState == RobotState.Running)
//        {
//            instructionRunner = StartCoroutine(ExecuteInstructions());
//        }
//        else if (newState == RobotState.Error && config.ErrorHandler != null)
//        {
//            instructionRunner = StartCoroutine(config.ErrorHandler.ExecuteErrorInstructions(this));
//        }
//    }

//    private IEnumerator ExecuteInstructions()
//    {
//        Instructions = instructionQueue.GetNextInstruction(true);
//        while (currentState != RobotState.Idle && Instructions.MoveNext())
//        {
//            var instructionResult = Instructions.Current.Execute(this);
//            if (config.ErrorHandler != null && config.ErrorHandler.DetectErrorState(this, instructionResult))
//            {
//                currentState = RobotState.Error;
//            }
//            yield return new WaitForSeconds(executionDelay);
//        }

//        CurrentState = RobotState.Idle;
//    }
//}
