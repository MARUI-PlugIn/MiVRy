package com.maruiplugin.mivrywidget;

import androidx.appcompat.app.AppCompatActivity;

import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.LinearLayout;

import java.util.List;

public class AppLaunchSelectorActivity extends AppCompatActivity {

    private static final int ITEM_BUTTON_TAG = 1;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_applaunchselector);
    }

    @Override
    protected void onResume() {
        super.onResume();
        LinearLayout items_list_layout = findViewById(R.id.applaunchselector_scrollview_layout);
        View v = items_list_layout.findViewWithTag(ITEM_BUTTON_TAG);
        while (v != null) {
            items_list_layout.removeView(v);
            v = items_list_layout.findViewWithTag(ITEM_BUTTON_TAG);
        }

        final PackageManager pm = getPackageManager();
        List<ApplicationInfo> packages = pm.getInstalledApplications(PackageManager.GET_META_DATA);

        for (ApplicationInfo pkg : packages) {
            Button item_button = new Button(this);
            final String pkg_name = pkg.packageName;
            final String app_name = pm.getApplicationLabel(pkg).toString();
            // final String uri = pm.getLaunchIntentForPackage(pkg_name);
            item_button.setText(app_name);
            item_button.setAllCaps(false);
            item_button.setWidth(300);
            // item_button.setBackground(getDrawable(R.drawable.custom_button));
            android.view.View.OnClickListener button_listener = new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    if (ItemEditorActivity.item != null && ItemManager.mivry != null) {
                        ItemEditorActivity.item.type = Item.Type.AppLaunch;
                        ItemEditorActivity.item.uri = pkg_name;
                        ItemEditorActivity.item.name = "LaunchApp: " + app_name;
                        ItemManager.mivry.SetGestureName(
                                ItemEditorActivity.item.gesture_id,
                                ItemEditorActivity.item.name + "|AppLaunch|" +pkg_name
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
    }
}
