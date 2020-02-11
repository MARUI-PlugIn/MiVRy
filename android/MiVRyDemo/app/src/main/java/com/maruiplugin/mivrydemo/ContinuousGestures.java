package com.maruiplugin.mivrydemo;

import android.content.res.Resources;
import android.graphics.Rect;
import android.os.Bundle;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.SeekBar;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;
import androidx.constraintlayout.widget.ConstraintLayout;

import com.maruiplugin.mivry.MiVRy;
import com.maruiplugin.mivry.MiVRyContinuousIdentificationListener;
import com.maruiplugin.mivry.MiVRyTrainingListener;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

public class ContinuousGestures extends AppCompatActivity implements MiVRyContinuousIdentificationListener {

    private ButtonListenerRecord button_listener_record;
    private ButtonListenerCreate button_listener_create;
    private ButtonListenerFinish button_listener_finish;
    private ButtonListenerReset  button_listener_reset;
    private ButtonListenerQuit   button_listener_quit;
    private SeekbarListenerSmoothingPositional seekbar_listener_smoothing_positional;
    private SeekbarListenerSmoothingContinuous seekbar_listener_smoothing_continuous;
    private TrainingListener     training_listener;

    private static ConstraintLayout layout_continuous;
    private static Button button_record;
    private static Button button_create;
    private static Button button_finish;
    private static Button button_reset;
    private static Button button_quit;
    private static TextView textview_message;
    private static TextView textview_keyword;
    private static TextView textview_gesturelist;
    private static TextView textview_sensorinfo;
    private static SeekBar seekbar_smoothing_positional;
    private static SeekBar  seekbar_smoothing_continuous;

    private static MiVRy mivry;
    private static int recording_gesture_id;
    private static double recognition_preformance;
    private static String save_gesture_database_path;
    private static final String[] codewords_sourcelist = {
            "red", "blue", "green", "yellow", "orange", "teal", "grey"
    };

    private List<String> codewords = null;

    static private Random prng = null;

    private String getRandomCodeword() {
        if (codewords == null || codewords.size() == 0) {
            return "[ERROR]: No more codewords - please add more";
        }
        int i = prng.nextInt(codewords.size());
        String c = codewords.get(i);
        codewords.remove(i);
        return c;
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.continuous_gestures);

        //save_gesture_database_path = getFilesDir().getAbsolutePath();
        save_gesture_database_path = getExternalFilesDir(null).getAbsolutePath();
        prng = new Random();
        mivry = new MiVRy(this);
        mivry.SetContinuousIdentificationPeriod(1000);
        mivry.SetContinuousIdentificationSmoothing(3);

