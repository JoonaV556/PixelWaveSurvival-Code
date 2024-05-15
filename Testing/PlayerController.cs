// using System;
// using UnityEngine;

// /// <summary>
// /// Advanced rigidbody-based character controller. Reads input from PlayerInput component and moves the player character accordingly.
// /// </summary>
// /// <remarks>
// /// If you want to read input from custom input source, just replace references to PlayerInput with your custom input source.
// /// </remarks>
// public class PlayerController : MonoBehaviour
// {

//     // Features which affect movement speed - such as sprinting, sneaking, crouching - are modeled using a movement mode enum. 
//     //
//     // If character wishes to sprint, the script changes movement mode to sprint. If character wishes to sneak, it changes movement mode to sneak.
//     // Movement mode is updated in the UpdateMovementMode() function based on player input and current conditions.
//     // The movement mode is used to decide which movement speed to use in the UpdateMovementSpeed function. The desired movement speed is later used in the FixedUpdate function to apply movement force to the character.
//     // Leaning/peeking behind corners is not included in the movement mode filtering, because leaning can be performed in multiple movement states and does not affect movement speed per se.
//     //
//     // Note: Movement modes such as crouching have specific conditions that must be met before player can enter crouch mode. For example, player cannot crouch if he is currently sprinting (That would be silly).
//     // These conditions are checked in the CanCrouch(), CanSprint() etc. functions.

//     #region Properties

//     public PlayerInput PlayerInput;

//     public float JumpForce = 60f;

//     public float AirSteeringForce = 20f;

//     public float InAirAdditionalGravity = 10f;

//     public float CrouchEnterSpeed = 4f;

//     public Transform CameraTransform;
//     public Transform BodyTransform;
//     public Transform CameraRootTransform;

//     public LayerMask EnvironmentLayerMask;

//     public float GroundedDrag = 13f;
//     public float DragWhileInAir = 0f;

//     public float LeanSpeed = 4f;

//     public float MaxLeaningAngle = 25f;

//     [Range(0f, 1f)]
//     public float GroundAngleCheckDistance = 0.3f;

//     [Tooltip("How steep slopes the player can walk on?")]
//     public float MaxSlopeMoveAngle = 45f;

//     [Tooltip("Adjust movement speeds for different movement modes here")]
//     public MovementSpeedSettings MovementSpeedSettings = new MovementSpeedSettings
//     {
//         WalkSpeed = 10f,
//         RunSpeed = 15f,
//         SneakSpeed = 5f,
//         CrouchSpeed = 5f
//     };

//     [Tooltip("If characters velocity exceeds this value, it is considered moving.")]
//     public float isMovingThreshold = 1f;

//     public float JumpCooldown = 0.5f;

//     /// <summary>
//     /// Maximum angle between character's look direction and move direction to allow sprinting. Use this to prevent player from sprinting backwards or sideways.
//     /// </summary>
//     public float SprintMoveMaxAngle = 30f;

//     public bool EnableDebugVisuals = false;
//     public bool EnableDebugLogs = false;

//     Rigidbody rigidbody;

//     /// <summary>
//     /// Current movement mode of the character
//     /// </summary>
//     MovementMode movementMode = MovementMode.Walk;
//     bool grounded, standingOnSlope;
//     bool groundedLastFrame = true;
//     // bool applyingAirSteering = false;
//     bool isMovingOnGround = false;
//     public Action OnStartedMovingOnGround;
//     public Action OnStoppedMovingOnGround;
//     public Action OnLandedOnGround;
//     public Action OnJumped;
//     Vector3 slopeMoveDirection, groundNormal;
//     float velocityLastFrame = 0f;
//     public float velocityThisFrame = 0f;
//     Vector2 moveInput, lookInput;
//     float slopeAngle;
//     float movementSpeedMultiplier = 100f;
//     float actualMoveSpeed;
//     float crouchAlpha = 0f;
//     CapsuleCollider characterCollider;
//     float capsuleRadius;
//     float sphereOverlapHeight;
//     float capsuleStandingHeight;
//     float capsuleCrouchingHeight;
//     float capsuleStandingCenterY;
//     float capsuleCrouchingCenterY;
//     float cameraStandingHeight;
//     float cameraCrouchingHeight;
//     float crouchAlphaBeforeObstructionCheck;
//     float cameraStartXAngle = 90f;
//     float cameraMaxXAngle = 179f;
//     float cameraMinXAngle = 1f;
//     float cameraXAngle;
//     float leanLeftAlpha = 0f;
//     float leanRightAlpha = 0f;
//     bool leaningLeft;
//     bool leaningRight;
//     float leanRootZRotation;
//     float lastLeftLeanAlpha;
//     float lastRightLeanAlpha;
//     /// <summary>
//     /// Scales movement force applied based on character rigidbody mass. If mass is 5, scale is 1. If mass is 10 scale is 2. If mass is 1, scale is 0.2.
//     /// </summary>
//     float weightToForceScale = 1f;
//     bool jumpOnCooldown = false;
//     bool recentlyLandedOnGround = false;

