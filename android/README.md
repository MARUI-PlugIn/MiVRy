MiVRy - 3D GESTURE RECOGNITION FOR ANDROID
Copyright (c) 2022 MARUI-PlugIn (inc.)

### License:
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,  THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR  PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY  OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


### HOW TO USE:

(1) Import the library into your project:
(1.1) Select File > New > New Module.
(1.2) Select Import .JAR/.AAR Package then click Next.
(1.3) Select the location of MiVRy.aar and click "Finish".
(1.4) In your project's build.gradle add to the "dependencies" list the following: implementation project(':mivry')

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


