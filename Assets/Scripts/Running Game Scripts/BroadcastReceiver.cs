using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BroadcastReceiver : MonoBehaviour
{
    public Text debugForPluginText;
    public GameObject pin;
    public GameObject startButton;
    public GameObject stopButton;
    public GameObject distancePanel;

    private bool shouldStart;
    private AndroidJavaClass jc;
    private string javaMessage = "";
    private bool isMapInitialized;

    private double lastLat;
    private double lastLong;
    private GameObject pinObj;
    private GameObject mainCamera;

    private double distance;
    private const int earthRadius = 6371000;
    private double deltaLat;
    private double deltaLng;
    private double a;
    private double c;

    private System.DateTime startTime;
    private System.DateTime stopTime;

    // Start is called before the first frame update
    void Start()
    {
        // Acces the android java receiver we made
        jc = new AndroidJavaClass("ro.pub.cs.systems.eim.unitylocationplugin.MyReceiver");
        // We call our java class function to create our MyReceiver java object
        jc.CallStatic("createInstance");

        isMapInitialized = false;
        shouldStart = false;
        lastLat = 0;
        lastLong = 0;
        distance = 0;
        mainCamera = GameObject.Find("Main Camera");
        //startService("ro.pub.cs.systems.eim.unitylocationplugin.PluginStarter");
    }

    // Update is called once per frame
    void Update()
    {
        if (!shouldStart)
        {
            return;
        }
        // debugForPluginText.text = "NULL BEFORE UPDATE";
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
                pinObj.transform.localScale = new Vector3(5, 50, 5);

                // Update distance
                if (lastLat != 0 && lastLong != 0)
                {
                    // Not first coordinates
                    deltaLat = (latitude - lastLat) * Mathf.PI / 180;
                    deltaLng = (longitude - lastLong) * Mathf.PI / 180;

                    a = System.Math.Sin(deltaLat / 2) * System.Math.Sin(deltaLat / 2)
                        + System.Math.Cos(lastLat * Mathf.PI / 180) * System.Math.Cos(latitude * Mathf.PI / 180)
                        * System.Math.Sin(deltaLng / 2) * System.Math.Sin(deltaLng / 2);
                    c = 2 * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1 - a));

                    distance += earthRadius * c;

                    distancePanel.GetComponentInChildren<Text>().text = ((int)distance).ToString() + " m";
                }

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

    public void startUpdate()
    {
        shouldStart = true;
        startButton.SetActive(false);
        stopButton.SetActive(true);
        startTime = System.DateTime.Now;
    }

    public void stopUpdate()
    {
        shouldStart = false;
        stopTime = System.DateTime.Now;

        PlayerPrefs.SetFloat("lastDistance", (float)distance);
        PlayerPrefs.SetFloat("totalDistance", PlayerPrefs.GetFloat("totalDistance", 0) + (float)distance);
        float avgSpeed = (float)distance / 1000 / (float)(stopTime - startTime).TotalHours;
        PlayerPrefs.SetFloat("lastAvgSpeed", avgSpeed);
        if (avgSpeed > PlayerPrefs.GetFloat("maxAvgSpeed", 0))
        {
            PlayerPrefs.SetFloat("maxAvgSpeed", avgSpeed);
        }
    }

    public void openStatistics()
    {
        SceneManager.LoadScene("RunningStatisticsScene");
    }

}