//     bool allowSprintingInWantedDirection = false;

//     #endregion

//     private void Awake()
//     {
//         // Lock cursor 
//         Cursor.lockState = CursorLockMode.Locked;
//         Cursor.visible = false;

//         // Get or add rigidbody component
//         if (gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
//         {
//             rigidbody = rb;
//         }
//         else
//         {
//             rigidbody = gameObject.AddComponent<Rigidbody>();
//         }
//         rigidbody.isKinematic = false;

//         // Get capsule collider data
//         characterCollider = GetComponent<CapsuleCollider>();
//         capsuleStandingHeight = characterCollider.height;
//         capsuleCrouchingHeight = capsuleStandingHeight / 2f;
//         capsuleStandingCenterY = characterCollider.center.y;
//         capsuleCrouchingCenterY = capsuleStandingCenterY / 2f;
//         cameraStandingHeight = CameraTransform.localPosition.y;
//         cameraCrouchingHeight = 0.59f;

//         capsuleRadius = characterCollider.radius;

//         // Init camera angle
//         cameraXAngle = cameraStartXAngle;
//     }

//     private void Update()
//     {
//         // Get input from input source
//         FetchInput();

//         // Check if player is grounded + if player is standing on slope
//         GetGroundInfo();

//         // Update movement status
//         UpdateMovementStatus();

//         // Update movement mode based on input
//         UpdateMovementMode();

//         // Do crouching stuff if crouching 
//         if (movementMode == MovementMode.Crouch) HandleCrouching();

//         // Do leaning stuff
//         HandleLeaning();

//         // Update movement speed based on movement mode
//         UpdateMovementSpeed();

//         // Handle jumping
//         if (PlayerInput.TryingToJump && CanJump())
//         {
//             Jump();
//         }

//         // Unlock cursor if escape is pressed
//         HandleCursorUnlock();

//         // Disable drag if not grounded
//         if (!grounded)
//         {
//             rigidbody.drag = DragWhileInAir;
//         }
//         else
//         {
//             rigidbody.drag = GroundedDrag;
//         }

//         // Reset recently landed on ground status for next frame 
//         if (recentlyLandedOnGround)
//         {
//             recentlyLandedOnGround = false;
//         }
//     }

//     private void FixedUpdate()
//     {
//         // Move if detected any movement input
//         bool receivedMoveInput = moveInput != Vector2.zero;
//         if (receivedMoveInput)
//         {
//             ApplyMovement();
//         }
//     }

//     private void LateUpdate()
//     {
//         // Handle player rotation based on look input

//         // Rotate whole player object around y-axis
//         BodyTransform.Rotate(new Vector3(0, lookInput.x, 0), Space.Self);

//         // Limit camera rotation around x-axis
//         bool overMaxXAngle = (cameraXAngle - lookInput.y) > cameraMaxXAngle;
//         bool underMinXAngle = (cameraXAngle - lookInput.y) < cameraMinXAngle;

//         if (!overMaxXAngle && !underMinXAngle)
//         {
//             // Rotate camera around x-axis
//             CameraTransform.Rotate(new Vector3(-lookInput.y, 0, 0), Space.Self);
//             cameraXAngle -= lookInput.y;
//         }
//     }

//     private void OnDrawGizmos()
//     {
//         if (EnableDebugVisuals)
//         {
//             Gizmos.color = Color.green;
//             Gizmos.DrawWireCube(
//                 transform.position,
//                 new Vector3(0.6f, 0.1f, 0.6f)
//             );
//         }
//     }

//     #region Functions

