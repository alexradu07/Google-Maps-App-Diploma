using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Collections.Generic;

public class OutDoorCarSceneController : MonoBehaviour
{
    public static bool canTakeControl = false;
    public GameObject locationError;
    public GameObject loadingMessage;
    private String firstCoord;
    private String secondCoord;
    private String response = "";
    private string req;
    public static ResponseParse jsonResponse = null;
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("Canvas/Speedo/needle").SetActive(false);
        GameObject.Find("Canvas/Speedo/speedo").SetActive(false);
        GameObject.Find("Canvas/Speedo/needle2").SetActive(false);
        GameObject.Find("Canvas/Speedo/revs").SetActive(false);
        GameObject.Find("Canvas/MiniMap/Image").SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public async Task Func()
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetStringAsync(String.Format("https://api.mapbox.com/geocoding/v5/mapbox.places/" + req + ".json?access_token=pk.eyJ1IjoibWl0emEwMDEwIiwiYSI6ImNrbmg2ZWE5ejJoeHUycGxjeTB6cXFkZWgifQ.bVjRsva6Fcuil-vUwsm9Ag"));
            this.response = response;
        }
    }

    public async Task Func2()
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetStringAsync(String.Format("https://api.mapbox.com/geocoding/v5/mapbox.places/" + req + ".json?access_token=pk.eyJ1IjoibWl0emEwMDEwIiwiYSI6ImNrbmg2ZWE5ejJoeHUycGxjeTB6cXFkZWgifQ.bVjRsva6Fcuil-vUwsm9Ag"));
            this.response = response;
        }
    }

    public async void SelectLocation()
    {

        string address = GameObject.Find("Canvas/InputField/Text").GetComponent<Text>().text;
        GameObject.Find("Canvas/Panel").SetActive(false);
        GameObject.Find("Canvas/LocationButton").SetActive(false);
        GameObject.Find("Canvas/InputField/Text").SetActive(false);
        GameObject.Find("Canvas/InputField").SetActive(false);
        canTakeControl = true;
        req = address.Replace(' ', '+');
        //Debug.Log(GameObject.Find("GoogleMaps").GetComponent<MapsService>().ApiKey);
        //List<IMultipartFormSection> data = new List<IMultipartFormSection>();
        //data.Add(new MultipartFormDataSection(req));
        /*HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.mapbox.com/geocoding/v5/mapbox.places/" + req + ".json?access_token=pk.eyJ1IjoibWl0emEwMDEwIiwiYSI6ImNrbmg2ZWE5ejJoeHUycGxjeTB6cXFkZWgifQ.bVjRsva6Fcuil-vUwsm9Ag");
        var resp = (HttpWebResponse)request.GetResponse();
        string response = new StreamReader(resp.GetResponseStream()).ReadToEnd();*/
        //await Func();
        Debug.Log(response);
        int index = response.IndexOf("center");
        index += 9;
        firstCoord = "";
        secondCoord = "";
        bool finishedFirst = false;
        /*while (response[index].CompareTo(']') != 0)
        {
            if (response[index].CompareTo(',') == 0)
            {
                finishedFirst = true;
                index++;
                continue;
            }
            if (!finishedFirst)
            {
                firstCoord += response[index];
            }
            else
            {
                secondCoord += response[index];
            }
            index++;
        }
        Debug.Log("afisez firstcoord" + firstCoord);
        Debug.Log("afisez secondcoord" + secondCoord);
        */
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        {
            Permission.RequestUserPermission(Permission.CoarseLocation);
        }
        StartCoroutine(GetCurrentLocation());
        /*switch (locationString)
        {
            case "Grozavesti":
                GameObject.Find("GoogleMaps").GetComponent<OutdoorCarMapLoader>().LoadMap(44.4526337, 26.0518842, Convert.ToDouble(secondCoord), Convert.ToDouble(firstCoord));
                break;
            case "Unirii":
                GameObject.Find("GoogleMaps").GetComponent<OutdoorCarMapLoader>().LoadMap(44.426929, 26.1011807, Convert.ToDouble(secondCoord), Convert.ToDouble(firstCoord));
                break;
            case "Brasov":
                GameObject.Find("GoogleMaps").GetComponent<OutdoorCarMapLoader>().LoadMap(45.6431122, 25.5858238, Convert.ToDouble(secondCoord), Convert.ToDouble(firstCoord));
                break;
            default:
                break;
        }*/
    }
    IEnumerator DisplayLocationError()
    {
        //Debug.Log(GetCurrentMethod());
        locationError.SetActive(true);
        yield return new WaitForSeconds(1);
        locationError.SetActive(false);
    }

    IEnumerator DisplayLoadingMessage()
    {
        //Debug.Log(GetCurrentMethod());
        loadingMessage.SetActive(true);
        yield return new WaitForSeconds(2);
        loadingMessage.SetActive(false);
    }

    IEnumerator GetCurrentLocation()
    {
        //Debug.Log(GetCurrentMethod());
        StartCoroutine(DisplayLoadingMessage());
        yield return new WaitForSeconds(1);
        if (!Input.location.isEnabledByUser)
        {
            StartCoroutine(DisplayLocationError());
            yield break;
        }
        Input.location.Start(5, 1);
        yield return new WaitForSeconds(1);
        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            Debug.Log("Location is initializing");
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Location initialization failed");
            yield break;
        }
        else
        {
            Debug.Log("Latitude\t: " + Input.location.lastData.latitude);
            Debug.Log("Longitude\t: " + Input.location.lastData.longitude);
            Manager.dynamicLatitude = (double)Input.location.lastData.latitude;
            Manager.dynamicLongitude = (double)Input.location.lastData.longitude;
            Debug.Log("https://maps.googleapis.com/maps/api/directions/json?origin=" + Input.location.lastData.latitude.ToString().Replace(",", ".") +
                ", " + Input.location.lastData.longitude.ToString().Replace(",", ".") + "&destination=" + req + "&key=AIzaSyAx5CM56TpQzOm-yVb33upTMbhnIMKa-44");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://maps.googleapis.com/maps/api/directions/json?origin=" + Input.location.lastData.latitude.ToString().Replace(",",".") +
                ", " + Input.location.lastData.longitude.ToString().Replace(",", ".") + "&destination=" + req + "&key=AIzaSyAx5CM56TpQzOm-yVb33upTMbhnIMKa-44");
            var resp = (HttpWebResponse)request.GetResponse();
            string response = new StreamReader(resp.GetResponseStream()).ReadToEnd();
            Debug.Log(response);
            jsonResponse = JsonConvert.DeserializeObject<ResponseParse>(response);
            GameObject.Find("GoogleMaps").GetComponent<OutdoorCarMapLoader>().LoadMap(Input.location.lastData.latitude, Input.location.lastData.longitude);
            Manager.locationQueryComplete = true;
        }

        //Input.location.Stop();
        Manager.gameStarted = true;
    }
}

