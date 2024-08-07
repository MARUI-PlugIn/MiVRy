package com.maruiplugin.mivrydemo;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.res.Resources;
import android.graphics.Rect;
import android.os.Bundle;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.constraintlayout.widget.ConstraintLayout;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import com.maruiplugin.mivry.MiVRy;
import com.maruiplugin.mivry.MiVRyContinuousIdentificationListener;
import com.maruiplugin.mivry.MiVRyTrainingListener;

import java.io.File;
import java.io.FilenameFilter;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;

public class ContinuousGestures extends AppCompatActivity implements MiVRyContinuousIdentificationListener {

    private ButtonListenerRecord button_listener_record;
    private ButtonListenerCreate button_listener_create;
    private ButtonListenerFinish button_listener_finish;
    private ButtonListenerReset  button_listener_reset;
    private ButtonListenerQuit   button_listener_quit;
    private ButtonListenerSettings button_listener_settings;
    private ButtonListenerLoad   button_listener_load;
    private ButtonListenerSave   button_listener_save;
    private TrainingListener     training_listener;

    private static ConstraintLayout layout_continuous;
    private static Button button_record;
    private static Button button_create;
    private static Button button_finish;
    private static Button button_reset;
    private static Button button_quit;
    private static Button button_settings;
    private static Button button_load;
    private static Button button_save;
    private static TextView textview_message;
    private static TextView textview_keyword;
    private static TextView textview_gesturelist;