//     /// <summary>
//     /// This should be removed. This is just for testing purposes.
//     /// </summary>
//     private static void HandleCursorUnlock()
//     {
//         if (Input.GetKeyDown(KeyCode.Escape))
//         {
//             Cursor.lockState = CursorLockMode.None;
//             Cursor.visible = true;
//         }
//     }

//     /// <summary>
//     /// Gathers and sends info about player movement events (walking started, walking stopped etc.)
//     /// </summary>
//     private void UpdateMovementStatus()
//     {
//         // Get info about player movement
//         velocityThisFrame = rigidbody.velocity.magnitude;
//         //if (EnableDebugLogs) Debug.Log("Velocity this frame: " + velocityThisFrame);
//         // This is nightmarish
//         bool startedGroundMovement = velocityThisFrame > isMovingThreshold && (velocityLastFrame < isMovingThreshold || recentlyLandedOnGround) && !isMovingOnGround && grounded;
//         bool stoppedGroundMovement = velocityThisFrame < isMovingThreshold && velocityLastFrame > isMovingThreshold && isMovingOnGround;

//         if (startedGroundMovement)
//         {
//             isMovingOnGround = true;
//             OnStartedMovingOnGround?.Invoke();
//             // if (EnableDebugLogs) Debug.Log("Character started moving on ground. Cause: velocity increased.");
//         }
//         if (stoppedGroundMovement)
//         {
//             isMovingOnGround = false;
//             OnStoppedMovingOnGround?.Invoke();
//             // if (EnableDebugLogs) Debug.Log("Character stopped moving on ground. Cause: velocity below threshold.");
//         }

//         velocityLastFrame = velocityThisFrame;
//     }

//     private void Jump()
//     {
//         rigidbody.AddForce(Vector3.up * (JumpForce * weightToForceScale), ForceMode.Impulse);
//         isMovingOnGround = false;
//         OnStoppedMovingOnGround?.Invoke();
//         if (EnableDebugLogs) Debug.Log("Stopped moving on ground. Cause: Jumped.");
//         OnJumped?.Invoke();
//         jumpOnCooldown = true;
//         Invoke(nameof(ResetJump), JumpCooldown);
//         if (EnableDebugLogs) Debug.Log("Character jumped");
//     }

//     private void ResetJump()
//     {
//         jumpOnCooldown = false;
//     }

//     private void UpdateMovementMode()
//     {
//         // Handle sprinting
//         if (PlayerInput.TryingToSprint && CanSprint())
//         {
//             movementMode = MovementMode.Sprint;
//         }
//         else if (movementMode == MovementMode.Sprint)
//         {
//             movementMode = MovementMode.Walk;
//         }

//         // Handle crouching
//         if (PlayerInput.TryingToCrouch && CanCrouch())
//         {
//             movementMode = MovementMode.Crouch;
//         }

//         // Handle sneaking
//         if (PlayerInput.TryingToSneak && CanSneak())
//         {
//             movementMode = MovementMode.Sneak;
//         }
//         if (movementMode == MovementMode.Sneak && !PlayerInput.TryingToSneak)
//         {
//             movementMode = MovementMode.Walk;
//         }
//     }

//     private void ApplyMovement()
//     {
//         float forceToApply;
//         Vector3 moveDirection;

//         if (grounded)
//         {
//             if (standingOnSlope)
//             {
//                 // Check if slope is too steep
//                 // Prevent movement on too steep slopes
//                 if (slopeAngle > MaxSlopeMoveAngle) return;
//                 moveDirection = MoveDirectionOnSlope(GroundMoveDirection());
//                 forceToApply = actualMoveSpeed;
//             }
//             else
//             {
//                 moveDirection = GroundMoveDirection();
//                 forceToApply = actualMoveSpeed;
//             }
//             // applyingAirSteering = false;
//         }
//         else
//         {
//             // In air - allow slight air steering
//             moveDirection = GroundMoveDirection();
//             forceToApply = AirSteeringForce;
//             // applyingAirSteering = true;
//         }
//         // Update force scaling based on rigidbody mass
//         weightToForceScale = rigidbody.mass / 5f;

//         // Apply movement force
//         rigidbody.AddRelativeForce(moveDirection * forceToApply * weightToForceScale, ForceMode.Force);

//         // Limit movement velocity on x and z axis
//         Vector3 velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
//         if (velocity.magnitude > actualMoveSpeed)
//         {
//             Vector3 limitedVelocity = velocity.normalized * actualMoveSpeed;
//             rigidbody.velocity = new Vector3(limitedVelocity.x, rigidbody.velocity.y, limitedVelocity.z);
//             print("limiting velocity");
//         }

