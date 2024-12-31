using UnityEngine;

public class LerpedRotation
{
    public bool isRotating;
    public float currentSpeed;
    public float acceleration;
    public float maxRotationSpeed;

    private float rotationProgress;
    private Transform rotationTransform;

    public LerpedRotation(Transform pRotationTransform, float pAcceleration, float pMaxRotationSpeed)
    {
        rotationTransform = pRotationTransform;
        acceleration = pAcceleration;
        maxRotationSpeed = pMaxRotationSpeed;
    }
    public bool RotateWithAcceleration(Quaternion startRotation, Quaternion targetRotation)
    {
        if (rotationProgress >= 1f)
        {
            isRotating = false;
            currentSpeed = 0f;
            rotationTransform.rotation = targetRotation;
            return false;
        }

        float totalRotationAngle = Quaternion.Angle(startRotation, targetRotation);
        float distanceCovered = totalRotationAngle * rotationProgress;
        float distanceLeft = totalRotationAngle - distanceCovered;
        float stopDist = Mathf.Pow(currentSpeed, 2) / (2 * acceleration);
        if (distanceLeft > stopDist)
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
        rotationProgress += deltaRotation / totalRotationAngle;

        rotationTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationProgress);

        return true;
    }
}