    public  static MiVRy mivry;
    private static boolean recording = false;
    public  static long recognition_interval = 250;
    private static int recording_gesture_id = -1;
    private static int load_gesture_database_index = 0;
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
    }

    @Override
    protected void onResume() {
        super.onResume();
        this.init();
    }

    private void init() {
        mivry.DeleteAllGestures();

        this.codewords = new ArrayList<String>();
        for (int i=0; i<codewords_sourcelist.length; i++) {
            codewords.add(codewords_sourcelist[i]);
        }

        String c = this.getRandomCodeword();
        this.recording_gesture_id = mivry.CreateGesture(c);
        load_gesture_database_index = 0;

        this.textview_message = findViewById(R.id.text_message);
        textview_message.setText("Use the button below\nto record gestures.");
        this.textview_keyword = findViewById(R.id.text_keyword);
        textview_keyword.setText("Your keyword is: "+c);
        this.textview_gesturelist = findViewById(R.id.text_gesturelist);
        textview_gesturelist.setVisibility(View.INVISIBLE);

        layout_continuous = findViewById(R.id.layout_continuous);
        Resources resources = getResources();
        layout_continuous.setBackgroundColor(resources.getColor(resources.getIdentifier(c, "color", getPackageName())));

        button_record = findViewById(R.id.button_record);
        this.button_listener_record = new ButtonListenerRecord();
        button_record.setOnTouchListener(this.button_listener_record);
        button_record.setText("Tap here\nand move phone\nto record gesture samples");

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

        button_settings = findViewById(R.id.button_settings);
        this.button_listener_settings = new ButtonListenerSettings();
        button_settings.setOnTouchListener(this.button_listener_settings);
        button_settings.setVisibility(View.VISIBLE);
        button_settings.setEnabled(true);

        button_load = findViewById(R.id.button_load);
        this.button_listener_load = new ButtonListenerLoad();
        button_load.setOnTouchListener(this.button_listener_load);
        if (getGestureDatabaseFile(0) == null) {
            button_load.setVisibility(View.INVISIBLE);
            button_load.setEnabled(false);
        } else {
            button_load.setVisibility(View.VISIBLE);
            button_load.setEnabled(true);
        }

        button_save = findViewById(R.id.button_save);
        this.button_listener_save = new ButtonListenerSave();
        button_save.setOnTouchListener(this.button_listener_save);
        button_save.setVisibility(View.INVISIBLE);
        button_save.setEnabled(false);

        this.training_listener = new TrainingListener();
        mivry.SetTrainingListener(this.training_listener);
    }

    public class ButtonListenerRecord implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_DOWN) {
                return true;
            }
            if (!recording) {
                mivry.StartGesture(recording_gesture_id);
                mivry.SetContinuousIdentificationListener(ContinuousGestures.this, recognition_interval);
                recording = true;
                button_record.setBackgroundResource(R.drawable.custom_button_active);
                if (recording_gesture_id >= 0) {
                    button_record.setText("Recording...\n(Move your phone)\nTap again to stop");
                } else {
                    button_record.setText("Identifying...\n(Move your phone)\nTap again to stop");
                }
                button_finish.setEnabled(false);
                button_finish.setVisibility(View.INVISIBLE);
                button_create.setEnabled(false);
                button_create.setVisibility(View.INVISIBLE);
                button_reset.setEnabled(false);
                button_reset.setVisibility(View.INVISIBLE);
                button_quit.setEnabled(false);
                button_quit.setVisibility(View.INVISIBLE);
                button_load.setEnabled(false);
                button_load.setVisibility(View.INVISIBLE);
                button_save.setEnabled(false);
                button_save.setVisibility(View.INVISIBLE);
                button_settings.setEnabled(false);
                button_settings.setVisibility(View.INVISIBLE);
                return true;
            }
            // if we arrive here, then a second tap ended the gesture recording process
            recording = false;
            button_record.setBackgroundResource(R.drawable.custom_button);
            mivry.SetContinuousIdentificationListener(null);
            mivry.CancelGesture();
            if (recording_gesture_id >= 0) {
                button_record.setText("Tap here\nand move phone\nto record gesture samples");
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
                button_record.setText("Tap here\nand move phone\nto identify gestures");
                button_finish.setEnabled(true);
                button_finish.setVisibility(View.VISIBLE);
                button_create.setEnabled(true);
                button_create.setVisibility(View.VISIBLE);
            }
            button_reset.setEnabled(true);
            button_reset.setVisibility(View.VISIBLE);
            button_quit.setEnabled(true);
            button_quit.setVisibility(View.VISIBLE);
            if (getGestureDatabaseFile(0) == null) {
                button_load.setVisibility(View.INVISIBLE);
                button_load.setEnabled(false);
            } else {
                button_load.setVisibility(View.VISIBLE);
                button_load.setEnabled(true);
            }
            button_save.setVisibility(View.VISIBLE);
            button_save.setEnabled(true);
            button_settings.setVisibility(View.VISIBLE);
            button_settings.setEnabled(true);
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
                button_record.setText("Tap here\nand move phone\nto identify gestures");
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
            button_record.setText("Tap here\nand move phone\nto record gesture samples");
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


    public class ButtonListenerSettings implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_UP) {
                return true;
            }
            Intent i = new Intent(ContinuousGestures.this, ContinuousGesturesSettings.class);
            ContinuousGestures.this.startActivity(i);
            return true;
        }
    }


    public String getGestureDatabaseFile(int index) {
        File dir = new File(save_gesture_database_path);
        if (!dir.exists() || !dir.isDirectory()) {
            return null;
        }
        FilenameFilter filter = new FilenameFilter() {
            @Override
            public boolean accept(File file, String s) {
                return s.contains("Continuous") && s.contains(".dat");
            }
        };
        File[] files = dir.listFiles(filter);
        if (files.length == 0) {
            return null;
        }
        return files[files.length-1-(index % files.length)].getAbsolutePath();
    }


    public class ButtonListenerLoad implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_UP) {
                return true;
            }
            String path = getGestureDatabaseFile(load_gesture_database_index);
            if (path == null) {
                Toast.makeText(getApplicationContext(), "No gesture database file found", Toast.LENGTH_LONG).show();
                return true;
            }
            load_gesture_database_index++;
            int ret = mivry.LoadFromFile(path);
            if (ret != 0) {
                Toast.makeText(getApplicationContext(), "Failed to load gesture database ("+ret+")", Toast.LENGTH_LONG).show();
                return true;
            }
            Toast.makeText(getApplicationContext(), "Loaded: " + path, Toast.LENGTH_LONG).show();
            textview_message.setText("Gestures loaded!\nRecorded performance:");
            textview_keyword.setText(String.format("%.1f%%",mivry.RecognitionScore() * 100.0));
            button_finish.setVisibility(View.VISIBLE);
            button_finish.setEnabled(true);
            button_finish.setText("Restart training");
            button_create.setVisibility(View.VISIBLE);
            button_create.setEnabled(true);
            button_create.setText("Tap to add\na new gesture");
            button_record.setVisibility(View.VISIBLE);
            button_record.setEnabled(true);
            button_record.setText("Tap here\nand move phone\nto identify gestures");
            String gesture_list = "Recorded gestures:\n";
            int n = mivry.NumberOfGestures();
            if (n > 0) {
                gesture_list = gesture_list + mivry.GetGestureName(n-1);
                for (int i=n-2; i>=0; i--) {
                    gesture_list = gesture_list + ", " + mivry.GetGestureName(i);
                }
            }
            textview_gesturelist.setText(gesture_list);
            textview_gesturelist.setVisibility(View.VISIBLE);
            button_load.setVisibility(View.VISIBLE);
            button_load.setEnabled(true);
            button_save.setVisibility(View.VISIBLE);
            button_save.setEnabled(true);
            Resources resources = getResources();
            layout_continuous.setBackgroundColor(resources.getColor(resources.getIdentifier("white", "color", getPackageName())));
            recording_gesture_id = -1;
            return true;
        }
    }

    public class ButtonListenerSave implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_UP) {
                return true;
            }
            int permissionCheck = ContextCompat.checkSelfPermission(ContinuousGestures.this, Manifest.permission.WRITE_EXTERNAL_STORAGE);
            if (permissionCheck != PackageManager.PERMISSION_GRANTED) {
                ActivityCompat.requestPermissions(ContinuousGestures.this, new String[]{Manifest.permission.WRITE_EXTERNAL_STORAGE}, 0);
            }
            android.text.format.DateFormat df = new android.text.format.DateFormat();
            CharSequence date = df.format("yyyy-MM-dd_HHmmss", new java.util.Date());
            String path = save_gesture_database_path + "/GesturesContinuous" + date + ".dat";
            int ret = mivry.SaveToFile(path);
            if (ret == 0) {
                Toast.makeText(getApplicationContext(), "Saved to: " + path, Toast.LENGTH_LONG).show();
            } else {
                Toast.makeText(getApplicationContext(), "Failed to save gesture database ("+ret+")", Toast.LENGTH_LONG).show();
            }
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
        @Override
        public void trainingFinishCallback(double performance)
        {
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
                button_record.setText("Tap here\nand move phone\nto identify gestures");
                String gesture_list = "Recorded gestures:\n";
                int n = mivry.NumberOfGestures();
                if (n > 0) {
                    gesture_list = gesture_list + mivry.GetGestureName(n-1);
                    for (int i=n-2; i>=0; i--) {
                        gesture_list = gesture_list + ", " + mivry.GetGestureName(i);
                    }
                }
                textview_gesturelist.setText(gesture_list);
                textview_gesturelist.setVisibility(View.VISIBLE);
                if (getGestureDatabaseFile(0) == null) {
                    button_load.setVisibility(View.INVISIBLE);
                    button_load.setEnabled(false);
                } else {
                    button_load.setVisibility(View.VISIBLE);
                    button_load.setEnabled(true);
                }
                button_save.setVisibility(View.VISIBLE);
                button_save.setEnabled(true);
                Resources resources = getResources();
                layout_continuous.setBackgroundColor(resources.getColor(resources.getIdentifier("white", "color", getPackageName())));
            }});
        }
    }


    @Override
    public void onRequestPermissionsResult (int requestCode, String[] permissions, int[] grantResults) {
        if (requestCode != 0) {
            Log.d("MiVRy", "Unknown permission request code: " + requestCode);
        }
        for (int i=0; i<grantResults.length; i++) {
            if (grantResults[i] != PackageManager.PERMISSION_GRANTED) {
                Log.d("MiVRy", "Permission denied");
            }
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
