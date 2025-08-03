using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RobotConfig", menuName = "Scriptable Objects/RobotConfig")]
public class RobotConfig : ScriptableObject
{
    public string DisplayName;

    [SerializeReference, SubclassSelector]
    public Instruction[] InstructionQueue;

    [SerializeReference, SubclassSelector]
    public IErrorBehaviour ErrorHandler;

    public bool CanLift = false;
    public float ExecutionDelay = 1.0f;
}
