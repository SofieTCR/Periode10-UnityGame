using UnityEngine;

public class DeployBehaviour : MonoBehaviour
{
    public bool isDeployed = false;
    public Vector3 deployVector = new Vector3(0, 0, -115);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (isDeployed)
        {
            var euler = transform.localEulerAngles;
            euler += deployVector;
            transform.localEulerAngles = euler;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // todo deploy logic
    }
}
