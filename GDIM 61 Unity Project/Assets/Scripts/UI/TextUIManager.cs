using TMPro;
using UnityEngine;

public class TextUIManager : MonoBehaviour {
    public static TextUIManager Instance { get; private set; }

    [SerializeField] GameObject hoverMessage;
    TMP_Text hoverMessageText;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
        }
    }
}
