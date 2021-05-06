using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BroadcastReceiver : MonoBehaviour
{
    public Text debugForPluginText;
    private AndroidJavaClass jc;
    private string javaMessage = "";

    // Start is called before the first frame update
    void Start()
    {
        debugForPluginText.text = "NULL BEFORE START";
        // Acces the android java receiver we made
        jc = new AndroidJavaClass("ro.pub.cs.systems.eim.unitylocationplugin.MyReceiver");
        // We call our java class function to create our MyReceiver java object
        jc.CallStatic("createInstance");
        debugForPluginText.text = "NULL AFTER START";

        //startService("ro.pub.cs.systems.eim.unitylocationplugin.PluginStarter");
    }

    // Update is called once per frame
    void Update()
    {
        debugForPluginText.text = "NULL BEFORE UPDATE";
        // We get the text property of our receiver
        //javaMessage = jc.GetStatic<string>("text");

        double latitude = jc.GetStatic<double>("latitude");
        double longitude = jc.GetStatic<double>("longitude"); ;
        /*
        if (javaMessage == null)
        {
            debugForPluginText.text = "NULL";
        } else
        {
            debugForPluginText.text = latitude + " " + longitude;
        }*/
        if (latitude != 0 && longitude != 0)
        {
            debugForPluginText.text = latitude + " " + longitude;
        }
    }

    /*void startService(string packageName)
    {
        AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass customClass = new AndroidJavaClass(packageName);
        customClass.CallStatic("StartCheckerService", unityActivity);
    }*/
}
