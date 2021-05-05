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



public class OutDoorCarSceneController : MonoBehaviour
{
    public static bool canTakeControl = false;
    public GameObject locationError;
    public GameObject loadingMessage;
    private String firstCoord;
    private String secondCoord;
    private String response = "";
    private string req;
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

    public async void SelectLocation()
    {

        string address = GameObject.Find("Canvas/InputField/Text").GetComponent<Text>().text;
        GameObject.Find("Canvas/Panel").SetActive(false);
        GameObject.Find("Canvas/LocationButton").SetActive(false);
        GameObject.Find("Canvas/Speedo/needle").SetActive(true);
        GameObject.Find("Canvas/Speedo/speedo").SetActive(true);
        GameObject.Find("Canvas/Speedo/needle2").SetActive(true);
        GameObject.Find("Canvas/Speedo/revs").SetActive(true);
        GameObject.Find("Canvas/MiniMap/Image").SetActive(true);
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
        await Func();
        Debug.Log(response);
        int index = response.IndexOf("center");
        index += 9;
        firstCoord = "";
        secondCoord = "";
        bool finishedFirst = false;
        while (response[index].CompareTo(']') != 0)
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
        firstCoord = firstCoord.Replace('.', ',');
        secondCoord = secondCoord.Replace('.', ',');
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
        Input.location.Start();
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
            Manager.dynamicLatitude = Input.location.lastData.latitude;
            Manager.dynamicLongitude = Input.location.lastData.longitude;
            GameObject.Find("GoogleMaps").GetComponent<OutdoorCarMapLoader>().LoadMap(Input.location.lastData.latitude, Input.location.lastData.longitude, Convert.ToDouble(secondCoord), Convert.ToDouble(firstCoord));
            Manager.locationQueryComplete = true;
        }

        //Input.location.Stop();
        Manager.gameStarted = true;
    }
}
