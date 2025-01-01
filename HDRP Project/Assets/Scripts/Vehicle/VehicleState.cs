using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VehicleState : MonoBehaviour
{
    public bool isPlayer = false;
    public bool isAI = false;
    public float Throttle = 0f;
    public float Steer = 0f;
    public bool LegsDeployed;
    public bool FinsDeployed;
    public bool IsGrounded => _isGrounded;
    public float? TimeGrounded => _timeGrounded;
    public float? VelocityGrounded => _velocityGrounded;
    public float? DistanceGrounded => _distanceGrounded;
    public float? AngleGrounded => _angleGrounded;
    public float StableTimer = 2f;
    public float LinearMax = 0.5f;
    public float AngularMax = 0.01f;
    public bool IsStable
    {
        get
        {
            if (!IsGrounded) return false;
            if (linearVelocities.Average() >= LinearMax) return false;
            if (angularVelocities.Average() >= AngularMax) return false;
            return true;
        }
    }
    public float Altitude
    {
        get
        {
            float lowestPoint = float.MaxValue;

            if (colliders == null)
                colliders = GetComponentsInChildren<Collider>();

            foreach (Collider collider in colliders)
            {
                Bounds bounds = collider.bounds;
                lowestPoint = Mathf.Min(lowestPoint, bounds.min.y);
            }

            if (Physics.Raycast(new Vector3(transform.position.x, lowestPoint + 0.05f, transform.position.z), Vector3.down, out RaycastHit hit, Mathf.Infinity))
            {
                return hit.distance;
            }
            else
            {
                return Mathf.Infinity; // Impossible
            }
        }
    }
    public Vector3 Velocity => rb.linearVelocity;

    private float ThrottleResponseSpeed = 0.5f;
    private List<DeployBehaviour> LandingLegs;
    private List<DeployBehaviour> Gridfins;
    private Collider[] colliders;
    private Rigidbody rb;
    private bool _legsDeployed;
    private bool _finsDeployed;
    private DamageBahaviour db;
    private EngineBehaviour eb;
    private bool _isGrounded;
    private float _timeGrounded;
    private float? _velocityGrounded;
    private float? _distanceGrounded;
    private float? _angleGrounded;
    private Queue<float> linearVelocities = new Queue<float>();
    private Queue<float> angularVelocities = new Queue<float>();
    private bool isControllable = true;

    void Start()
    {
        var Deployables = gameObject.GetComponentsInChildren<DeployBehaviour>();
        LandingLegs = Deployables.Where(d => d.gameObject.name.Contains("leg", System.StringComparison.InvariantCultureIgnoreCase)).ToList();
        Gridfins = Deployables.Where(d => d.gameObject.name.Contains("fin", System.StringComparison.InvariantCultureIgnoreCase)).ToList();
        LandingLegs.ForEach(l => l.isDeployed = LegsDeployed); _legsDeployed = LegsDeployed;
        Gridfins.ForEach(f => f.isDeployed = FinsDeployed); _finsDeployed = FinsDeployed;
        rb = GetComponent<Rigidbody>();
        db = GetComponent<DamageBahaviour>();
        eb = GetComponentInChildren<EngineBehaviour>();
        db.OnPartBreak.AddListener(PartBreak);
    }
    void Update()
    {
        if (isPlayer && isControllable)
        {
            if (Input.GetKey(KeyCode.LeftControl)) Throttle = Mathf.Max(0f, Throttle -= ThrottleResponseSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.LeftShift)) Throttle = Mathf.Min(1f, Throttle += ThrottleResponseSpeed * Time.deltaTime);
            if (Input.GetKeyDown(KeyCode.Z)) Throttle = 1f;
            if (Input.GetKeyDown(KeyCode.X)) Throttle = 0f;

            if (Input.GetKey(KeyCode.A)) Steer = -1f;
            else if (Input.GetKey(KeyCode.D)) Steer = 1f;
            else Steer = 0f;

            if (!LegsDeployed && Input.GetKeyDown(KeyCode.G)) // Player can't retract legs.
                LegsDeployed = !LegsDeployed;
            if (Input.GetKeyDown(KeyCode.B))
                FinsDeployed = !FinsDeployed;
        }
        else if (isAI && isControllable)
        {
            if (!LegsDeployed && (Altitude < 50 || Altitude / Velocity.y < 6)) LegsDeployed = true;
            if (!IsGrounded)
            {
                const float gravity = 9.81f; // Earth's gravitational acceleration (m/s^2)
                const float touchdownSpeed = 2.5f;

                float requiredDeceleration = (Mathf.Pow(Velocity.magnitude, 2) - Mathf.Pow(touchdownSpeed, 2)) / (2 * Altitude);
                float netForce = rb.mass * requiredDeceleration;
                float requiredThrust = netForce + (rb.mass * gravity);
                float throttle = requiredThrust / eb.Thrust;
                if (Throttle != 0 || throttle > .95f) // Suicide burn... ish
                    Throttle = Mathf.Clamp(throttle, 0f, 1f);
            }
            else Throttle = 0;
        }

        if (_legsDeployed != LegsDeployed)
        {
            LandingLegs.ForEach(l => l.isDeployed = LegsDeployed);
            _legsDeployed = LegsDeployed;
        }
        if (_finsDeployed != FinsDeployed)
        {
            Gridfins.ForEach(f => f.isDeployed = FinsDeployed);
            _finsDeployed = FinsDeployed;
        }
    }

    private void FixedUpdate()
    {
        if (transform.position.y < -1) db.DestroyVehicle(transform.position);
        AddToBuffers();
        if (isControllable && IsStable) SafeVehicle();
    }

    private void PartBreak(GameObject arg0)
    {
        if (arg0 == gameObject) colliders = null;
        // Remove all rotation and axis limits except roll, vehicle is dissasembling all bets are off.
        rb.constraints = RigidbodyConstraints.FreezeRotationY;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsGrounded && collision.gameObject.layer == 7)
        {
            _isGrounded = true;
            _timeGrounded = Time.time;
            if (_velocityGrounded == null) _velocityGrounded = collision.relativeVelocity.magnitude;
            if (_distanceGrounded == null) _distanceGrounded = transform.TransformPoint(new Vector3()).magnitude; // TODO replace with actual target distance.
            if (_angleGrounded == null) _angleGrounded = Quaternion.Angle(Quaternion.identity, transform.rotation);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (IsGrounded && collision.gameObject.layer == 7) _isGrounded = false;
    }

    private void AddToBuffers()
    {
        linearVelocities.Enqueue(rb.linearVelocity.magnitude);
        while (linearVelocities.Count > 0 && linearVelocities.Count * Time.fixedDeltaTime > StableTimer) linearVelocities.Dequeue();
        angularVelocities.Enqueue(rb.angularVelocity.magnitude);
        while (angularVelocities.Count > 0 && angularVelocities.Count * Time.fixedDeltaTime > StableTimer) angularVelocities.Dequeue();
    }

    private void SafeVehicle()
    {
        Throttle = 0;
        Steer = 0;
        isControllable = false;
    }
}
