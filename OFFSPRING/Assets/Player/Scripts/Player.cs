using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using KinematicCharacterController;

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
    [Space]
    public KCharacterController KinematicCharacterController;
    public CharacterCamera CameraController;

    [Space]
    public Transform CameraLookAtTransform;

    [Space]
    public Animator CharacterAnimator;

    [Space]
    public PlayerInteractionScript playerInteractionScript;

    [Space]
    private PlayerCharacterInputs _characterInputs;
    private IA_Default inputActions;

    public static Player Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        inputActions = new IA_Default();
        KinematicCharacterController.PlayerManager = this;
        KinematicCharacterController.Animator = CharacterAnimator;
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
        // TODO MARC SPRINT 2: Esto hace 0 falta, quitalo

        // Handle rotating the camera along with physics movers
        if (CameraController.RotateWithPhysicsMover && KinematicCharacterController.Motor.AttachedRigidbody != null)
        {
            CameraController.PlanarDirection = KinematicCharacterController.Motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation * CameraController.PlanarDirection;
            CameraController.PlanarDirection = Vector3.ProjectOnPlane(CameraController.PlanarDirection, KinematicCharacterController.Motor.CharacterUp).normalized;
        }

        Vector3 lookInputVector = new Vector3(_characterInputs.lookInput.x, _characterInputs.lookInput.y, 0f);

        // Apply inputs to the camera
        CameraController.UpdateWithInput(Time.deltaTime, lookInputVector);
    }
    private void HandleCharacterInput()
    { 
        // Apply inputs to character
        KinematicCharacterController.SetInputs(ref _characterInputs);
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

    void OnInteract(InputAction.CallbackContext context)
    {
        playerInteractionScript.Interaction();

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

        //Interaction
        inputActions.Gameplay.Interact.performed += OnInteract;
    }

    void OnDisable()
    {
        inputActions.Gameplay.Disable();

        inputActions.Gameplay.Move.performed -= OnWalk;
        inputActions.Gameplay.Move.canceled -= OnWalk;
        inputActions.Gameplay.Look.performed -= OnLook;
        inputActions.Gameplay.Look.canceled -= OnLook;

        //Interaction
        inputActions.Gameplay.Interact.performed -= OnInteract;
    }
}