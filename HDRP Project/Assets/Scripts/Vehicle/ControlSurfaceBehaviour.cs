using UnityEngine;

public class ControlSurfaceBehaviour : MonoBehaviour
{
    public GameObject Stage;
    public Vector3 RotationAxis = new Vector3(0, 1, 0);
    public float SurfaceArea = 1.5f;
    public float ActuationRange = 35f;
    public float ActuationSpeed = 160f;
    public float ActuationAcceleration = 90f;

    private VehicleState state;
    private DeployBehaviour deploy;
    private Quaternion defaultRotation;
    private LerpedRotation rotationController;

    void Start()
    {
        rotationController = new LerpedRotation(transform, ActuationAcceleration, ActuationSpeed);
        state = Stage.GetComponent<VehicleState>();
        deploy = gameObject.GetComponentInChildren<DeployBehaviour>();
        defaultRotation = transform.localRotation;
    }

    void Update()
    {
        if (rotationController.maxRotationSpeed != ActuationSpeed) rotationController.maxRotationSpeed = ActuationSpeed;
        if (rotationController.acceleration != ActuationAcceleration) rotationController.acceleration = ActuationAcceleration;

        Quaternion targetRotation = defaultRotation;
        if (deploy.isDeployed)
            targetRotation *= Quaternion.Euler(RotationAxis * ActuationRange * state.Steer);
        rotationController.RotateWithAcceleration(targetRotation);
    }
}
