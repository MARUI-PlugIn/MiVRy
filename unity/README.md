# MiVRy 3D Gesture Recognition Plug-In for Unity
Copyright (c) 2020 MARUI-PlugIn (inc.)

### License:
<a rel="license" href="http://creativecommons.org/licenses/by-nc/4.0/"><img alt="Creative Commons License" style="border-width:0" src="https://i.creativecommons.org/l/by-nc/4.0/88x31.png" /></a><br />MiVRy is licensed under a <a rel="license" href="http://creativecommons.org/licenses/by-nc/4.0/">Creative Commons Attribution-NonCommercial 4.0 International License</a>.

This plug-in is free for non-commercial use. Commercial use is prohibited. See the license statement below.
If you want to use this plug-in in a commercial application, please contact us at support@marui-plugin.com for a commercial license.

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
This software is free to use for non-commercial purposes. You may use this software in part or in full for any project that does not pursue financial gain, including free software  and projectes completed for evaluation or educational purposes only. Any use for commercial purposes is prohibited.
You may not sell or rent any software that includes this software in part or in full, either in it's original form or in altered form.
If you wish to use this software in a commercial application, please contact us at support@marui-plugin.com to obtain a commercial license.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,  THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR  PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY  OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
