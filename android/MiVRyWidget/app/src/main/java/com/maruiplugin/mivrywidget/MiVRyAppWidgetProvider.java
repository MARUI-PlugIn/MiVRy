package com.maruiplugin.mivrywidget;

import android.app.PendingIntent;
import android.appwidget.AppWidgetManager;
import android.appwidget.AppWidgetProvider;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.os.Handler;
import android.widget.RemoteViews;

import com.maruiplugin.mivry.MiVRy;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;

public class MiVRyAppWidgetProvider extends AppWidgetProvider {

    private static final String WIDGET_CLICKED = "MiVRyWidgetButtonClick";
    private static boolean is_gesturing = false;
    private static Context context = null;

    @Override
    public void onUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds) {
        super.onUpdate(context, appWidgetManager, appWidgetIds);
        MiVRyAppWidgetProvider.context = context;

        Intent intent = new Intent(context, MiVRyAppWidgetProvider.class);
        intent.setAction(WIDGET_CLICKED);
        intent.putExtra(AppWidgetManager.EXTRA_APPWIDGET_IDS, appWidgetIds);
        PendingIntent pendingIntent = PendingIntent.getBroadcast(context, 0, intent, 0);

        RemoteViews views = new RemoteViews(context.getPackageName(), R.layout.widget_layout);
        views.setOnClickPendingIntent(R.id.widget_button, pendingIntent);

        appWidgetManager.updateAppWidget(appWidgetIds, views);
    }

    public static PendingIntent buildButtonPendingIntent(Context context) {
        Intent intent = new Intent(context, MiVRyAppWidgetProvider.class);
        intent.setAction(WIDGET_CLICKED);
        return PendingIntent.getBroadcast(context, 0, intent, PendingIntent.FLAG_UPDATE_CURRENT);
    }

    @Override
    public void onReceive(Context context, Intent intent) {
        super.onReceive(context, intent);
        MiVRyAppWidgetProvider.context = context;

        RemoteViews views = new RemoteViews(context.getPackageName(), R.layout.widget_layout);
        views.setOnClickPendingIntent(R.id.widget_button, buildButtonPendingIntent(context));

        if (!WIDGET_CLICKED.equals(intent.getAction())) {
            debugLog("widget received non-click interaction");
            return;
        }
        debugLog("widget click interaction received");

        if (!ItemManager.isInitialized()) {
            debugLog("initializing ItemManager");
            ItemManager.init(context);
            ItemManager.load();
        }
        // RemoteViews views = new RemoteViews(context.getPackageName(), R.layout.widget_layout);
        AppWidgetManager manager = AppWidgetManager.getInstance(context);
        ComponentName component = new ComponentName(context, MiVRyAppWidgetProvider.class);
        // views.setInt(R.id.widget_button, "setImageViewResources", R.drawable.ic_widget_active);
        // views.setImageViewResource(R.id.widget_button, R.drawable.ic_widget_active);

        String manualend_str = Settings.get(context, "ManualGestureEnd");
        boolean manualend = (manualend_str == null || manualend_str == "0") ? false : true;

        if (!is_gesturing) {
            debugLog("starting a gesture");
            is_gesturing = true;
            views.setInt(R.id.widget_button, "setImageResource", R.drawable.ic_widget_active);
            manager.updateAppWidget(component, views);
            ItemManager.mivry.StartGesture();
            if (!manualend) {
                double gesturelen = 1.0;
                try {
                    String s = Settings.get(context, "GestureLen");
                    if (s != null) {
                        gesturelen = Double.parseDouble(s);
                    }
                } finally {
                    //
                }
                final Handler handler = new Handler();
                handler.postDelayed(new Runnable() {
                    @Override
                    public void run() {
                        gestureEnd();
                    }
                }, (long)(gesturelen*1000.0));
            }
            return;
        } // else:

        gestureEnd();
    }

    private void gestureEnd() {
        debugLog("ending a gesture");
        is_gesturing = false;
        RemoteViews views = new RemoteViews(context.getPackageName(), R.layout.widget_layout);
        views.setOnClickPendingIntent(R.id.widget_button, buildButtonPendingIntent(context));
        AppWidgetManager manager = AppWidgetManager.getInstance(context);
        ComponentName component = new ComponentName(context, MiVRyAppWidgetProvider.class);
        views.setInt(R.id.widget_button, "setImageResource", R.drawable.ic_widget_foreground);
        manager.updateAppWidget(component, views);
        MiVRy.GestureRecognitionResult ret = ItemManager.mivry.EndGesture();
        if (ret.gesture_id < 0 || ret.gesture_id >= ItemManager.items.size()) {
            debugLog("failed to identify gesture");
            return;
        }
        Item item = ItemManager.items.get(ret.gesture_id);
        debugLog("identified gesture for " + item.name + " - trying to execute");
        if (item.execute(MiVRyAppWidgetProvider.context)) {
            debugLog("SUCCESS");
        } else {
            debugLog("FAILED");
        }
    }


    private static void debugLog(String msg) {
        if (MiVRyAppWidgetProvider.context == null) {
            return;
        }
        String path = context.getExternalFilesDir(null).getAbsolutePath() + "/debug.log";
        File file = new File(path);
        try {
            if (!file.exists()) {
                file.createNewFile();
            }
            FileOutputStream stream = new FileOutputStream(file, true);
            OutputStreamWriter writer = new OutputStreamWriter(stream);
            Date now = Calendar.getInstance().getTime();
            SimpleDateFormat format = new SimpleDateFormat("YYYY-MM-dd HH:mm:ss : ");
            writer.write(format.format(now) + msg + "\n");
            writer.flush();
            writer.close();
            stream.flush();
            stream.close();
        } catch (FileNotFoundException e) {
            //
        } catch (IOException e) {
            //
        }
    }
}
