using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [Header("FOV")]
    public bool changeFOV = false;
    public float FOV;

    [Header("Distance")]
    public bool changeDistance = false;
    public float Distance;

    [Header("Roll Angle")]
    public bool changeRollAngle = false;
    public float RollAngle;

    [Header("Look at Transform")]
    public bool changeLookAtTransform = false;
    public Transform LookAtTransform;

    [Header("Follow Transform")]
    public bool changeFollowTransform = false;
    public Transform FollowTransform;

    [Header("Camera Shake")]
    public bool triggerCameraShake = false;
    public float duration;
    public float magnitude;
    public bool controllerVibration = false;


    private CharacterCamera playerCamera;

    private void Start()
    {
        playerCamera = Player.Instance.CameraController;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (changeFOV) playerCamera.SetFov(FOV);
            if (changeDistance) playerCamera.SetDistance(Distance);
            if (changeRollAngle) playerCamera.SetTargetRollAngle(RollAngle);
            if (changeLookAtTransform && LookAtTransform != null) playerCamera.SetLookatTransform(LookAtTransform);
            if (changeFollowTransform) playerCamera.SetFollowTransform(FollowTransform);
            if (triggerCameraShake) playerCamera.TriggerCameraShake(duration, magnitude, controllerVibration);
        }
    }
}