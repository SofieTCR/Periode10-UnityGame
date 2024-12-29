using System.Linq;
using UnityEngine;

public class LegPiston : MonoBehaviour
{
    public Transform target;
    public Transform[] pistonSegments;
    private float totalLength;
    public float[] segmentLengths = new float[5] { 3.3021853585f, 3.3021853585f, 3.3021853585f, 3.297066f, 2.3650634f};
    public float scale = 100;

    private void Start()
    {
        if (target == null) { DisableModule(pMessage: $"{nameof(target)} is required"); return; }
        if (pistonSegments == null || pistonSegments.Length == 0) { DisableModule(pMessage: $"{nameof(pistonSegments)} is required"); return; }
        totalLength = segmentLengths.Sum();
    }

    private void DisableModule(string pMessage = null)
    {
        if (!string.IsNullOrEmpty(pMessage))
            Debug.LogError($"[{nameof(LegPiston)}]: {pMessage}");
        this.enabled = false;
    }
    void Update()
    {
        float distance = Vector3.Distance(pistonSegments[0].position, target.position) - segmentLengths[0];

        float proportion = (distance - totalLength) / totalLength;
        for (int i = 1; i < pistonSegments.Length; i++)
        {
            float segmentExtension = segmentLengths[i-1] + segmentLengths[i] * proportion;
            pistonSegments[i].localPosition = new Vector3(0, 0, segmentExtension/scale);
            // -115 angle
        }
    }
}
