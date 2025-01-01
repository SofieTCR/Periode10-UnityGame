using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThrottleSlider : MonoBehaviour
{
    private Slider slider;
    public TextMeshProUGUI percentageText;


    void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = LevelManager.PlayerState?.Throttle ?? 0f;

        slider.onValueChanged.AddListener(OnSliderValueChanged);
        UpdatePercentageText(slider.value);
    }

    void Update()
    {
        float throttle = LevelManager.PlayerState?.Throttle ?? 0f;
        slider.value = throttle;
        UpdatePercentageText(slider.value);
    }

    void OnSliderValueChanged(float value)
    {
        if (LevelManager.PlayerState != null)
            LevelManager.PlayerState.Throttle = value;
    }

    void UpdatePercentageText(float value)
    {
        float percentage = value * 100f;
        percentageText.text = $"{percentage:F0}%";
    }
}
