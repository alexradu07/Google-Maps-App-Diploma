﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Coord;
using UnityEditor;
using System.Timers;
using System.Diagnostics;
using UnityEngine.UI;

using Debug = UnityEngine.Debug;

public class DeliveryCarController : MonoBehaviour
{
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
    public GameObject orderPickupAck;
    public GameObject arrow;
    private float maxAngle;
    private float angle;
    public Transform frontWheelTransform, rearLeftWheelTransform, rearRightWheelTransform;
    private DeliveryMapLoader mapLoader;
    private bool waitingForOrder;
    private bool receivedOrder;
    private bool deliveringOrder;
    private bool onWayToRestaurant;
    private bool coroutineStarted;
    private Result currentRestaurant;
    private GameObject currentDeliverySpot;
    private Stopwatch watch;
    private bool tuktukActive;
    private Text statusText;

    // Start is called before the first frame update
    void Start()
    {
        maxAngle = 30;
        angle = 0;
        mapLoader = GameObject.Find("GoogleMaps").GetComponent<DeliveryMapLoader>();
        if (mapLoader == null)
        {
            Debug.Log("maploader is null");
        }
        waitingForOrder = true;
        receivedOrder = false;
        deliveringOrder = false;
        onWayToRestaurant = false;
        arrow.SetActive(false);
        marker.SetActive(false);
        watch = new Stopwatch();
        watch.Start();
        coroutineStarted = true;
        tuktukActive = true;
        statusText = GameObject.Find("Canvas/tuktukStatus/statusText").GetComponent<Text>();
        StartCoroutine(TimerTicked(watch));
    }

    public void UpdateWheelPosition(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 position = wheelTransform.position;
        Quaternion rotation = wheelTransform.rotation;
        wheelCollider.GetWorldPose(out position, out rotation);

        wheelTransform.position = position;
        wheelTransform.rotation = rotation;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCarPosition();

        if (mapLoader.objectContainer == null)
        {
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
            Vector3 arrowDirection = (marker.transform.position - this.transform.position).normalized;
            arrow.transform.position = this.transform.position + new Vector3(0, 4, 0);
            arrow.transform.LookAt(marker.transform);
            arrow.transform.rotation = Quaternion.Euler(arrow.transform.eulerAngles.x,
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
            Vector3 arrowDirection = (marker.transform.position - this.transform.position).normalized;
            arrow.transform.position = this.transform.position + new Vector3(0, 4, 0);
            arrow.transform.LookAt(marker.transform);
            arrow.transform.rotation = Quaternion.Euler(arrow.transform.eulerAngles.x,
                    arrow.transform.eulerAngles.y - 90,
                    arrow.transform.eulerAngles.z);

            if (Vector3.Distance(rb.transform.position, marker.transform.position) - 100 < .7f)
            {
                orderPickupAck.SetActive(true);
                Text prompt = GameObject.Find("Canvas/OrderPickUpAck/PromptText").GetComponent<Text>();
                prompt.text = "You have successfully delivered order to destination.";
            }
        }

        if (!coroutineStarted)
        {
            coroutineStarted = true;
            watch.Start();
            StartCoroutine(TimerTicked(watch));
        }
    }

    void UpdateCarPosition()
    {
        frontWheelCollider.motorTorque = 0;
        rearLeftWheelCollider.brakeTorque = 0;
        rearRightWheelCollider.brakeTorque = 0;
        frontWheelCollider.brakeTorque = 0;
        if (Input.GetKey(KeyCode.A))
        {
            if (angle > -maxAngle)
            {
                angle -= 2;
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (angle < maxAngle)
            {
                angle += 2;
            }
        }
        if (Input.GetKey(KeyCode.W))
        {
            frontWheelCollider.motorTorque = 500;
        }
        if (Input.GetKey(KeyCode.S))
        {
            frontWheelCollider.motorTorque = -200;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            rearLeftWheelCollider.brakeTorque = 500;
            rearRightWheelCollider.brakeTorque = 500;
            //frontWheelCollider.brakeTorque = 500;
        }

        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            if (angle > 0)
            {
                angle -= 2;
            }
            else if (angle < 0)
            {
                angle += 2;
            }
        }
        frontWheelCollider.steerAngle = angle;
        UpdateWheelPosition(rearRightWheelCollider, rearRightWheelTransform);
        UpdateWheelPosition(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateWheelPosition(frontWheelCollider, frontWheelTransform);
    }

    private IEnumerator TimerTicked(Stopwatch wotspatch)
    {
        while (wotspatch.Elapsed.TotalSeconds < 5)
            yield return null;
        if (tuktukActive)
        {
            if (waitingForOrder)
            {
                if (!dialogPanel.activeSelf)
                {
                    System.Random random = new System.Random();
                    int orderRestaurantIndex = random.Next(mapLoader.objectContainer.results.Count);
                    Debug.Log("Random number selected : " + orderRestaurantIndex);
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
        Vector3 restaurantPosition = mapLoader.mapsService.Coords.FromLatLngToVector3(new LatLng(currentRestaurant.geometry.location.lat, currentRestaurant.geometry.location.lng));
        marker.transform.position = restaurantPosition + new Vector3(0, 100.5f, 0);
        arrow.SetActive(true);
    }

    public void onRejectOrder()
    {
        dialogPanel.SetActive(false);
        Debug.Log("Order Rejected");

    }

    public void onPromptOk()
    {
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
        }
        else if (deliveringOrder)
        {
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
    }
}