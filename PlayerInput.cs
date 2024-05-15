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

    // Properties to get input values, read these from objects which require input
    public static Vector2 MoveInput { get; private set; }

    public List<ButtonInfo> buttons = new List<ButtonInfo>();

    // local references to input actions
    InputAction moveAction;

    private void OnEnable()
    {
        // Init input action asset and actions
        inputActionAsset.Enable();
        inputActionAsset.FindActionMap("Player").Enable();

        // Get input actions from input action asset
        moveAction = inputActionAsset.FindAction("Movement");

        // sprintAction = inputActionAsset.FindAction("Sprint");
        // jumpAction = inputActionAsset.FindAction("Jump");
        // sneakAction = inputActionAsset.FindAction("Sneak");
        // crouchAction = inputActionAsset.FindAction("Crouch");
        // leanLeftAction = inputActionAsset.FindAction("LeanLeft");
        // leanRightAction = inputActionAsset.FindAction("LeanRight");
        // flashLightAction = inputActionAsset.FindAction("ToggleFlashlight");
        // lookAction = inputActionAsset.FindAction("Look");

        // // Init button data 
        // sprintButton = new ButtonInfo(sprintAction);
        // buttons.Add(sprintButton);
        // jumpButton = new ButtonInfo(jumpAction);
        // buttons.Add(jumpButton);
        // sneakButton = new ButtonInfo(sneakAction);
        // buttons.Add(sneakButton);
        // crouchButton = new ButtonInfo(crouchAction);
        // buttons.Add(crouchButton);
        // leanLeftButton = new ButtonInfo(leanLeftAction);
        // buttons.Add(leanLeftButton);
        // leanRightButton = new ButtonInfo(leanRightAction);
        // buttons.Add(leanRightButton);

        // Test input action events
        // sprintAction.started += (ctx) => Debug.Log("Sprint started");
        // sprintAction.performed += (ctx) => Debug.Log("Sprint performed");
        // sprintAction.canceled += (ctx) => Debug.Log("Sprint canceled");
    }

    private void Start()
    {
        // Init input action asset and actions
        inputActionAsset.Enable();
        inputActionAsset.FindActionMap("Player").Enable();
        Debug.Log("inputactionasset found: " + inputActionAsset != null);

        // Get input actions from input action asset
        moveAction = inputActionAsset.FindAction("Movement");
        Debug.Log("moveAction found: " + moveAction != null);

        // Update input for the first frame
        UpdateInput();
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