//         // Apply additional gravity while in air - This is to prevent ridiculous jumps
//         if (!grounded)
//         {
//             rigidbody.AddForce(Vector3.down * (InAirAdditionalGravity * weightToForceScale), ForceMode.Force);
//         }
//     }

//     private Vector3 MoveDirectionOnSlope(Vector3 moveDirectionOnGround)
//     {
//         return Vector3.ProjectOnPlane(moveDirectionOnGround, groundNormal).normalized;
//     }

//     /// <summary>
//     /// Returns player's wanted movement direction based on input, in world-space, on this frame.
//     /// </summary>
//     /// <returns>Normalized movement direction vector in world-space</returns>
//     private Vector3 GroundMoveDirection()
//     {
//         return BodyTransform.TransformVector(new Vector3(moveInput.x, 0f, moveInput.y)).normalized;
//     }

//     /// <summary>
//     /// Checks if player is standing on ground, ground normal, and if player is standing on slope
//     /// </summary>
//     private void GetGroundInfo()
//     {
//         // Calculate ray start position and length
//         Vector3 rayStart = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
//         float rayLength = GroundAngleCheckDistance + 0.1f;

//         // Check the normal of the ground below player
//         Ray ray = new Ray(rayStart, Vector3.down);
//         bool gotGroundAngle = Physics.Raycast(
//             ray,
//             out RaycastHit hitInfo,
//             rayLength, EnvironmentLayerMask,
//             QueryTriggerInteraction.Ignore
//             );
//         if (gotGroundAngle)
//         {
//             groundNormal = hitInfo.normal;
//         }
//         else
//         {
//             groundNormal = Vector3.up;
//         }

//         // Check if player is grounded
//         Collider[] groundHits = Physics.OverlapBox(
//             transform.position,
//             new Vector3(0.3f, 0.05f, 0.3f),
//             Quaternion.identity,
//             EnvironmentLayerMask,
//             QueryTriggerInteraction.Ignore
//         );
//         grounded = groundHits.Length > 0;

//         // Check if player has left ground or landed on ground
//         bool leftGround = groundedLastFrame && !grounded;
//         if (leftGround)
//         {
//             isMovingOnGround = false;
//             OnStoppedMovingOnGround?.Invoke();
//             if (EnableDebugLogs) Debug.Log("Character left ground");
//         }
//         bool landedOnGround = !groundedLastFrame && grounded;
//         if (landedOnGround)
//         {
//             OnLandedOnGround?.Invoke();
//             recentlyLandedOnGround = true;
//             if (EnableDebugLogs) Debug.Log("Character landed on ground");
//         }

//         // Check if player is standing on slope
//         slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
//         standingOnSlope = slopeAngle > 0f;

//         groundedLastFrame = grounded;
//     }

//     /// <summary>
//     /// Updates movement speed based on movement mode
//     /// </summary>
//     private void UpdateMovementSpeed()
//     {
//         switch (movementMode)
//         {
//             case MovementMode.Walk:
//                 actualMoveSpeed = MovementSpeedSettings.WalkSpeed;
//                 break;
//             case MovementMode.Sprint:
//                 actualMoveSpeed = MovementSpeedSettings.RunSpeed;
//                 break;
//             case MovementMode.Sneak:
//                 actualMoveSpeed = MovementSpeedSettings.SneakSpeed;
//                 break;
//             case MovementMode.Crouch:
//                 actualMoveSpeed = MovementSpeedSettings.CrouchSpeed;
//                 break;
//         }
//         actualMoveSpeed *= movementSpeedMultiplier;
//     }

//     /// <summary>
//     /// Fetches input from input source
//     /// </summary>
//     private void FetchInput()
//     {
//         moveInput = PlayerInput.MoveInput;
//         lookInput = PlayerInput.LookInput;
//     }

//     private void HandleCrouching()
//     {
//         // Check whether player is trying to enter or exit crouch
//         bool enteringCrouch = movementMode == MovementMode.Crouch && PlayerInput.TryingToCrouch;
//         bool leavingCrouch = movementMode == MovementMode.Crouch && !PlayerInput.TryingToCrouch;

