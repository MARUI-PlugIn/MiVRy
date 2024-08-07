package com.maruiplugin.mivrywidget;

import android.content.Context;
import android.content.Intent;
import android.net.Uri;

public class Item {

    enum Type {
        None
        ,
        WebLink
        ,
        AppLaunch
        ,
        CallContact
        ,
        MapLocation
    };

    public int gesture_id;
    public String name;
    public Type type;
    public String uri;

    public Item(int gesture_id, String name, Type type, String uri) {
        this.gesture_id = gesture_id;
        this.name = name;
        this.type = type;
        this.uri = uri;
    }

    public boolean execute(Context context) {
        if (context == null) {
            return false;
        }
        if (this.uri == null) {
            return false;
        }
        switch (this.type) {
            case WebLink:
                Uri webpage = this.uri.matches("^https?://.*")
                            ? Uri.parse(this.uri)
                            : Uri.parse("http://" + this.uri); // "http://www.android.com"
                Intent web_intent = new Intent(Intent.ACTION_VIEW, webpage);
                web_intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                context.startActivity(web_intent);
                return true;
            case CallContact:
                Uri number = Uri.parse(this.uri); // "tel:5551234"
                Intent call_intent = new Intent(Intent.ACTION_DIAL, number);
                call_intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                context.startActivity(call_intent);
                return true;
            case MapLocation:
                String maps_uri = "";
                if (this.uri.matches("^geo:.*")) {
                    maps_uri = this.uri;
                } else {
                    if (this.uri.matches("^-?[0-9.]*,-?[0-9.]*$")) {
                        // map point based on latitude/longitude: "geo:37.422219,-122.08364?z=14" where z param is zoom level
                        maps_uri = "geo:" + this.uri;
                    } else { // search string: "geo:0,0?q=1600+Amphitheatre+Parkway,+Mountain+View,+California"
                        maps_uri = "geo:0,0?q=" + this.uri.replaceAll(" ", "+");
                    }
                }
                Uri location = Uri.parse(maps_uri);
                Intent map_intent = new Intent(Intent.ACTION_VIEW, location);
                map_intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                context.startActivity(map_intent);
                return true;
            case AppLaunch:
                Intent app_intent = context.getPackageManager().getLaunchIntentForPackage(this.uri); // "com.linkedin.android"
                app_intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                context.startActivity(app_intent);
                return true;
        }
        return false; // no valid type
    }
}
