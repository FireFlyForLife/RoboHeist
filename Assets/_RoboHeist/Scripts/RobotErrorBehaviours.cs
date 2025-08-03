using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RobotState
{
    Idle,
    Running,
    Error,
}

public interface IErrorBehaviour
{
    bool DetectErrorState(RobotEntityBehaviour robot, InstructionResult result) => false;
    IEnumerator ExecuteErrorInstructions(RobotEntityBehaviour robot) => null;
}

[System.Serializable]
public class PanicError : IErrorBehaviour
{
    public float panicTime = 10.0f;
    public float panicExecutionDelay = 0.5f;

    public bool DetectErrorState(RobotEntityBehaviour robot, InstructionResult result)
    {
        return result.WasBlocked;
    }

    public IEnumerator ExecuteErrorInstructions(RobotEntityBehaviour robot)
    {
        Instruction[] allInstructions = new Instruction[]
        {
            //new NoOp(),
            new MoveForward(),
            new MoveForward(),
            new MoveForward(),
            new TurnLeft(),
            new TurnRight()
        };
        float panicEndTime = Time.time + panicTime;
        while (Time.time < panicEndTime)
        {
            allInstructions[Random.Range(0, allInstructions.Length - 1)].Execute(robot);
            yield return new WaitForSeconds(panicExecutionDelay);
        }
    }
}
