package com.maruiplugin.mivrydemo;

import androidx.appcompat.app.AppCompatActivity;
import androidx.constraintlayout.widget.ConstraintLayout;

import android.content.res.Resources;
import android.graphics.Color;
import android.graphics.Rect;
import android.os.Bundle;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.view.Window;
import android.widget.Button;
import android.widget.TextView;
import android.text.format.DateFormat;

import com.maruiplugin.mivry.MiVRy;
import com.maruiplugin.mivry.MiVRy.GestureRecognitionResult;
import com.maruiplugin.mivry.MiVRyContinuousIdentificationListener;
import com.maruiplugin.mivry.MiVRyTrainingListener;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;
import java.lang.Runnable;
import java.util.Date;

public class MainActivity extends AppCompatActivity {

    private ButtonListenerRecord button_listener_record;
    private ButtonListenerCreate button_listener_create;
    private ButtonListenerFinish button_listener_finish;
    private TrainingListener     training_listener;

    private static ConstraintLayout layout_main;
    private static Button button_record;
    private static Button button_create;
    private static Button button_finish;
    private static TextView textview_message;
    private static TextView textview_keyword;
    private static TextView textview_gesturelist;
    private static TextView textview_buildstamp;

    private static MiVRy mivry;
    private static int recording_gesture_id;
    private static double recognition_preformance;
    private static String save_gesture_database_path;
    private static final String[] codewords_sourcelist = {
            "tycoon", "peasant", "police", "discover", "construct",
            "skip", "nuclear", "defeat", "shoot", "vote", "sell",
            "wife", "brother", "mail" , "forget", "health", "future",
            "minority", "moment", "layer", "stealth", "joy", "inheritance",
            "fireball", "relief", "panik", "monster", "peace"
            /*
            "triangle", "rectangle", "circle", "line down", "line up", "swipe left", "swipe right", "plus sign", "x sign", "infinite sign",
            "shake sideways", "pick up", "put down", "swipe arm", "rotate right", "rotate left", "bank left", "bank right"
            */
            /*
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
            "A", "B", "C", "D", "N", "C", "S", "W",
            "b", "V", "Z", "&"
             */
    };

    private List<String> codewords = null;

    static private Random prng = null;

    private String getRandomCodeword() {
        if (codewords == null) {
            codewords = new ArrayList<String>();
            for (int i=0; i<codewords_sourcelist.length; i++) {
                codewords.add(codewords_sourcelist[i]);
            }
        }
        int i = prng.nextInt(codewords.size());
        String c = codewords.get(i);
        codewords.remove(i);
        return c;
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        //save_gesture_database_path = getFilesDir().getAbsolutePath();
        save_gesture_database_path = getExternalFilesDir(null).getAbsolutePath();
        prng = new Random();
        mivry = new MiVRy(this);
        String c = this.getRandomCodeword();
        this.recording_gesture_id = mivry.CreateGesture(c);

        this.textview_message = findViewById(R.id.text_message);
        textview_message.setText("Use the button below\nto record gestures.");
        this.textview_keyword = findViewById(R.id.text_keyword);
        textview_keyword.setText("Your keyword is: "+c);
        this.textview_gesturelist = findViewById(R.id.text_gesturelist);
        textview_gesturelist.setVisibility(View.INVISIBLE);
        this.textview_buildstamp = findViewById(R.id.text_buildstamp);
        textview_buildstamp.setText("Version: "+BuildConfig.BUILD_STAMP);

        layout_main = findViewById(R.id.layout_main);

        button_record = findViewById(R.id.button_record);
        this.button_listener_record = new ButtonListenerRecord();
        button_record.setOnTouchListener(this.button_listener_record);
        button_record.setText("Touch and hold\nand move phone\nto record gesture sample");

        button_create = findViewById(R.id.button_create);
        this.button_listener_create = new ButtonListenerCreate();
        button_create.setOnTouchListener(this.button_listener_create);
        button_create.setVisibility(View.INVISIBLE);
        button_create.setEnabled(false);

        button_finish = findViewById(R.id.button_finish);
        this.button_listener_finish = new ButtonListenerFinish();
        button_finish.setOnTouchListener(this.button_listener_finish);
        button_finish.setVisibility(View.INVISIBLE);
        button_finish.setEnabled(false);

        this.training_listener = new TrainingListener();
        mivry.SetTrainingListener(this.training_listener);
    }

