using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RobotConfig", menuName = "Scriptable Objects/RobotConfig")]
public class RobotConfig : ScriptableObject
{
    [SerializeReference, SubclassSelector]
    public Instruction[] InstructionQueue;

    [SerializeReference, SubclassSelector]
    public IErrorBehaviour ErrorHandler;
}
