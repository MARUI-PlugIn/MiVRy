# MiVRy 3D Gesture Recognition AI
Copyright (c) 2023 MARUI-PlugIn (inc.)

MiVRy is an artificial intelligence (AI) that can learn to detect and distinguish motion patterns. It is aimed at human gesture recognition and can be trained to understand gestures such as a *"come here"* wave, performing a dance move, using tactical signs and gestures as used by military and police or you very own secret sign language.

It is cross-platform and can be used with any VR device, mobile device (phone, smart watch) or with camera-based tracking systems (kinect).

[![Watch the video](https://img.youtube.com/vi/N-84gPjY0Eo/hqdefault.jpg)](https://youtu.be/N-84gPjY0Eo)

## License:
MiVRy is licensed under The MIT License (MIT):

Copyright © 2022 株式会社MARUI-PlugIn (inc.)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

**WARNING:**
This license does NOT include non source code content (3d models, textures, animations, etc.) which are only provided as part of usage samples or tools.
These content files may NOT be used in any way except with MiVRy and may not be copied, modified, merged, published, distributed, sublicensed or sold.
The copyright to these files remain with the respective creators.

## Release Notes:

### v2.8 (2023-06-29)

- Improved continous gesture recognition.
- Added functions to change gesture sample type (normal vs continuous) after recording.
- Improved support for large data sets.
- Improved coordinate systems conversions between different plugins.
- Added function to prune (crop) current gesture motion.
- Fixed bug in copyGesture function.

### v2.7 (2023-02-26)

- Fixed issue in Unreal coordinate system conversion.
- Improved internal memory handling for increased performance.
- Fixed issue with identifying Vive controllers in Unity plugins.
- Added support for aribtrary controller components and license files to UE MiVRyActor.
- License information can now be set via file.

### v2.6 (2022-11-27)

- Created new UnityQuestHands plug-in to use hand tracking on Meta Quest2.
- Added a new setting to limit the maximum number of parallel training threads.
- Updating the head position during gesturing is now a setting stored inside the AI.
- Sub-gestures (parts) of gesture combinations can now be enabled/disabled individually without losing data.
- Added option to save metadata with gestures and GestureCombinations.
- EndStrokeAndGetAllProbabilities() function now returns the identified gesture ID.
- Bugfixes.

### v2.5 (2022-08-22)

- Extended internal neural network parameters.
- Improved support for new Unity input system.
- Gesture Manager now imports/exports files to an easier-to-reach location.
- Fixed potential crashes in file loading and training.
- Added UE Blueprint access to GestureCombinations.NumberOfParts.

### v2.4 (2022-07-17)

- Improved GestureManager (can now display recorded samples, dragged by handles, new environment).
- Improved similarity calculation, respecting rotations.
- Added functions to Mivry.cs that can be used with Unity InputActions.
- Fixed issues in file loading.
- Made GestureManagerEditor serialize (save) settings.

### v2.3 (2022-06-21)

- Added RotationOrder setting to RotationalFrameOfReference to avoid problems with Unity/Unreal rotations.
- Improved initial training parameters.
- Added new cubemap/skybox to GestureManager.
- Added continuous identification support to Mivry.cs and fixed related issues.
- Bugfixes.

### v2.2 (2022-05-22)

- Updated Unreal plug-in for UE5 support.
- Added asynchronous file saving (saveToFileAsync).
- Fixed delay in starting threads (train, load, and save).
- Fixed critical issue in Android .so libray where sha256 / license check would not work or even crash.

### v2.1 (2022-05-01)

- Added updateHeadPosition function to allow for compensating head motions during gesturing.
- Added coordinate system conversion functions for OpenXR, OculusVR, SteamVR (Unity) and Unreal.
- Added support for Unity Bolt visual graph scripting.
- Fixed issues in gesture scale / SD calculation.
- Fixed floating point precision issue in saving gesture database files.
- Improved GestureManager usability.
- Updated to Unity package to Unity2020.3.30, removed dependencies.

### v2.0 (2022-02-16)

- New licensing.
- Added asynchronous loadFromFile and loadFromBuffer functions.
- Fixed Unity-Coords-Compatibility in Unreal plug-in.
