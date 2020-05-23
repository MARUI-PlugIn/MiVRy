package com.maruiplugin.mivrywidget;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;

public class ItemTypeSelectorActivity extends AppCompatActivity {

    Button button_weblink;
    ButtonListenerWebLink button_listener_weblink;
    Button button_applaunch;
    ButtonListenerAppLaunch button_listener_applaunch;
    Button button_callcontact;
    ButtonListenerCallContact button_listener_callcontact;
    Button button_maplocation;
    ButtonListenerMapLocation button_listener_maplocation;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_itemtypeselector);
    }

    @Override
    protected void onResume() {
        super.onResume();

        button_weblink = findViewById(R.id.itemtypeselector_button_weblink);
        button_listener_weblink = new ButtonListenerWebLink();
        button_weblink.setOnClickListener(button_listener_weblink);

        button_applaunch = findViewById(R.id.itemtypeselector_button_applaunch);
        button_listener_applaunch = new ButtonListenerAppLaunch();
        button_applaunch.setOnClickListener(button_listener_applaunch);

        button_callcontact = findViewById(R.id.itemtypeselector_button_callcontact);
        button_listener_callcontact = new ButtonListenerCallContact();
        button_callcontact.setOnClickListener(button_listener_callcontact);

        button_maplocation = findViewById(R.id.itemtypeselector_button_maplocation);
        button_listener_maplocation = new ButtonListenerMapLocation();
        button_maplocation.setOnClickListener(button_listener_maplocation);
    }

    public class ButtonListenerWebLink implements View.OnClickListener {
        @Override
        public void onClick(View v) {
            Intent intent = new Intent(ItemTypeSelectorActivity.this, WebLinkSelectorActivity.class);
            startActivity(intent);
            finish();
        }
    }

    public class ButtonListenerAppLaunch implements View.OnClickListener {
        @Override
        public void onClick(View v) {
            Intent intent = new Intent(ItemTypeSelectorActivity.this, AppLaunchSelectorActivity.class);
            startActivity(intent);
            finish();
        }
    }

    public class ButtonListenerCallContact implements View.OnClickListener {
        @Override
        public void onClick(View v) {
            Intent intent = new Intent(ItemTypeSelectorActivity.this, CallContactSelectorActivity.class);
            startActivity(intent);
            finish();
        }
    }

    public class ButtonListenerMapLocation implements View.OnClickListener {
        @Override
        public void onClick(View v) {
            Intent intent = new Intent(ItemTypeSelectorActivity.this, MapLocationSelectorActivity.class);
            startActivity(intent);
            finish();
        }
    }
}
