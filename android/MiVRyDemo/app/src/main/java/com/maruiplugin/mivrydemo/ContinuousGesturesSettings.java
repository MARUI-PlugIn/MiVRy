package com.maruiplugin.mivrydemo;

import androidx.appcompat.app.AppCompatActivity;

import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.SeekBar;
import android.widget.TextView;

public class ContinuousGesturesSettings extends AppCompatActivity {

    private ButtonListenerReturn button_listener_return;
    private SeekbarListenerSmoothingPositional seekbar_listener_smoothing_positional;
    private SeekbarListenerSmoothingContinuous seekbar_listener_smoothing_continuous;
    private TextviewListenerGesturePeriod textview_listener_gesture_period;
    private TextviewListenerRecognitionInterval textview_listener_recognition_interval;

    private static Button   button_return;
    private static TextView textview_sensorinfo;
    private static SeekBar  seekbar_smoothing_positional;
    private static SeekBar  seekbar_smoothing_continuous;
    private static TextView number_gestureperiod;
    private static TextView number_recognitioninterval;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.continuous_gestures_settings);

        this.textview_sensorinfo = findViewById(R.id.text_sensorinfo);
        textview_sensorinfo.setText(ContinuousGestures.mivry.GetActiveSensorTypes());

        button_return = findViewById(R.id.button_return);
        this.button_listener_return = new ButtonListenerReturn();
        button_return.setOnTouchListener(this.button_listener_return);

        seekbar_smoothing_positional = findViewById(R.id.seekbar_smoothing_positional);
        this.seekbar_listener_smoothing_positional = new SeekbarListenerSmoothingPositional();
        seekbar_smoothing_positional.setOnSeekBarChangeListener(this.seekbar_listener_smoothing_positional);
        seekbar_smoothing_positional.setProgress((int)(ContinuousGestures.mivry.GetPositionalSmoothingFactor() * 100.0f));

        seekbar_smoothing_continuous = findViewById(R.id.seekbar_smoothing_continuous);
        this.seekbar_listener_smoothing_continuous = new SeekbarListenerSmoothingContinuous();
        seekbar_smoothing_continuous.setOnSeekBarChangeListener(this.seekbar_listener_smoothing_continuous);
        seekbar_smoothing_continuous.setProgress((int)ContinuousGestures.mivry.GetContinuousIdentificationSmoothing());

        number_gestureperiod = findViewById(R.id.number_gestureperiod);
        textview_listener_gesture_period = new TextviewListenerGesturePeriod();
        number_gestureperiod.setOnEditorActionListener(textview_listener_gesture_period);
        number_gestureperiod.setText("" + (ContinuousGestures.mivry.GetContinuousIdentificationPeriod() * 0.001));

        number_recognitioninterval = findViewById(R.id.number_recognitioninterval);
        textview_listener_recognition_interval = new TextviewListenerRecognitionInterval();
        number_recognitioninterval.setOnEditorActionListener(textview_listener_recognition_interval);
        number_recognitioninterval.setText("" + (ContinuousGestures.recognition_interval * 0.001));
    }


    public class SeekbarListenerSmoothingPositional implements SeekBar.OnSeekBarChangeListener {
        @Override
        public void onStartTrackingTouch(SeekBar seekBar) {}

        @Override
        public void onStopTrackingTouch(SeekBar seekBar) {}

        @Override
        public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
            ContinuousGestures.mivry.SetPositionalSmoothingFactor(((float)progress) / 100.0f);
        }
    }

    public class SeekbarListenerSmoothingContinuous implements SeekBar.OnSeekBarChangeListener {
        @Override
        public void onStartTrackingTouch(SeekBar seekBar) {}

        @Override
        public void onStopTrackingTouch(SeekBar seekBar) {}

        @Override
        public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
            ContinuousGestures.mivry.SetContinuousIdentificationSmoothing(progress);
        }
    }

    public class TextviewListenerGesturePeriod implements TextView.OnEditorActionListener {
        @Override
        public boolean onEditorAction(TextView v, int actionId, KeyEvent event) {
            ContinuousGestures.mivry.SetContinuousIdentificationPeriod((long)(Double.parseDouble(v.getText().toString()) * 1000.0));
            return false;
        }
    }

    public class TextviewListenerRecognitionInterval implements TextView.OnEditorActionListener {
        @Override
        public boolean onEditorAction(TextView v, int actionId, KeyEvent event) {
            ContinuousGestures.recognition_interval = (long)(Double.parseDouble(v.getText().toString()) * 1000.0);
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
