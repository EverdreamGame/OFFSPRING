using UnityEngine;

public class ChangeCameraSettingsWhenTriggerEnter : MonoBehaviour
{
    CharacterCamera cameraToChange;

    public float distanceFromTarget;

    private void Start()
    {
        cameraToChange = Player.Instance.CameraController;
    }

    private void OnTriggerEnter(Collider other)
    {
        cameraToChange.DefaultDistance = distanceFromTarget;
    }
}
