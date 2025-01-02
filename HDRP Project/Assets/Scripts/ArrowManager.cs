using UnityEngine;

public class ArrowManager : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Camera mainCamera;
    public float arrowHeight = 5f;

    private GameObject[] arrows;
    private Vector3[] relativePositions;
    private float pulseSpeed = 2f;
    private float pulseAmplitude = 5f;

    void Start()
    {
        relativePositions = new Vector3[]
        {
            new Vector3(10f, arrowHeight, 10f),
            new Vector3(10f, arrowHeight, -10f),
            new Vector3(-10f, arrowHeight, 10f),
            new Vector3(-10f, arrowHeight, -10f)
        };
    }

    void Update()
    {
        if (LevelManager.PlayerObjectActive && LevelManager.PlayerState.TargetLandingZone != null)
        {
            var landingZonePosition = LevelManager.PlayerState.TargetLandingZone.transform.position;

            if (arrows == null)
            {
                arrows = new GameObject[4];
                for (int i = 0; i < 4; i++)
                {
                    arrows[i] = Instantiate(arrowPrefab, landingZonePosition + relativePositions[i], Quaternion.identity);
                    arrows[i].transform.LookAt(landingZonePosition, Vector3.up);
                }
            }

            float distance = Vector3.Distance(LevelManager.PlayerState.TargetLandingZone.transform.position, mainCamera.transform.position);
            float scaleFactor = distance / 35.0f;
            var scale = Vector3.one * Mathf.Clamp(scaleFactor, 3, 50);
            float pulseOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;

            for (int i = 0; i < arrows.Length; i++)
            {
                if (!arrows[i].activeSelf) arrows[i].SetActive(true);
                var pulsingPosition = relativePositions[i] + (relativePositions[i].normalized * pulseOffset);
                arrows[i].transform.position = landingZonePosition + pulsingPosition;
                arrows[i].transform.localScale = scale;
            }
        }
        else if (arrows != null)
        {
            foreach (GameObject arrow in arrows)
            {
                if (arrow.activeSelf) arrow.SetActive(false);
            }
        }
    }
}
