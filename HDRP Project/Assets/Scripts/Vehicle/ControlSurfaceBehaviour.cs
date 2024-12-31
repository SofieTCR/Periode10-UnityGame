using UnityEngine;

public class ControlSurfaceBehaviour : MonoBehaviour
{
    public GameObject Stage;
    public Vector3 RotationAxis = new Vector3(0, 1, 0);
    public float SurfaceArea = 1.5f;
    public float ActuationRange = 35f;
    public float ActuationSpeed = 160f;
    public float ActuationAcceleration = 90f;
    public float ControlAuthority = 2f;
    public float AirDensity = 1.225f;    // Air density at sea level in kg/m³

    private VehicleState state;
    private Rigidbody RB;
    private DeployBehaviour deploy;
    private Quaternion defaultRotation;
    private LerpedRotation rotationController;
    private Transform childTransform;
    private MeshFilter childMesh;

    void Start()
    {
        rotationController = new LerpedRotation(transform, ActuationAcceleration, ActuationSpeed);
        state = Stage.GetComponent<VehicleState>();
        RB = Stage.GetComponent<Rigidbody>();
        deploy = gameObject.GetComponentInChildren<DeployBehaviour>();
        defaultRotation = transform.localRotation;
        childTransform = transform.GetChild(0).GetChild(2);
        childMesh = childTransform.GetComponent<MeshFilter>();
    }

    void Update()
    {
        if (!Stage.gameObject.activeSelf) return;
        if (rotationController.maxRotationSpeed != ActuationSpeed) rotationController.maxRotationSpeed = ActuationSpeed;
        if (rotationController.acceleration != ActuationAcceleration) rotationController.acceleration = ActuationAcceleration;

        Quaternion targetRotation = defaultRotation;
        if (deploy.isDeployed)
            targetRotation *= Quaternion.Euler(RotationAxis * ActuationRange * state.Steer);
        rotationController.RotateWithAcceleration(targetRotation);
    }

    private void FixedUpdate()
    {
        if (!deploy.isDeployed || !Stage.gameObject.activeSelf)
            return;

        Vector3 localCenter = childMesh.mesh.bounds.center;
        Vector3 worldCenter = childTransform.TransformPoint(localCenter);
        Vector3 relativeVelocity = RB.GetPointVelocity(worldCenter);
        Vector3 normal = transform.TransformDirection(Vector3.forward);

        if (relativeVelocity.magnitude < 0.01f) return;

        float normalVelocity = Vector3.Dot(relativeVelocity, normal);
        Vector3 velocityInPlane = Vector3.ProjectOnPlane(relativeVelocity, normal);
        float dragCoefficient = 1.0f;

        // Normal drag force
        float normalDragForceMagnitude = 0.5f * AirDensity * normalVelocity * normalVelocity * SurfaceArea * dragCoefficient;
        Vector3 normalDragForce = -Mathf.Sign(normalVelocity) * normalDragForceMagnitude * normal;

        // Planar drag force
        float planarVelocityMagnitude = velocityInPlane.magnitude;
        float planarDragForceMagnitude = ControlAuthority * AirDensity * planarVelocityMagnitude * planarVelocityMagnitude * SurfaceArea * dragCoefficient;
        Vector3 planarDragForce = -velocityInPlane.normalized * planarDragForceMagnitude;

        Vector3 totalDragForce = normalDragForce + planarDragForce;

        RB.AddForceAtPosition(totalDragForce, worldCenter);
        Debug.DrawLine(worldCenter, worldCenter + totalDragForce.normalized, Color.red, Time.fixedDeltaTime);
    }

}
