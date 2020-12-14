package com.maruiplugin.mivrydemo;

import androidx.appcompat.app.AppCompatActivity;
import androidx.constraintlayout.widget.ConstraintLayout;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.graphics.Rect;
import android.os.Bundle;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import com.maruiplugin.mivry.MiVRy;
import com.maruiplugin.mivry.MiVRyTrainingListener;

import java.io.File;
import java.io.FilenameFilter;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;

public class BasicGestures extends AppCompatActivity
{
    private ButtonListenerRecord button_listener_record;
    private ButtonListenerCreate button_listener_create;
    private ButtonListenerFinish button_listener_finish;
    private ButtonListenerReset button_listener_reset;
    private ButtonListenerQuit button_listener_quit;
    private ButtonListenerSettings button_listener_settings;
    private ButtonListenerLoad button_listener_load;
    private ButtonListenerSave button_listener_save;
    private ButtonListenerUndo button_listener_undo;
    private TrainingListener training_listener;

    private static ConstraintLayout layout_basic;
    private static Button button_record;
    private static Button button_create;
    private static Button button_finish;
    private static Button button_reset;
    private static Button button_quit;
    private static Button button_settings;
    private static Button button_load;
    private static Button button_save;
    private static Button button_undo;
    private static TextView textview_message;
    private static TextView textview_keyword;
    private static TextView textview_performance;
    private static TextView textview_gesturelist;

    public static MiVRy mivry;
    private static int recording_gesture_id;
    private static double recognition_preformance;
    private static String save_gesture_database_path;
    private static int load_gesture_database_index = 0;
    private static final String[] codewords_sourcelist_abstract = {
            "tycoon", "peasant", "police", "discover", "construct",
            "skip", "nuclear", "defeat", "shoot", "vote", "sell",
            "wife", "brother", "mail", "forget", "health", "future",
            "minority", "moment", "layer", "stealth", "joy", "inheritance",
            "fireball", "relief", "panik", "monster", "peace"
    };
    private static final String[] codewords_sourcelist_shapes = {
            "triangle", "rectangle", "circle", "line down", "line up", "swipe left", "swipe right", "plus sign", "x sign", "infinite sign",
            "shake sideways", "pick up", "put down", "swipe arm", "rotate right", "rotate left", "bank left", "bank right"
    };
    private static final String[] codewords_sourcelist_characters = {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
            "A", "B", "C", "D", "D", "E", "F", "G", "H", "I", "J", "K",
            "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W",
            "X", "Y", "Z"
    };
    private static int which_codewords_sourcelist = 0;

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
        setContentView(R.layout.basic_gestures);

        //save_gesture_database_path = getFilesDir().getAbsolutePath();
        save_gesture_database_path = getExternalFilesDir(null).getAbsolutePath();
        prng = new Random();
        mivry = new MiVRy(this);