//         // Lerp crouch alpha value based on entering or exiting crouch 0 = standing, 1 = full crouch
//         if (enteringCrouch)
//         {
//             crouchAlpha += CrouchEnterSpeed * Time.deltaTime;
//         }
//         if (leavingCrouch)
//         {
//             // Decrease crouch alpha
//             crouchAlphaBeforeObstructionCheck = crouchAlpha;
//             crouchAlpha -= CrouchEnterSpeed * Time.deltaTime;

//             // Check if there are any obstacles above the player that would prevent standing up
//             float casuleHeightAfterApplyingAlpha = Mathf.Lerp(capsuleStandingHeight, capsuleCrouchingHeight, crouchAlpha);
//             sphereOverlapHeight = (transform.position.y + casuleHeightAfterApplyingAlpha) - capsuleRadius;
//             Collider[] overlappedColliders = Physics.OverlapSphere(
//                 new Vector3(transform.position.x, sphereOverlapHeight, transform.position.z),
//                 capsuleRadius,
//                 EnvironmentLayerMask,
//                 QueryTriggerInteraction.Ignore
//                 );

//             // If we hit obstructions above, revert crouch alpha back to previous value
//             bool hitOstructionsAbove = overlappedColliders.Length > 0;
//             if (hitOstructionsAbove)
//             {
//                 crouchAlpha = crouchAlphaBeforeObstructionCheck;
//                 // print("Hit obstruction above, reverting crouch alpha back to previous value.");
//             }
//         }
//         crouchAlpha = Mathf.Clamp(crouchAlpha, 0f, 1f);

//         // Lerp capsule collider height and offset with crouch alpha
//         characterCollider.height = Mathf.Lerp(capsuleStandingHeight, capsuleCrouchingHeight, crouchAlpha);
//         characterCollider.center = new Vector3(characterCollider.center.x, Mathf.Lerp(capsuleStandingCenterY, capsuleCrouchingCenterY, crouchAlpha), characterCollider.center.z);
//         // Lerp camera height with crouch alpha
//         CameraTransform.localPosition = new Vector3(CameraTransform.localPosition.x, Mathf.Lerp(cameraStandingHeight, cameraCrouchingHeight, crouchAlpha), CameraTransform.localPosition.z);
//         // Lerp movement speed with crouch alpha
//         actualMoveSpeed = Mathf.Lerp(MovementSpeedSettings.WalkSpeed, MovementSpeedSettings.CrouchSpeed, crouchAlpha);

//         // Change back to walk if back to full standing and not trying to crouch
//         bool shouldExitCrouch = crouchAlpha == 0f && !PlayerInput.TryingToCrouch;
//         if (shouldExitCrouch)
//         {
//             movementMode = MovementMode.Walk;
//         }
//     }

//     private void HandleLeaning()
//     {
//         HandleLeanLeft();
//         HandleLeanRight();

//         // Update leaning status
//         if (leanLeftAlpha > 0f)
//         {
//             leaningLeft = true;
//             leaningRight = false;
//         }
//         else if (leanRightAlpha > 0f)
//         {
//             leaningRight = true;
//             leaningLeft = false;
//         }
//         else
//         {
//             leaningLeft = false;
//             leaningRight = false;
//         }
//     }

//     private void HandleLeanRight()
//     {
//         // Lean right
//         bool shouldLeanRight = PlayerInput.TryingToLeanRight && leanLeftAlpha == 0f && CanLean();
//         if (shouldLeanRight)
//         {
//             lastRightLeanAlpha = leanRightAlpha;
//             leanRightAlpha += LeanSpeed * Time.deltaTime;
//             leanRightAlpha = Mathf.Clamp(leanRightAlpha, 0f, 1f);

//             // Rotate camera root prematurely with alpha to check for collisions
//             UpdateCameraLeanRotation();

//             // Do collision check
//             Collider[] overlappedColliders = Physics.OverlapSphere(
//                             new Vector3(CameraTransform.position.x, CameraTransform.position.y, CameraTransform.position.z),
//                             capsuleRadius,
//                             EnvironmentLayerMask,
//                             QueryTriggerInteraction.Ignore
//                             );

//             // Check if collisions at new head position
//             bool collisions = overlappedColliders.Length > 0;

