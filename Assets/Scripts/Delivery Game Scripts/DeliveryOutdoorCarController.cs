﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Coord;
using Google.Maps.Unity.Intersections;
using System.Diagnostics;
using UnityEngine.UI;

using Debug = UnityEngine.Debug;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;

public class DeliveryOutdoorCarController : MonoBehaviour
{
    private Vector3 previousGPSLocation = new Vector3(0, 0, 0);
    private Vector3 currentGPSLocation = new Vector3(0, 0, 0);
    private Vector3 currentInterpolationLocation = new Vector3(0, 0, 0);
    private bool firstGPSUpdateRecevied = false;
    public Rigidbody rb;
    public GameObject frontWheel;
    public GameObject rearLeftWheel;
    public GameObject rearRightWheel;
    public WheelCollider frontWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;
    public GameObject tuktuk;
    public GameObject marker;
    public GameObject dialogPanel;
    public GameObject dialogExitPanel;
    public GameObject orderPickupAck;
    public GameObject arrow;
    public GameObject timerPanel;
    public GameObject timerText;
    public GameObject minimap;
    public GameObject tukTukStatusDialog;
    public GameObject pathSphere;
    public GameObject backButton;
    public GameObject panel;
    public Transform frontWheelTransform, rearLeftWheelTransform, rearRightWheelTransform;
    public Text statusText;
    public GameObject leftButtonSelectVehicle;
    public GameObject rightButtonSelectVehicle;
    private float maxAngle;
    private float angle;
    private DeliveryOutdoorMapLoader mapLoader;
    private bool waitingForOrder;
    private bool deliveringOrder;
    private bool onWayToRestaurant;
    private bool coroutineStarted;
    private Result currentRestaurant;
    private GameObject currentDeliverySpot;
    private Stopwatch watch;
    private bool tuktukActive;
    private List<GameObject> currentActivePath;
    private bool timerStarted;

    // Start is called before the first frame update
    void Start()
    {
        maxAngle = 30;
        angle = 0;
        mapLoader = GameObject.Find("GoogleMaps").GetComponent<DeliveryOutdoorMapLoader>();
        if (mapLoader == null)
        {
            Debug.Log("maploader is null");
        }
#if UNITY_EDITOR_WIN
        //frontButton.SetActive(false);
        //reverseButton.SetActive(false);
        //leftButton.SetActive(false);
        //rightButton.SetActive(false);
        //brakeButton.SetActive(false);
#endif
        deliveringOrder = false;
        onWayToRestaurant = false;
        arrow.SetActive(false);
        marker.SetActive(false);
        timerStarted = false;
        coroutineStarted = false;
        watch = new Stopwatch();
    }

