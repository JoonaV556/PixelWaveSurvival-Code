using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugFeatures : MonoBehaviour
{
    void Update()
    {
        // Restart game by pressing F1
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var loadedSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            UnityEngine.SceneManagement.SceneManager.LoadScene(loadedSceneName);
        }
    }
}
