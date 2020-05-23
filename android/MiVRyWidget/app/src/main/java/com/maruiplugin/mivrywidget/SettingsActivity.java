package com.maruiplugin.mivrywidget;

import androidx.appcompat.app.AppCompatActivity;

import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.Switch;
import android.widget.TextView;

public class SettingsActivity extends AppCompatActivity {

    static SettingsActivity me;
    static EditText number_gesturelen;
    static Switch switch_manualend;
    static Button button_back;
    static EditText number_trainingtime;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_settings);
    }

    @Override
    protected void onResume() {
        super.onResume();

        me = this;

        number_gesturelen = findViewById(R.id.settings_number_gesturelen);
        number_gesturelen.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (me == null || number_gesturelen == null) {
                    return;
                }
                try {
                    double d = Double.parseDouble(number_gesturelen.getText().toString());
                    Settings.set(me, "GestureLen", String.valueOf(d));
                } catch (NumberFormatException e) {
                    //
                }
                refresh();
            }
        });

        switch_manualend = findViewById(R.id.settings_switch_manualend);
        switch_manualend.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
            @Override
            public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                if (me == null) {
                    return;
                }
                Settings.set(me, "ManualGestureEnd", isChecked ? "1" : "0");
                refresh();
            }
        });

        number_trainingtime = findViewById(R.id.settings_number_trainingtime);
        number_trainingtime.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (me == null || number_trainingtime == null) {
                    return;
                }
                try {
                    int i = Integer.parseInt(number_trainingtime.getText().toString());
                    if (i > 0) {
                        Settings.set(me, "MaxTrainingTime", String.valueOf(i));
                        if (ItemManager.mivry != null) {
                            ItemManager.mivry.SetMaxTrainingTime(i);
                        }
                    }
                } catch (NumberFormatException e) {
                    //
                }
                refresh();
            }
        });

        button_back = findViewById(R.id.settings_button_back);
        button_back.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                finish();
            }
        });
        refresh();
    }

    protected static void refresh() {
        if (me == null || number_gesturelen == null || switch_manualend == null) {
            return;
        }
        String gesturelen = Settings.get(me, "GestureLen");
        if (gesturelen == null) {
            number_gesturelen.setText("1.0");
        } else {
            number_gesturelen.setText(gesturelen);
        }

        String manualend = Settings.get(me, "ManualGestureEnd");
        if (manualend == null || manualend == "0") {
            switch_manualend.setChecked(false);
            number_gesturelen.setEnabled(true);
        } else {
            switch_manualend.setChecked(true);
            number_gesturelen.setEnabled(false);
        }

        String trainingtime = Settings.get(me, "MaxTrainingTime");
        if (trainingtime != null && ItemManager.mivry != null) {
            try {
                ItemManager.mivry.SetMaxTrainingTime(Integer.parseInt(trainingtime));
            } catch (NumberFormatException e) {
                //
            }
        }
    }
}