    public class ButtonListenerRecord implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            switch (action) {
                case MotionEvent.ACTION_DOWN:
                    mivry.StartGesture(recording_gesture_id);
                    button_record.setBackgroundColor(0xFFFF0000);
                    return true;
                case MotionEvent.ACTION_MOVE:
                    final Rect r = new Rect();
                    v.getHitRect(r);
                    final float x = event.getX() + r.left;
                    final float y = event.getY() + r.top;
                    if (!r.contains((int) x, (int) y)) {
                        mivry.CancelGesture();
                        button_record.setBackgroundColor(0xFFCCCCCC);
                    }
                    return true;
                case MotionEvent.ACTION_UP:
                    break; // additional action required - will be performed below
                default:
                    return true; // ignore other events
            }
            // if we arrive here, then a gesture was finished
            button_record.setBackgroundColor(0xFFCCCCCC);
            MiVRy.GestureRecognitionResult ret = mivry.EndGesture();
            if (recording_gesture_id >= 0) {
                int n = mivry.GetGestureNumberOfSamples(recording_gesture_id);
                String c = mivry.GetGestureName(recording_gesture_id);
                textview_message.setText("Recording gestures for keyword:\n" + c);
                textview_keyword.setText(String.format("Number of recorded samples: %d", n));
                if (n >= 1) { // TODO: minimum number of samples enforced?
                    button_finish.setEnabled(true);
                    button_finish.setVisibility(View.VISIBLE);
                    button_finish.setText("Finish recording&\nStart training");
                }
            } else {
                if (ret.gesture_id >=0 ) {
                    textview_message.setText("Identified gesture:\n" + mivry.GetGestureName(ret.gesture_id));
                    textview_keyword.setText(String.format("(Confidence: %.1f%%)", ret.similarity * 100.0));
                } else {
                    textview_message.setText("Failed to identify gesture.");
                    textview_keyword.setText("Error " + (-ret.gesture_id));
                }
            }
            return true;
        }
    }

    public class ButtonListenerFinish implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_UP) {
                return true;
            }
            if (mivry.IsTraining()) {
                mivry.StopTraining();
                recording_gesture_id = -1;
                textview_message.setText("Training stopped.\nFinal performance:");
                textview_keyword.setText(String.format("(%.1f%%)", mivry.RecognitionScore()*100.0));
                button_record.setEnabled(true);
                button_record.setVisibility(View.VISIBLE);
                button_record.setText("Touch and hold\nand move phone\nto perform gesture");
                button_create.setEnabled(true);
                button_create.setVisibility(View.VISIBLE);
                button_create.setText("Tap to create\nnew gesture");
                button_finish.setEnabled(true);
                button_finish.setVisibility(View.VISIBLE);
                button_finish.setText("Restart training");
            } else {
                mivry.SetMaxTrainingTime(60);
                mivry.StartTraining();
                recording_gesture_id = -1;
                textview_message.setText("Training...");
                textview_keyword.setText(String.format("(%.1f%%)", 0.0));
                button_record.setEnabled(false);
                button_record.setVisibility(View.INVISIBLE);
                button_create.setEnabled(false);
                button_create.setVisibility(View.INVISIBLE);
                button_finish.setEnabled(true);
                button_finish.setVisibility(View.VISIBLE);
                button_finish.setText("Stop training");
            }
            return true;
        }
    }


    public class ButtonListenerCreate implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int ret;
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_UP) {
                return true;
            }
            String c = getRandomCodeword();
            recording_gesture_id = mivry.CreateGesture(c);

            textview_message.setText("Recording new gesture.\nKeyword: "+c);
            textview_keyword.setText("\"Number of recorded samples: 0");
            button_record.setEnabled(true);
            button_record.setVisibility(View.VISIBLE);
            button_record.setText("Touch and hold\nand move phone\nto record gesture sample");

            button_create.setEnabled(false);
            button_create.setVisibility(View.INVISIBLE);

            button_finish.setEnabled(false);
            button_finish.setVisibility(View.INVISIBLE);
            return true;
        }
    }

    public class TrainingListener implements MiVRyTrainingListener {
        @Override
        public void trainingUpdateCallback(double performance)
        {
            recognition_preformance = performance;
            runOnUiThread(new Runnable() { public void run() {
                textview_keyword.setText(String.format("(%.1f%%)", recognition_preformance * 100.0));
            }});
        }
        public void trainingFinishCallback(double performance)
        {
            android.text.format.DateFormat df = new android.text.format.DateFormat();
            CharSequence date = df.format("yyyy-MM-dd_HHmmss", new java.util.Date());
            boolean success = mivry.SaveToFile(save_gesture_database_path + "/MiVRy" + date + ".dat");
            if (!success) {
                Log.d("MiVRyDemo", "Failed to save gesture database to "+ save_gesture_database_path);
            }
            recognition_preformance = performance;
            recording_gesture_id = -1;
            runOnUiThread(new Runnable() { public void run() {
                textview_message.setText("Training finished!\nFinal performance:");
                textview_keyword.setText(String.format("%.1f%%",recognition_preformance * 100.0));
                button_finish.setVisibility(View.VISIBLE);
                button_finish.setEnabled(true);
                button_finish.setText("Restart training");
                button_create.setVisibility(View.VISIBLE);
                button_create.setEnabled(true);
                button_create.setText("Tap to create\nanother gesture");
                button_record.setVisibility(View.VISIBLE);
                button_record.setEnabled(true);
                button_record.setText("Touch and hold\nand move phone\nto perform gesture");
                String gesture_list = "Recorded gestures:\n";
                for (int i = mivry.NumberOfGestures()-1; i>=0; i--) {
                    gesture_list = gesture_list + mivry.GetGestureName(i) + "\n";
                }
                textview_gesturelist.setText(gesture_list);
                textview_gesturelist.setVisibility(View.VISIBLE);
            }});
        }
    }
}