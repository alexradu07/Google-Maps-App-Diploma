using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Android;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Debug = UnityEngine.Debug;

public class DeliveryOutdoorSceneController : MonoBehaviour
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
        //Debug.Log(GetCurrentMethod());
        SceneManager.LoadScene(sceneName);
    }

    public void SelectLocation()
    {
        //Debug.Log(GetCurrentMethod());
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        {
            Permission.RequestUserPermission(Permission.CoarseLocation);
        }
        StartCoroutine(GetCurrentLocation());
        GameObject.Find("Canvas/Panel").SetActive(false);
        minimap.SetActive(true);
        tukTukStatusDialog.SetActive(true);
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
            GameObject.Find("GoogleMaps").GetComponent<DeliveryOutdoorMapLoader>().LoadMap(Input.location.lastData.latitude, Input.location.lastData.longitude);
            Manager.locationQueryComplete = true;
        }

        Input.location.Stop();
        Manager.gameStarted = true;
    }

    public void onSelectVehicleButton()
    {
        //Debug.Log(GetCurrentMethod());
        GameObject.Find("Canvas/Panel").SetActive(false);
        backButton.SetActive(true);
        Manager.choosingCar = true;
        leftButtonSelectVehicle.SetActive(true);
        rightButtonSelectVehicle.SetActive(true);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public string GetCurrentMethod()
    {
        var st = new StackTrace();
        var sf = st.GetFrame(1);

        return sf.GetMethod().Name;
    }
}