        this.init();
    }

    private void init() {
        this.codewords = new ArrayList<String>();
        for (int i=0; i<codewords_sourcelist.length; i++) {
            codewords.add(codewords_sourcelist[i]);
        }

        String c = this.getRandomCodeword();
        this.recording_gesture_id = mivry.CreateGesture(c);

        this.textview_message = findViewById(R.id.text_message);
        textview_message.setText("Use the button below\nto record gestures.");
        this.textview_keyword = findViewById(R.id.text_keyword);
        textview_keyword.setText("Your keyword is: "+c);
        this.textview_gesturelist = findViewById(R.id.text_gesturelist);
        textview_gesturelist.setVisibility(View.INVISIBLE);
        this.textview_sensorinfo = findViewById(R.id.text_sensorinfo);
        textview_sensorinfo.setText(mivry.GetActiveSensorTypes());

        layout_continuous = findViewById(R.id.layout_continuous);
        Resources resources = getResources();
        layout_continuous.setBackgroundColor(resources.getColor(resources.getIdentifier(c, "color", getPackageName())));

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

        button_reset = findViewById(R.id.button_reset);
        this.button_listener_reset = new ButtonListenerReset();
        button_reset.setOnTouchListener(this.button_listener_reset);
        button_reset.setVisibility(View.VISIBLE);
        button_reset.setEnabled(true);

        button_quit = findViewById(R.id.button_quit);
        this.button_listener_quit = new ButtonListenerQuit();
        button_quit.setOnTouchListener(this.button_listener_quit);
        button_quit.setVisibility(View.VISIBLE);
        button_quit.setEnabled(true);

        seekbar_smoothing_positional = findViewById(R.id.seekbar_smoothing_positional);
        this.seekbar_listener_smoothing_positional = new SeekbarListenerSmoothingPositional();
        seekbar_smoothing_positional.setOnSeekBarChangeListener(this.seekbar_listener_smoothing_positional);
        seekbar_smoothing_positional.setVisibility(View.VISIBLE);
        seekbar_smoothing_positional.setEnabled(true);
        seekbar_smoothing_positional.setProgress((int)(mivry.GetPositionalSmoothingFactor() * 100.0f));

        seekbar_smoothing_continuous = findViewById(R.id.seekbar_smoothing_continuous);
        this.seekbar_listener_smoothing_continuous = new SeekbarListenerSmoothingContinuous();
        seekbar_smoothing_continuous.setOnSeekBarChangeListener(this.seekbar_listener_smoothing_continuous);
        seekbar_smoothing_continuous.setVisibility(View.VISIBLE);
        seekbar_smoothing_continuous.setEnabled(true);
        seekbar_smoothing_continuous.setProgress((int)mivry.GetContinuousIdentificationSmoothing());

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
                    mivry.SetContinuousIdentificationListener(ContinuousGestures.this);
                    button_record.setBackgroundResource(R.drawable.custom_button_active);
                    button_finish.setEnabled(false);
                    button_finish.setVisibility(View.INVISIBLE);
                    button_create.setEnabled(false);
                    button_create.setVisibility(View.INVISIBLE);
                    button_reset.setEnabled(false);
                    button_reset.setVisibility(View.INVISIBLE);
                    button_quit.setEnabled(false);
                    button_quit.setVisibility(View.INVISIBLE);
                    return true;
                case MotionEvent.ACTION_MOVE:
                    final Rect r = new Rect();
                    v.getHitRect(r);
                    final float x = event.getX() + r.left;
                    final float y = event.getY() + r.top;
                    if (!r.contains((int) x, (int) y)) {
                        mivry.CancelGesture();
                        mivry.SetContinuousIdentificationListener(null);
                        button_record.setBackgroundResource(R.drawable.custom_button);
                    }
                    return true;
                case MotionEvent.ACTION_UP:
                    break; // additional action required - will be performed below
                default:
                    return true; // ignore other events
            }
            // if we arrive here, then a gesture was finished
            button_record.setBackgroundResource(R.drawable.custom_button);
            mivry.SetContinuousIdentificationListener(null);
            mivry.CancelGesture();
            if (recording_gesture_id >= 0) {
                int n = mivry.GetGestureNumberOfSamples(recording_gesture_id);
                String c = mivry.GetGestureName(recording_gesture_id);
                textview_message.setText("Recording gestures for keyword:\n" + c);
                textview_keyword.setText(String.format("Number of recorded samples: %d", n));
                if (n >= 10) { // minimum number of samples enforced
                    button_finish.setEnabled(true);
                    button_finish.setVisibility(View.VISIBLE);
                    button_finish.setText("Finish recording&\nStart training");
                    button_create.setEnabled(true);
                    button_create.setVisibility(View.VISIBLE);
                    button_create.setText("Tap to record\nanother gesture");
                }
            } else {
                button_finish.setEnabled(true);
                button_finish.setVisibility(View.VISIBLE);
                button_create.setEnabled(true);
                button_create.setVisibility(View.VISIBLE);
            }
            button_reset.setEnabled(true);
            button_reset.setVisibility(View.VISIBLE);
            button_quit.setEnabled(true);
            button_quit.setVisibility(View.VISIBLE);
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
            Resources resources = getResources();
            layout_continuous.setBackgroundColor(resources.getColor(resources.getIdentifier(c, "color", getPackageName())));

            button_create.setEnabled(false);
            button_create.setVisibility(View.INVISIBLE);

            button_finish.setEnabled(false);
            button_finish.setVisibility(View.INVISIBLE);
            return true;
        }
    }


    public class ButtonListenerReset implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_UP) {
                return true;
            }
            mivry.DeleteAllGestures();
            init();
            return true;
        }
    }

    public class ButtonListenerQuit implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_UP) {
                return true;
            }
            finish();
            return true;
        }
    }

    public class SeekbarListenerSmoothingPositional implements SeekBar.OnSeekBarChangeListener {
        @Override
        public void onStartTrackingTouch(SeekBar seekBar) {}

        @Override
        public void onStopTrackingTouch(SeekBar seekBar) {}

        @Override
        public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
            mivry.SetPositionalSmoothingFactor(((float)progress) / 100.0f);
        }
    }

    public class SeekbarListenerSmoothingContinuous implements SeekBar.OnSeekBarChangeListener {
        @Override
        public void onStartTrackingTouch(SeekBar seekBar) {}

        @Override
        public void onStopTrackingTouch(SeekBar seekBar) {}

        @Override
        public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
            Log.d("MiVRy", "smoothing=" + progress);
            mivry.SetContinuousIdentificationSmoothing(progress);
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
        @Override
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
                Resources resources = getResources();
                layout_continuous.setBackgroundColor(resources.getColor(resources.getIdentifier("white", "color", getPackageName())));
            }});
        }
    }

    @Override
    public void continuousIdentificationCallback(MiVRy.GestureRecognitionResult result)
    {
        String gesture_name = mivry.GetGestureName(result.gesture_id);
        textview_message.setText("Identifying gesture:\n" + gesture_name);
        textview_keyword.setText(String.format("(Confidence: %.1f%%)", result.similarity * 100.0));
        if (result.gesture_id >= 0) {
            Resources resources = getResources();
            layout_continuous.setBackgroundColor(resources.getColor(resources.getIdentifier(gesture_name, "color", getPackageName())));
        }
    }

    @Override
    public void continuousRecordingCallback(int gesture_id, int samples_recorded)
    {
        textview_message.setText("Recoding gesture:\n" + mivry.GetGestureName(gesture_id));
        textview_keyword.setText("(" + samples_recorded + " samples recorded)");
    }
}
