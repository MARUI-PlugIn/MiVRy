package com.maruiplugin.mivrywidget;

import android.content.Context;
import android.content.Intent;
import android.widget.Toast;

import com.maruiplugin.mivry.MiVRy;

import java.util.LinkedList;
import java.util.List;

public class ItemManager {

    public static MiVRy mivry;
    public static List<Item> items;
    public static String gesture_database_file_path;

    public static boolean isInitialized() {
        return mivry != null && items != null;
    }

    public static void init(Context context) {
        mivry = new MiVRy(context);
        items = new LinkedList<Item>();
        gesture_database_file_path = context.getExternalFilesDir(null).getAbsolutePath() + "/gestures.dat";
    }

    public static boolean load() {
        if (mivry == null || items == null) {
            return false;
        }
        mivry.DeleteAllGestures();
        items.clear();
        boolean success = mivry.LoadFromFile(gesture_database_file_path);
        if (!success) {
            mivry.DeleteAllGestures();
            items.clear();
            return false;
        }
        final int num_gestures = mivry.NumberOfGestures();
        for (int g=0; g<num_gestures; g++) {
            String gesture_name = mivry.GetGestureName(g);
            String[] parts = gesture_name.split("\\|");
            if (parts.length != 3) {
                Item item = new Item(g, "FAILED TO LOAD", Item.Type.None, "");
                items.add(item);
                continue;
            }
            String name = parts[0];
            Item.Type type;
            switch (parts[1]) {
                case "AppLaunch":
                    type = Item.Type.AppLaunch;
                    break;
                case "MapLocation":
                    type = Item.Type.MapLocation;
                    break;
                case "WebLink":
                    type = Item.Type.WebLink;
                    break;
                case "CallContact":
                    type = Item.Type.CallContact;
                    break;
                default:
                    type = Item.Type.None;

            }
            if (g < items.size()) {
                Item item = items.get(g);
                item.gesture_id = g;
                item.name = name;
                item.type = type;
                item.uri = parts[2];
            } else {
                Item item = new Item(g, name, type, parts[2]);
                items.add(item);
            }
        }
        return true;
    }

    public static boolean save() {
        if (mivry == null) {
            return false;
        }
        boolean s = mivry.SaveToFile(gesture_database_file_path);
        if (MainActivity.me != null) {
            Toast.makeText(MainActivity.me, (s) ? "Gestures saved" : "ERROR! Failed to save gestures", Toast.LENGTH_LONG).show();
        }
        return s;
    }

    public static int addNewItem() {
        if (mivry == null || items == null) {
            return -1;
        }
        int id = mivry.NumberOfGestures();
        mivry.CreateGesture("");
        Item item = new Item(id, "", Item.Type.None, "");
        items.add(item);
        return id;
    }

    public static boolean deleteItem(int item) {
        if (mivry == null || items == null) {
            return false;
        }
        if (item < 0 || item >= items.size()) {
            return false;
        }
        items.remove(item);
        mivry.DeleteGesture(item);
        return true;
    }

}
