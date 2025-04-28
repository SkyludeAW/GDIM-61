using System;
using TMPro;
using UnityEngine;

public class TemporaryTimer : MonoBehaviour {
    [SerializeField] private TMP_Text _timerText;

    private void Update() {
        _timerText.text = TimeSpan.FromSeconds(Time.timeSinceLevelLoad).ToString(@"hh\:mm\:ss");
    }
}
