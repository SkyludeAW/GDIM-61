using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {
    public static SceneController Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void GoToMainMenu() {
        SceneManager.LoadSceneAsync("Main Menu");
    }

    public void Restart() {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        GameStateManager.Instance.CanPause = true;
        GameStateManager.Instance.TriggerPause(false);
    }

    public void GoToLevel(int index) {
        SceneManager.LoadSceneAsync("Level " + index);
        GameStateManager.Instance.CanPause = true;
        GameStateManager.Instance.TriggerPause(false);
    }

    public void GoToDefeatScene() {
        SceneManager.LoadSceneAsync("Defeat");
        GameStateManager.Instance.CanPause = true;
        GameStateManager.Instance.TriggerPause(false);
    }
}
