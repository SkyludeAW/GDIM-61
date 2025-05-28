using UnityEngine;

public class GameStateManager : MonoBehaviour {
    public static GameStateManager Instance;

    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject inGameUI;

    [SerializeField] GameObject victoryEndScreen;
    [SerializeField] GameObject defeatEndScreen;

    [SerializeField] CountdownTimer timer;

    public bool CanPause;
    public bool IsPaused { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }

        CanPause = true;
    }

    private void Update() {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && CanPause)
            TriggerPause(!IsPaused);
    }

    public void TriggerPause(bool pause) {
        if (pause) {
            IsPaused = true;
            Time.timeScale = 0f;
            UIManager.Instance?.DeselectCard();

            pauseMenu?.SetActive(true);
            inGameUI?.SetActive(false);
            GameController.Instance.enabled = false;
        } else {
            IsPaused = false;
            Time.timeScale = 1f;

            pauseMenu?.SetActive(false);
            inGameUI?.SetActive(true);
            GameController.Instance.enabled = true;

            GameController.Instance.SwitchControlState(GameController.ControlState.InBattle);
        }
    }

    public void EndGame(bool victory = true) {
        TriggerPause(false);

        UIManager.Instance?.DeselectCard();
        inGameUI?.SetActive(false);
        GameController.Instance.enabled = false;
        CanPause = false;

        if (victory) {
            victoryEndScreen?.SetActive(true);
        } else {
            defeatEndScreen?.SetActive(true);
        }
    }
}
