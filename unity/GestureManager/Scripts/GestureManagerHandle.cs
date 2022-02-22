/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.0
 * Copyright (c) 2022 MARUI-PlugIn (inc.)
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY 
 * OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
