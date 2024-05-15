using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Different states the game can be in
public enum GameState {
    WaitingToStart,
    Playing,
    GameOver
}

// TODO:
// - Create an abstract parent class & move non abstract behavior to child classes (pausing game with timescale etc.)

/// <summary>
/// Class which tracks and updates the games general state.
/// It also reacts to and orchestrates other game systems by notifying them with events
/// </summary>
public class GameManager : MonoBehaviour {

    #region Properties

    // Tracks the current state of the game
    public static GameState CurrentState { get; private set; }

    // Specific events could be used for more clarity between different systems 
    // public UnityEvent OnGameWaitingToStart;
    // public UnityEvent OnGameStarted;
    // public UnityEvent OnGameOver;


    // Which state the game starts in
    [SerializeField, Header("General settings")]
    private GameState StartState = GameState.WaitingToStart;
    [Space(10)]
    [Header("Event listeners")]
    [Space(3)]
    // Invoked when game state changes
    public UnityEvent<GameState> OnGameStateChanged;

    #endregion

    // Called at start - init is done here
    private void Start() {
        Init();
    }

    // Track if any key was pressed
    private void Update() {
        if (Keyboard.current.anyKey.wasPressedThisFrame) {
            // Start game if any key was pressed and if waiting to start
            if (CurrentState == GameState.WaitingToStart) {
                SetGameState(GameState.Playing);
            }
            // Reset game at game over state if any key is pressed
            if (CurrentState == GameState.GameOver) {
                // Reset game by reloading the scene
                SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            }
        }
    }

    // Init GameManager
    private void Init() {
        // Set initial game state
        SetGameState(StartState);
    }

    // Updates current state & invokes UnityEvents
    private void SetGameState(GameState newState) {
        // Do general state-related behavior
        switch (newState) {
            case GameState.WaitingToStart:
                // Pause game at start
                Time.timeScale = 0f;
                break;

            case GameState.Playing:
                // Exit pause
                Time.timeScale = 1f;
                break;

            case GameState.GameOver:
                // Pause game when game ends
                Time.timeScale = 0f;
                break;
        }
        // Update state
        CurrentState = newState;

        // Notify listeners
        OnGameStateChanged?.Invoke(CurrentState);
    }

    // Call this when player dies
    public void OnPlayerDeath() {
        SetGameState(GameState.GameOver);
    }


}
