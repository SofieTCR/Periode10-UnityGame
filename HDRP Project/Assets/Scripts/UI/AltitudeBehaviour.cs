using TMPro;
using UnityEngine;

public class AltitudeBehaviour : MonoBehaviour
{
    public TextMeshProUGUI fieldText;

    void Update()
    {
        float altitude = LevelManager.PlayerObjectActive ? LevelManager.PlayerState.Altitude : 0f;
        UpdateFieldText(altitude);
    }

    void UpdateFieldText(float value)
    {
        fieldText.text = $"{value:F0}m";
    }
}
