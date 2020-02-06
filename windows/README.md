# MiVRy 3D Gesture Recognition AI - Windows Library
Copyright (c) 2020 MARUI-PlugIn (inc.)

This is the Windows library (.lib/.dll) version of MiVRy.

This repository contains the MiVRy gesture recognition package for Windows.


### License:
<a rel="license" href="http://creativecommons.org/licenses/by-nc/4.0/"><img alt="Creative Commons License" style="border-width:0" src="https://i.creativecommons.org/l/by-nc/4.0/88x31.png" /></a><br />MiVRy is licensed under a <a rel="license" href="http://creativecommons.org/licenses/by-nc/4.0/">Creative Commons Attribution-NonCommercial 4.0 International License</a>.

This software is free to use for non-commercial purposes. You may use this software in part or in full for any project that does not pursue financial gain, including free software and projects completed for evaluation or educational purposes only. Any use for commercial purposes is prohibited.
You may not sell or rent any software that includes this software in part or in full, either in it's original form or in altered form.

**If you wish to use this software in a commercial application, please contact us to obtain a commercial license.**

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,  THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR  PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY  OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


### HOW TO USE:

(1) Place the GestureRecognition.h
as well as the .dll and/or .lib files appropriate for your platform
in your project. 


(2) Create a new Gesture recognition object and register the gestures that you want to identify later.
```
IGestureRecognition* gr = new GestureRecognition();
int myFirstGesture = gr->createGesture("my first gesture");
int mySecondGesture = gr->createGesture("my second gesture");
```


(3) Record a number of samples for each gesture by calling startStroke(), contdStroke() and endStroke()
for your registered gestures, each time inputting the headset and controller transformation.
```
double hmd_p[3] = <your headset or reference coordinate system position>;
double hmd_q[4] = <your headset or reference coordinate system rotation as x,y,z,w quaternions>;
gr->startStroke(hmd_p, hmd_q, myFirstGesture);

// repeat the following while performing the gesture with your controller:
double p[3] = <your VR controller position in space>;
double q[4] = <your VR controller rotation in space as x,y,z,w quaternions>;
gr->contdStrokeQ(p,q);
// ^ repead while performing the gesture with your controller.

gr->endStroke();
```
Repeat this multiple times for each gesture you want to identify.
We recommend recording at least 20 samples for each gesture.


(4) Start the training process by calling startTraining().
You can optionally register callback functions to receive updates on the learning progress
by calling setTrainingUpdateCallback() and setTrainingFinishCallback().
```
gr->setMaxTrainingTime(10); // Set training time to 10 seconds.
gr->startTraining();
```
You can stop the training process by calling stopTraining().
After training, you can check the gesture identification performance by calling recognitionScore()
(a value of 1 means 100% correct recognition).


(5) Now you can identify new gestures performed by the user in the same way
as you were recording samples:
```
double hmd_p[3] = <your headset or reference coordinate system position>;
double hmd_q[4] = <your headset or reference coordinate system rotation as x,y,z,w quaternions>;
gr->startStroke(hmd_p, hmd_q);

// repeat the following while performing the gesture with your controller:
double p[3] = <your VR controller position in space>;
double q[4] = <your VR controller rotation in space as x,y,z,w quaternions>;
gr->contdStroke(p,q);
// ^ repeat while performing the gesture with your controller.

int identifiedGesture = gr->endStroke();
if (identifiedGesture == myFirstGesture) {
    // ...
}
```


(6) Now you can save and load the artificial intelligence.
```
gr->saveToFile("C:/myGestures.dat");
// ...
gr->loadFromFile("C:/myGestures.dat");
```


