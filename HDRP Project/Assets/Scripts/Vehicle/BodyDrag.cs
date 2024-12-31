using UnityEngine;

public class BodyDrag : MonoBehaviour
{
    public float height = 50f;
    public float diameter = 3.66f;
    public float dragCoefficient = 0.75f;
    public float liftCoefficient = 0.35f;
    public float airDensity = 1.225f;    // Air density at sea level (kg/m�)

    private float frontalArea;
    private float sideArea;
    private Rigidbody RB;

    private void Start()
    {
        RB = gameObject.GetComponent<Rigidbody>();
        frontalArea = Mathf.PI * Mathf.Pow(diameter / 2, 2);
        sideArea = height * diameter;
    }

    private void FixedUpdate()
    {
        Vector3 velocity = RB.linearVelocity;
        float speed = velocity.magnitude;

        if (speed > 0.01f)
        {
            Vector3 velocityDirection = velocity.normalized;
            float angleOfAttack = Mathf.Asin(Vector3.Dot(transform.forward, velocityDirection)) * Mathf.Rad2Deg;

            Vector3 dragForce = -0.5f * airDensity * speed * speed * dragCoefficient * frontalArea * velocityDirection;
            Vector3 liftDirection = Vector3.Cross(velocityDirection, Vector3.Cross(transform.forward, velocityDirection)).normalized;
            float liftForceMagnitude = -0.5f * airDensity * speed * speed * liftCoefficient * sideArea * Mathf.Sin(angleOfAttack * Mathf.Deg2Rad);

            RB.AddForce(dragForce);
            RB.AddForce(liftDirection * liftForceMagnitude);
            var drawPoint = transform.TransformPoint(new Vector3(0, 0, 10));
            Debug.Log($"Speed: {Mathf.Round(speed * 100) / 100} m/s, AOA: {Mathf.Round(angleOfAttack * 100) / 100} degrees");
            Debug.DrawLine(drawPoint, drawPoint + dragForce.normalized, Color.red, Time.fixedDeltaTime);
            Debug.DrawLine(drawPoint, drawPoint + (liftDirection * liftForceMagnitude).normalized, Color.blue, Time.fixedDeltaTime);
        }
    }
}
