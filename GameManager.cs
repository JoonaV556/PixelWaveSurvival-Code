using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Initializes static systems such as player stats etc.
    // Only one should ever exist.
    // Should be created in the first scene and persist through all scenes

    public static GameManager instance;
    public static PlayerData PlayerData;

    private void Awake()
    {
        InitSingleton();
        InitSystems();
    }

    public void InitSingleton()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Duplicate GameManager detected, destroying this instance and object");
            Destroy(gameObject);
        }
    }

    private void InitSystems()
    {
        // Initialize player data
        PlayerData = new PlayerData();
    }
}
