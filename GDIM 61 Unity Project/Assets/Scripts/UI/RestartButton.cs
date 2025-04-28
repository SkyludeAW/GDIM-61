using UnityEngine;

public class RestartButton : MonoBehaviour {
    public void Restart() {
        SceneController.Instance.GoToLevel(-1);
    }
}