public class distance
{
    public String text { get; set; }
    public String value { get; set; }
}

public class duration
{
    public String text { get; set; }
    public String value { get; set; }
}

public class lat_long
{
    public double lat { get; set; }
    public double lng { get; set; }
}

public class polyline
{
    public String points { get; set; }
}

public class geocoded_waypoints
{
    public String status { get; set; }
    public String place_id { get; set; }
    public List<String> types { get; set; }
}


public class steps
{
    public distance distance { get; set; }
    public duration duration { get; set; }
    public lat_long end_location { get; set; }
    public String html_instructions { get; set; }
    public polyline polyline { get; set; }
    public lat_long start_location { get; set; }
    public String travel_mode { get; set; }
}

public class legs
{
    public distance distance { get; set; }
    public duration duration { get; set; }
    public String end_address { get; set; }
    public lat_long end_location { get; set; }
    public String start_address { get; set; }
    public lat_long start_location { get; set; }
    public List<steps> steps { get; set; }
}

public class bounds
{
    public lat_long northeast { get; set; }
    public lat_long southwest { get; set; }
}
public class routes
{
    public bounds bounds { get; set; }
    public String copyrights { get; set; }
    public List<legs> legs { get; set; }
    public polyline overview_polyline { get; set; }
    public String summary { get; set; }
    public List<String> warnings { get; set; }
    public List<String> waypoint_order { get; set; }
}

public class ResponseParse
{
    public List<geocoded_waypoints> geocoded_waypoints { get; set; }
    public List<routes> routes { get; set; }
    public String status { get; set; }
}
