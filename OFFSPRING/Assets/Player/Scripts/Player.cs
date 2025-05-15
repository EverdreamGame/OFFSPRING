using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Events;

public struct PlayerCharacterInputs
{
    public Vector2 walkInput;
    public Vector2 lookInput;
    public bool sprintInput;
    public bool jumpInput;

    public Quaternion cameraRotation;
}

public enum PlayerInputDevice
{
    Keyboard = 0,
    Gamepad
}

// TODO MARC: Implementar player states
public enum PlayerState
{
    Playing = 0,
    Dead,
    Paused
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
    [Header("Pause")]
    public RectTransform PauseMenu;

    [Space]
    [Header("Components")]
    public Animator CharacterAnimator;

    [Space]
    [Header("Events")]
    public UnityEvent OnPaused;

    // Private
    private PlayerCharacterInputs _characterInputs;
    private IA_Default inputActions;
    private bool _isPaused = false;

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

        _characterInputs.sprintInput = false;
        _characterInputs.jumpInput = false;
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
        _characterInputs.jumpInput = false;
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

    void OnSprint(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0.1f) _characterInputs.sprintInput = true;
        else _characterInputs.sprintInput = false;

        CurrentInputDevice = GetCurrentInputDevice(context);
    }

    void OnJump(InputAction.CallbackContext context)
    {
        _characterInputs.jumpInput = true;

        CurrentInputDevice = GetCurrentInputDevice(context);
    }

    void OnPause(InputAction.CallbackContext context)
    {
        _isPaused = !_isPaused;
        if (PauseMenu != null) PauseMenu.gameObject.SetActive(_isPaused);

        if (_isPaused)
        {
            KinematicCharacterController.TransitionToState(CharacterState.Paused);
            OnPaused.Invoke();

            Cursor.visible = true;
        }
        else
        {
            // TODO MARC: De momento esto esta bien pero deberias hacer un check para saber si deberia volver a nadar o al default
            KinematicCharacterController.TransitionToState(CharacterState.Swimming);

            Cursor.visible = false;
        }

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
        inputActions.Gameplay.Sprint.performed += OnSprint;
        inputActions.Gameplay.Sprint.canceled += OnSprint;
        inputActions.Gameplay.Jump.performed += OnJump;
        inputActions.Gameplay.Pause.performed += OnPause;

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
        inputActions.Gameplay.Sprint.performed -= OnSprint;
        inputActions.Gameplay.Sprint.canceled -= OnSprint;
        inputActions.Gameplay.Jump.performed -= OnJump;
        inputActions.Gameplay.Pause.performed -= OnPause;

        //Interaction
        inputActions.Gameplay.Interact.performed -= OnInteract;
    }
}