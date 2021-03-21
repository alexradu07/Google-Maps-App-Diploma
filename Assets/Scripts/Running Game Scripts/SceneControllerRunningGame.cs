using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Android;

public class SceneControllerRunningGame : MonoBehaviour
{
    public GameObject locationError;
    public GameObject loadingMessage;

    // Start is called before the first frame update
    void Start()
    {

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
        string locationString = GameObject.Find("Canvas/LocationDropdownSelector/Label").GetComponent<Text>().text;
        switch (locationString)
        {
            case "Grozavesti":
                GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().LoadMap(44.4433837, 26.0618934);
                GameObject.Find("Canvas/Panel").SetActive(false);
                GameObject.Find("Canvas/LocationDropdownSelector").SetActive(false);
                GameObject.Find("Canvas/DifficultyDropdownSelector").SetActive(false);
                GameObject.Find("Canvas/TimeDropdownSelector").SetActive(false);
                GameObject.Find("Canvas/LocationText").SetActive(false);
                GameObject.Find("Canvas/DifficultyText").SetActive(false);
                GameObject.Find("Canvas/TimeText").SetActive(false);
                GameObject.Find("Canvas/LocationButton").SetActive(false);
                break;
            case "Unirii":
                GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().LoadMap(44.426929, 26.1011807);
                GameObject.Find("Canvas/Panel").SetActive(false);
                GameObject.Find("Canvas/LocationDropdownSelector").SetActive(false);
                GameObject.Find("Canvas/DifficultyDropdownSelector").SetActive(false);
                GameObject.Find("Canvas/TimeDropdownSelector").SetActive(false);
                GameObject.Find("Canvas/LocationText").SetActive(false);
                GameObject.Find("Canvas/DifficultyText").SetActive(false);
                GameObject.Find("Canvas/TimeText").SetActive(false);
                GameObject.Find("Canvas/LocationButton").SetActive(false);
                break;
            case "Brasov":
                GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().LoadMap(45.6431122, 25.5858238);
                GameObject.Find("Canvas/Panel").SetActive(false);
                GameObject.Find("Canvas/LocationDropdownSelector").SetActive(false);
                GameObject.Find("Canvas/DifficultyDropdownSelector").SetActive(false);
                GameObject.Find("Canvas/TimeDropdownSelector").SetActive(false);
                GameObject.Find("Canvas/LocationText").SetActive(false);
                GameObject.Find("Canvas/DifficultyText").SetActive(false);
                GameObject.Find("Canvas/TimeText").SetActive(false);
                GameObject.Find("Canvas/LocationButton").SetActive(false);
                break;
            case "Current location":
                if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
                {
                    Permission.RequestUserPermission(Permission.CoarseLocation);
                }
                StartCoroutine(StartLocation());
                break;
            default:
                break;
        }
    }

    public void SelectDifficulty()
    {
        string difficulty = GameObject.Find("Canvas/DifficultyDropdownSelector/Label").GetComponent<Text>().text;
        GameObject.Find("RoadController").GetComponent<RoadScript>().difficulty = difficulty;
    }

    public void SelectTime()
    {
        string time = GameObject.Find("Canvas/TimeDropdownSelector/Label").GetComponent<Text>().text;
        GameObject.Find("RoadController").GetComponent<TimerScript>().timeLeft = int.Parse(time);
    }

    IEnumerator DisplayLocationError()
    {
        locationError.SetActive(true);
        yield return new WaitForSeconds(5);
        locationError.SetActive(false);
    }

    IEnumerator DisplayLoadingMessage()
    {
        loadingMessage.SetActive(true);
        yield return new WaitForSeconds(2);
        loadingMessage.SetActive(false);
    }

    IEnumerator StartLocation()
    {
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
        GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().LoadMap(Input.location.lastData.latitude, Input.location.lastData.longitude);
        GameObject.Find("Canvas/Panel").SetActive(false);
        GameObject.Find("Canvas/LocationDropdownSelector").SetActive(false);
        GameObject.Find("Canvas/DifficultyDropdownSelector").SetActive(false);
        GameObject.Find("Canvas/TimeDropdownSelector").SetActive(false);
        GameObject.Find("Canvas/LocationText").SetActive(false);
        GameObject.Find("Canvas/DifficultyText").SetActive(false);
        GameObject.Find("Canvas/TimeText").SetActive(false);
        GameObject.Find("Canvas/LocationButton").SetActive(false);
        Input.location.Stop();
    }
}
