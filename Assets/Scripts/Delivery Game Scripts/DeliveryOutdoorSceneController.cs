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
    private bool locationServiceStarted = false;
    private float deltaTime = .0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (locationServiceStarted && deltaTime > 1)
        {
            UpdateLocationData();
            deltaTime = .0f;
        }
        deltaTime += Time.deltaTime;
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
        Input.location.Start(5, 5);
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
            Debug.Log("Face loadMap de la locatia : " + Manager.dynamicLatitude  + " " + Manager.dynamicLongitude);
            GameObject.Find("GoogleMaps").GetComponent<DeliveryOutdoorMapLoader>().LoadMap(Manager.dynamicLatitude, Manager.dynamicLongitude);
            Manager.locationQueryComplete = true;
        }
        locationServiceStarted = true;

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

    public void UpdateLocationData()
    {
        LocationInfo current = Input.location.lastData;

        Manager.dynamicLatitude = current.latitude;
        Manager.dynamicLongitude = current.longitude;
        Debug.Log("latitude\t: " + Manager.dynamicLatitude);
        Debug.Log("longitude\t: " + Manager.dynamicLongitude);
    }

    public void OnDestroy()
    {
        Input.location.Stop();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public string GetCurrentMethod()
    {
        var st = new StackTrace();
        var sf = st.GetFrame(1);

        return sf.GetMethod().Name;
    }
}
