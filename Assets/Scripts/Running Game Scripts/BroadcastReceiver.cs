using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BroadcastReceiver : MonoBehaviour
{
    public Text debugForPluginText;
    public GameObject pin;

    private AndroidJavaClass jc;
    private string javaMessage = "";
    private bool isMapInitialized;

    private double lastLat;
    private double lastLong;
    private GameObject pinObj;
    private GameObject mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        debugForPluginText.text = "NULL BEFORE START";
        // Acces the android java receiver we made
        jc = new AndroidJavaClass("ro.pub.cs.systems.eim.unitylocationplugin.MyReceiver");
        // We call our java class function to create our MyReceiver java object
        jc.CallStatic("createInstance");
        debugForPluginText.text = "NULL AFTER START";

        isMapInitialized = false;
        lastLat = 0;
        lastLong = 0;
        mainCamera = GameObject.Find("Main Camera");
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

            if (!isMapInitialized)
            {
                isMapInitialized = true;
                GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().LoadMap(latitude, longitude);
                //GameObject.Find("Canvas/Panel").SetActive(false);
            }

            if (latitude != lastLat && longitude != lastLong)
            {
                pinObj = Object.Instantiate(pin,
                    GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().mapsService.Coords.FromLatLngToVector3(new Google.Maps.Coord.LatLng(latitude, longitude)),
                    Quaternion.Euler(0, 0, 0));
                pinObj.transform.localScale = new Vector3(5, 1, 5);
            }

            lastLat = latitude;
            lastLong = longitude;
        }

        if (pinObj != null)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position,
                new Vector3(pinObj.transform.position.x, mainCamera.transform.position.y, pinObj.transform.position.z),
                0.1f);
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
