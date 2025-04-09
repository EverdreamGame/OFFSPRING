using System.Collections;
using UnityEngine;

public class CharacterCamera : MonoBehaviour
{
    [HideInInspector] public Player PlayerManager;

    [Space]
    [Header("Framing")]
    public float FollowingSharpness = 1f;

    [Space]
    [Header("Position")]
    public Vector3 DirectionFromPlayer = Vector3.right;
    public float DefaultDistance = 6f;
    public float MinDistance = 0f;
    public float MaxDistance = 10f;
    //public float DistanceMovementSpeed = 5f;
    //public float DistanceMovementSharpness = 10f;

    [Space]
    [Header("Rotation")]
    public float RotationSharpness = 1000f;
    public float TargetRollAngle = 0f;

    [Space]
    [Header("Camera Shake")]
    public bool isCameraShaking = false;

    [Space]
    [Header("Transforms")]
    /// <summary>
    /// Transform the camera will look at
    /// </summary>
    [Tooltip("Position the camera will look at.")]
    public Transform LookAtTransform;

    /// <summary>
    /// Transform the camera will follow. The camera will try to mimic its position 
    /// If it's null, the camera will calculate its position using
    /// "Default distance" & "DirectionFromPlayer"
    /// </summary>
    [Tooltip("Position the camera will follow. The camera will try to mimic its position. If it's null, the camera will calculate its position using \"Default distance\" & \"DirectionFromPlayer\" (see Camera inspector).")]
    public Transform FollowTransfrom;

    [HideInInspector] public Vector3 PlanarDirection { get; set; }

    void OnValidate()
    {
        DefaultDistance = Mathf.Clamp(DefaultDistance, MinDistance, MaxDistance);

        if (TargetRollAngle > 359f) TargetRollAngle = 0f;
        if (TargetRollAngle < 0f) TargetRollAngle = 359f;
    }
    void Awake()
    {
        PlanarDirection = Vector3.forward;
    }

    void Update()
    {
        // ========== POSITION ==========
        Vector3 targetPosition;

        if (LookAtTransform != null)
        {
            if (FollowTransfrom != null)
            {
                // Interpola hacia la posición del transform a seguir
                targetPosition = FollowTransfrom.position;
            }
            else
            {
                // Si no hay FollowTransform, usar offset desde LookAt
                Vector3 offsetDirection = DirectionFromPlayer.normalized;
                offsetDirection *= Mathf.Clamp(DefaultDistance, MinDistance, MaxDistance);
                targetPosition = LookAtTransform.position + offsetDirection;
            }
            transform.position = Vector3.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-FollowingSharpness * Time.deltaTime));

            // ========== ROTATION ==========
            Vector3 toLookTarget = (LookAtTransform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(toLookTarget);

            // Aplica rotación de roll alrededor del eje forward del objetivo
            Quaternion rollRotation = Quaternion.AngleAxis(TargetRollAngle, targetRotation * Vector3.forward);
            targetRotation = rollRotation * targetRotation;

            // Interpolación suave
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-RotationSharpness * Time.deltaTime));
        }
        else
        {
            Debug.LogError("Error CharacterCamera: LookAtTransform is null");
        }
    }

    // ========================================== CAMERA SHAKE ==========================================
    public void TriggerCameraShake(float duration = 0.5f, float magnitude = 0.5f, bool controllerVibration = true)
    {
        if (isCameraShaking) StopCoroutine(CameraShakeCoroutine(duration, magnitude));
        StartCoroutine(CameraShakeCoroutine(duration, magnitude));
    }

    IEnumerator CameraShakeCoroutine(float duration, float magnitude)
    {
        isCameraShaking = true;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            yield return new WaitForEndOfFrame();

            Vector3 offsetX = transform.right * Random.Range(-1f, 1f) * magnitude;
            Vector3 offsetY = transform.up * Random.Range(-1f, 1f) * magnitude;

            transform.position += offsetX + offsetY;

            elapsedTime += Time.deltaTime;
            yield return null; // Espera un frame
        }

        isCameraShaking = false;
    }
}