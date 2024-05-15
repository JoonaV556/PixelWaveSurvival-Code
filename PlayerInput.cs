using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
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

    [Tooltip("Log input vectors to console")]
    public bool LogInput = false;

    public bool ToggleSprint = false;
    public bool ToggleSneak = false;
    public bool ToggleCrouch = true;

    // Properties to get input values, read these from objects which require input
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool TryingToSprint { get; private set; }
    public bool TryingToJump { get; private set; }
    public bool TryingToSneak { get; private set; }
    public bool TryingToCrouch { get; private set; }
    public bool TryingToLeanLeft { get; private set; }
    public bool TryingToLeanRight { get; private set; }
    public bool TryingToLean { get; private set; }

    public List<ButtonInfo> buttons = new List<ButtonInfo>();

    // local references to input actions
    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;
    InputAction sneakAction;
    InputAction crouchAction;
    InputAction leanLeftAction;
    InputAction leanRightAction;
    InputAction flashLightAction;
    InputAction lookAction;

    ButtonInfo sprintButton;
    ButtonInfo jumpButton;
    ButtonInfo sneakButton;
    ButtonInfo crouchButton;
    ButtonInfo leanLeftButton;
    ButtonInfo leanRightButton;

    private void OnEnable()
    {
        // Init input action asset and actions
        inputActionAsset.Enable();
        inputActionAsset.FindActionMap("Character").Enable();

        // Get input actions from input action asset
        moveAction = inputActionAsset.FindAction("Move");
        sprintAction = inputActionAsset.FindAction("Sprint");
        jumpAction = inputActionAsset.FindAction("Jump");
        sneakAction = inputActionAsset.FindAction("Sneak");
        crouchAction = inputActionAsset.FindAction("Crouch");
        leanLeftAction = inputActionAsset.FindAction("LeanLeft");
        leanRightAction = inputActionAsset.FindAction("LeanRight");
        flashLightAction = inputActionAsset.FindAction("ToggleFlashlight");
        lookAction = inputActionAsset.FindAction("Look");

        // Init button data 
        sprintButton = new ButtonInfo(sprintAction);
        buttons.Add(sprintButton);
        jumpButton = new ButtonInfo(jumpAction);
        buttons.Add(jumpButton);
        sneakButton = new ButtonInfo(sneakAction);
        buttons.Add(sneakButton);
        crouchButton = new ButtonInfo(crouchAction);
        buttons.Add(crouchButton);
        leanLeftButton = new ButtonInfo(leanLeftAction);
        buttons.Add(leanLeftButton);
        leanRightButton = new ButtonInfo(leanRightAction);
        buttons.Add(leanRightButton);

        // Test input action events
        // sprintAction.started += (ctx) => Debug.Log("Sprint started");
        // sprintAction.performed += (ctx) => Debug.Log("Sprint performed");
        // sprintAction.canceled += (ctx) => Debug.Log("Sprint canceled");
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
        LookInput = lookAction.ReadValue<Vector2>() * LookSensitivityMultiplier;

        // Handle sprint input
        if (ToggleSprint)
        {
            if (sprintButton.PressedThisFrame)
            {
                TryingToSprint = !TryingToSprint;
            }
        }
        else
        {
            TryingToSprint = sprintButton.HeldDown;
        }
        // Handle sneak input
        if (ToggleSneak)
        {
            if (sneakButton.PressedThisFrame)
            {
                TryingToSneak = !TryingToSneak;
            }
        }
        else
        {
            TryingToSneak = sneakButton.HeldDown;
        }
        // Handle crouch input
        if (ToggleCrouch)
        {
            if (crouchButton.PressedThisFrame)
            {
                TryingToCrouch = !TryingToCrouch;
            }
        }
        else
        {
            TryingToCrouch = crouchButton.HeldDown;
        }

        // Handle jump input
        TryingToJump = jumpButton.PressedThisFrame;

        // Handle lean input
        TryingToLeanLeft = leanLeftButton.HeldDown;
        TryingToLeanRight = leanRightButton.HeldDown;
        TryingToLean = TryingToLeanLeft || TryingToLeanRight;


        // Log input if enabled
        if (LogInput)
        {
            Debug.Log("MoveInput: " + MoveInput);
            Debug.Log("LookInput: " + LookInput);
        }
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

