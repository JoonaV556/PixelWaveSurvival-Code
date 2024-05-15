using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SimpleRuntimeUI : MonoBehaviour {
    private Button _button;
    private Toggle _toggle;
    private ProgressBar _progressBar;

    private int _clickCount;

    //Add logic that interacts with the UI controls in the `OnEnable` methods
    private void OnEnable() {
        // Get UIDocument reference
        UIDocument uiDocument = GetComponent<UIDocument>();

        // Get references to button and toggle
        _button = uiDocument.rootVisualElement.Q("button") as Button;
        _toggle = uiDocument.rootVisualElement.Q("toggle") as Toggle;
        _progressBar = uiDocument.rootVisualElement.Q("ProgressBar") as ProgressBar;
        
        // Init progress bar progress
        _progressBar.value = 100f;

        // When button is clicked, trigger PrintClickMessage()
        _button.RegisterCallback<ClickEvent>(PrintClickMessage);

        var _inputFields = uiDocument.rootVisualElement.Q("input-message");
        _inputFields.RegisterCallback<ChangeEvent<string>>(InputMessage);

        // Hide UI at start
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    private void OnDisable() {
        // Unregister callbacks
        _button.UnregisterCallback<ClickEvent>(PrintClickMessage);
    }

    private void PrintClickMessage(ClickEvent evt) {
        // Reduce progress bar progress
        _progressBar.value -= 10f;

        ++_clickCount;

        Debug.Log($"{"button"} was clicked!" +
                (_toggle.value ? " Count: " + _clickCount : ""));
    }

    public static void InputMessage(ChangeEvent<string> evt) {
        Debug.Log(evt.newValue);
    }
}
