package com.maruiplugin.mivrywidget;

import androidx.appcompat.app.AppCompatActivity;

import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.TextView;

import com.maruiplugin.mivry.MiVRyTrainingListener;

public class TrainingActivity extends AppCompatActivity implements MiVRyTrainingListener {

    public static TextView text_title;
    public static ProgressBar progressbar;
    public static TextView text_progress;
    public static Button button_stop;
    public static Long training_start;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_training);

        text_title = findViewById(R.id.training_text_title);
        progressbar = findViewById(R.id.training_progressbar);
        progressbar.setProgress(0);
        progressbar.setMax(100);
        progressbar.incrementProgressBy(1);
        button_stop = findViewById(R.id.training_button_stop);
        android.view.View.OnClickListener button_stop_listener = new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (ItemManager.mivry != null && ItemManager.mivry.IsTraining()) {
                    ItemManager.mivry.StopTraining();
                    return;
                } // else: something went wrong
                runOnUiThread(new Runnable() { public void run() {
                    ItemManager.mivry.SetTrainingListener(null);
                    ItemManager.save();
                    MainActivity.refresh();
                    finish();
                };});
            }
        };
        button_stop.setOnClickListener(button_stop_listener);

        text_progress = findViewById(R.id.training_text_progress);

        if (ItemManager.mivry == null) {
            finish();
        }

        ItemManager.mivry.SetTrainingListener(this);
        training_start = System.currentTimeMillis();
        if (!ItemManager.mivry.StartTraining() || !ItemManager.mivry.IsTraining()) {
            finish();
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
    }


    @Override
    public void trainingUpdateCallback(double performance)
    {
        final double p = performance;
        runOnUiThread(new Runnable() { public void run() {
            // Toast.makeText(MainActivity.me, "Training... (" + p + "%)", Toast.LENGTH_SHORT).show();
            if (progressbar != null && ItemManager.mivry != null) {
                float max = (float) ItemManager.mivry.GetMaxTrainingTime() * 1000;
                float now = (float)(System.currentTimeMillis() - training_start);
                progressbar.setProgress((int)(100.0f * now / max));
            }
            if (text_progress != null) {
                text_progress.setText("" + ((int)(100.0 * p)) + "%");
            }
        };});
    }
    @Override
    public void trainingFinishCallback(double performance)
    {
        final double p = performance;
        runOnUiThread(new Runnable() { public void run() {
            ItemManager.mivry.SetTrainingListener(null);
            ItemManager.save();
            MainActivity.refresh();
            finish();
        };});
    }
}
