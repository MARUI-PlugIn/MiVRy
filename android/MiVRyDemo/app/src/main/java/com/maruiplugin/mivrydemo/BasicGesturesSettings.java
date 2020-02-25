package com.maruiplugin.mivrydemo;

import androidx.appcompat.app.AppCompatActivity;

import android.os.Bundle;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.SeekBar;
import android.widget.TextView;

public class BasicGesturesSettings extends AppCompatActivity {

    private ButtonListenerReturn button_listener_return;
    private SeekbarListenerSmoothingPositional seekbar_listener_smoothing_positional;
    private TextviewListenerTrainingTime        textview_listener_training_time;

    private static Button button_return;
    private static TextView textview_sensorinfo;
    private static SeekBar seekbar_smoothing_positional;
    private static TextView number_trainingtime;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.basic_gestures_settings);
    }


    @Override
    protected void onResume() {
        super.onResume();
        this.textview_sensorinfo = findViewById(R.id.text_sensorinfo);
        textview_sensorinfo.setText(BasicGestures.mivry.GetActiveSensorTypes());

        button_return = findViewById(R.id.button_return);
        this.button_listener_return = new ButtonListenerReturn();
        button_return.setOnTouchListener(this.button_listener_return);

        seekbar_smoothing_positional = findViewById(R.id.seekbar_smoothing_positional);
        this.seekbar_listener_smoothing_positional = new SeekbarListenerSmoothingPositional();
        seekbar_smoothing_positional.setOnSeekBarChangeListener(this.seekbar_listener_smoothing_positional);
        seekbar_smoothing_positional.setProgress((int)(BasicGestures.mivry.GetPositionalSmoothingFactor() * 100.0f));

        number_trainingtime = findViewById(R.id.number_trainingtime);
        textview_listener_training_time = new TextviewListenerTrainingTime();
        number_trainingtime.setOnEditorActionListener(textview_listener_training_time);
        number_trainingtime.setText("" + BasicGestures.mivry.GetMaxTrainingTime());
    }

    public class SeekbarListenerSmoothingPositional implements SeekBar.OnSeekBarChangeListener {
        @Override
        public void onStartTrackingTouch(SeekBar seekBar) {}

        @Override
        public void onStopTrackingTouch(SeekBar seekBar) {}

        @Override
        public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
            BasicGestures.mivry.SetPositionalSmoothingFactor(((float)progress) / 100.0f);
        }
    }

    public class TextviewListenerTrainingTime implements TextView.OnEditorActionListener {
        @Override
        public boolean onEditorAction(TextView v, int actionId, KeyEvent event) {
            BasicGestures.mivry.SetMaxTrainingTime((int)(Double.parseDouble(v.getText().toString())));
            return false;
        }
    }

    public class ButtonListenerReturn implements View.OnTouchListener {
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
}
