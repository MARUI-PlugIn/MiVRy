using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureManagerHandle : MonoBehaviour
{
    public float radius = 0.05f;
    public float magneticField = 0.01f;
    public float intersectionDepth = 0.01f;

    private Quaternion rotationOffset = Quaternion.AngleAxis(90.0f, Vector3.right) * Quaternion.AngleAxis(180.0f, Vector3.up);

    public void OnTriggerStay(Collider other)
    {
        if (!other.name.EndsWith("pointer"))
            return;
        if (GestureManagerVR.isGesturing)
            return;
        SphereCollider sphereCollider = (SphereCollider)other;
        Vector3 dir = (this.gameObject.transform.position - other.transform.position);
        float dist = dir.magnitude;
        if (dist == 0)
            return;
        dir /= dist;
        float touchDist = this.radius + (sphereCollider.radius * sphereCollider.transform.localScale.x) - intersectionDepth;
        float fieldDist = touchDist + magneticField * Mathf.Min(2.0f, Time.deltaTime * 50.0f);
        if (dist > fieldDist)
            return; // escaped the magnetic field
        this.transform.parent.position = this.transform.parent.position + (dir * (touchDist - dist));
        if (Camera.main != null) {
            Vector3 lookDir = Camera.main.transform.position - this.transform.parent.position;
            lookDir.y = 0; // not facing up or down
            this.transform.parent.rotation = Quaternion.LookRotation(lookDir) * rotationOffset;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        this.OnTriggerStay(other);
    }
}
