using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExplosionBahaviour : MonoBehaviour
{
    public Dictionary<float, float> soundIntensity = new Dictionary<float, float>
    {
        { 0, .5f },
        { 13, .5f },
        { 15, 0 },
    };

    public Dictionary<float, float> fireScale = new Dictionary<float, float>
    {
        { 0, .0f },
        { 2, 30f },
        { 8, 12.5f },
        { 12, 7f },
        { 15, 0 },
    };

    private Transform fireVFX;
    private AudioSource fireSFX;
    private float StartTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fireSFX = transform.GetChild(0).GetComponent<AudioSource>();
        fireVFX = transform.GetChild(1);
        Destroy(gameObject, Mathf.Max(fireScale.Keys.Last(), soundIntensity.Keys.Last()));
        StartTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        fireSFX.volume = EvalutateCurve(soundIntensity, Time.time - StartTime);
        var scale = EvalutateCurve(fireScale, Time.time - StartTime);
        fireVFX.localScale = new Vector3(scale, scale, scale);
    }

    private float EvalutateCurve(Dictionary<float, float> floatCurve, float time)
    {
        if (time < floatCurve.Keys.First()) return 0f;
        if (time > floatCurve.Keys.Last()) return floatCurve[floatCurve.Keys.Last()];
        KeyValuePair<float, float> start = new KeyValuePair<float, float>();
        KeyValuePair<float, float> end = new KeyValuePair<float, float>();

        foreach (var kvp in floatCurve)
        {
            if (time > kvp.Key) start = kvp;
            if (time < kvp.Key)
            {
                end = kvp;
                break;
            }
        }

        var ratio = (time - start.Key) / (end.Key - start.Key);
        return start.Value + (end.Value - start.Value) * ratio;
    }
}
