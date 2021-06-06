using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Android;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Debug = UnityEngine.Debug;

public class DeliverySceneController : MonoBehaviour
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

    public GameObject tasksPanel;
    public GameObject mainPanel;
    public GameObject task1Progress;
    public GameObject task2Progress;
    public GameObject task3Progress;

    // Start is called before the first frame update
    void Start()
    {
        LoadPreferences();
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
        Manager.consecutiveOutdoorDeliveries = 0;
        PlayerPrefs.SetInt(Manager.consecutiveOutdoorDeliveriesString, Manager.consecutiveOutdoorDeliveries);

        string locationString = GameObject.Find("Canvas/Panel/LocationDropdownSelector/Label").GetComponent<Text>().text;
        switch (locationString)
        {
            case "Grozavesti":
                GameObject.Find("GoogleMaps").GetComponent<DeliveryMapLoader>().LoadMap(44.4433837, 26.0618934);
                Manager.gameStarted = true;
                break;
            case "Unirii":
                GameObject.Find("GoogleMaps").GetComponent<DeliveryMapLoader>().LoadMap(44.426929, 26.1011807);
                Manager.gameStarted = true;
                break;
            case "Brasov":
                GameObject.Find("GoogleMaps").GetComponent<DeliveryMapLoader>().LoadMap(45.6431122, 25.5858238);
                Manager.gameStarted = true;
                break;
            case "Current Location":
                if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
                {
                    Permission.RequestUserPermission(Permission.CoarseLocation);
                }
                StartCoroutine(GetCurrentLocation());
                break;
            default:
                break;
        }
        GameObject.Find("Canvas/Panel").SetActive(false);
        if (Manager.selectedVehicle == Manager.TUKTUK_SELECTED)
        {
            dodge.SetActive(false);
            tuktuk.SetActive(true);
            cameraObject.GetComponent<DeliveryCameraScript>().setObjectToFollow(tuktuk.transform);
        } else if (Manager.selectedVehicle == Manager.DODGE_SELECTED)
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
        } else
        {
            Debug.Log("Latitude\t: " + Input.location.lastData.latitude);
            Debug.Log("Longitude\t: " + Input.location.lastData.longitude);
            Manager.dynamicLatitude = Input.location.lastData.latitude;
            Manager.dynamicLongitude = Input.location.lastData.longitude;
            GameObject.Find("GoogleMaps").GetComponent<DeliveryMapLoader>().LoadMap(Input.location.lastData.latitude, Input.location.lastData.longitude);
            Manager.locationQueryComplete = true;
        }

        Input.location.Stop();
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
        } else
        {
            if (Manager.secondCarUnlocked == 0)
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
            if (Manager.secondCarUnlocked == 0)
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
        Manager.secondCarUnlocked = PlayerPrefs.GetInt(Manager.secondCarUnlockedString, 0);
        Manager.completedOutdoorDeliveries = PlayerPrefs.GetInt(Manager.completedOutdoorDeliveriesString, 0);
        Manager.multipleRestaurantsUnlocked = PlayerPrefs.GetInt(Manager.multipleRestaurantsUnlockedString, 0);
        Manager.fastestOutdoorDelivery = PlayerPrefs.GetInt(Manager.fastestOutdoorDeliveryString, 0);
        Manager.dynamicLoadingUnlocked = PlayerPrefs.GetInt(Manager.dynamicLoadingUnlockedString, 0);
        Manager.consecutiveOutdoorDeliveries = PlayerPrefs.GetInt(Manager.consecutiveOutdoorDeliveriesString, 0);
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
        if (dodge.activeSelf && Manager.secondCarUnlocked == 1)
        {
            Manager.selectedVehicle = Manager.DODGE_SELECTED;
        }
        else if (dodge.activeSelf && Manager.secondCarUnlocked == 0)
        {
            Manager.selectedVehicle = Manager.TUKTUK_SELECTED;
            lockedVehicleMessage.SetActive(true);
        }

        if (tuktuk.activeSelf)
        {
            Manager.selectedVehicle = Manager.TUKTUK_SELECTED;
        }
    }

    public void onSeeTasksButton()
    {
        //LoadPreferences(); // Maybe needed here ?? 
        GameObject.Find("Canvas/Panel").SetActive(false);
        Manager.choosingCar = true; // FIXME: try some other way here
        tasksPanel.SetActive(true);
        if (Manager.secondCarUnlocked == 1)
        {
            task1Progress.GetComponent<Text>().text = "Progress: Completed";
        }
        else
        {
            task1Progress.GetComponent<Text>().text = "Progress: " + (Manager.completedOutdoorDeliveries / 10) * 100 + "%";
        }

        if (Manager.multipleRestaurantsUnlocked == 1)
        {
            task1Progress.GetComponent<Text>().text = "Progress: completed";
        }
        else
        {
            task2Progress.GetComponent<Text>().text = "Fastest delivery: " + Manager.fastestOutdoorDelivery + " seconds";
        }

        if (Manager.dynamicLoadingUnlocked == 1)
        {
            task3Progress.GetComponent<Text>().text = "Progress: completed";
        }
        else
        {
            task3Progress.GetComponent<Text>().text = "Most consecutive deliveries: " + Manager.consecutiveOutdoorDeliveries;
        }
    }

    public void onTasksBackButton()
    {
        Manager.choosingCar = false; // FIXME
        tasksPanel.SetActive(false);
        mainPanel.SetActive(true);

    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public string GetCurrentMethod()
    {
        var st = new StackTrace();
        var sf = st.GetFrame(1);

        return sf.GetMethod().Name;
    }
}
