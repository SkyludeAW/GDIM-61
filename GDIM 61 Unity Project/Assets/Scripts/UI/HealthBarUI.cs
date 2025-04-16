using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour {
    [SerializeField] private Slider _slider;
    [SerializeField] private Gradient _gradient;
    [SerializeField] private Image _fill;

    public void SetHealth(float health) {
        _slider.value = health;
        _fill.color = _gradient.Evaluate(health);
    }
}
