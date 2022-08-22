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

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(GestureManager))]
public class GestureManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // DrawDefaultInspector();

        serializedObject.Update();
        var license_id_prop = serializedObject.FindProperty("license_id");
        var license_key_prop = serializedObject.FindProperty("license_key");
        var unityXrPlugin_prop = serializedObject.FindProperty("unityXrPlugin");
        var mivryCoordinateSystem_prop = serializedObject.FindProperty("mivryCoordinateSystem");
        var file_load_combinations_prop = serializedObject.FindProperty("file_load_combinations");
        var file_import_combinations_prop = serializedObject.FindProperty("file_import_combinations");
        var file_load_subgestures_prop = serializedObject.FindProperty("file_load_subgestures");
        var file_load_subgestures_i_prop = serializedObject.FindProperty("file_load_subgestures_i");
        var file_load_gestures_prop = serializedObject.FindProperty("file_load_gestures");
        var file_import_gestures_prop = serializedObject.FindProperty("file_import_gestures");
        var file_save_combinations_prop = serializedObject.FindProperty("file_save_combinations");
        var file_save_gestures_prop = serializedObject.FindProperty("file_save_gestures");
        var create_combination_name_prop = serializedObject.FindProperty("create_combination_name");
        var create_gesture_name_prop = serializedObject.FindProperty("create_gesture_name");
        // var create_gesture_names_prop = serializedObject.FindProperty("create_gesture_names");
        var record_gesture_id_prop = serializedObject.FindProperty("record_gesture_id");
        var record_combination_id_prop = serializedObject.FindProperty("record_combination_id");
        var lefthand_combination_part_prop = serializedObject.FindProperty("lefthand_combination_part");
        var righthand_combination_part_prop = serializedObject.FindProperty("righthand_combination_part");
        var copy_gesture_from_part_prop = serializedObject.FindProperty("copy_gesture_from_part");
        var copy_gesture_to_part_prop = serializedObject.FindProperty("copy_gesture_to_part");
        var copy_gesture_to_id_prop = serializedObject.FindProperty("copy_gesture_to_id");
        var copy_gesture_mirror_prop = serializedObject.FindProperty("copy_gesture_mirror");
        var copy_gesture_rotate_prop = serializedObject.FindProperty("copy_gesture_rotate");
        var compensate_head_motion_prop = serializedObject.FindProperty("compensate_head_motion");

        GestureManager gm = (GestureManager)target;

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("NUMBER OF GESTURE PARTS:");
        gm.numberOfParts = EditorGUILayout.IntField("Number of parts", gm.numberOfParts);
        EditorGUILayout.LabelField(" ", "1 for one-handed,");
        EditorGUILayout.LabelField(" ", "2 for two-handed or two sequential gestures,");
        EditorGUILayout.LabelField(" ", "3 for three sequential gestures, ...");
        EditorGUILayout.EndVertical();

        if (gm.gc != null)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("HAND/PART MAPPING:", "(which hand performs which part)");
            int num_parts = gm.gc.numberOfParts();
            string[] part_names = new string[num_parts];
            for (int i = 0; i < num_parts; i++)
            {
                part_names[i] = "part (or side) " + i.ToString();
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Left hand:");
            lefthand_combination_part_prop.intValue = EditorGUILayout.Popup(lefthand_combination_part_prop.intValue, part_names);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Right hand:");
            righthand_combination_part_prop.intValue = EditorGUILayout.Popup(righthand_combination_part_prop.intValue, part_names);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        string[] unityXrPlugins = { "OpenXR", "OculusVR", "SteamVR" };
        string[] mivryCoordinateSystems = { "OpenXR", "OculusVR", "SteamVR", "UnrealEngine" };

        if (gm.gr == null && gm.gc == null)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("LICENSE:", "(Leave empty for free version)");
            license_id_prop.stringValue = gm.license_id = EditorGUILayout.TextField("Licence ID", license_id_prop.stringValue);
            license_key_prop.stringValue = gm.license_key = EditorGUILayout.TextField("Licence Key", license_key_prop.stringValue);
            EditorGUILayout.EndVertical();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(gm);
                EditorSceneManager.MarkSceneDirty(gm.gameObject.scene);
            }
            serializedObject.ApplyModifiedProperties();
            return;
        }

        // Gesture file management
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("GESTURE FILES:");
        if (gm.gr != null)
        {
            file_load_gestures_prop.stringValue = EditorGUILayout.TextField("Load gestures file:", file_load_gestures_prop.stringValue);
            if (GUILayout.Button("Load Gestures from File"))
            {
                gm.loadFromFile();
            }
            file_import_gestures_prop.stringValue = EditorGUILayout.TextField("Import gestures file:", file_import_gestures_prop.stringValue);
            if (GUILayout.Button("Import Gestures from File"))
            {
                gm.importFromFile();
            }
            file_save_gestures_prop.stringValue = EditorGUILayout.TextField("Save gestures file:", file_save_gestures_prop.stringValue);
            if (GUILayout.Button("Save Gestures to File"))
            {
                gm.saveToFile();
            }
        } else if (gm.gc != null)
        {
            file_load_combinations_prop.stringValue = EditorGUILayout.TextField("Load GestureCombinations File: ", file_load_combinations_prop.stringValue);
            if (GUILayout.Button("Load GestureCombinations File"))
            {
                gm.loadFromFile();
            }
            file_import_combinations_prop.stringValue = EditorGUILayout.TextField("Import GestureCombinations File: ", file_import_combinations_prop.stringValue);
            if (GUILayout.Button("Import GestureCombinations File"))
            {
                gm.importFromFile();
            }
            file_save_combinations_prop.stringValue = EditorGUILayout.TextField("Save GestureCombinations File: ", file_save_combinations_prop.stringValue);
            if (GUILayout.Button("Save GestureCombinations File"))
            {
                gm.saveToFile();
            }
            EditorGUILayout.LabelField("(optional) Import single-handed gesture file:");
            file_load_subgestures_prop.stringValue = EditorGUILayout.TextField("Import SubGestures File:", file_load_subgestures_prop.stringValue);
            file_load_subgestures_i_prop.intValue = EditorGUILayout.IntField("^ ... for subgesture #", file_load_subgestures_i_prop.intValue);
            if (GUILayout.Button("Import SubGesture File"))
            {
                int ret = gm.gc.loadGestureFromFile(gm.file_load_subgestures_i, gm.file_load_subgestures);
                Debug.Log((ret == 0 ? "Gesture file imported successfully" : $"[ERROR] Failed to import gesture file ({ret})."));
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("GESTURES:");
        if (gm.gr != null)
        {
            int num_gestures = gm.gr.numberOfGestures();
            for (int i = 0; i < num_gestures; i++)
            {
                string gesture_name = gm.gr.getGestureName(i);
                int gesture_samples = gm.gr.getGestureNumberOfSamples(i);
                GUILayout.BeginHorizontal();
                string new_gesture_name = EditorGUILayout.TextField(gesture_name);
                if (gesture_name != new_gesture_name)
                {
                    int ret = gm.gr.setGestureName(i, new_gesture_name);
                    if (ret != 0)
                    {
                        Debug.Log("[ERROR] Failed to rename gesture: " + GestureRecognition.getErrorMessage(ret));
                    }
                }
                EditorGUILayout.LabelField(gesture_samples.ToString() + " samples", GUILayout.Width(70));
                if (GUILayout.Button("Delete Last Sample"))
                {
                    int ret = gm.gr.deleteGestureSample(i, gesture_samples-1);
                    if (ret != 0)
                    {
                        Debug.Log("[ERROR] Failed to delete gesture sample: " + GestureRecognition.getErrorMessage(ret));
                    }

                }
                if (GUILayout.Button("Delete All Samples"))
                {
                    int ret = gm.gr.deleteAllGestureSamples(i);
                    if (ret != 0)
                    {
                        Debug.Log("[ERROR] Failed to delete gesture samples: " + GestureRecognition.getErrorMessage(ret));
                    }

                }
                if (GUILayout.Button("Delete Gesture"))
                {
                    int ret = gm.gr.deleteGesture(i);
                    if (ret != 0)
                    {
                        Debug.Log("[ERROR] Failed to delete gesture: " + GestureRecognition.getErrorMessage(ret));
                    }

                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            create_gesture_name_prop.stringValue = EditorGUILayout.TextField(create_gesture_name_prop.stringValue);
            if (GUILayout.Button("Create new gesture"))
            {
                int gesture_id = gm.gr.createGesture(gm.create_gesture_name);
                if (gesture_id < 0)
                {
                    Debug.Log("[ERROR] Failed to create gesture.");
                }
                gm.create_gesture_name = "(new gesture name)";

            }
            GUILayout.EndHorizontal();
            file_load_gestures_prop.stringValue = EditorGUILayout.TextField("Import gestures:", file_load_gestures_prop.stringValue);
            if (GUILayout.Button("Import Gestures from File"))
            {
                GestureRecognition importGR = new GestureRecognition();
                int ret = importGR.loadFromFile(gm.file_load_gestures);
                if (ret != 0)
                {
                    Debug.Log($"[ERROR] Failed to load gesture file ({ret}).");
                } else {
                    ret = gm.gr.importGestures(importGR);
                    if (ret != 0)
                    {
                        Debug.Log($"[ERROR] Failed to import gesture file ({ret})");
                    } else
                    {
                        Debug.Log("Gesture file imported successfully");
                    }
                }
            }
        }
        else if (gm.gc != null)
        {
            int num_combinations = gm.gc.numberOfGestureCombinations();
            int num_parts = gm.gc.numberOfParts();
            for (int combination_id = 0; combination_id < num_combinations; combination_id++)
            {
                string combination_name = gm.gc.getGestureCombinationName(combination_id);
                GUILayout.BeginHorizontal();
                string new_combination_name = EditorGUILayout.TextField(combination_name);
                if (combination_name != new_combination_name)
                {
                    int ret = gm.gc.setGestureCombinationName(combination_id, new_combination_name);
                    if (ret != 0)
                    {
                        Debug.Log("[ERROR] Failed to rename GestureCombination: " + GestureRecognition.getErrorMessage(ret));
                    }
                }
                if (GUILayout.Button("Delete"))
                {
                    int ret = gm.gc.deleteGestureCombination(combination_id);
                    if (ret != 0)
                    {
                        Debug.Log("[ERROR] Failed to delete gesture: " + GestureRecognition.getErrorMessage(ret));
                    }

                }
                GUILayout.EndHorizontal();
                for (int i = 0; i < num_parts; i++)
                {
                    int num_gestures = gm.gc.numberOfGestures(i);
                    string[] gesture_names = new string[num_gestures+1];
                    gesture_names[0] = "[NONE]";
                    for (int k = 0; k < num_gestures; k++)
                    {
                        gesture_names[k + 1] = gm.gc.getGestureName(i, k);
                    }
                    int connected_gesture_id = gm.gc.getCombinationPartGesture(combination_id, i);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Subgesture for part/side " + i);
                    int new_connected_gesture_id = EditorGUILayout.Popup(connected_gesture_id + 1, gesture_names) - 1;
                    GUILayout.EndHorizontal();
                    if (new_connected_gesture_id != connected_gesture_id)
                    {
                        int ret = gm.gc.setCombinationPartGesture(combination_id, i, new_connected_gesture_id);
                        if (ret != 0)
                        {
                            Debug.Log("[ERROR] Failed to change GestureCombination: " + GestureRecognition.getErrorMessage(ret));
                        }
                    }
                }
            }
            GUILayout.BeginHorizontal();
            create_combination_name_prop.stringValue = EditorGUILayout.TextField(create_combination_name_prop.stringValue);
            if (GUILayout.Button("Create new Gesture Combination"))
            {
                int gesture_id = gm.gc.createGestureCombination(gm.create_combination_name);
                if (gesture_id < 0)
                {
                    Debug.Log("[ERROR] Failed to create Gesture Combination.");
                }
                gm.create_combination_name = "(new Gesture Combination name)";
            }
            GUILayout.EndHorizontal();
            
            for (int part = 0; part < num_parts; part++)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Combination Part/Side:", part.ToString());
                int num_gestures = gm.gc.numberOfGestures(part);
                for (int i = 0; i < num_gestures; i++)
                {
                    string gesture_name = gm.gc.getGestureName(part, i);
                    int gesture_samples = gm.gc.getGestureNumberOfSamples(part, i);
                    GUILayout.BeginHorizontal();
                    string new_gesture_name = EditorGUILayout.TextField(gesture_name);
                    if (gesture_name != new_gesture_name)
                    {
                        int ret = gm.gc.setGestureName(part, i, new_gesture_name);
                        if (ret != 0)
                        {
                            Debug.Log("[ERROR] Failed to rename gesture: " + GestureRecognition.getErrorMessage(ret));
                        }
                    }
                    EditorGUILayout.LabelField(gesture_samples.ToString() + " samples", GUILayout.Width(70));
                    if (GUILayout.Button("Delete Last Sample"))
                    {
                        int ret = gm.gc.deleteGestureSample(part, i, gesture_samples-1);
                        if (ret != 0)
                        {
                            Debug.Log("[ERROR] Failed to delete gesture sample: " + GestureRecognition.getErrorMessage(ret));
                        }
                    }
                    if (GUILayout.Button("Delete All Samples"))
                    {
                        int ret = gm.gc.deleteAllGestureSamples(part, i);
                        if (ret != 0)
                        {
                            Debug.Log("[ERROR] Failed to delete gesture samples: " + GestureRecognition.getErrorMessage(ret));
                        }
                    }
                    if (GUILayout.Button("Delete"))
                    {
                        int ret = gm.gc.deleteGesture(part, i);
                        if (ret != 0)
                        {
                            Debug.Log("[ERROR] Failed to delete gesture: " + GestureRecognition.getErrorMessage(ret));
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                if (gm.create_gesture_names == null || gm.create_gesture_names.Length < num_parts)
                {
                    gm.create_gesture_names = new string[num_parts];
                    for (int i=0; i<num_parts; i++)
                    {
                        gm.create_gesture_names[i] = "(new gesture name)";
                    }
                }
                gm.create_gesture_names[part] = EditorGUILayout.TextField(gm.create_gesture_names[part]);
                if (GUILayout.Button("Add new gesture"))
                {
                    int gesture_id = gm.gc.createGesture(part, gm.create_gesture_names[part]);
                    if (gesture_id < 0)
                    {
                        Debug.Log("[ERROR] Failed to create gesture.");
                    }
                    gm.create_gesture_names[part] = "(new gesture name)";

                }
                GUILayout.EndHorizontal();
            }
            // copy gestures
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Copy gestures:");
            string[] part_names = new string[num_parts];
            for (int i = 0; i < num_parts; i++)
            {
                part_names[i] = i.ToString();
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("From (part/side):");
            copy_gesture_from_part_prop.intValue = EditorGUILayout.Popup(copy_gesture_from_part_prop.intValue, part_names);
            EditorGUILayout.EndHorizontal();

            int copy_gestures_from_num = gm.gc.numberOfGestures(gm.copy_gesture_from_part);
            string[] copy_gesture_from_names = new string[copy_gestures_from_num];
            for (int i = 0; i < copy_gestures_from_num; i++)
            {
                copy_gesture_from_names[i] = gm.gc.getGestureName(gm.copy_gesture_from_part, i);
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Gesture to copy:");
            gm.copy_gesture_from_id = EditorGUILayout.Popup(gm.copy_gesture_from_id, copy_gesture_from_names);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("To (part/side):");
            copy_gesture_to_part_prop.intValue = EditorGUILayout.Popup(copy_gesture_to_part_prop.intValue, part_names);
            EditorGUILayout.EndHorizontal();
            
            int copy_gestures_to_num = gm.gc.numberOfGestures(gm.copy_gesture_to_part);
            string[] copy_gesture_to_names = new string[copy_gestures_to_num + 1];
            copy_gesture_to_names[0] = "[NEW GESTURE]";
            for (int i = 0; i < copy_gestures_to_num; i++)
            {
                copy_gesture_to_names[i+1] = gm.gc.getGestureName(gm.copy_gesture_to_part, i);
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Copy into:");
            copy_gesture_to_id_prop.intValue = EditorGUILayout.Popup(copy_gesture_to_id_prop.intValue, copy_gesture_to_names);
            EditorGUILayout.EndHorizontal();
            
            copy_gesture_mirror_prop.boolValue = EditorGUILayout.Toggle("Mirror left/right", copy_gesture_mirror_prop.boolValue);
            copy_gesture_rotate_prop.boolValue = EditorGUILayout.Toggle("Rotate 180 degrees", copy_gesture_rotate_prop.boolValue);

            if (GUILayout.Button("Copy"))
            {
                int new_gesture = gm.gc.copyGesture(gm.copy_gesture_from_part, gm.copy_gesture_from_id, gm.copy_gesture_to_part, gm.copy_gesture_to_id - 1, gm.copy_gesture_mirror ^ gm.copy_gesture_rotate, false, gm.copy_gesture_rotate);
                if (new_gesture < 0)
                {
                    Debug.Log("[ERROR] Failed to copy gesture.");
                }
            }

            //EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("ROTATIONAL FRAME OF REFERENCE:");
        string[] framesOfReference = {"Head", "World"};
        gm.frameOfReferenceYaw = (GestureRecognition.FrameOfReference)EditorGUILayout.Popup("Yaw (left/right)", (int)gm.frameOfReferenceYaw, framesOfReference);
        gm.frameOfReferenceUpDownPitch = (GestureRecognition.FrameOfReference)EditorGUILayout.Popup("Pitch (up/down)", (int)gm.frameOfReferenceUpDownPitch, framesOfReference);
        gm.frameOfReferenceRollTilt = (GestureRecognition.FrameOfReference)EditorGUILayout.Popup("Tilt (roll)", (int)gm.frameOfReferenceRollTilt, framesOfReference);
        string[] rotationOrders = { "XYZ", "XZY", "YXZ (Unity)", "YZX", "ZXY", "ZYX (Unreal)" };
        gm.frameOfReferenceRotationOrder = (GestureRecognition.RotationOrder)EditorGUILayout.Popup("Rotation Order", (int)gm.frameOfReferenceRotationOrder, rotationOrders);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("RECORD GESTURE SAMPLES:");
        if (gm.gr != null)
        {
            int num_gestures = gm.gr.numberOfGestures();
            string[] gesture_names = new string[num_gestures+1];
            gesture_names[0] = "[Testing, not recording]";
            for (int i=0; i<num_gestures; i++)
            {
                gesture_names[i+1] = gm.gr.getGestureName(i);
            }
            record_gesture_id_prop.intValue = EditorGUILayout.Popup(record_gesture_id_prop.intValue + 1, gesture_names) - 1;
        } else if (gm.gc != null)
        {
            int num_combinations = gm.gc.numberOfGestureCombinations();
            string[] combination_names = new string[num_combinations + 1];
            combination_names[0] = "[Testing, not recording]";
            for (int i = 0; i < num_combinations; i++)
            {
                combination_names[i+1] = gm.gc.getGestureCombinationName(i);
            }
            record_combination_id_prop.intValue = EditorGUILayout.Popup(record_combination_id_prop.intValue + 1, combination_names) - 1;
        }
        compensate_head_motion_prop.boolValue = EditorGUILayout.Toggle("Compensate head motion during gesture", compensate_head_motion_prop.boolValue);
        EditorGUILayout.LabelField("COORDINATE SYSTEM CONVERSION:", "");
        unityXrPlugin_prop.intValue = EditorGUILayout.Popup("Unity XR Plug-in", unityXrPlugin_prop.intValue, unityXrPlugins);
        mivryCoordinateSystem_prop.intValue = EditorGUILayout.Popup("MiVRy Coordinate System", mivryCoordinateSystem_prop.intValue, mivryCoordinateSystems);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("START/STOP TRAINING:");
        if (gm.gr != null)
        {
            EditorGUILayout.LabelField("Performance:", (gm.gr.recognitionScore() * 100.0).ToString("0.00") +"%");
            EditorGUILayout.LabelField("Currently training:", (gm.gr.isTraining() ? "yes" : "no"));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start training"))
            {
                if (gm.gr.isTraining())
                {
                    Debug.Log("Already training...");
                } else
                {
                    int ret = gm.gr.startTraining();
                    if (ret == 0)
                    {
                        gm.training_started = true;
                        Debug.Log("Training started");
                    }
                    else
                    {
                        gm.training_started = false;
                        Debug.Log("[ERROR] Failed to start training: " + GestureRecognition.getErrorMessage(ret));
                    }
                }
            }
            if (GUILayout.Button("Stop training"))
            {
                gm.gr.stopTraining();
            }
            GUILayout.EndHorizontal();
        } else if (gm.gc != null)
        {
            EditorGUILayout.LabelField("Performance:", (gm.gc.recognitionScore() * 100.0).ToString("0.00") + "%");
            EditorGUILayout.LabelField("Currently training:", (gm.gc.isTraining() ? "yes" : "no"));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start training"))
            {
                if (gm.gc.isTraining())
                {
                    Debug.Log("Already training...");
                } else {
                    int ret = gm.gc.startTraining();
                    if (ret == 0)
                    {
                        gm.training_started = true;
                        Debug.Log("Training started");
                    }
                    else
                    {
                        gm.training_started = false;
                        Debug.Log("[ERROR] Failed to start training: " + GestureRecognition.getErrorMessage(ret));
                    }
                }
            }
            if (GUILayout.Button("Stop training"))
            {
                gm.gc.stopTraining();
            }
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("LICENSE:", "(Leave empty for free version)");
        license_id_prop.stringValue = gm.license_id = EditorGUILayout.TextField("Licence ID", license_id_prop.stringValue);
        license_key_prop.stringValue = gm.license_key = EditorGUILayout.TextField("Licence Key", license_key_prop.stringValue);
        EditorGUILayout.EndVertical();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(gm);
            EditorSceneManager.MarkSceneDirty(gm.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
