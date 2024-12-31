using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VehicleState : MonoBehaviour
{
    public bool isPlayer = false;
    public float Throttle = 0f;
    public float Steer = 0f;
    public bool LegsDeployed;
    public bool FinsDeployed;

    private float ThrottleResponseSpeed = 0.5f;
    private List<DeployBehaviour> LandingLegs;
    private List<DeployBehaviour> Gridfins;
    private bool _legsDeployed;
    private bool _finsDeployed;

    void Start()
    {
        var Deployables = gameObject.GetComponentsInChildren<DeployBehaviour>();
        LandingLegs = Deployables.Where(d => d.gameObject.name.Contains("leg", System.StringComparison.InvariantCultureIgnoreCase)).ToList();
        Gridfins = Deployables.Where(d => d.gameObject.name.Contains("fin", System.StringComparison.InvariantCultureIgnoreCase)).ToList();
        LandingLegs.ForEach(l => l.isDeployed = LegsDeployed); _legsDeployed = LegsDeployed;
        Gridfins.ForEach(f => f.isDeployed = FinsDeployed); _finsDeployed = FinsDeployed;
    }
    void Update()
    {
        if (isPlayer)
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
}
