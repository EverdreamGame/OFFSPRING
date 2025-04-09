using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

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
    public float RotationSpeed = 1f;
    public float RotationSharpness = 1000f;

    [Space]
    [Header("Camera Shake")]
    public bool isCameraShaking = false;

    [Space]
    [Header("Position")]
    /// <summary>
    /// Transform the camera will look at
    /// </summary>
    [Tooltip("Transform the camera will look at.")]
    public Transform LookAtTransform;

    /// <summary>
    /// Transform the camera will follow. The camera will try to mimic its position 
    /// If it's null, the camera will calculate its position using
    /// "Default distance" & "DirectionFromPlayer"
    /// </summary>
    [Tooltip("Transform the camera will follow. The camera will try to mimic its position. If it's null, the camera will calculate its position using \"Default distance\" & \"DirectionFromPlayer\" (see Camera inspector).")]
    public Transform FollowTransfrom;

    [HideInInspector] public Vector3 PlanarDirection { get; set; }

    void OnValidate()
    {
        DefaultDistance = Mathf.Clamp(DefaultDistance, MinDistance, MaxDistance);
    }
    void Awake()
    {
        PlanarDirection = Vector3.forward;
    }

    void Update()
    {
        // TODO MARC SPRINT 2: Haz esto mas limpio

        // Position
        Vector3 targetPosition;
        if (FollowTransfrom != null)
        {
            targetPosition = FollowTransfrom.position;
        }
        else 
        { 
            targetPosition = LookAtTransform.position + (DirectionFromPlayer * DefaultDistance);
        }
        transform.position = Vector3.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-FollowingSharpness * Time.deltaTime));

        // TODO MARC SPRINT 2: Haz roll de la camara yeah perdonen

        // Rotation
        Vector3 targetDirection = (LookAtTransform.position - transform.position).normalized;
        transform.forward = Vector3.Slerp(transform.forward, targetDirection, 1f - Mathf.Exp(-RotationSharpness * Time.deltaTime));
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