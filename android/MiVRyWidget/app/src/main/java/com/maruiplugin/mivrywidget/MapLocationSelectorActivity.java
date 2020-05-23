package com.maruiplugin.mivrywidget;

import androidx.appcompat.app.AppCompatActivity;

import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

public class MapLocationSelectorActivity extends AppCompatActivity {

    static TextView textview_url;
    static Button button_ok;
    static ButtonListenerOK button_listener_ok;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_maplocationselector);
    }

    @Override
    protected void onResume() {
        super.onResume();

        textview_url = findViewById(R.id.maplocationselector_edittext_url);

        button_ok = findViewById(R.id.maplocationselector_button_ok);
        button_listener_ok = new ButtonListenerOK();
        button_ok.setOnClickListener(button_listener_ok);
    }

    public class ButtonListenerOK implements View.OnClickListener {
        @Override
        public void onClick(View v) {
            if (ItemEditorActivity.item==null || ItemManager.mivry==null) {
                finish();
                return;
            }
            final String url = textview_url.getText().toString();
            final String name = "Map Location : " + url;
            ItemEditorActivity.item.type = Item.Type.MapLocation;
            ItemEditorActivity.item.uri = url;
            ItemEditorActivity.item.name = name;
            ItemManager.mivry.SetGestureName(ItemEditorActivity.item.gesture_id, name+"|MapLocation|"+url);
            ItemManager.save();
            ItemEditorActivity.refresh();
            finish();
        }
    }
}
