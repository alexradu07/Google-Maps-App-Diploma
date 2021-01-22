using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Coord;
using UnityEditor;
using System.Timers;
using System.Diagnostics;
using UnityEngine.UI;


using Debug = UnityEngine.Debug;

public class DeliveryPlayerScript : MonoBehaviour
{
    private Rigidbody rb;
    public Camera playerCamera;
    public float cameraX;
    public float cameraY;
    public float cameraZ;
    public GameObject marker;
    public GameObject dialogPanel;
    public GameObject orderPickupAck;
    public GameObject arrow;
    private DeliveryMapLoader mapLoader;
    private bool waitingForOrder;
    private bool receivedOrder;
    private bool deliveringOrder;
    private bool onWayToRestaurant;
    private bool coroutineStarted;
    private Result currentRestaurant;
    private GameObject currentDeliverySpot;
    private Stopwatch watch;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mapLoader = GameObject.Find("GoogleMaps").GetComponent<DeliveryMapLoader>();
        waitingForOrder = true;
        receivedOrder = false;
        deliveringOrder = false;
        onWayToRestaurant = false;
        arrow.SetActive(false);
        marker.SetActive(false);
        watch = new Stopwatch();
        watch.Start();
        coroutineStarted = true;
        StartCoroutine(TimerTicked(watch));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
            rb.AddForce(Vector3.left * 10);
        if (Input.GetKey(KeyCode.D))
            rb.AddForce(Vector3.right * 10);
        if (Input.GetKey(KeyCode.W))
            rb.AddForce(new Vector3(0, 0, 10));
        if (Input.GetKey(KeyCode.S))
            rb.AddForce(new Vector3(0, 0, -10));

        playerCamera.transform.position = this.transform.position + new Vector3(cameraX, cameraY, cameraZ);

        if (mapLoader.objectContainer == null)
        {
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

            if (Vector3.Distance(rb.transform.position, marker.transform.position) - 100 < .3f)
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
            
            if (Vector3.Distance(rb.transform.position, marker.transform.position) - 100 < .3f)
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

    private IEnumerator TimerTicked(Stopwatch wotspatch)
    {
        while (wotspatch.Elapsed.TotalSeconds < 5)
            yield return null;
        Debug.Log("5 secs elapsed.");
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
        } else if (deliveringOrder)
        {
            deliveringOrder = false;
            waitingForOrder = true;
            orderPickupAck.SetActive(false);
            marker.SetActive(false);
            arrow.SetActive(false);
        }
    }
}
