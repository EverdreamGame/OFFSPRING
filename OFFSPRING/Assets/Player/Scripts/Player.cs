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
    // Public
    [Header("Input device (read only)")]
    public PlayerInputDevice CurrentInputDevice;

    [Space]
    [Header("Scripts")]
    public KCharacterController KinematicCharacterController;
    public CharacterCamera CameraController;
    public PlayerInteractionScript playerInteractionScript;

    [Space]
    [Header("Components")]
    public Animator CharacterAnimator;

    // Private
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
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        _characterInputs.cameraRotation = CameraController.transform.rotation;

        HandleCharacterInput();
        ResetInputs();
    }

    // ======================== HANDLE INPUTS ========================
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