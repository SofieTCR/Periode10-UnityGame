using System.Collections.Generic;
using UnityEngine;

public class EngineBehaviour : MonoBehaviour
{
    public GameObject Stage;
    public GameObject FireVFX;
    public AudioSource FireSFX;
    public Vector3 GimbalAxis = new Vector3(0, 0, -1);
    public float GimbalRange = 12f;
    public float GimbalSpeed = 120f;
    public float GimbalAcceleration = 180f;
    public float Thrust = 450e3f;
    public Dictionary<float, float> FirePosition = new Dictionary<float, float>
    {
        { 0, -.01f },
        { 1, -.04f },
    };

    public Dictionary<float, float> FireScale = new Dictionary<float, float>
    {
        { 0, 0.01f },
        { 1, 0.12f },
    };

    private VehicleState state;
    private Rigidbody RB;
    private LerpedRotation rotationController;
    private Quaternion defaultRotation;
    private void Start()
    {
        state = Stage.GetComponent<VehicleState>();
        RB = Stage.GetComponent<Rigidbody>();
        rotationController = new LerpedRotation(transform, GimbalAcceleration, GimbalSpeed);
        defaultRotation = transform.localRotation;
    }

    private void Update()
    {
        if (rotationController.maxRotationSpeed != GimbalSpeed) rotationController.maxRotationSpeed = GimbalSpeed;
        if (rotationController.acceleration != GimbalAcceleration) rotationController.acceleration = GimbalAcceleration;

        Quaternion targetRotation = defaultRotation;
        if (state.Throttle > 0f && Stage.gameObject.activeSelf)
        {
            targetRotation *= Quaternion.Euler(GimbalAxis * GimbalRange * state.Steer);
            var firePos = FireVFX.transform.localPosition;
            firePos.y = ExplosionBahaviour.EvalutateCurve(FirePosition, state.Throttle);
            FireVFX.transform.localPosition = firePos;
            var fireScale = FireVFX.transform.localScale;
            fireScale.y = Mathf.Max(ExplosionBahaviour.EvalutateCurve(FireScale, state.Throttle), 0.01f);
            FireVFX.transform.localScale = fireScale;
            if (!FireVFX.activeSelf) FireVFX.SetActive(true);
            FireSFX.volume = Mathf.Min(state.Throttle, 1f);
        }
        else
        {
            if (FireVFX.activeSelf) FireVFX.SetActive(false);
            FireSFX.volume = 0f;
        }
        rotationController.RotateWithAcceleration(targetRotation);
    }

    private void FixedUpdate()
    {
        if (!Stage.gameObject.activeSelf || state.Throttle <= 0f) return;
        var thrustVec = transform.up.normalized * Thrust * state.Throttle;
        RB.AddForceAtPosition(thrustVec, transform.position);
        var drawPoint = transform.TransformPoint(new Vector3(0, 0f, 0.1f));
        Debug.DrawLine(drawPoint, drawPoint + thrustVec.normalized, Color.green, Time.fixedDeltaTime);
    }
}
