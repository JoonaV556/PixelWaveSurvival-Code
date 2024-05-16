using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Initializes static systems such as player stats etc.
    // Only one should ever exist.
    // Should be created in the first scene and persist through all scenes

    public static GameManager instance;

    private void Awake()
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
}
