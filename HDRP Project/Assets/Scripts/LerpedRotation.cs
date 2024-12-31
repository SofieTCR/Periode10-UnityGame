using UnityEngine;

public class LerpedRotation
{
    public bool isRotating;
    public float currentSpeed;
    public float acceleration;
    public float maxRotationSpeed;

    private Quaternion currentRotation;
    private Transform rotationTransform;

    public LerpedRotation(Transform pRotationTransform, float pAcceleration, float pMaxRotationSpeed)
    {
        rotationTransform = pRotationTransform;
        acceleration = pAcceleration;
        maxRotationSpeed = pMaxRotationSpeed;
        currentRotation = pRotationTransform.localRotation;
    }

    public bool RotateWithAcceleration(Quaternion targetRotation)
    {
        float totalRotationAngle = Quaternion.Angle(currentRotation, targetRotation);

        if (totalRotationAngle < 0.01f)
        {
            isRotating = false;
            currentSpeed = 0f;
            rotationTransform.localRotation = targetRotation;
            currentRotation = targetRotation;
            return false;
        }

        float stopDist = Mathf.Pow(currentSpeed, 2) / (2 * acceleration);
        if (totalRotationAngle > stopDist)
        {
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxRotationSpeed);
        }
        else
        {
            currentSpeed -= acceleration * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0.1f);
        }

        float deltaRotation = currentSpeed * Time.deltaTime;
        float rotationFraction = deltaRotation / totalRotationAngle;

        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFraction);
        rotationTransform.localRotation = currentRotation;

        return true;
    }
}
