package com.maruiplugin.mivrywidget;

import android.content.Context;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.util.TreeMap;

public class Settings {

    private static TreeMap<String, String> settings = null;
    private static String settings_file_path = null;

    private static String settingsFilePath(Context context) {
        return context.getExternalFilesDir(null).getAbsolutePath() + "/settings.dat";
    }

    private static void load(Context context) {
        settings = new TreeMap<String, String>();
        if (settings_file_path == null) {
            if (context == null) {
                return;
            }
            settings_file_path = settingsFilePath(context);
        }
        final File file = new File(settings_file_path);
        if (!file.exists()) {
            return;
        }
        try {
            FileInputStream stream = new FileInputStream(file);
            BufferedReader reader = new BufferedReader(new InputStreamReader(stream));
            String line = reader.readLine();
            while(line != null){
                String[] split = line.split("=");
                if (split.length == 2) {
                    settings.put(split[0], split[1]);
                }
                line = reader.readLine();
            }
        } catch (FileNotFoundException e) {
            //
        } catch (IOException e) {
            //
        }
    }

    private static void save(Context context) {
        if (settings == null) {
            return;
        }
        if (settings_file_path == null) {
            if (context == null) {
                return;
            }
            settings_file_path = settingsFilePath(context);
        }
        File file = new File(settings_file_path);
        try {
            if (!file.exists()) {
                file.createNewFile();
            }
            FileOutputStream stream = new FileOutputStream(file, false);
            OutputStreamWriter writer = new OutputStreamWriter(stream);
            for (TreeMap.Entry<String, String> entry : settings.entrySet()) {
                writer.write(entry.getKey() + "=" + entry.getValue() + "\n");
            }
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

    public static void set(Context context, String key, String value){
        if (settings == null) {
            load(context);
        }
        settings.put(key, value);
        save(context);
    }

    public static String get(Context context, String key) {
        if (settings == null) {
            load(context);
        }
        if (settings.containsKey(key)) {
            return settings.get(key);
        }
        return null;
    }
}
