using TMPro;
using UnityEngine;

public class Attribute : MonoBehaviour {
    public string Name;
    public TMP_Text Value;

    private void Awake() {
        Value ??= GetComponentInChildren<TMP_Text>();
    }
}
