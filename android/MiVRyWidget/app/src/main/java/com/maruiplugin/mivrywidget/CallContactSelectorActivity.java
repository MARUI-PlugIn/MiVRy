package com.maruiplugin.mivrywidget;

import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import android.Manifest;
import android.content.ContentResolver;
import android.content.pm.PackageManager;
import android.database.Cursor;
import android.os.Bundle;
import android.provider.ContactsContract;
import android.view.View;
import android.widget.Button;
import android.widget.LinearLayout;

public class CallContactSelectorActivity extends AppCompatActivity {

    private static final int ITEM_BUTTON_TAG = 1;
    public static final int PERMISSIONS_REQUEST_READ_CONTACTS = 23;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_callcontactselector);
    }

    @Override
    protected void onResume() {
        super.onResume();
        LinearLayout items_list_layout = findViewById(R.id.callcontactselector_scrollview_layout);
        View v = items_list_layout.findViewWithTag(ITEM_BUTTON_TAG);
        while (v != null) {
            items_list_layout.removeView(v);
            v = items_list_layout.findViewWithTag(ITEM_BUTTON_TAG);
        }

        if (ContextCompat.checkSelfPermission(this, Manifest.permission.READ_CONTACTS) != PackageManager.PERMISSION_GRANTED) {
            // Permission is not granted
            ActivityCompat.requestPermissions(this, new String[]{Manifest.permission.READ_CONTACTS}, PERMISSIONS_REQUEST_READ_CONTACTS);
        }
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.READ_CONTACTS) != PackageManager.PERMISSION_GRANTED) {
            return;
        }


        ContentResolver cr = getContentResolver();
        Cursor cur = cr.query(ContactsContract.Contacts.CONTENT_URI,null, null, null, null);

        if ((cur != null ? cur.getCount() : 0) > 0) {
            while (cur != null && cur.moveToNext()) {
                String id = cur.getString(cur.getColumnIndex(ContactsContract.Contacts._ID));
                final String name = cur.getString(cur.getColumnIndex(ContactsContract.Contacts.DISPLAY_NAME));

                if (cur.getInt(cur.getColumnIndex(ContactsContract.Contacts.HAS_PHONE_NUMBER)) > 0) {
                    Cursor pCur = cr.query(
                            ContactsContract.CommonDataKinds.Phone.CONTENT_URI,
                            null,
                            ContactsContract.CommonDataKinds.Phone.CONTACT_ID + " = ?",
                            new String[]{id}, null
                    );
                    while (pCur.moveToNext()) {
                        String phoneNo = pCur.getString(pCur.getColumnIndex(ContactsContract.CommonDataKinds.Phone.NUMBER));

                        Button item_button = new Button(this);
                        item_button.setText(name + " (" + phoneNo + ")");
                        item_button.setAllCaps(false);
                        item_button.setWidth(300);
                        final String number = phoneNo;
                        android.view.View.OnClickListener button_listener = new View.OnClickListener() {
                            @Override
                            public void onClick(View v) {
                                if (ItemEditorActivity.item != null && ItemManager.mivry != null) {
                                    ItemEditorActivity.item.type = Item.Type.AppLaunch;
                                    ItemEditorActivity.item.uri = number;
                                    ItemEditorActivity.item.name = "Call: " + name;
                                    ItemManager.mivry.SetGestureName(
                                            ItemEditorActivity.item.gesture_id,
                                            ItemEditorActivity.item.name + "|CallContact|tel:" +number
                                    );
                                    ItemManager.save();
                                    ItemEditorActivity.refresh();
                                }
                                finish();
                            }
                        };
                        item_button.setOnClickListener(button_listener);
                        item_button.setBackgroundResource(R.drawable.target_button);
                        LinearLayout.LayoutParams params = (LinearLayout.LayoutParams)item_button.getLayoutParams();
                        if (params == null) {
                            params = new LinearLayout.LayoutParams(-1, -1);
                        }
                        params.topMargin = 10;
                        params.leftMargin = 10;
                        params.rightMargin = 10;
                        item_button.setLayoutParams(params);
                        items_list_layout.addView(item_button);
                    }
                    pCur.close();
                }
            }
        }
        if(cur!=null){
            cur.close();
        }
    }
}