        this.init();
    }

    private void init() {
        mivry.DeleteAllGestures();

        this.codewords = new ArrayList<String>();
        String[] codewords_sourcelist = null;
        switch (which_codewords_sourcelist) {
            case 0:
                codewords_sourcelist = codewords_sourcelist_abstract;
                which_codewords_sourcelist = 1; // for next time
                break;
            case 1:
                codewords_sourcelist = codewords_sourcelist_shapes;
                which_codewords_sourcelist = 2; // for next time
                break;
            default:
                codewords_sourcelist = codewords_sourcelist_characters;
                which_codewords_sourcelist = 0; // for next time

        }
        for (int i=0; i<codewords_sourcelist.length; i++) {
            codewords.add(codewords_sourcelist[i]);
        }
        String c = this.getRandomCodeword();
        this.recording_gesture_id = mivry.CreateGesture(c);
        load_gesture_database_index = 0;

        this.textview_message = findViewById(R.id.text_message);
        textview_message.setText("Welcome to MiVRy! Use the button below\nto record gestures. Your keyword is:");
        this.textview_keyword = findViewById(R.id.text_keyword);
        textview_keyword.setText(c);
        this.textview_performance = findViewById(R.id.text_performance);
        textview_performance.setText("");
        this.textview_gesturelist = findViewById(R.id.text_gesturelist);
        textview_gesturelist.setVisibility(View.INVISIBLE);

        layout_basic = findViewById(R.id.layout_basic);

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

        button_undo = findViewById(R.id.button_undo);
        this.button_listener_undo = new ButtonListenerUndo();
        button_undo.setOnTouchListener(this.button_listener_undo);
        button_undo.setVisibility(View.INVISIBLE);
        button_undo.setEnabled(false);

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
            MiVRy.GestureRecognitionResult ret = mivry.EndGesture();
            if (recording_gesture_id >= 0) {
                int n = mivry.GetGestureNumberOfSamples(recording_gesture_id);
                String c = mivry.GetGestureName(recording_gesture_id);
                textview_message.setText("Recording gestures for keyword:");
                textview_keyword.setText(c);
                textview_performance.setText(String.format("Number of recorded samples: %d", n));
                if (n >= 10) { // minimum number of samples
                    button_finish.setEnabled(true);
                    button_finish.setVisibility(View.VISIBLE);
                    button_finish.setText("Finish recording&\nStart training");
                    button_create.setEnabled(true);
                    button_create.setVisibility(View.VISIBLE);
                    button_create.setText("Tap to record\nanother gesture");
                }
                textview_gesturelist.setVisibility(View.INVISIBLE);
                button_undo.setVisibility(View.VISIBLE);
                button_undo.setEnabled(true);
            } else {
                if (ret.gesture_id >=0 ) {
                    textview_message.setText("Identified gesture:");
                    textview_keyword.setText(mivry.GetGestureName(ret.gesture_id));
                    textview_performance.setText(String.format("(Confidence: %.1f%%)", ret.similarity * 100.0));
                } else if (ret.gesture_id == MiVRy.ERROR_NOTGESTURING) {
                    // Sometimes we get a second "ACTION_UP" event.
                    // -> Don't overwrite the result of the previous call.
                } else {
                    textview_message.setText("Failed to identify gesture.");
                    textview_keyword.setText("Error " + (-ret.gesture_id));
                    textview_performance.setText("");
                }
                button_finish.setEnabled(true);
                button_finish.setVisibility(View.VISIBLE);
                button_create.setEnabled(true);
                button_create.setVisibility(View.VISIBLE);
                button_undo.setEnabled(false);
                button_undo.setVisibility(View.INVISIBLE);
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
                textview_performance.setText("");
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
                textview_performance.setText("");
                button_record.setEnabled(false);
                button_record.setVisibility(View.INVISIBLE);
                button_create.setEnabled(false);
                button_create.setVisibility(View.INVISIBLE);
                button_finish.setEnabled(true);
                button_finish.setVisibility(View.VISIBLE);
                button_finish.setText("Stop training");
            }
            button_undo.setVisibility(View.INVISIBLE);
            button_undo.setEnabled(false);
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

            textview_message.setText("Recording new gesture.\nKeyword:");
            textview_keyword.setText(c);
            textview_performance.setText("Number of recorded samples: 0");
            button_record.setEnabled(true);
            button_record.setVisibility(View.VISIBLE);
            button_record.setText("Touch and hold\nand move phone\nto record gesture sample");
            button_create.setEnabled(false);
            button_create.setVisibility(View.INVISIBLE);
            button_finish.setEnabled(false);
            button_finish.setVisibility(View.INVISIBLE);
            button_undo.setEnabled(false);
            button_undo.setVisibility(View.INVISIBLE);
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
            Intent i = new Intent(BasicGestures.this, BasicGesturesSettings.class);
            BasicGestures.this.startActivity(i);
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
                return s.contains("Basic") && s.contains(".dat");
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
            textview_message.setText("Gestures Loaded!\nRecorded performance:");
            textview_keyword.setText(String.format("%.1f%%",mivry.RecognitionScore() * 100.0));
            textview_performance.setText("");
            button_finish.setVisibility(View.VISIBLE);
            button_finish.setEnabled(true);
            button_finish.setText("Restart training");
            button_create.setVisibility(View.VISIBLE);
            button_create.setEnabled(true);
            button_create.setText("Tap to add\na new gesture");
            button_record.setVisibility(View.VISIBLE);
            button_record.setEnabled(true);
            button_record.setText("Touch and hold\nand move phone\nto perform gesture");
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
            button_undo.setVisibility(View.INVISIBLE);
            button_undo.setEnabled(false);
            button_load.setVisibility(View.VISIBLE);
            button_load.setEnabled(true);
            button_save.setVisibility(View.VISIBLE);
            button_save.setEnabled(true);
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

            int permissionCheck = ContextCompat.checkSelfPermission(BasicGestures.this, Manifest.permission.WRITE_EXTERNAL_STORAGE);
            if (permissionCheck != PackageManager.PERMISSION_GRANTED) {
                ActivityCompat.requestPermissions(BasicGestures.this, new String[]{Manifest.permission.WRITE_EXTERNAL_STORAGE}, 0);
            }
            android.text.format.DateFormat df = new android.text.format.DateFormat();
            CharSequence date = df.format("yyyy-MM-dd_HHmmss", new java.util.Date());
            String path = save_gesture_database_path + "/GesturesBasic" + date + ".dat";
            int ret = mivry.SaveToFile(path);
            if (ret == 0) {
                Toast.makeText(getApplicationContext(), "Saved to: " + path, Toast.LENGTH_LONG).show();
            } else {
                Toast.makeText(getApplicationContext(), "Failed to save gesture database ("+ret+")", Toast.LENGTH_LONG).show();
            }
            return true;
        }
    }

    public class ButtonListenerUndo implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_UP) {
                return true;
            }
            if (recording_gesture_id < 0) {
                return true;
            }
            int num_samples = mivry.GetGestureNumberOfSamples(recording_gesture_id);
            if (num_samples > 0) {
                mivry.DeleteGestureSample(recording_gesture_id, num_samples-1);
            }
            num_samples = mivry.GetGestureNumberOfSamples(recording_gesture_id);
            textview_performance.setText(String.format("Number of recorded samples: %d", num_samples));
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
            recognition_preformance = performance;
            recording_gesture_id = -1;
            runOnUiThread(new Runnable() { public void run() {
                textview_message.setText("Training finished!\nFinal performance:");
                textview_keyword.setText(String.format("%.1f%%",recognition_preformance * 100.0));
                textview_performance.setText("");
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
                int n = mivry.NumberOfGestures();
                if (n > 0) {
                    gesture_list = gesture_list + mivry.GetGestureName(n-1);
                    for (int i=n-2; i>=0; i--) {
                        gesture_list = gesture_list + ", " + mivry.GetGestureName(i);
                    }
                }
                textview_gesturelist.setText(gesture_list);
                textview_gesturelist.setVisibility(View.VISIBLE);
                button_undo.setVisibility(View.INVISIBLE);
                button_undo.setEnabled(false);
                if (getGestureDatabaseFile(0) == null) {
                    button_load.setVisibility(View.INVISIBLE);
                    button_load.setEnabled(false);
                } else {
                    button_load.setVisibility(View.VISIBLE);
                    button_load.setEnabled(true);
                }
                button_save.setVisibility(View.VISIBLE);
                button_save.setEnabled(true);
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
}
