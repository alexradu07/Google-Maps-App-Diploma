using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneControllerRunningGame : MonoBehaviour
{
    public GameObject locationError;

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
                break;
            case "Unirii":
                GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().LoadMap(44.426929, 26.1011807);
                break;
            case "Brasov":
                GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().LoadMap(45.6431122, 25.5858238);
                break;
            case "Current location":
                StartCoroutine(StartLocation());
                
                break;
            default:
                break;
        }

        GameObject.Find("Canvas/Panel").SetActive(false);
        GameObject.Find("Canvas/LocationDropdownSelector").SetActive(false);
        GameObject.Find("Canvas/LocationButton").SetActive(false);
    }

    IEnumerator DisplayLocationError()
    {
        locationError.SetActive(true);
        yield return new WaitForSeconds(5);
        locationError.SetActive(false);
    }

    IEnumerator StartLocation()
    {
        yield return new WaitForSeconds(5);
        if (!Input.location.isEnabledByUser)
        {
            StartCoroutine(DisplayLocationError());
            yield break;
        }
        Input.location.Start();
        yield return new WaitForSeconds(5);
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
        Input.location.Stop();
    }
}
