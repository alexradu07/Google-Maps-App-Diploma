using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Google.Maps;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class CarSceneController : MonoBehaviour
{
    public static bool canTakeControl = false;
    public static ResponseParse jsonResponse = null;
    public GameObject popup;
    public GameObject backbutton;
    public GameObject speedo;
    public GameObject minimap;
    // Start is called before the first frame update
    void Start()
    {
        //GameObject.Find("Canvas/Speedo/speedo").SetActive(false);
        //GameObject.Find("Canvas/Speedo/needle2").SetActive(false);
        //GameObject.Find("Canvas/Speedo/revs").SetActive(false);
        //GameObject.Find("Canvas/MiniMap/Image").SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void TogglePrompt()
    {
        popup.SetActive(true);
    }

    public void noPrompt()
    {
        popup.SetActive(false);
    }


    public async void SelectLocationAsync()
    {

        string address = GameObject.Find("Canvas/InputField/Text").GetComponent<Text>().text;
        string locationString = GameObject.Find("Canvas/LocationDropdownSelector/Label").GetComponent<Text>().text;
        backbutton.SetActive(true);
        GameObject.Find("Canvas/LocationDropdownSelector").SetActive(false);
        GameObject.Find("Canvas/LocationButton").SetActive(false);
        speedo.SetActive(true);
        minimap.SetActive(true);
        GameObject.Find("Canvas/Panel").SetActive(false);
        GameObject.Find("Canvas/InputField/Text").SetActive(false);
        GameObject.Find("Canvas/InputField").SetActive(false);
        GameObject.Find("Canvas/BackButton").SetActive(false);
        canTakeControl = true;
        string req = address.Replace(' ', '+');
        //Debug.Log(GameObject.Find("GoogleMaps").GetComponent<MapsService>().ApiKey);
        //List<IMultipartFormSection> data = new List<IMultipartFormSection>();
        //data.Add(new MultipartFormDataSection(req));
        /*HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.mapbox.com/geocoding/v5/mapbox.places/" + req + ".json?access_token=pk.eyJ1IjoibWl0emEwMDEwIiwiYSI6ImNrbmg2ZWE5ejJoeHUycGxjeTB6cXFkZWgifQ.bVjRsva6Fcuil-vUwsm9Ag");
        var resp = (HttpWebResponse)request.GetResponse();
        string response = new StreamReader(resp.GetResponseStream()).ReadToEnd();*/
        var cerere = new HttpClient();
        string response = await cerere.GetStringAsync("https://maps.googleapis.com/maps/api/directions/json?origin=" + "44.4526337" +
                ", " + "26.0518842" + "&destination=" + req + "&key=AIzaSyAx5CM56TpQzOm-yVb33upTMbhnIMKa-44");
        jsonResponse = JsonConvert.DeserializeObject<ResponseParse>(response);
        Double secondCoord = jsonResponse.routes[0].legs[0].end_location.lat;
        Double firstCoord = jsonResponse.routes[0].legs[0].end_location.lng;
        /*Debug.Log(response);
        int index = response.IndexOf("center");
        index += 9;
        string firstCoord = "";
        string secondCoord = "";
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
        */
        switch (locationString)
        {
            case "Grozavesti":
                GameObject.Find("GoogleMaps").GetComponent<CarMapLoader>().LoadMap(44.4526337, 26.0518842, secondCoord, firstCoord);
                break;
            case "Unirii":
                GameObject.Find("GoogleMaps").GetComponent<CarMapLoader>().LoadMap(44.426929, 26.1011807, secondCoord, firstCoord);
                break;
            case "Brasov":
                GameObject.Find("GoogleMaps").GetComponent<CarMapLoader>().LoadMap(45.6431122, 25.5858238, secondCoord, firstCoord);
                break;
            default:
                break;
        }
    }
}