    public void UpdateWheelPosition(WheelCollider wheelCollider, Transform wheelTransform)
    {
        //Debug.Log(GetCurrentMethod());
        Vector3 position = wheelTransform.position;
        Quaternion rotation = wheelTransform.rotation;
        wheelCollider.GetWorldPose(out position, out rotation);

        wheelTransform.position = position;
        wheelTransform.rotation = rotation;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(GetCurrentMethod());
        if (!Manager.gameStarted)
        {
            Debug.Log("Game not started for vehicle");
            return;
        }
        if (!timerStarted)
        {
            Debug.Log("Intra fix o data pe aici");
            waitingForOrder = true;
            tuktukActive = true;
            //watch.Start();
            //StartCoroutine(TimerTicked(watch));
            //coroutineStarted = true;
            timerStarted = true;
            backButton.SetActive(true);
            mapLoader.setQueryNeeded();
        }
        UpdateCarPosition();

        if (mapLoader.objectContainer == null)
        {
            Debug.Log("Object container is null");
            return;
        }

        if (tuktukActive)
        {
            statusText.text = "Status : Active";
        }
        else
        {
            statusText.text = "Status : Inactive";
            return;
        }

        if (onWayToRestaurant)
        {
            Debug.Log("On way to restaurant");
            Vector3 arrowDirection = (marker.transform.position - this.transform.position).normalized;
            arrow.transform.position = this.transform.position + new Vector3(0, 4, 0);
            arrow.transform.LookAt(marker.transform);
            arrow.transform.rotation = Quaternion.Euler(0,
                    arrow.transform.eulerAngles.y - 90,
                    arrow.transform.eulerAngles.z);
            if (Vector3.Distance(rb.transform.position, marker.transform.position) - 100 < .7f)
            {
                orderPickupAck.SetActive(true);
                Text prompt = GameObject.Find("Canvas/OrderPickUpAck/PromptText").GetComponent<Text>();
                prompt.text = "You picked up order from " + currentRestaurant.name + ". Now deliver order to destination.";
            }
        }
        if (deliveringOrder)
        {
            Debug.Log("Deliering order");
            Vector3 arrowDirection = (marker.transform.position - this.transform.position).normalized;
            arrow.transform.position = this.transform.position + new Vector3(0, 4, 0);
            arrow.transform.LookAt(marker.transform);
            arrow.transform.rotation = Quaternion.Euler(0,
                    arrow.transform.eulerAngles.y - 90,
                    arrow.transform.eulerAngles.z);

            if (Vector3.Distance(rb.transform.position, marker.transform.position) - 100 < .7f)
            {
                orderPickupAck.SetActive(true);
                Text prompt = GameObject.Find("Canvas/OrderPickUpAck/PromptText").GetComponent<Text>();
                prompt.text = "You have successfully delivered order to destination.";
                DeliveryTimerScript timerScript = timerText.GetComponent<DeliveryTimerScript>();
                timerScript.StopTimer();
            }
        }
        if (!coroutineStarted)
        {
            Debug.Log("Starting coroutine one per 10 secs");
            coroutineStarted = true;
            watch.Start();
            StartCoroutine(TimerTicked(watch));
        }
    }

    void UpdateCarPosition()
    {
        //Debug.Log(GetCurrentMethod());
        if (Manager.locationQueryComplete)
        {
            Vector3 currentGPSLocationLocal = mapLoader.mapsService.Coords.FromLatLngToVector3(new LatLng(Manager.dynamicLatitude, Manager.dynamicLongitude));
            if (firstGPSUpdateRecevied)
            {
                if (currentGPSLocationLocal.Equals(currentGPSLocation))
                {
                    currentInterpolationLocation = Vector3.Lerp(currentInterpolationLocation, currentGPSLocation, .1f);
                    Debug.Log("No more updated GPS position");
                    Debug.Log("Interpolated location :" + currentInterpolationLocation);
                }
                else
                {
                    previousGPSLocation = currentGPSLocation;
                    currentGPSLocation = currentGPSLocationLocal;
                    currentInterpolationLocation = previousGPSLocation;
                    //currentInterpolationLocation = Vector3.Lerp(currentInterpolationLocation, previousGPSLocation, .1f); try this out
                    Debug.Log("Received updated GPS location : " + currentGPSLocationLocal);

                    Vector3 directionVector = (currentGPSLocation - previousGPSLocation).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(directionVector);
                    tuktuk.transform.rotation = lookRotation;
                }
                tuktuk.transform.position = currentInterpolationLocation;
               
            }
            else
            {
                firstGPSUpdateRecevied = true;
                currentInterpolationLocation = currentGPSLocationLocal;
                previousGPSLocation = currentGPSLocationLocal;
                currentGPSLocation = currentGPSLocationLocal;
            }
        }

    }


