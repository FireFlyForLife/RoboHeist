using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class InstructionQueue
{
    enum Direction
    {
        Forward = 1,
        Backward = -1
    }

    [SerializeField]
    private Instruction[] instructions;

    [SerializeField]
    private int instructionCounter = 0;

    public InstructionQueue(Instruction[] queue)
    {
        Reset(queue);
    }

    public IEnumerator<Instruction> GetNextInstruction(bool loop)
    {
        if (!instructions.Any(i => i != null))
        {
            yield break;
        }

        while (true)
        {
            yield return GetInstruction(Direction.Forward, loop);
        }
    }
    public IEnumerator<Instruction> GetPreviousInstruction(bool loop)
    {
        if (!instructions.Any(i => i != null))
        {
            yield break;
        }

        while (true)
        {
            yield return GetInstruction(Direction.Backward, loop);
        }
    }

    public void Reset(Instruction[] queue)
    {
        instructions = new Instruction[queue.Length];
        queue.CopyTo(instructions, 0);
        instructionCounter = 0;
    }

    private Instruction GetInstruction(Direction direction, bool loop)
    {
        while (instructions.Any(i => i != null))
        {
            Instruction instruction = instructions[instructionCounter];
            int unwrappedCounter = (instructionCounter + (int)direction);
            instructionCounter = unwrappedCounter % instructions.Length;

            // Detect the loop point.
            if (!loop && instructionCounter != unwrappedCounter)
            {
                break;
            }

            if (instruction == null)
            {
                continue;
            }

            return instruction;
        }
        return null;
    }
}
