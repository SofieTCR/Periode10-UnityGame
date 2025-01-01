using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

public class DamageBahaviour : MonoBehaviour
{
    public class PartBreakEvent : UnityEngine.Events.UnityEvent<GameObject> { }
    public PartBreakEvent OnPartBreak = new();

    public GameObject[] instabreakObjects = new GameObject[0];
    public GameObject[] tolerantObjects = new GameObject[0];
    public float NormalVelocityTolerance = 12f;
    public float AngleTolerance = 15f;
    public GameObject destroyedPrefab;
    public GameObject firePrefab;
    private List<GameObject> looseParts = new List<GameObject>();
    private Rigidbody parentRB;
    private bool isDestroyed = false;

    private void Start()
    {
        parentRB = GetComponent<Rigidbody>();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed) return;
        var velocity = collision.relativeVelocity.magnitude;
        var upVector = transform.up;
        foreach(var contact in collision.contacts)
        {
            var tmpObj = contact.thisCollider.gameObject;
            if ((tmpObj == gameObject && contact.otherCollider.tag != "RocketPart")
            || instabreakObjects.Contains(tmpObj) 
            || (tolerantObjects.Contains(tmpObj) 
            && (velocity > NormalVelocityTolerance 
            || Vector3.Angle(upVector, contact.normal) > AngleTolerance)))
            {
                HandleSnap(tmpObj, contact.point);
                if (isDestroyed) return;
            }
        }
    }

    void HandleSnap(GameObject obj, Vector3 impactPos)
    {
        switch (obj.name)
        {
            case "LegMesh":
                if (obj.transform.parent.parent == null) break;
                DestroyLeg(obj);
                break;
            case "grid fin outer rim":
                if (obj.transform.parent.parent.parent == null) break;
                DestroyGridfin(obj);
                break;
            default:
                DestroyVehicle(impactPos);
                break;
        }
    }

    void DestroyLeg(GameObject obj)
    {
        string objParentName = obj.transform.parent.gameObject.name;
        var dir = objParentName.Split(' ').Last();
        var strutParent = obj.transform.parent.parent.GetChild(0);
        GameObject strut = strutParent.Cast<Transform>()
                          .Select(child => child.gameObject)
                          .FirstOrDefault(go => go.name.Contains(dir, StringComparison.OrdinalIgnoreCase));

        if (strut != null)
        {
            looseParts.Add(strut);
            var look = strut.GetComponent<LookAtConstraint>();
            var piston = strut.GetComponentInChildren<LegPiston>();
            float length = piston.segmentLengths.Sum() / 100 * (piston.scale / 100);
            Component.Destroy(look);
            Component.Destroy(piston);
            strut.transform.SetParent(null);
            var RB_Strut = strut.AddComponent<Rigidbody>();
            RB_Strut.linearVelocity = parentRB.GetPointVelocity(obj.transform.parent.position);
            RB_Strut.angularVelocity = parentRB.angularVelocity;
            RB_Strut.angularDamping = parentRB.angularDamping;
            RB_Strut.linearDamping = parentRB.linearDamping;
            RB_Strut.mass = 100;
            parentRB.mass -= RB_Strut.mass;
            RB_Strut.constraints = RigidbodyConstraints.FreezeRotationY;
            var collider = strut.AddComponent<CapsuleCollider>();
            collider.radius = 0.001f;
            collider.height = length;
            collider.center = new Vector3(0, length / 2, 0);
        }
        looseParts.Add(obj.transform.parent.gameObject);
        obj.transform.parent.SetParent(null);
        var RB = obj.transform.parent.gameObject.AddComponent<Rigidbody>();
        RB.linearVelocity = parentRB.GetPointVelocity(obj.transform.parent.position);
        RB.angularVelocity = parentRB.angularVelocity;
        RB.angularDamping = parentRB.angularDamping;
        RB.linearDamping = parentRB.linearDamping;
        RB.mass = 1000;
        parentRB.mass -= RB.mass;
        OnPartBreak.Invoke(gameObject);
    }

    void DestroyGridfin(GameObject obj)
    {
        string objParentName = obj.transform.parent.parent.parent.gameObject.name;
        var parent = obj.transform.parent.parent;
        parent.SetParent(null);
        looseParts.Add(parent.gameObject);
        var RB = parent.gameObject.AddComponent<Rigidbody>();
        RB.linearVelocity = parentRB.GetPointVelocity(obj.transform.parent.position);
        RB.angularVelocity = parentRB.angularVelocity;
        RB.angularDamping = parentRB.angularDamping;
        RB.linearDamping = parentRB.linearDamping;
        RB.mass = 400;
        parentRB.mass -= RB.mass;
        parent.gameObject.AddComponent<MeshCollider>().convex = true;
        OnPartBreak.Invoke(gameObject);
    }

    public void DestroyVehicle(Vector3 impactPos)
    {
        foreach (var tmpObj in instabreakObjects.Concat(tolerantObjects))
        {
            HandleSnap(tmpObj, new Vector3());
        }
        gameObject.SetActive(false);
        var destroyedBits = Instantiate(destroyedPrefab, transform.position, transform.rotation);
        looseParts.Add(Instantiate(firePrefab, impactPos, Quaternion.identity));
        foreach (var rb in destroyedBits.GetComponentsInChildren<Rigidbody>()) looseParts.Add(rb.gameObject);
        Destroy(gameObject, GameSettings.TimeBetweenLevels);
        Destroy(destroyedBits, GameSettings.TimeBetweenLevels);
        isDestroyed = true;
        var explosionOrigin = transform.TransformPoint(transform.GetChild(1).GetComponent<MeshFilter>().mesh.bounds.center);
        foreach (var part in looseParts)
        {
            var rb = part.GetComponent<Rigidbody>();
            if (rb != null) rb.AddExplosionForce(8e2f, explosionOrigin, 100f, 0, ForceMode.Acceleration);
        }
    }

    private void OnDestroy()
    {
        foreach (var obj in looseParts) if (obj != null) Destroy(obj);
    }
}
