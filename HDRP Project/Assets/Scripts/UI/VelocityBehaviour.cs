using TMPro;
using UnityEngine;

public class VelocityBehaviour : MonoBehaviour
{
    public TextMeshProUGUI fieldText;

    void Update()
    {
        float velocity = LevelManager.PlayerState?.Velocity.magnitude ?? 0f;
        UpdateFieldText(velocity);
    }

    void UpdateFieldText(float value)
    {
        fieldText.text = $"{value:F1}m/s";
    }
}
