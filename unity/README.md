MiVRy - 3D Gesture Recognition plug-in for Unity
Copyright (c) 2022 MARUI-PlugIn (inc.)

[IMPORTANT!] This plug-in is free to use. However, the “free” license use is limited to 100 gesture recognitions (or 100 seconds of continuous gesture recognition) per session. To unlock unlimited gesture recognition, please purchase a license at https://www.marui-plugin.com/mivry/ 
[IMPORTANT!] This license is for the use of the MiVRy Gesture Recognition plug-in (.dll and .so files as well as the source code)
and does not include the use of the asset files used in the sample scenes (pixie character model and textures etc.)

Check out our YouTube channel for tutorials, demos, and news updates:
https://www.youtube.com/playlist?list=PLYt4XosVlCmWtmDmx1lS8OVGU70tmQOfv

Making good user interaction for VR is hard. The number of buttons often isn't enough and memorizing button combinations is challenging for users.
Gestures are a great solution! Allow your users to wave their 3D controllers like a magic want and have wonderful things happening. Draw an arrow to shoot a magic missile, make a spiral to summon a hurricane, shake your controller to reload your gun, or just swipe left and right to "undo" or "redo" previous operations.

MARUI has many years of experience of creating VR/AR/XR user interfaces for 3D design software.
Now YOU can use its powerful gesture recognition module in Unity.

This is a highly advanced artificial intelligence that can learn to understand your 3D controller motions.
The gestures can be both direction specific ("swipe left" vs. "swipe right") or direction independent ("draw an arrow facing in any direction") - either way, you will receive the direction, position, and scale at which the user performed the gesture!
Draw a large 3d cube and there it will appear, with the appropriate scale and orientation.

### Key features:
- Real 3D gestures - like waving a magic wand in all three dimensions
- Support for multi-part gesture combinations such as two-handed gestures or sequential combinations of gestures
- Record your own gestures - simple and straightforward
- Easy to use - single C# class
- Can have multiple sets of gestures simultaneously (for example: different sets of gestures for different buttons)
- High recognition fidelity
- Outputs the position, scale, and orientation at which the gesture was performed
- High performance (back-end written in optimized C/C++)
- Includes a Unity sample project that explains how to use the plug-in
- Save gestures to file for later loading


### Included files:
- GestureRecognition_Win_x86_64.dll : The gesture recognition plug-in for Windows. Place this file in your Unity project under /Assets/Plugins/
- GestureRecognition_Android_ARM64.so : The gesture recognition plug-in for Android (ARM). Place this file in your Unity project under /Assets/Plugins/
- GestureRecognition_Android_x86_64.so : The gesture recognition plug-in for Android (ARM). Place this file in your Unity project under /Assets/Plugins/
- GestureRecognition.cs : C# script for using the plugin. Include this file in your Unity project (for examples under /Assets/Scenes/Scripts/)
- GestureCombinations.cs : C# script for using the plugin for multi-part gesture combinations (eg. two-handed gestures). Include this file in your Unity project (for examples under /Assets/Scenes/Scripts/)
- Sample_OneHanded.unity / .cs : Unity sample scene and script for one-handed gestures.
- Sample_OneHanded_Gestures.dat : Example gesture database file to one-handed gestures.
- Sample_TwoHanded.unity / .cs : Unity sample scene and script for two-handed gestures.
- Sample_TwoHanded_Gestures.dat : Example gesture database file to two-handed gestures.
- GestureManager.unity : Unity scene that provides a convenient manager UI for creating and handling gesture files.
- all other files: assets used in the sample Unity projects.



### How to use:
Place the GestureRecognition_Win_x86_64.dll (Windows) and/or GestureRecognition_Android_ARM64.so (Android / MobileVR)
files in your Unity project in the /Assets/Plugins/ folder,
and the GestureRecognition.cs and/or GestureCombinations.cs files in your scripts folder (eg. /Assets/Scenes/Scripts/).
In any of your project's scripts, create a "GestureRecognition" (or "GestureCombinations") object.
> GestureRecognition gr = new GestureRecognition();
or:
> GestureCombinations gc = new GestureCombinations();
This object provides all the functions for recording, identifying and saving gestures.


### License:
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,  THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR  PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY  OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

