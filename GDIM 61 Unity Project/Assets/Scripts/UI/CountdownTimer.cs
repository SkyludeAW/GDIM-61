using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CountdownTimer : MonoBehaviour {
    public UnityEvent OnFinish;

    [SerializeField] private TMP_Text _timerText;

    public float TimerLength = 300f;
    public float TimeLeft;
    private bool _active;

    private void Awake() {
        TimeLeft = TimerLength;
        TriggerTimer();
    }

    private void Update() {
        if (!_active)
            return;

        _timerText.text = TimeSpan.FromSeconds(TimeLeft).ToString(@"mm\:ss");
        TimeLeft -= Time.deltaTime;
        if (TimeLeft < 0)
            OnFinish?.Invoke();
    }

    public void TriggerTimer(bool start = true) {
        _active = start;
    }

}
