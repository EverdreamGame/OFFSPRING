using UnityEngine;
using KinematicCharacterController;
using System.Collections.Generic;
using UnityEngine.Events;

public enum CharacterState
{
    Default,
    Swimming
}

public enum OrientationMethod
{
    TowardsCamera,
    TowardsMovement,
    TowardsLookInput
}

public enum BonusOrientationMethod
{
    None,
    TowardsGravity,
    TowardsGroundSlopeAndGravity,
}

public class KCharacterController : MonoBehaviour, ICharacterController
{
    public KinematicCharacterMotor Motor;

    [HideInInspector] public Animator Animator;
    [HideInInspector] public Player PlayerManager; 

    public CharacterState CurrentCharacterState;

    [Space]
    [Header("Stable Movement")]
    public float MaxStableMoveSpeed = 10f;
    public float StableMovementSharpness = 15f;
    public float OrientationSharpness = 10f;
    public OrientationMethod OrientationMethod = OrientationMethod.TowardsMovement;

    [Space]
    [Header("Air Movement")]
    public float MaxAirMoveSpeed = 15f;
    public float AirAccelerationSpeed = 15f;
    public float AirDrag = 0.1f;

    [Space]
    [Header("Underwater Movement")]
    public float MaxUnderwaterSpeed = 7f;
    public float MaxUnderwaterSprintSpeed = 15f;
    public float UnderwaterImpulseSpeed = 22f;
    public float UnderwaterAccelerationSpeed = 15f;
    public float UnderwaterDrag = 1f;
    public float UnderwaterOrientationSharpness = 5f;
    public float UnderwaterImpulseCooldown = 0.5f;

    [Space]
    [Header("Stun Player")]
    public float DefaultStunTime = 0.6f;
    // TODO MARC: ir rellenando con atributos

    [Space]
    [Header("Joints & Constrains")]
    // TODO MARC: Cosas de interacciones como pesos o yoquese

    [Space]
    [Header("Misc")]
    public List<Collider> IgnoredColliders = new List<Collider>();
    public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
    public float BonusOrientationSharpness = 10f;
    public Vector3 Gravity = new Vector3(0, -30f, 0);

    // TODO MARC: Poner eventos para acciones (OnPlayerStun...)
    [Space]
    [Header("Events")]
    public UnityEvent OnPlayerStun, OnUnderwaterImpulse;

    // Private fields -------------------------------------------------
    Vector3 _moveInputVector = Vector3.zero;
    Vector3 _lookInputVector = Vector3.zero;
    Vector3 _internalVelocityAdd = Vector3.zero;
    Vector3 _targetVelocity = Vector3.zero;
    Vector3 _movementDirection = Vector3.zero;
    Quaternion _targetRotation = Quaternion.identity;
    bool _setRotationRequest = false;
    bool _setVelocityRequest = false;
    bool _jumpRequest = false;
    bool _canJump = true;
    bool _canSprint = true;

    bool _isSprinting = false;
    bool _isStunned = false;

    float _lastImpulseTime = 0f;
    float _lastStunTime = 0f;

    void Awake()
    {
        // Handle initial state
        TransitionToState(CurrentCharacterState);

        // Assign the characterController to the motor
        Motor.CharacterController = this;
    }

    // ========================================== STATES ==========================================
    public void TransitionToState(CharacterState newState)
    {
        CharacterState tmpInitialState = CurrentCharacterState;
        OnStateExit(tmpInitialState, newState);
        CurrentCharacterState = newState;
        OnStateEnter(newState, tmpInitialState);
    }
    public void OnStateEnter(CharacterState state, CharacterState fromState)
    {
        switch (state)
        {
            case CharacterState.Default:
                {
                    break;
                }
            case CharacterState.Swimming:
                {
                    // If this is ennabled while swimming you will not swim up after hitting ground
                    Motor.SetGroundSolvingActivation(false);

                    Motor.HasPlanarConstraint = true;

                    // TODO MARC: Cuando haya animaciones y modelo definitivos cambiar esto
                    Motor.SetCapsuleDimensions(1.14f, 2.25f, 0f);
                    break;
                }
        }
    }
    public void OnStateExit(CharacterState state, CharacterState toState)
    {
        switch (state)
        {
            case CharacterState.Default:
                {
                    break;
                }
            case CharacterState.Swimming:
                {
                    // If this is ennabled while swimming you will not swim up after hitting ground
                    Motor.SetGroundSolvingActivation(true);

                    Motor.HasPlanarConstraint = false;

                    Motor.SetCapsuleDimensions(0.5f, 2.25f, 0f);
                    break;
                }
        }
    }

