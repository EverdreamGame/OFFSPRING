using UnityEngine;
using UnityEngine.InputSystem;
using KinematicCharacterController;
using System.Collections;

public struct PlayerCharacterInputs
{
    public Vector2 walkInput;
    public Vector2 lookInput;

    public Quaternion cameraRotation;
}

public enum PlayerInputDevice
{
    Keyboard = 0,
    Gamepad
}

public class Player : MonoBehaviour
{
    public PlayerInputDevice CurrentInputDevice;

    public CharacterController CharacterController;
    public CharacterCamera CameraController;

    public Transform CameraLookAtTransform;

    private PlayerCharacterInputs _characterInputs;
    private IA_Default inputActions;

    void Awake()
    {
        inputActions = new IA_Default();
        CharacterController.PlayerManager = this;
        CameraController.PlayerManager = this;
        CameraController.SetLookAtTransform(CameraLookAtTransform);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        _characterInputs.cameraRotation = CameraController.transform.rotation;

        HandleCharacterInput();
        HandleCameraInput();
        ResetInputs();
    }

    // ======================== HANDLE INPUTS ========================
    private void HandleCameraInput()
    {
        // Handle rotating the camera along with physics movers
        if (CameraController.RotateWithPhysicsMover && CharacterController.Motor.AttachedRigidbody != null)
        {
            CameraController.PlanarDirection = CharacterController.Motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation * CameraController.PlanarDirection;
            CameraController.PlanarDirection = Vector3.ProjectOnPlane(CameraController.PlanarDirection, CharacterController.Motor.CharacterUp).normalized;
        }

        Vector3 lookInputVector = new Vector3(_characterInputs.lookInput.x, _characterInputs.lookInput.y, 0f);

        // Apply inputs to the camera
        CameraController.UpdateWithInput(Time.deltaTime, lookInputVector);
    }
    private void HandleCharacterInput()
    { 

        // Apply inputs to character
        CharacterController.SetInputs(ref _characterInputs);
    }
    void ResetInputs()
    {
        StartCoroutine(ResetInputsOnEndOfFrame());
    }

    IEnumerator ResetInputsOnEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        // Reset inputs
    }

    // ======================== INPUT ACTIONS ========================
    void OnWalk(InputAction.CallbackContext context)
    {
        _characterInputs.walkInput = context.ReadValue<Vector2>();

        CurrentInputDevice = GetCurrentInputDevice(context);
    }

    void OnLook(InputAction.CallbackContext context)
    {
        _characterInputs.lookInput = context.ReadValue<Vector2>();

        CurrentInputDevice = GetCurrentInputDevice(context);
    }

    PlayerInputDevice GetCurrentInputDevice(InputAction.CallbackContext context)
    {
        if (context.control.device is UnityEngine.InputSystem.Gamepad)
        {
            return PlayerInputDevice.Gamepad;
        }
        else if (context.control.device is UnityEngine.InputSystem.Mouse || context.control.device is UnityEngine.InputSystem.Keyboard)
        {
            return PlayerInputDevice.Keyboard;
        }

        // Retorno por defecto si no se detecta ninguno de los anteriores
        return PlayerInputDevice.Keyboard;
    }

    void OnEnable()
    {
        inputActions.Gameplay.Enable();

        inputActions.Gameplay.Move.performed += OnWalk;
        inputActions.Gameplay.Move.canceled += OnWalk;
        inputActions.Gameplay.Look.performed += OnLook;
        inputActions.Gameplay.Look.canceled += OnLook;
    }
    void OnDisable()
    {
        inputActions.Gameplay.Disable();

        inputActions.Gameplay.Move.performed -= OnWalk;
        inputActions.Gameplay.Move.canceled -= OnWalk;
        inputActions.Gameplay.Look.performed -= OnLook;
        inputActions.Gameplay.Look.canceled -= OnLook;
    }
}