    List<GameObject> GeneratePath(Vector3 destinationPosition)
    {
        //Debug.Log(GetCurrentMethod());
        List<RoadLatticeNode> allLattices;
        allLattices = new List<RoadLatticeNode>(mapLoader.mapsService.RoadLattice.Nodes);
        Vector3 currentPosition = tuktuk.transform.position;
        float minDistance = 1000;
        Vector3 closestPosition = new Vector3(1000, 1000, 1000);
        float minDistanceUser = 1000;
        Vector3 closestPositionUser = new Vector3(1000, 1000, 1000);
        RoadLatticeNode destinationNode;
        destinationNode = allLattices[0];
        RoadLatticeNode sourceNode;
        sourceNode = allLattices[0];
        foreach (RoadLatticeNode lattice in allLattices)
        {
            Vector3 roadLatticePosition = new Vector3(lattice.Location.x, 0, lattice.Location.y);
            float distance = Vector3.Distance(roadLatticePosition, destinationPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPosition = roadLatticePosition;
                destinationNode = lattice;
            }
            distance = Vector3.Distance(roadLatticePosition, currentPosition);
            if (distance < minDistanceUser)
            {
                minDistanceUser = distance;
                closestPositionUser = roadLatticePosition;
                sourceNode = lattice;
            }
        }
        marker.transform.position = closestPosition + new Vector3(0, 100.5f, 0);
        List<RoadLatticeNode> pathToDestination;
        pathToDestination = new List<RoadLatticeNode>(RoadLattice.FindPath(sourceNode, destinationNode, 10000, null));
        List<GameObject> createdPathObjects = new List<GameObject>();
        foreach (RoadLatticeNode lattice in pathToDestination)
        {
            if (createdPathObjects.Count == 0)
            {
                createdPathObjects.Add(Object.Instantiate(pathSphere, new Vector3(lattice.Location.x, 0, lattice.Location.y), Quaternion.identity));
            }
            else
            {
                float currentDistance = Vector3.Distance(new Vector3(lattice.Location.x, 0, lattice.Location.y), createdPathObjects[createdPathObjects.Count - 1].transform.position);
                if (currentDistance > 5 && currentDistance < 10)
                {
                    createdPathObjects.Add(Object.Instantiate(pathSphere, new Vector3(lattice.Location.x, 0, lattice.Location.y), Quaternion.identity));
                }
                else if (currentDistance >= 10)
                {
                    int numberAddedArrows = (int)(currentDistance / 10) - 1;
                    Vector3 directionVector = (new Vector3(lattice.Location.x, 0, lattice.Location.y) - createdPathObjects[createdPathObjects.Count - 1].transform.position).normalized;
                    for (int i = 1; i <= numberAddedArrows; ++i)
                    {
                        Vector3 targetPosition = createdPathObjects[createdPathObjects.Count - 1].transform.position + directionVector * 10;
                        createdPathObjects.Add(Object.Instantiate(pathSphere, targetPosition, Quaternion.identity));
                    }
                }

            }
        }
        for (int i = 0; i < createdPathObjects.Count; ++i)
        {
            if (i != createdPathObjects.Count - 1)
            {
                createdPathObjects[i].transform.LookAt(createdPathObjects[i + 1].transform);
                createdPathObjects[i].transform.rotation = Quaternion.Euler(0,
                    createdPathObjects[i].transform.eulerAngles.y + 90,
                    createdPathObjects[i].transform.eulerAngles.z);
            }
        }
        return createdPathObjects;
    }

    private IEnumerator TimerTicked(Stopwatch wotspatch)
    {
        while (wotspatch.Elapsed.TotalSeconds < 10)
        {
            yield return null;
        }
        if (tuktukActive)
        {
            if (waitingForOrder && Manager.gameStarted)
            {
                if (!dialogPanel.activeSelf)
                {
                    timerPanel.SetActive(false);
                    System.Random random = new System.Random();
                    if (mapLoader.objectContainer == null)
                    {
                        while (mapLoader.objectContainer == null)
                        {
                            Debug.Log("Aici crapa de fapt !!! ");
                            ;
                        }
                    }

                    int orderRestaurantIndex = random.Next(mapLoader.objectContainer.results.Count);
                    Debug.Log("Random number selected : " + orderRestaurantIndex);
                    if (mapLoader.objectContainer.results.Count == 0)
                    {
                        yield break;
                    }
                    currentRestaurant = mapLoader.objectContainer.results[orderRestaurantIndex];
                    dialogPanel.SetActive(true);
                    Text question = GameObject.Find("Canvas/DialogPanel/QuestionText").GetComponent<Text>();
                    question.text = "New order from " + currentRestaurant.name + ". Do you want to deliver the order?";

                }
            }
        }
        wotspatch.Reset();
        coroutineStarted = false;
        StopCoroutine("TimerTicked");
    }

    void OnDestroy()
    {
        Debug.Log("OnDestroy");
    }

