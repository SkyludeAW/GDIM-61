using System;
using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour {
    [SerializeField] private TMP_Text _timerText;

    public float TimerLength = 300f;
    public float TimeLeft;
    private bool _active;

    private void Awake() {
        TimeLeft = TimerLength;
        TriggerTimer();
    }

    private void Update() {
        _timerText.text = TimeSpan.FromSeconds(TimeLeft).ToString(@"mm\:ss");
        TimeLeft -= Time.deltaTime;
    }

    public void TriggerTimer(bool start = true) {
        _active = start;
    }
}
