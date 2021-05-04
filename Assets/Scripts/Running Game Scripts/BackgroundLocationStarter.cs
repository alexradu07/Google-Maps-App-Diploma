using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundLocationStarter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        startService("ro.pub.cs.systems.eim.unitylocationplugin.PluginStarter");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void startService(string packageName)
    {
        AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass customClass = new AndroidJavaClass(packageName);
        customClass.CallStatic("StartCheckerService", unityActivity);
    }
}
