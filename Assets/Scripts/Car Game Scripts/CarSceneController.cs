using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Google.Maps;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class CarSceneController : MonoBehaviour
{
    public static bool canTakeControl = false;
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

    public void SelectLocation()
    {

        string address = GameObject.Find("Canvas/InputField/Text").GetComponent<Text>().text;
        string locationString = GameObject.Find("Canvas/LocationDropdownSelector/Label").GetComponent<Text>().text;
        GameObject.Find("Canvas/Panel").SetActive(false);
        GameObject.Find("Canvas/LocationDropdownSelector").SetActive(false);
        GameObject.Find("Canvas/LocationButton").SetActive(false);
        GameObject.Find("Canvas/Speedo/needle").SetActive(true);
        GameObject.Find("Canvas/Speedo/speedo").SetActive(true);
        GameObject.Find("Canvas/Speedo/needle2").SetActive(true);
        GameObject.Find("Canvas/Speedo/revs").SetActive(true);
        GameObject.Find("Canvas/MiniMap/Image").SetActive(true);
        GameObject.Find("Canvas/InputField/Text").SetActive(false);
        GameObject.Find("Canvas/InputField").SetActive(false);
        canTakeControl = true;
        string req = address.Replace(' ', '+');
        //Debug.Log(GameObject.Find("GoogleMaps").GetComponent<MapsService>().ApiKey);
        //List<IMultipartFormSection> data = new List<IMultipartFormSection>();
        //data.Add(new MultipartFormDataSection(req));
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.mapbox.com/geocoding/v5/mapbox.places/" + req + ".json?access_token=pk.eyJ1IjoibWl0emEwMDEwIiwiYSI6ImNrbmg2ZWE5ejJoeHUycGxjeTB6cXFkZWgifQ.bVjRsva6Fcuil-vUwsm9Ag");
        var resp = (HttpWebResponse)request.GetResponse();
        string response = new StreamReader(resp.GetResponseStream()).ReadToEnd();
        /*var cerere = new HttpClient();
        string response = cerere.GetStringAsync("https://api.mapbox.com/geocoding/v5/mapbox.places/" + req + ".json?access_token=pk.eyJ1IjoibWl0emEwMDEwIiwiYSI6ImNrbmg2ZWE5ejJoeHUycGxjeTB6cXFkZWgifQ.bVjRsva6Fcuil-vUwsm9Ag");
        Debug.Log(response);*/
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
        switch (locationString)
        {
            case "Grozavesti":
                GameObject.Find("GoogleMaps").GetComponent<CarMapLoader>().LoadMap(44.4526337, 26.0518842, Convert.ToDouble(secondCoord), Convert.ToDouble(firstCoord));
                break;
            case "Unirii":
                GameObject.Find("GoogleMaps").GetComponent<CarMapLoader>().LoadMap(44.426929, 26.1011807, Convert.ToDouble(secondCoord), Convert.ToDouble(firstCoord));
                break;
            case "Brasov":
                GameObject.Find("GoogleMaps").GetComponent<CarMapLoader>().LoadMap(45.6431122, 25.5858238, Convert.ToDouble(secondCoord), Convert.ToDouble(firstCoord));
                break;
            default:
                break;
        }
    }
}