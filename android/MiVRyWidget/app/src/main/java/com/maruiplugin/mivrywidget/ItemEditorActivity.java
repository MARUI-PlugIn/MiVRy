package com.maruiplugin.mivrywidget;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.graphics.Rect;
import android.os.Bundle;
import android.os.Handler;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import com.maruiplugin.mivry.MiVRy;
import com.maruiplugin.mivry.MiVRyTrainingListener;

public class ItemEditorActivity extends AppCompatActivity {

    public static Item item = null;
    private static TextView textview_name;
    private static TextviewListenerName textview_listener_name;
    private static TextView textview_recordedsamples;
    private static Button button_target;
    private static ButtonListenerTarget button_listener_target;
    private static Button button_record;
    private static ButtonListenerRecord button_listener_record;
    private static Button button_dellast;
    private static ButtonListenerDelLast button_listener_dellast;
    private static Button button_delall;
    private static ButtonListenerDelAll button_listener_delall;
    private static Button button_return;
    private static ButtonListenerReturn button_listener_return;
    static boolean is_recording;
    static boolean gestures_changed;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_itemeditor);
    }

    @Override
    protected void onResume() {
        super.onResume();

        if (!ItemManager.isInitialized()) {
            finish();
            return;
        }

        Bundle b = getIntent().getExtras();
        int id = -1; // or other values
        if(b == null) {
            finish();
            return;
        }
        id = b.getInt("id");
        if (ItemManager.items == null || id < 0 || id >= ItemManager.items.size()) {
            finish();
            return;
        }
        this.item = ItemManager.items.get(id);
        if (this.item == null) {
            finish();
            return;
        }

        this.textview_name = findViewById(R.id.itemeditor_edittext_name);
        this.textview_name.setText(item.name);
        this.textview_listener_name = new TextviewListenerName();
        this.textview_name.setOnEditorActionListener(textview_listener_name);


        this.button_target = findViewById(R.id.itemeditor_button_target);
        this.button_target.setText(item.uri);
        this.button_listener_target = new ButtonListenerTarget();
        this.button_target.setOnClickListener(this.button_listener_target);

        this.textview_recordedsamples = findViewById(R.id.itemeditor_text_recordedsamples);
        this.textview_recordedsamples.setText("Recorded Gesture Samples: "+(ItemManager.mivry.GetGestureNumberOfSamples(item.gesture_id)));

        this.button_record = findViewById(R.id.itemeditor_button_record);
        this.button_listener_record = new ButtonListenerRecord();
        this.button_record.setOnTouchListener(this.button_listener_record);

        this.button_dellast = findViewById(R.id.itemeditor_button_dellast);
        this.button_listener_dellast = new ButtonListenerDelLast();
        this.button_dellast.setOnClickListener(this.button_listener_dellast);

        this.button_delall = findViewById(R.id.itemeditor_button_delall);
        this.button_listener_delall = new ButtonListenerDelAll();
        this.button_delall.setOnClickListener(this.button_listener_delall);

        this.button_return = findViewById(R.id.itemeditor_button_return);
        this.button_listener_return = new ButtonListenerReturn();
        this.button_return.setOnClickListener(button_listener_return);

        refresh();

        is_recording = false;
        gestures_changed = false;
    }

    public static void refresh() {
        if (item.name.length() == 0) {
            textview_name.setText("<Tap to set name>");
        } else {
            textview_name.setText(item.name);
        }
        if (item.uri.length() == 0) {
            button_target.setText("<Tap to set target>");
        } else {
            button_target.setText(item.uri);
        }
        textview_recordedsamples.setText("Recorded Gesture Samples: "+(ItemManager.mivry.GetGestureNumberOfSamples(item.gesture_id)));
        button_record.setBackgroundResource(R.drawable.custom_button);
        button_dellast.setEnabled(true);
        button_dellast.setVisibility(View.VISIBLE);
        button_delall.setEnabled(true);
        button_delall.setVisibility(View.VISIBLE);
        button_return.setEnabled(true);
        button_return.setVisibility(View.VISIBLE);
    }

    public class TextviewListenerName implements TextView.OnEditorActionListener {
        @Override
        public boolean onEditorAction(TextView v, int actionId, KeyEvent event) {
            if (item == null || ItemManager.mivry == null) {
                return false;
            }
            String name = v.getText().toString();
            String type;
            switch (item.type) {
                case AppLaunch:
                    type = "AppLaunch";
                    break;
                case MapLocation:
                    type = "MapLocation";
                    break;
                case CallContact:
                    type = "CallContact";
                    break;
                case WebLink:
                    type = "WebLink";
                    break;
                case None:
                default:
                    type = "None";
                    break;
            }
            ItemManager.mivry.SetGestureName(item.gesture_id, name+"|"+type+"|"+item.uri);
            MainActivity.refresh();
            return false;
        }
    }

    public class ButtonListenerTarget implements View.OnClickListener {
        @Override
        public void onClick(View v) {
            Intent intent = new Intent(ItemEditorActivity.this, ItemTypeSelectorActivity.class);
            startActivity(intent);
        }
    }
    public class ButtonListenerRecord implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            switch (action) {
                case MotionEvent.ACTION_DOWN:
                    if (is_recording) {
                        return true; // already recording
                    }
                    is_recording = true;
                    ItemManager.mivry.StartGesture(item.gesture_id);
                    button_record.setBackgroundResource(R.drawable.custom_button_active);
                    button_dellast.setEnabled(false);
                    button_dellast.setVisibility(View.INVISIBLE);
                    button_delall.setEnabled(false);
                    button_delall.setVisibility(View.INVISIBLE);
                    button_return.setEnabled(false);
                    button_return.setVisibility(View.INVISIBLE);
                    String manualend_str = Settings.get(ItemEditorActivity.this, "ManualGestureEnd");
                    boolean manualend = (manualend_str == null || manualend_str == "0") ? false : true;
                    if (!manualend) {
                        double gesturelen = 1.0;
                        try {
                            String s = Settings.get(ItemEditorActivity.this, "GestureLen");
                            if (s != null) {
                                gesturelen = Double.parseDouble(s);
                            }
                        } finally {
                            //
                        }
                        final Handler handler = new Handler();
                        handler.postDelayed(new Runnable() {
                            @Override
                            public void run() {
                                gestureEnd();
                            }
                        }, (long)(gesturelen*1000.0));
                    }
                    return true;
                case MotionEvent.ACTION_MOVE:
                    final Rect r = new Rect();
                    v.getHitRect(r);
                    final float x = event.getX() + r.left;
                    final float y = event.getY() + r.top;
                    if (!r.contains((int) x, (int) y)) {
                        is_recording = false;
                        ItemManager.mivry.CancelGesture();
                        button_record.setBackgroundResource(R.drawable.custom_button);
                        button_dellast.setEnabled(true);
                        button_dellast.setVisibility(View.VISIBLE);
                        button_delall.setEnabled(true);
                        button_delall.setVisibility(View.VISIBLE);
                        button_return.setEnabled(true);
                        button_return.setVisibility(View.VISIBLE);
                    }
                    return true;
                case MotionEvent.ACTION_UP:
                    break; // additional action required - will be performed below
                default:
                    return true; // ignore other events
            }
            String manualend_str = Settings.get(ItemEditorActivity.this, "ManualGestureEnd");
            boolean manualend = (manualend_str == null || manualend_str == "0") ? false : true;
            if (manualend) {
                // if we arrive here, then a gesture was finished
                gestureEnd();
            }
            return true;
        }
    }

    private void gestureEnd() {
        is_recording = false;
        ItemManager.mivry.EndGesture();
        button_record.setBackgroundResource(R.drawable.custom_button);
        button_dellast.setEnabled(true);
        button_dellast.setVisibility(View.VISIBLE);
        button_delall.setEnabled(true);
        button_delall.setVisibility(View.VISIBLE);
        button_return.setEnabled(true);
        button_return.setVisibility(View.VISIBLE);
        int num_samples = ItemManager.mivry.GetGestureNumberOfSamples(item.gesture_id);
        textview_recordedsamples.setText("Recorded Gesture Samples: " + num_samples);
        gestures_changed = true;
    }


    public class ButtonListenerDelLast implements View.OnClickListener {
        @Override
        public void onClick(View v) {
            if (ItemManager.mivry == null || item==null) {
                return;
            }
            int num_samples = ItemManager.mivry.GetGestureNumberOfSamples(item.gesture_id);
            if (num_samples <= 0) {
                return;
            }
            ItemManager.mivry.DeleteGestureSample(item.gesture_id, num_samples-1);
            // Update text
            textview_recordedsamples.setText("Recorded Gesture Samples: "+(num_samples-1));
        }
    }

    public class ButtonListenerDelAll implements View.OnClickListener {
        @Override
        public void onClick(View v) {
            if (ItemManager.mivry == null || item==null) {
                return;
            }
            ItemManager.mivry.DeleteAllGestureSamples(item.gesture_id);
            // Update text
            textview_recordedsamples.setText("Recorded Gesture Samples: 0");
        }
    }

    public class ButtonListenerReturn implements View.OnClickListener {
        @Override
        public void onClick(View v) {
            if (gestures_changed) {
                Intent intent = new Intent(MainActivity.me, TrainingActivity.class);
                MainActivity.me.startActivity(intent);
                gestures_changed = false;
                finish();
                return;
            }
            MainActivity.refresh();
            finish();
        }
    }


}