//             if (collisions)
//             {
//                 // if collisions, revert alpha back to last value, return camera root to original rotation
//                 leanRightAlpha = lastRightLeanAlpha;
//                 // Revert camera rotation back to original
//                 UpdateCameraLeanRotation();
//             }
//         }
//         // Return from leaning right
//         if (!shouldLeanRight && leanRightAlpha > 0f)
//         {
//             leanRightAlpha -= LeanSpeed * Time.deltaTime;
//             leanRightAlpha = Mathf.Clamp(leanRightAlpha, 0f, 1f);
//             UpdateCameraLeanRotation();
//         }
//     }

//     private void HandleLeanLeft()
//     {
//         // Lean left 
//         bool shouldLeanLeft = PlayerInput.TryingToLeanLeft && leanRightAlpha == 0f && CanLean();
//         if (shouldLeanLeft)
//         {
//             lastLeftLeanAlpha = leanLeftAlpha;
//             leanLeftAlpha += LeanSpeed * Time.deltaTime;
//             leanLeftAlpha = Mathf.Clamp(leanLeftAlpha, 0f, 1f);

//             // Rotate camera root prematurely with alpha
//             UpdateCameraLeanRotation();

//             // Do collision check
//             Collider[] overlappedColliders = Physics.OverlapSphere(
//                             new Vector3(CameraTransform.position.x, CameraTransform.position.y, CameraTransform.position.z),
//                             capsuleRadius,
//                             EnvironmentLayerMask,
//                             QueryTriggerInteraction.Ignore
//                             );

//             // Check if collisions at new head position
//             bool collisions = overlappedColliders.Length > 0;

//             if (collisions)
//             {
//                 // if collisions, revert alpha back to last value, return camera root to original rotation
//                 leanLeftAlpha = lastLeftLeanAlpha;
//                 // Revert camera rotation back to original
//                 UpdateCameraLeanRotation();
//             }
//         }
//         // Return from leaning left
//         if (!shouldLeanLeft && leanLeftAlpha > 0f)
//         {
//             leanLeftAlpha -= LeanSpeed * Time.deltaTime;
//             leanLeftAlpha = Mathf.Clamp(leanLeftAlpha, 0f, 1f);
//             UpdateCameraLeanRotation();
//         }
//     }

//     private void UpdateCameraLeanRotation()
//     {
//         // Change rotation based on leaning direction
//         if (leaningLeft)
//         {
//             leanRootZRotation = Mathf.SmoothStep(0f, MaxLeaningAngle, leanLeftAlpha);
//         }
//         else
//         {
//             leanRootZRotation = -Mathf.SmoothStep(0f, MaxLeaningAngle, leanRightAlpha);
//         }
//         Vector3 cameraRootEuler = CameraRootTransform.localRotation.eulerAngles;
//         // Update camera rotation
//         CameraRootTransform.localRotation = Quaternion.Euler(cameraRootEuler.x, cameraRootEuler.y, leanRootZRotation);
//     }

//     /// <summary>
//     /// Checks if player can switch to sprinting mode based on current conditions
//     /// </summary>
//     /// <returns>True, if player is allowed to enter sprint mode</returns>
//     private bool CanSprint()
//     {
//         // Prevent player from running backwards or sideways
//         Vector3 LookDirection = Vector3.ProjectOnPlane(CameraTransform.forward.normalized, Vector3.up).normalized;
//         Vector3 MoveInputDirection = GroundMoveDirection();
//         // Check if the angle between players body facing direction and move input direction is less than max angle
//         float angle = Vector3.Angle(LookDirection, MoveInputDirection);
//         allowSprintingInWantedDirection = angle < SprintMoveMaxAngle;
//         //print("angle: " + angle);

//         return grounded && movementMode != MovementMode.Crouch && !leaningLeft && !leaningRight && allowSprintingInWantedDirection;
//     }

//     private bool CanSneak()
//     {
//         return grounded && movementMode != MovementMode.Sprint;
//     }

//     private bool CanJump()
//     {
//         return grounded && jumpOnCooldown == false;
//     }

//     private bool CanCrouch()
//     {
//         return grounded && movementMode != MovementMode.Sprint;
//     }

//     private bool CanLean()
//     {
//         return grounded && movementMode != MovementMode.Sprint;
//     }

//     #endregion

// }

// public enum MovementMode
// {
//     Walk,
//     Sprint,
//     Sneak,
//     Crouch
// }

// [Serializable]
// public struct MovementSpeedSettings
// {
//     public float WalkSpeed;
//     public float RunSpeed;
//     public float SneakSpeed;
//     public float CrouchSpeed;
// }