    // ========================================== INPUTS ==========================================
    public void SetInputs(ref PlayerCharacterInputs inputs)
    {
        // Calculate camera direction and rotation on the character plane
        Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.cameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.cameraRotation * Vector3.up, Motor.CharacterUp).normalized;
        }
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

        // Calculate Move and Look Input Vector 
        switch (CurrentCharacterState)
        {
            case CharacterState.Default:
                {
                    // Move and look inputs
                    _moveInputVector = cameraPlanarRotation * Vector3.ClampMagnitude(new Vector3(inputs.walkInput.x, 0, inputs.walkInput.y), 1f);

                    switch (OrientationMethod)
                    {
                        case OrientationMethod.TowardsCamera:
                            _lookInputVector = cameraPlanarDirection;
                            break;
                        case OrientationMethod.TowardsMovement:
                            _lookInputVector = _moveInputVector.normalized;
                            break;
                        case OrientationMethod.TowardsLookInput:
                            _lookInputVector = cameraPlanarRotation * new Vector3(inputs.lookInput.x, 0f, inputs.lookInput.y);
                            break;
                    }
                    break;
                }
            case CharacterState.Swimming:
                {
                    // Get camera-relative input when swimming
                    Vector3 camRight = inputs.cameraRotation * Vector3.right;
                    Vector3 camUp = inputs.cameraRotation * Vector3.up;

                    // Reorient input Y (vertical) y Z (forward) al espacio de la cámara
                    _moveInputVector = (inputs.walkInput.y * camUp + inputs.walkInput.x * camRight).normalized;
                    _lookInputVector = _moveInputVector;
                    break;
                }
        }

        // Other inputs
        if (inputs.jumpInput && _canJump)
        {
            _jumpRequest = true;
        }

        if (_canSprint)
        {
            _isSprinting = inputs.sprintInput;
        }
    }

    // ========================================== UPDATE ==========================================
    public void BeforeCharacterUpdate(float deltaTime)
    {

    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (_setRotationRequest)
        {
            currentRotation = _targetRotation;
            _setRotationRequest = false;
            return;
        }

        if (_isStunned) return;

        switch (CurrentCharacterState)
        {
            case CharacterState.Default:
                {
                    if (_lookInputVector.sqrMagnitude > 0f && OrientationSharpness > 0f)
                    {
                        // Smoothly interpolate from current to target look direction
                        Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                        // Set the current rotation (which will be used by the KinematicCharacterMotor)
                        currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                    }

                    Vector3 currentUp = (currentRotation * Vector3.up);
                    if (BonusOrientationMethod == BonusOrientationMethod.TowardsGravity)
                    {
                        // Rotate from current up to invert gravity
                        Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                    }
                    else if (BonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity)
                    {
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            Vector3 initialCharacterBottomHemiCenter = Motor.TransientPosition + (currentUp * Motor.Capsule.radius);

                            Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;

                            // Move the position to create a rotation around the bottom hemi center instead of around the pivot
                            Motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * Motor.Capsule.radius));
                        }
                        else
                        {
                            Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -Gravity.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                        }
                    }
                    else
                    {
                        Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
                    }
                    break;
                }
            case CharacterState.Swimming:
                {
                    // TODO MARC: Hacer esto más limpio

                    Quaternion TargetRotation = Quaternion.identity;
                    // Get target rotation
                    if (_canJump) // Si se está impulsando, no gira ni se puede cambiar de dirección
                    {
                        float cameraRollAngle = PlayerManager.CameraController.TargetRollAngle;

                        if (_lookInputVector.sqrMagnitude > 0f)
                        {
                            Quaternion rawRotation = Quaternion.LookRotation(_moveInputVector, Motor.CharacterUp);
                            Vector3 eulerAngles = rawRotation.eulerAngles;

                            // Ajustar euler angles para que el personaje no quede boca arriba
                            eulerAngles.z = 0;
                            if (eulerAngles.x > 90 && eulerAngles.x < 270)
                            {
                                eulerAngles.y += 180;
                                eulerAngles.x = 180 - eulerAngles.x;
                            }
                            TargetRotation = Quaternion.Euler(eulerAngles);
                        }
                        else
                        {
                            // TODO MARC: Hacer esto relativo a la rotación de la cámara

                            Vector3 eulerAngles = currentRotation.eulerAngles;
                            TargetRotation = Quaternion.Euler(0, eulerAngles.y, 0);
                        }
                    }
                    //else
                    //{
                    //    Quaternion rawRotation = Quaternion.LookRotation(_movementDirection, Motor.CharacterUp);
                    //    Vector3 eulerAngles = rawRotation.eulerAngles;

                    //    // Ajustar euler angles para que el personaje no quede boca arriba
                    //    eulerAngles.z = 0;
                    //    if (eulerAngles.x > 90 && eulerAngles.x < 270)
                    //    {
                    //        eulerAngles.y += 180;
                    //        eulerAngles.x = 180 - eulerAngles.x;
                    //    }
                    //    TargetRotation = Quaternion.Euler(eulerAngles);
                    //}

                    // Set current rotation
                    if (UnderwaterOrientationSharpness > 0)
                    {
                        // Smoothly interpolate from current to target rotation
                        currentRotation = Quaternion.Slerp(currentRotation, TargetRotation, 1 - Mathf.Exp(-UnderwaterOrientationSharpness * deltaTime)).normalized;
                    }
                    else
                    {
                        currentRotation = TargetRotation;
                    }
                    break;
                }

        }

    }
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (_setVelocityRequest)
        {
            currentVelocity = _targetVelocity;
            _setVelocityRequest = false;
            return;
        }

        if (_isStunned) return;

        // Manage character state
        switch (CurrentCharacterState)
        {
            case CharacterState.Default:
                {
                    // Ground movement
                    if (Motor.GroundingStatus.IsStableOnGround)
                    {
                        float currentVelocityMagnitude = currentVelocity.magnitude;

                        Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;

                        // Reorient velocity on slope
                        currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                        // Calculate target velocity
                        Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                        Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                        Vector3 targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                        // Smooth movement Velocity
                        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
                    }

                    // Air movement
                    else
                    {
                        // Add move input
                        if (_moveInputVector.sqrMagnitude > 0f)
                        {
                            Vector3 addedVelocity = _moveInputVector * AirAccelerationSpeed * deltaTime;

                            Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                            // Limit air velocity from inputs
                            if (currentVelocityOnInputsPlane.magnitude < MaxAirMoveSpeed)
                            {
                                // clamp addedVel to make total vel not exceed max vel on inputs plane
                                Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, MaxAirMoveSpeed);
                                addedVelocity = newTotal - currentVelocityOnInputsPlane;
                            }
                            else
                            {
                                // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                                if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                                {
                                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                                }
                            }

                            // Prevent air-climbing sloped walls
                            if (Motor.GroundingStatus.FoundAnyGround)
                            {
                                if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                                {
                                    Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                                }
                            }

                            // Apply added velocity
                            currentVelocity += addedVelocity;
                        }

                        // Gravity
                        currentVelocity += Gravity * deltaTime;

                        // Drag
                        currentVelocity *= (1f / (1f + (AirDrag * deltaTime)));
                    }

                    // Take into account additive velocity
                    if (_internalVelocityAdd.sqrMagnitude > 0f)
                    {
                        currentVelocity += _internalVelocityAdd;
                        _internalVelocityAdd = Vector3.zero;
                    }
                    break;
                }
            case CharacterState.Swimming:
                {
                    // Add move input
                    if (_jumpRequest)
                    {
                        if (_moveInputVector.sqrMagnitude > 0f)
                        {
                            currentVelocity = UnderwaterImpulseSpeed * _moveInputVector;

                            _jumpRequest = false;
                            _canJump = false;
                            _lastImpulseTime = Time.time;

                            Animator.SetBool("isMoving", true);

                            OnUnderwaterImpulse?.Invoke();
                        }
                        else
                        {
                            _jumpRequest = false;
                        }
                    }
                    else if (_canJump)
                    {
                        if (_moveInputVector.sqrMagnitude > 0f)
                        {
                            Vector3 addedVelocity = deltaTime * UnderwaterAccelerationSpeed * _moveInputVector;

                            Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterRight);

                            // Set max speed
                            float maxSpeed = MaxUnderwaterSpeed;
                            if (_isSprinting) maxSpeed = MaxUnderwaterSprintSpeed;

                            if (currentVelocityOnInputsPlane.magnitude < maxSpeed)
                            {
                                // clamp addedVel to make total vel not exceed max vel on inputs plane
                                Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, maxSpeed);
                                addedVelocity = newTotal - currentVelocityOnInputsPlane;
                            }
                            else
                            {
                                // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                                if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                                {
                                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                                }
                            }

                            // Apply added velocity
                            currentVelocity += addedVelocity;

                            // TODO MARC: Cuando haya animaciones y modelo definitivos arreglar esto
                            // Set capsule direction
                            //Motor.Capsule.direction = 2; // Z-Direction

                            // Animate
                            Animator.SetBool("isMoving", true);
                        }
                        else
                        {
                            // Set capsule direction
                            //Motor.Capsule.direction = 1; // Y-Direction

                            // Animate
                            Animator.SetBool("isMoving", false);
                        }
                    }
                    // Drag
                    currentVelocity *= (1f / (1f + (UnderwaterDrag * deltaTime)));

                break;
                }
        }

        // Manage movement constraints when interacting with an object
        ParentInteractionScript interaction = PlayerManager.playerInteractionScript.currentObjectInteraction;
        if (interaction != null)
        {
            if (interaction.CheckNextPlayerPositionAvailability(currentVelocity.normalized, currentVelocity.magnitude))
            {
                Debug.Log("Can move");
            }
            else
            {
                Debug.Log("Can not move");
                currentVelocity = Vector3.zero;
            }

            //currentVelocity = currentVelocity.normalized * interaction.GetMaximumSpeedToReachTheExtendedPosition(currentVelocity.normalized, currentVelocity.magnitude);
        }

        // Set velocity direction
        _movementDirection = currentVelocity.normalized;
    }
    public void AfterCharacterUpdate(float deltaTime)
    {
        Debug.DrawLine(transform.position, transform.position + _movementDirection * 3f, Color.green);

        switch (CurrentCharacterState)
        {
            case CharacterState.Default:
                {
                    break;
                }
            case CharacterState.Swimming:
                {
                    if (!_canJump && Time.time > _lastImpulseTime + UnderwaterImpulseCooldown)
                    {
                        _canJump = true;
                    }
                    break;
                }
        }

        // Handle stun
        if (_isStunned && Time.time > _lastStunTime + DefaultStunTime)
        {
            _isStunned = false;
        }
    }

    // ========================================== SET / ADD VELOCITY & ROTATION ==========================================
    public void SetRotation(Quaternion rot)
    {
        _setRotationRequest = true;
        _targetRotation = rot;
    }
    public void SetVelocity(Vector3 velocity)
    {
        _setVelocityRequest = true;
        _targetVelocity = velocity;
    }

    public void AddVelocity(Vector3 velocity)
    {
        switch (CurrentCharacterState)
        {
            case CharacterState.Default:
                {
                    _internalVelocityAdd += velocity;
                    break;
                }
        }
    }

    // ========================================== GROUNDING ==========================================
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }
    public void PostGroundingUpdate(float deltaTime)
    {
        // Handle landing and leaving ground
        if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
        {
            OnLanded();
        }
        else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
        {
            OnLeaveStableGround();
        }
    }
    void OnLanded()
    {

    }
    void OnLeaveStableGround()
    {

    }

    // ========================================== COLLISIONS ==========================================
    public bool IsColliderValidForCollisions(Collider coll)
    {
        if (IgnoredColliders.Count == 0)
        {
            return true;
        }

        if (IgnoredColliders.Contains(coll))
        {
            return false;
        }

        return true;
    }
    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    // ========================================== MOVEMENT HIT ==========================================
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        switch (CurrentCharacterState)
        {
            case CharacterState.Default:
                {
                    break;
                }
            case CharacterState.Swimming:
                {
                    // Si se choca cuando esta impulsandose stunea al player y lo hace rebotar
                    if (!_isStunned && !_canJump)
                    {
                        StunPlayer();
                        SetVelocity(-(hitPoint - transform.position).normalized * 3f);
                    }
                    break;
                }
        }
    }
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    // ========================================== OTHER STUFF ===========================================
    public void StunPlayer()
    {
        _isStunned = true;
        _lastStunTime = Time.time;

        OnPlayerStun?.Invoke();

        PlayerManager.CameraController.TriggerCameraShake(duration: 0.5f, magnitude: 0.1f, controllerVibration: false);
    }
}