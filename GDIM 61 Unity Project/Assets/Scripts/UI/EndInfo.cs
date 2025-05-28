using System;
using TMPro;
using UnityEngine;

public class EndInfo : MonoBehaviour {
    [SerializeField] TMP_Text infoText;
    [SerializeField] CountdownTimer timer;

    private void Awake() {
        infoText ??= gameObject.GetComponent<TMP_Text>();
    }

    private void OnEnable() {
        infoText.text = TimeSpan.FromSeconds(timer.TimeLeft).ToString(@"mm\:ss");
    }
}
