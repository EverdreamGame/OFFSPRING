using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

public class CharacterCamera : MonoBehaviour
{
    [HideInInspector] public Player PlayerManager;

    [Header("Sensibility")]
    [Range(0.5f, 2)]
    public float mouseSensitivity = 1;
    [Range(0.5f, 2)]
    public float controllerSensitivity = 1;

    [Header("Framing")]
    public Camera Camera;
    public Vector2 FollowPointFraming = new Vector2(0f, 0f);
    public float FollowingSharpness = 1f;

    [Header("Distance")]
    public float DefaultDistance = 6f;
    public float MinDistance = 0f;
    public float MaxDistance = 10f;
    public float DistanceMovementSpeed = 5f;
    public float DistanceMovementSharpness = 10f;

    [Header("Rotation")]
    [Range(-90f, 90f)] public float DefaultVerticalAngle = 20f;
    [Range(-90f, 90f)] public float MinVerticalAngle = -90f;
    [Range(-90f, 90f)] public float MaxVerticalAngle = 90f;
    public float RotationSpeed = 1f;
    public float RotationSharpness = 1000f;
    public bool RotateWithPhysicsMover = false;
    public bool LockAxisX = false;
    public bool LockAxisY = false;

    [Header("Camera Shake")]
    public bool isCameraShaking = false;

    [Header("Obstruction")]
    public float ObstructionCheckRadius = 0.2f;
    public LayerMask ObstructionLayers = -1;
    public float ObstructionSharpness = 10000f;
    public List<Collider> IgnoredColliders = new List<Collider>();

    public Transform LookAtTransform { get; private set; } // Transform the camera will look at

    private float _currentDistance;
    private float _targetDistance;
    private float _targetVerticalAngle;
    private Vector3 _currentFollowPosition;

    private bool _distanceIsObstructed;
    private int _obstructionCount;
    private RaycastHit[] _obstructions = new RaycastHit[MaxObstructions];
    private const int MaxObstructions = 32;

    [HideInInspector] public Vector3 PlanarDirection { get; set; }

    void OnValidate()
    {
        DefaultDistance = Mathf.Clamp(DefaultDistance, MinDistance, MaxDistance);
    }
    void Awake()
    {
        PlanarDirection = Vector3.forward;

        _currentDistance = DefaultDistance;
        _targetDistance = _currentDistance;

        _targetVerticalAngle = 0f;
    }

    public void SetLookAtTransform(Transform t)
    {
        LookAtTransform = t;
        PlanarDirection = LookAtTransform.forward;
        _currentFollowPosition = LookAtTransform.position;
    }

    public void UpdateWithInput(float deltaTime, Vector3 lookDirection)
    {
        HandleObstructions(deltaTime);
        UpdatePositionAndRotation(deltaTime, lookDirection);
    }

    void UpdatePositionAndRotation(float deltaTime, Vector3 rotationInput)
    {
        if (LookAtTransform)
        {
            if (LockAxisX)
            {
                rotationInput = new Vector3(0, rotationInput.y, 0);
            }
            if (LockAxisY)
            {
                rotationInput = new Vector3(rotationInput.x, 0, 0);
            }

            // Process rotation input
            Quaternion rotationFromInput = Quaternion.Euler(LookAtTransform.up * (rotationInput.x * RotationSpeed));
            PlanarDirection = rotationFromInput * PlanarDirection;
            PlanarDirection = Vector3.Cross(LookAtTransform.up, Vector3.Cross(PlanarDirection, LookAtTransform.up));
            Quaternion planarRot = Quaternion.LookRotation(PlanarDirection, LookAtTransform.up);

            _targetVerticalAngle -= (rotationInput.y * RotationSpeed);
            _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
            Quaternion verticalRot = Quaternion.Euler(_targetVerticalAngle, 0, 0);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, planarRot * verticalRot, 1f - Mathf.Exp(-RotationSharpness * deltaTime));

            // Apply rotation
            transform.rotation = targetRotation;

            // Find the smoothed follow position
            _currentFollowPosition = Vector3.Lerp(_currentFollowPosition, LookAtTransform.position, 1f - Mathf.Exp(-FollowingSharpness * deltaTime));

            // Find the smoothed camera orbit position
            Vector3 targetPosition = _currentFollowPosition - ((targetRotation * Vector3.forward) * _currentDistance);

            transform.position = targetPosition;
        }
    }

    void HandleObstructions(float deltaTime)
    {
        RaycastHit closestHit = new RaycastHit();
        closestHit.distance = Mathf.Infinity;
        _obstructionCount = Physics.SphereCastNonAlloc(_currentFollowPosition, ObstructionCheckRadius, -transform.forward, _obstructions, _targetDistance, ObstructionLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < _obstructionCount; i++)
        {
            bool isIgnored = false;
            for (int j = 0; j < IgnoredColliders.Count; j++)
            {
                if (IgnoredColliders[j] == _obstructions[i].collider)
                {
                    isIgnored = true;
                    break;
                }
            }

            if (!isIgnored && _obstructions[i].distance < closestHit.distance && _obstructions[i].distance > 0)
            {
                closestHit = _obstructions[i];
            }
        }

        // Si hay una obstrucción
        if (closestHit.distance < Mathf.Infinity)
        {
            _distanceIsObstructed = true;
            _currentDistance = Mathf.Lerp(_currentDistance, closestHit.distance, 1 - Mathf.Exp(-ObstructionSharpness * deltaTime));
        }
        else // Si no hay obstrucción
        {
            _distanceIsObstructed = false;
            _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, 1 - Mathf.Exp(-DistanceMovementSharpness * deltaTime));
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