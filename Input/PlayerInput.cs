using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : CharacterInput
{
    // Reads player input and provides it to other scripts
    // Only one should ever exist.
    // Should be created in the first scene and persist through all scenes

    public static PlayerInput Instance { get; private set; }

    /// <summary>
    /// Wrapper class for input actions to convert input data into a more readable format
    /// </summary>
    [Serializable]
    public class ButtonInfo
    {
        public bool PressedThisFrame { get; private set; }
        public bool ReleasedThisFrame { get; private set; }
        public bool HeldDown { get; private set; }

        InputAction _action;

        public ButtonInfo(InputAction action)
        {
            _action = action;
        }

        public void Update()
        {
            PressedThisFrame = _action.WasPressedThisFrame();
            ReleasedThisFrame = _action.WasReleasedThisFrame();
            if (PressedThisFrame && !HeldDown)
            {
                HeldDown = true;
            }
            if (ReleasedThisFrame && HeldDown)
            {
                HeldDown = false;
            }
        }
    }

    public InputActionAsset inputActionAsset;

    [Range(0f, 10f)]
    public float LookSensitivityMultiplier = 1f;

    public float ZoomSensitivityMultiplier = 1f;

    // Properties to get input values, read these from objects which require input

    public List<ButtonInfo> buttons = new List<ButtonInfo>();

    // local references to input actions
    InputAction moveAction;

    InputAction cameraZoomAction;

    InputAction switchWeaponSlotAction;

    public static Action<int> OnWeaponSlotSwitched;

    public static Action<float> OnCameraZoom;

    private void Awake()
    {
        // Check if instance already exists
        if (Instance == null)
        {
            // If no instance exists, this becomes the instance
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optionally: make this object persist across scenes
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Duplicate PlayerInput detected, destroying this instance and object");
            // If instance already exists and it's not this, destroy this to enforce the singleton pattern
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Init input action asset and actions
        inputActionAsset.Enable();
        // Debug.Log("inputactionasset found: " + inputActionAsset != null);

        // Get input actions from input action asset
        moveAction = inputActionAsset.FindAction("Movement");
        switchWeaponSlotAction = inputActionAsset.FindAction("SwitchActiveWeaponSlot");
        cameraZoomAction = inputActionAsset.FindAction("CameraZoom");
        // Debug.Log("moveAction found: " + moveAction != null);

        // Invoke event when weapon slot is switched    
        switchWeaponSlotAction.performed += WeaponSlotSwitched;
        cameraZoomAction.performed += CameraZoomDetected;

        // Update input for the first frame
        UpdateInput();
    }

    private void WeaponSlotSwitched(InputAction.CallbackContext ctx)
    {
        //Debug.Log("Switched weapon slot to: " + (int)ctx.ReadValue<float>());
        OnWeaponSlotSwitched?.Invoke((int)ctx.ReadValue<float>());
    }
    private void CameraZoomDetected(InputAction.CallbackContext ctx)
    {
        var zoomInput = ctx.ReadValue<float>();
        // Debug.Log("Camera zoom detected. value: " + ctx.ReadValue<float>());
        // Make zoom input 1 or -1 (original zoominput might be 120 or -120)
        var zoomAmount = zoomInput / Math.Abs(zoomInput) * ZoomSensitivityMultiplier;
        OnCameraZoom?.Invoke(zoomAmount);
    }

    private void Update()
    {
        FetchButtonData();
        UpdateInput();
    }

    /// <summary>
    /// Update publicly available input data according to fetched data
    /// </summary>
    private void UpdateInput()
    {
        // Read input from input actions
        MoveInput = moveAction.ReadValue<Vector2>().normalized;
        // LookInput = lookAction.ReadValue<Vector2>() * LookSensitivityMultiplier;
        // int switchWeaponSlot = (int)switchWeaponSlotAction.ReadValue<float>();
        // Debug.Log("Switch weapon slot input value: " + switchWeaponSlot);

        // // Handle sprint input
        // if (ToggleSprint)
        // {
        //     if (sprintButton.PressedThisFrame)
        //     {
        //         TryingToSprint = !TryingToSprint;
        //     }
        // }
        // else
        // {
        //     TryingToSprint = sprintButton.HeldDown;
        // }
        // // Handle sneak input
        // if (ToggleSneak)
        // {
        //     if (sneakButton.PressedThisFrame)
        //     {
        //         TryingToSneak = !TryingToSneak;
        //     }
        // }
        // else
        // {
        //     TryingToSneak = sneakButton.HeldDown;
        // }
        // // Handle crouch input
        // if (ToggleCrouch)
        // {
        //     if (crouchButton.PressedThisFrame)
        //     {
        //         TryingToCrouch = !TryingToCrouch;
        //     }
        // }
        // else
        // {
        //     TryingToCrouch = crouchButton.HeldDown;
        // }

        // // Handle jump input
        // TryingToJump = jumpButton.PressedThisFrame;

        // // Handle lean input
        // TryingToLeanLeft = leanLeftButton.HeldDown;
        // TryingToLeanRight = leanRightButton.HeldDown;
        // TryingToLean = TryingToLeanLeft || TryingToLeanRight;

    }

    private void FetchButtonData()
    {
        // Check if each button is held down, pressed, etc.
        foreach (var button in buttons)
        {
            button.Update();
        }
    }
}

