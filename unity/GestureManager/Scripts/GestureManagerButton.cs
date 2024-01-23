/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.10
 * Copyright (c) 2024 MARUI-PlugIn (inc.)
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
using UnityEngine;

public abstract class GestureManagerButton : MonoBehaviour
{

    [SerializeField] protected Material inactiveButtonMaterial;
    [SerializeField] protected Material hoverButtonMaterial;
    [SerializeField] protected Material activeButtonMaterial;

    protected Material material
    {
        set
        {
            var renderer = this.GetComponent<Renderer>();
            if (renderer != null) {
                renderer.material = value;
            }
            for (int i = this.transform.childCount - 1; i >= 0; i--) {
                GameObject child = this.transform.GetChild(i)?.gameObject;
                if (child == null) {
                    continue;
                }
                if (child.GetComponent<TextMesh>() != null) {
                    continue;
                }
                renderer = child.GetComponent<Renderer>();
                if (renderer != null) {
                    renderer.material = value;
                }
            }
        }
    }
}
