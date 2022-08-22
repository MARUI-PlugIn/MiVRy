/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.5
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

public class SubmenuTraining : MonoBehaviour
{
    private bool initialized = false;
    private TextMesh TrainingCurrentStatus;
    private TextMesh TrainingCurrentPerformance;

    void Start()
    {
        this.init();
    }

    private void init()
    {
        for (int i=0; i<this.transform.childCount; i++)
        {
            GameObject child = this.transform.GetChild(i).gameObject;
            switch (child.name)
            {
                case "SubmenuTrainingCurrentStatus":
                    TrainingCurrentStatus = child.GetComponent<TextMesh>();
                    break;
                case "SubmenuTrainingCurrentPerformance":
                    TrainingCurrentPerformance = child.GetComponent<TextMesh>();
                    break;
            }
        }
        this.initialized = true;
    }

    public void refresh()
    {
        if (!this.initialized)
            this.init();
        if (GestureManagerVR.me == null || GestureManagerVR.me.gestureManager == null)
            return;
        double score = 0;
        if (GestureManagerVR.me.gestureManager.gr != null)
        {
            if (GestureManagerVR.me.gestureManager.gr.isTraining() || GestureManagerVR.me.gestureManager.gr.isLoading())
            {
                TrainingCurrentStatus.text = "yes";
                score = GestureManagerVR.me.gestureManager.last_performance_report;
            } else
            {
                TrainingCurrentStatus.text = "no";
                score = GestureManagerVR.me.gestureManager.gr.recognitionScore();
            }
        } else if (GestureManagerVR.me.gestureManager.gc != null)
        {
            if (GestureManagerVR.me.gestureManager.gc.isTraining() || GestureManagerVR.me.gestureManager.gc.isLoading())
            {
                TrainingCurrentStatus.text = "yes";
                score = GestureManagerVR.me.gestureManager.last_performance_report;
            } else
            {
                TrainingCurrentStatus.text = "no";
                score = GestureManagerVR.me.gestureManager.gc.recognitionScore();
            }
        }
        score *= 100.0;
        TrainingCurrentPerformance.text = score.ToString("0.00") + "%";
    }
}
