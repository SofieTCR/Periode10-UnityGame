using UnityEngine;

public class ControlSurfaceBehaviour : MonoBehaviour
{
    public GameObject Stage;
    public Vector3 RotationAxis = new Vector3(0, 1, 0);
    public float SurfaceArea = 1.5f;
    public float ActuationRange = 35f;
    public float ActuationSpeed = 45f;
    public float ActuationAcceleration = 35f;

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

    // Update is called once per frame
    void Update()
    {
        if (deploy.isDeployed)
        {
            Quaternion targetRotation = defaultRotation * Quaternion.Euler(RotationAxis * ActuationRange * state.Steer);
            rotationController.RotateWithAcceleration(defaultRotation, targetRotation);
        }

    }
}
