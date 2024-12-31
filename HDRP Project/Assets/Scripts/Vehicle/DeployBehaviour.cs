using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployBehaviour : MonoBehaviour
{
    public Vector3 relativeRotation = new Vector3(0, 0, -115);
    public float maxRotationSpeed = 45f;
    public float acceleration = 15f;
    public bool isDeployed = false;
    public float randomSpeedModifier = 0f;

    private Quaternion defaultRotation;
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private bool isRotating = false;
    private bool? _isDeployed = null;
    private LerpedRotation rotationController;

    public void Start()
    {
        defaultRotation = transform.localRotation;
        rotationController = new LerpedRotation(transform, acceleration, maxRotationSpeed);
        StartCoroutine(AfterStart());
    }

    private IEnumerator AfterStart()
    {
        yield return null;

        _isDeployed = isDeployed;
        if (isDeployed)
        {
            transform.localRotation = defaultRotation * Quaternion.Euler(relativeRotation);
            rotationController = new LerpedRotation(transform, acceleration, maxRotationSpeed);
        }
    }

    public void Deploy()
    {
        if (!isRotating)
        {
            isRotating = true;
            rotationController.currentSpeed = maxRotationSpeed * randomSpeedModifier * Random.value;
            startRotation = transform.localRotation;
            targetRotation = defaultRotation * Quaternion.Euler(relativeRotation);
            _isDeployed = true;
        }
    }

    public void Retract()
    {
        if (!isRotating)
        {
            isRotating = true;
            rotationController.currentSpeed = maxRotationSpeed * randomSpeedModifier * Random.value;
            startRotation = transform.localRotation;
            targetRotation = defaultRotation;
            _isDeployed = false;
        }
    }

    void Update()
    {
        if (_isDeployed == null) return;
        if (rotationController.maxRotationSpeed != maxRotationSpeed) rotationController.maxRotationSpeed = maxRotationSpeed;
        if (rotationController.acceleration != acceleration) rotationController.acceleration = acceleration;

        if (isRotating)
        {
            isRotating = rotationController.RotateWithAcceleration(targetRotation);
        }
        else if (isDeployed != _isDeployed)
        {
            if (isDeployed) Deploy();
            else Retract();
        }
    }
}
