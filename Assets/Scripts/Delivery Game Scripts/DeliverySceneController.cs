using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Android;

public class DeliverySceneController : MonoBehaviour
{
    public GameObject locationError;
    public GameObject loadingMessage;
    public GameObject minimap;
    public GameObject tukTukStatusDialog;
    public GameObject backButton;
    public GameObject leftButtonSelectVehicle;
    public GameObject rightButtonSelectVehicle;

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
        string locationString = GameObject.Find("Canvas/Panel/LocationDropdownSelector/Label").GetComponent<Text>().text;
        switch (locationString)
        {
            case "Grozavesti":
                GameObject.Find("GoogleMaps").GetComponent<DeliveryMapLoader>().LoadMap(44.4433837, 26.0618934);
                break;
            case "Unirii":
                GameObject.Find("GoogleMaps").GetComponent<DeliveryMapLoader>().LoadMap(44.426929, 26.1011807);
                break;
            case "Brasov":
                GameObject.Find("GoogleMaps").GetComponent<DeliveryMapLoader>().LoadMap(45.6431122, 25.5858238);
                break;
            //case "Current Location":
            //    if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
            //    {
            //        Permission.RequestUserPermission(Permission.CoarseLocation);
            //    }
            //    StartCoroutine(GetCurrentLocation());
            //    break;
            default:
                break;
        }
        Manager.gameStarted = true;
        GameObject.Find("Canvas/Panel").SetActive(false);
        minimap.SetActive(true);
        tukTukStatusDialog.SetActive(true);
    }

    IEnumerator DisplayLocationError()
    {
        locationError.SetActive(true);
        yield return new WaitForSeconds(1);
        locationError.SetActive(false);
    }

    IEnumerator DisplayLoadingMessage()
    {
        loadingMessage.SetActive(true);
        yield return new WaitForSeconds(2);
        loadingMessage.SetActive(false);
    }

    IEnumerator GetCurrentLocation()
    {
        StartCoroutine(DisplayLoadingMessage());
        Debug.Log("1");
        //yield return new WaitForSeconds(1);
        Debug.Log("2");
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("3");
            StartCoroutine(DisplayLocationError());
            Debug.Log("4");
            yield break;
        }
        Debug.Log("5");
        Input.location.Start();
        Debug.Log("6");
        //yield return new WaitForSeconds(1);
        Debug.Log("7");
        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            Debug.Log("Location is initializing");
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            GameObject.Find("GoogleMaps").GetComponent<DeliveryMapLoader>().LoadMap(45, 25);
            Debug.Log("Location initialization failed");
            yield break;
        } else
        {
            Debug.Log("Latitude\t: " + Input.location.lastData.latitude);
            Debug.Log("Longitude\t: " + Input.location.lastData.longitude);
            GameObject.Find("GoogleMaps").GetComponent<DeliveryMapLoader>().LoadMap(Input.location.lastData.latitude, Input.location.lastData.longitude);
        }
        Debug.Log("8");

        //GameObject.Find("GoogleMaps").GetComponent<DeliveryMapLoader>().LoadMap(45, 25);
        //yield return null;
        Input.location.Stop();
    }

    public void onSelectVehicleButton()
    {
        GameObject.Find("Canvas/Panel").SetActive(false);
        backButton.SetActive(true);
        Manager.choosingCar = true;
        leftButtonSelectVehicle.SetActive(true);
        rightButtonSelectVehicle.SetActive(true);
    }
}
