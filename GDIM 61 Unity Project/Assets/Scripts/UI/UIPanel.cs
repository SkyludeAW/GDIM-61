using UnityEngine;

public class UIPanel : MonoBehaviour {
    static GameObject activePanel;

    public void ToggleActive(bool active) {
        if (active) {
            activePanel?.SetActive(false);
            activePanel = this.gameObject;
            gameObject.SetActive(true);
        } else {
            activePanel = null;
            gameObject.SetActive(false);
        }
    }

    public void Update() {
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Mouse0) && gameObject.activeSelf) {
            ToggleActive(false);
        }
    }
}
