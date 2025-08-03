using UnityEngine;

public class DoRotateRigidbody : MonoBehaviour
{
    public Vector3 angularVelocity = new Vector3(0.0f, 4.0f, 0.0f);

    void Start()
    {
        GetComponent<Rigidbody>().angularVelocity = angularVelocity;
    }
}
