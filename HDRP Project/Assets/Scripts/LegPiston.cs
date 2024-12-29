using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class LegPiston : MonoBehaviour
{
    public Transform target;
    public Transform pistonBase;
    public Transform[] pistonSegments;
    private float totalLength;
    private float[] segmentLengths;

    private void Start()
    {
        if (target == null) { DisableModule(pMessage: $"{nameof(target)} is required"); return; }
        if (pistonBase == null) { DisableModule(pMessage: $"{nameof(pistonBase)} is required"); return; }
        if (pistonBase == null) { DisableModule(pMessage : $"{nameof(pistonBase)} is required"); return; }
        if (pistonSegments == null || pistonSegments.Length == 0) { DisableModule(pMessage: $"{nameof(pistonSegments)} is required"); return; }
    }

    private void DisableModule(string pMessage = null)
    {
        if (!string.IsNullOrEmpty(pMessage))
            Debug.LogError($"[{nameof(LegPiston)}]: {pMessage}");
        this.enabled = false;
    }
    void noUpdate()
    {
        float distance = Vector3.Distance(pistonBase.position, target.position);

        if (totalLength == 0)
        {
            segmentLengths = new float[pistonSegments.Length];
            for (int i = 0; i < pistonSegments.Length; i++)
            {
                MeshRenderer meshRenderer = pistonSegments[i].GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    float segmentLength = meshRenderer.bounds.size.z;
                    segmentLengths[i] = segmentLength;
                    totalLength += segmentLength;
                }
                else
                {
                    DisableModule($"Segment {pistonSegments[i].name} does not have a MeshRenderer.");
                    return;
                }
            }
        }

        float proportion = distance / totalLength;
        for (int i = 0; i < pistonSegments.Length; i++)
        {
            float segmentExtension = segmentLengths[i] * proportion;
            pistonSegments[i].localPosition = new Vector3(0, 0, segmentExtension);
        }
    }
}
