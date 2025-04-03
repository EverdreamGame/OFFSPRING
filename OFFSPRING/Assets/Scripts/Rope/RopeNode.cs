using UnityEngine;

public class RopeNode : MonoBehaviour
{
    [HideInInspector] public Vector3 previousPosition;

    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }
}