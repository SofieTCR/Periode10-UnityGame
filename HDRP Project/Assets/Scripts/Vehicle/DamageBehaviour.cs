using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

public class DamageBahaviour : MonoBehaviour
{
    public GameObject[] instabreakObjects = new GameObject[0];
    public GameObject[] tolerantObjects = new GameObject[0];
    public float NormalVelocityTolerance = 12f;
    public float AngleTolerance = 15f;
    private List<GameObject> looseParts = new List<GameObject>();
    private Rigidbody parentRB;

    private void Start()
    {
        parentRB = GetComponent<Rigidbody>();
    }
    void OnCollisionEnter(Collision collision)
    {
        var velocity = collision.relativeVelocity.magnitude;
        var upVector = transform.up;
        foreach(var contact in collision.contacts)
        {
            var tmpObj = contact.thisCollider.gameObject;
            if (tmpObj == gameObject
            || instabreakObjects.Contains(tmpObj) 
            || (tolerantObjects.Contains(tmpObj) 
            && (velocity > NormalVelocityTolerance 
            || Vector3.Angle(upVector, contact.normal) > AngleTolerance)))
            {
                HandleSnap(tmpObj);
            }
        }
    }

    void HandleSnap(GameObject obj, Vector3 impactPos = new Vector3())
    {
        switch (obj.name)
        {
            case "LegMesh":
                if (obj.transform.parent.parent == null) break;
                DestroyLeg(obj);
                break;
            case "grid fin outer rim":
                Debug.Log($"[{obj.transform.parent.parent.parent.gameObject.name}] would've snapped");
                break;
            default:
                Debug.Log("Full Explosion!!");
                break;
        }
    }

    void DestroyLeg(GameObject obj)
    {
        string objParentName = obj.transform.parent.gameObject.name;
        Debug.Log($"[{objParentName}] snapped");
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
            RB_Strut.constraints = RigidbodyConstraints.FreezeRotationY;
            var collider = strut.AddComponent<CapsuleCollider>();
            collider.radius = 0.001f;
            collider.height = length;
            collider.center = new Vector3(0, length / 2, 0);
        }
        looseParts.Add(obj);
        obj.transform.parent.SetParent(null);
        var RB = obj.transform.parent.gameObject.AddComponent<Rigidbody>();
        RB.linearVelocity = parentRB.GetPointVelocity(obj.transform.parent.position);
        RB.angularVelocity = parentRB.angularVelocity;
        RB.angularDamping = parentRB.angularDamping;
        RB.linearDamping = parentRB.linearDamping;
        RB.mass = 1000;
    }

    private void OnDestroy()
    {
        foreach (var obj in looseParts) Destroy(obj);
    }
}
