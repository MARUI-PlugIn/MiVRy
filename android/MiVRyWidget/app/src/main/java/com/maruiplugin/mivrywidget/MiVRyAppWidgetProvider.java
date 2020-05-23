package com.maruiplugin.mivrywidget;

import android.app.PendingIntent;
import android.appwidget.AppWidgetManager;
import android.appwidget.AppWidgetProvider;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import android.widget.RemoteViews;

import com.maruiplugin.mivry.MiVRy;

public class MiVRyAppWidgetProvider extends AppWidgetProvider {

    private static final String WIDGET_CLICKED = "MiVRyWidgetButtonClick";
    private static boolean is_gesturing = false;
    private static MiVRyAppWidgetProvider me = null;
    private static Context context = null;

    @Override
    public void onUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds) {
        super.onUpdate(context, appWidgetManager, appWidgetIds);
        // This is called to update the App Widget at intervals defined by the updatePeriodMillis attribute in the AppWidgetProviderInfo.
        RemoteViews remoteViews;
        ComponentName watchWidget;

        remoteViews = new RemoteViews(context.getPackageName(), R.layout.widget_layout);
        watchWidget = new ComponentName(context, MiVRyAppWidgetProvider.class);

        Intent intent = new Intent(context, getClass());
        intent.setAction(WIDGET_CLICKED);
        PendingIntent pendingIntent = PendingIntent.getBroadcast(context, 0, intent, 0);

        remoteViews.setOnClickPendingIntent(R.id.widget_button, pendingIntent);
        appWidgetManager.updateAppWidget(watchWidget, remoteViews);

        MiVRyAppWidgetProvider.me = this;
        MiVRyAppWidgetProvider.context = context;
    }

    @Override
    public void onReceive(Context context, Intent intent) {
        super.onReceive(context, intent);

        if (!WIDGET_CLICKED.equals(intent.getAction())) {
            return;
        }
        MiVRyAppWidgetProvider.context = context;

        if (!ItemManager.isInitialized()) {
            ItemManager.init(context);
            ItemManager.load();
        }
        RemoteViews views = new RemoteViews(context.getPackageName(), R.layout.widget_layout);
        AppWidgetManager manager = AppWidgetManager.getInstance(context);
        ComponentName component = new ComponentName(context, MiVRyAppWidgetProvider.class);
        // views.setInt(R.id.widget_button, "setImageViewResources", R.drawable.ic_widget_active);
        // views.setImageViewResource(R.id.widget_button, R.drawable.ic_widget_active);

        String manualend_str = Settings.get(context, "ManualGestureEnd");
        boolean manualend = (manualend_str == null || manualend_str == "0") ? false : true;


        if (!is_gesturing) {
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

        if (manualend) {
            gestureEnd();
        }
    }

    private void gestureEnd() {
        is_gesturing = false;
        RemoteViews views = new RemoteViews(context.getPackageName(), R.layout.widget_layout);
        AppWidgetManager manager = AppWidgetManager.getInstance(context);
        ComponentName component = new ComponentName(context, MiVRyAppWidgetProvider.class);
        views.setInt(R.id.widget_button, "setImageResource", R.drawable.ic_widget_foreground);
        manager.updateAppWidget(component, views);
        MiVRy.GestureRecognitionResult ret = ItemManager.mivry.EndGesture();
        if (ret.gesture_id < 0 || ret.gesture_id >= ItemManager.items.size()) {
            return;
        }
        Item item = ItemManager.items.get(ret.gesture_id);
        item.execute();
    }
}
