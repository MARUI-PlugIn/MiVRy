package com.maruiplugin.mivrydemo;

import androidx.appcompat.app.AppCompatActivity;
import androidx.constraintlayout.widget.ConstraintLayout;

import android.content.Intent;
import android.os.Bundle;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import com.maruiplugin.mivry.MiVRy;

public class MainActivity extends AppCompatActivity {

    private ButtonListenerBasicGestures button_listener_basic_gestures;
    private ButtonListenerContinuousGestures button_listener_continuous_gestures;
    private ButtonListenerQuit   button_listener_quit;

    private static ConstraintLayout layout_main;
    private static Button button_basic_gestures;
    private static Button button_continuous_gestures;
    private static Button button_quit;
    private static TextView textview_buildstamp;

    private static MiVRy mivry;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        this.textview_buildstamp = findViewById(R.id.text_buildstamp);
        textview_buildstamp.setText("Version: " + mivry.BUILD_STAMP);

        layout_main = findViewById(R.id.layout_main);

        button_basic_gestures = findViewById(R.id.button_basic_gestures);
        this.button_listener_basic_gestures = new ButtonListenerBasicGestures();
        button_basic_gestures.setOnTouchListener(this.button_listener_basic_gestures);

        button_continuous_gestures = findViewById(R.id.button_continuous_gestures);
        this.button_listener_continuous_gestures = new ButtonListenerContinuousGestures();
        button_continuous_gestures.setOnTouchListener(this.button_listener_continuous_gestures);

        button_quit = findViewById(R.id.button_quit);
        this.button_listener_quit = new ButtonListenerQuit();
        button_quit.setOnTouchListener(this.button_listener_quit);
    }

    public class ButtonListenerBasicGestures implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_UP) {
                return true;
            }
            Intent i = new Intent(MainActivity.this, BasicGestures.class);
            MainActivity.this.startActivity(i);
            return true;
        }
    }

    public class ButtonListenerContinuousGestures implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_UP) {
                return true;
            }
            Intent i = new Intent(MainActivity.this, ContinuousGestures.class);
            MainActivity.this.startActivity(i);
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
            android.os.Process.killProcess(android.os.Process.myPid());
            System.exit(1);
            return true;
        }
    }
}
