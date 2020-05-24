package com.maruiplugin.mivrywidget;

import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.app.AppCompatDelegate;

import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.drawable.Drawable;
import android.net.Uri;
import android.os.Bundle;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.Toast;

import com.maruiplugin.mivry.MiVRy;
import com.maruiplugin.mivry.MiVRyTrainingListener;

import java.util.LinkedList;
import java.util.List;

public class MainActivity extends AppCompatActivity {

    public static MainActivity me = null;

    private static Button button_createnew;
    private static ButtonListenerCreateNew button_listener_createnew;
    private static Button button_settings;
    private static ButtonListenerSettings  button_listener_settings;
    private static Button button_retrain;
    private static ButtonListenerRetrain  button_listener_retrain;

    private static final int ITEM_BUTTON_TAG = 23;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        AppCompatDelegate.setCompatVectorFromResourcesEnabled(true);
        setContentView(R.layout.activity_main);

        me = this;

        button_createnew = findViewById(R.id.main_button_additem);
        this.button_listener_createnew = new ButtonListenerCreateNew();
        button_createnew.setOnTouchListener(this.button_listener_createnew);

        button_settings = findViewById(R.id.main_button_settings);
        this.button_listener_settings = new ButtonListenerSettings();
        button_settings.setOnTouchListener(this.button_listener_settings);

        button_retrain = findViewById(R.id.main_button_retrain);
        this.button_listener_retrain = new ButtonListenerRetrain();
        button_retrain.setOnTouchListener(this.button_listener_retrain);
    }

    @Override
    protected void onResume() {
        super.onResume();
        refresh();
    }

    public static void refresh() {
        LinearLayout items_list_layout = MainActivity.me.findViewById(R.id.main_itemslist_layout);
        View v = items_list_layout.findViewWithTag(ITEM_BUTTON_TAG);
        while (v != null) {
            items_list_layout.removeView(v);
            v = items_list_layout.findViewWithTag(ITEM_BUTTON_TAG);
        }
        if (!ItemManager.isInitialized()) {
            ItemManager.init(me);
        }
        ItemManager.load();

        for (int i=0; i<ItemManager.items.size(); i++) {
            Item item = ItemManager.items.get(i);
            Button item_button = new Button(MainActivity.me);
            item_button.setText(item.name);
            item_button.setAllCaps(false);
            item_button.setWidth(300);
            final int button_id = i;
            final String button_name = item.name;
            // item_button.setBackground(getDrawable(R.drawable.custom_button));
            android.view.View.OnClickListener button_listener = new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    Intent intent = new Intent(MainActivity.me, ItemEditorActivity.class);
                    Bundle b = new Bundle();
                    b.putInt("id", button_id);
                    intent.putExtras(b);
                    MainActivity.me.startActivity(intent);
                }
            };
            item_button.setOnClickListener(button_listener);
            android.view.View.OnLongClickListener button_listener_long = new View.OnLongClickListener() {
                @Override
                public boolean onLongClick(View v) {
                    AlertDialog.Builder b =new AlertDialog.Builder(MainActivity.me);
                    b.setTitle("Delete");
                    b.setMessage("Delete item '"+button_name+"'");
                    b.setIcon(android.R.drawable.ic_dialog_alert);
                    b.setPositiveButton(android.R.string.yes, new DialogInterface.OnClickListener() {
                        public void onClick(DialogInterface dialog, int whichButton) {
                            ItemManager.deleteItem(button_id);
                            Intent intent = new Intent(MainActivity.me, TrainingActivity.class);
                            MainActivity.me.startActivity(intent);
                        }});
                    b.setNegativeButton(android.R.string.no, null).show();
                    return true;
                }
            };
            item_button.setLongClickable(true);
            item_button.setOnLongClickListener(button_listener_long);
            item_button.setTag(ITEM_BUTTON_TAG);
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

    public class ButtonListenerCreateNew implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_UP) {
                return true;
            }
            int id = ItemManager.addNewItem();
            Intent intent = new Intent(MainActivity.this, ItemEditorActivity.class);
            Bundle b = new Bundle();
            b.putInt("id", id);
            intent.putExtras(b);
            startActivity(intent);
            return true;
        }
    }


    public class ButtonListenerSettings implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_UP) {
                return true;
            }
            Intent intent = new Intent(MainActivity.this, SettingsActivity.class);
            startActivity(intent);
            return true;
        }
    }

    public class ButtonListenerRetrain implements View.OnTouchListener {
        @Override
        public boolean onTouch(View v, MotionEvent event) {
            int action = event.getActionMasked();
            if (action != MotionEvent.ACTION_UP) {
                return true;
            }
            Intent intent = new Intent(MainActivity.this, TrainingActivity.class);
            startActivity(intent);
            return true;
        }
    }
}