    public void onAcceptOrder()
    {
        dialogPanel.SetActive(false);
        Debug.Log("Order Accepted");
        onWayToRestaurant = true;
        waitingForOrder = false;
        marker.SetActive(true);
        mapLoader.setQueryNotNeeded();
        Vector3 restaurantPosition = mapLoader.mapsService.Coords.FromLatLngToVector3(new LatLng(currentRestaurant.geometry.location.lat, currentRestaurant.geometry.location.lng));
        marker.transform.position = restaurantPosition + new Vector3(0, 100.5f, 0);
        arrow.SetActive(true);
        timerPanel.SetActive(true);
        DeliveryTimerScript timerScript = timerText.GetComponent<DeliveryTimerScript>();
        timerScript.ResetTimer();
        timerScript.StartTimer();
        currentActivePath = GeneratePath(marker.transform.position);
    }

    public void onRejectOrder()
    {
        dialogPanel.SetActive(false);
        Debug.Log("Order Rejected");

    }

    public void onPromptOk()
    {
        if (currentActivePath.Count != 0)
        {
            foreach (GameObject spherePath in currentActivePath)
            {
                spherePath.SetActive(false);
            }
            currentActivePath.Clear();
        }
        if (onWayToRestaurant)
        {
            onWayToRestaurant = false;
            deliveringOrder = true;
            orderPickupAck.SetActive(false);
            GameObject[] buildingObjects = GameObject.FindObjectsOfType<GameObject>();
            List<GameObject> extrudedStructures = new List<GameObject>();
            foreach (GameObject obj in buildingObjects)
            {
                if (obj.transform.parent != null && obj.transform.parent.name == "GoogleMaps"
                    && obj.name.StartsWith("ExtrudedStructure"))
                {
                    extrudedStructures.Add(obj);
                }
            }
            System.Random random = new System.Random();
            int deliveryPointIndex = random.Next(extrudedStructures.Count);
            marker.SetActive(true);
            Debug.Log("number of extruded structure : " + deliveryPointIndex);
            currentDeliverySpot = extrudedStructures[deliveryPointIndex];
            marker.transform.position = currentDeliverySpot.transform.position + new Vector3(0, 100.5f, 0);
            arrow.SetActive(true);
            Debug.Log("Delivering order");
            currentActivePath = GeneratePath(marker.transform.position);
        }
        else if (deliveringOrder)
        {
            mapLoader.setQueryNeeded();
            deliveringOrder = false;
            waitingForOrder = true;
            orderPickupAck.SetActive(false);
            marker.SetActive(false);
            arrow.SetActive(false);
        }
    }

    public void toggleStatus()
    {
        tuktukActive = !tuktukActive;
        if (!tuktukActive)
        {
            timerPanel.SetActive(false);
            DeliveryTimerScript timerScript = timerText.GetComponent<DeliveryTimerScript>();
            timerScript.ResetTimer();
        }
    }

    public void onBackButton()
    {
        if (!Manager.gameStarted)
        {
            panel.SetActive(true);
            backButton.SetActive(false);
            leftButtonSelectVehicle.SetActive(false);
            rightButtonSelectVehicle.SetActive(false);
        }
        else
        {
            toggleStatus();
            if (dialogExitPanel.activeSelf)
            {
                if (deliveringOrder)
                {
                    timerPanel.SetActive(true);
                    arrow.SetActive(true);
                }
                dialogExitPanel.SetActive(false);
            }
            else
            {
                arrow.SetActive(false);
                dialogExitPanel.SetActive(true);
                Text question = GameObject.Find("Canvas/ExitPanel/QuestionText").GetComponent<Text>();
                question.text = "Do you really want to exit ?";
            }
        }
    }

    public void onExitGame()
    {

    }

    public void onCancelExitGame()
    {
        dialogExitPanel.SetActive(false);
        toggleStatus();
        if (deliveringOrder)
        {
            timerPanel.SetActive(true);
            arrow.SetActive(true);
        }
    }

    public void onLeftButtonSelectVehicle()
    {

    }

    public void onRightButtonSelectVehicle()
    {

    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public string GetCurrentMethod()
    {
        var st = new StackTrace();
        var sf = st.GetFrame(1);

        return sf.GetMethod().Name;
    }
}
