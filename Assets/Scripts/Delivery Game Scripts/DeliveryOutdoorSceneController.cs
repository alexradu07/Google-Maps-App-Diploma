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
    public GameObject tuktuk;
    public GameObject dodge;
    public GameObject cameraObject;
    public GameObject dodgeStatusDialog;
    public GameObject lockImage;
    public GameObject selectButton;
    public GameObject lockedVehicleMessage;

    private int secondCarUnlocked;

    private bool locationServiceStarted = false;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (locationServiceStarted)
        {
            UpdateLocationData();
        }
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
        if (Manager.selectedVehicle == Manager.TUKTUK_SELECTED)
        {
            dodge.SetActive(false);
            tuktuk.SetActive(true);
            cameraObject.GetComponent<DeliveryCameraScript>().setObjectToFollow(tuktuk.transform);
        }
        else if (Manager.selectedVehicle == Manager.DODGE_SELECTED)
        {
            dodge.SetActive(true);
            tuktuk.SetActive(false);
            cameraObject.GetComponent<DeliveryCameraScript>().setObjectToFollow(dodge.transform);
        }
        minimap.SetActive(true);
        if (dodge.activeSelf)
        {
            dodgeStatusDialog.SetActive(true);
        }
        else
        {
            tukTukStatusDialog.SetActive(true);
        }
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

    public void onLeftButtonSelectVehicle()
    {
        if (dodge.activeSelf)
        {
            lockImage.SetActive(false);
            selectButton.SetActive(true);
            lockedVehicleMessage.SetActive(false);
            dodge.SetActive(false);
            tuktuk.SetActive(true);
            cameraObject.GetComponent<DeliveryCameraScript>().setObjectToFollow(tuktuk.transform);
        }
        else
        {
            if (secondCarUnlocked == 0)
            {
                lockImage.SetActive(true);
            }
            dodge.SetActive(true);
            tuktuk.SetActive(false);
            cameraObject.GetComponent<DeliveryCameraScript>().setObjectToFollow(dodge.transform);
        }

    }

    public void onRightButtonSelectVehicle()
    {
        if (dodge.activeSelf)
        {
            lockImage.SetActive(false);
            selectButton.SetActive(true);
            lockedVehicleMessage.SetActive(false);
            dodge.SetActive(false);
            tuktuk.SetActive(true);
            cameraObject.GetComponent<DeliveryCameraScript>().setObjectToFollow(tuktuk.transform);
        }
        else
        {
            if (secondCarUnlocked == 0)
            {
                lockImage.SetActive(true);
            }
            dodge.SetActive(true);
            tuktuk.SetActive(false);
            cameraObject.GetComponent<DeliveryCameraScript>().setObjectToFollow(dodge.transform);
        }

    }

    public void LoadPreferences()
    {
        secondCarUnlocked = PlayerPrefs.GetInt("SecondCarUnlocked", 0);
    }

    public void onSelectVehicleButton()
    {
        //Debug.Log(GetCurrentMethod());
        LoadPreferences();
        GameObject.Find("Canvas/Panel").SetActive(false);
        selectButton.SetActive(true);
        backButton.SetActive(true);
        Manager.choosingCar = true;
        leftButtonSelectVehicle.SetActive(true);
        rightButtonSelectVehicle.SetActive(true);
    }

    public void onSelectButton()
    {
        if (dodge.activeSelf && secondCarUnlocked == 1)
        {
            Manager.selectedVehicle = Manager.DODGE_SELECTED;
        }
        else if (dodge.activeSelf && secondCarUnlocked == 0)
        {
            Manager.selectedVehicle = Manager.TUKTUK_SELECTED;
            lockedVehicleMessage.SetActive(true);
        }

        if (tuktuk.activeSelf)
        {
            Manager.selectedVehicle = Manager.TUKTUK_SELECTED;
        }
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
