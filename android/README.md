# MiVRy 3D Gesture Recognition AI - Android Integration
Copyright (c) 2020 MARUI-PlugIn (inc.)

This is the android package (AAR) version of MiVRy.

This repository contains the MiVRy gesture recognition package as well as sample applications that illustrate how MiVRy can be used on android devices such as smart phones or smart watches.


### License:
<a rel="license" href="http://creativecommons.org/licenses/by-nc/4.0/"><img alt="Creative Commons License" style="border-width:0" src="https://i.creativecommons.org/l/by-nc/4.0/88x31.png" /></a><br />MiVRy is licensed under a <a rel="license" href="http://creativecommons.org/licenses/by-nc/4.0/">Creative Commons Attribution-NonCommercial 4.0 International License</a>.

This software is free to use for non-commercial purposes. You may use this software in part or in full for any project that does not pursue financial gain, including free software and projects completed for evaluation or educational purposes only. Any use for commercial purposes is prohibited.
You may not sell or rent any software that includes this software in part or in full, either in it's original form or in altered form.

**If you wish to use this software in a commercial application, please contact us to obtain a commercial license.**

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,  THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR  PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY  OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


### HOW TO USE:

(1) Import the library into your project:
(1.1) Select File > New > New Module.
(1.2) Select Import .JAR/.AAR Package then click Next.
(1.3) Select the location of MiVRy.aar and click "Finish".

(2) Include in your java files:
(2.1) Add the import to the top of your file:
      import com.maruiplugin.mivry.MiVRy;
(2.2) Create a MiVRy object:
      MiVRy mivry = new MiVRy();

(3) Create gestures:
(3.1) Register a new gesture with:
      int gesture_id = mivry.CreateGesture("my gesture");
(3.2) Start the gesture recording with:
      mivry.StartGesture(gesture_id);
(3.3) Move the device to perform the gesture.
(3.4) Finish the recording with:
      mivry.EndGesture();
(3.5) Repeat the process (3.2 ~ 3.4) multiple times. We recommend recording at least 20 samples.

(4) Identify gestures:
(4.1) Start the gesture with:
      mivry.StartGesture(-1);
(4.2) Move the device to perform the gesture.
(4.3) End the gesture and get the ID of the identified gesture with
      int gesture_id = mivry.EndGesture();
(4.4) If needed, the name of the identified gesture can be acquired with
      String gesture_name = mivry.GetGestureName(gesture_id);


