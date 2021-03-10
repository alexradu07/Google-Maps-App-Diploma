using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Coord;
using Google.Maps.Unity.Intersections;
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
    public GameObject timerPanel;
    public GameObject timerText;
    public GameObject minimap;
    public GameObject tukTukStatusDialog;
    public GameObject pathSphere;
    public Transform frontWheelTransform, rearLeftWheelTransform, rearRightWheelTransform;
    private float maxAngle;
    private float angle;
    private DeliveryMapLoader mapLoader;
    private bool waitingForOrder;
    private bool deliveringOrder;
    private bool onWayToRestaurant;
    private bool coroutineStarted;
    private Result currentRestaurant;
    private GameObject currentDeliverySpot;
    private Stopwatch watch;
    private bool tuktukActive;
    private Text statusText;
    private List<GameObject> currentActivePath;

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
        deliveringOrder = false;
        onWayToRestaurant = false;
        arrow.SetActive(false);
        marker.SetActive(false);
        minimap.SetActive(true);
        tukTukStatusDialog.SetActive(true);
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
                angle -= 1;
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (angle < maxAngle)
            {
                angle += 1;
            }
        }
        if (Input.GetKey(KeyCode.W))
        {
            frontWheelCollider.motorTorque = 500;
        }
        if (Input.GetKey(KeyCode.S))
        {
            frontWheelCollider.motorTorque = -300;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            frontWheelCollider.brakeTorque = 400;
            rearLeftWheelCollider.brakeTorque = 600;
            rearRightWheelCollider.brakeTorque = 600;
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

    List<GameObject> GeneratePath(Vector3 destinationPosition)
    {
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
                } else if (currentDistance >= 10)
                {
                    int numberAddedArrows = (int) (currentDistance / 10) - 1;
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
        while (wotspatch.Elapsed.TotalSeconds < 5)
            yield return null;
        if (tuktukActive)
        {
            if (waitingForOrder)
            {
                if (!dialogPanel.activeSelf)
                {
                    timerPanel.SetActive(false);
                    System.Random random = new System.Random();
                    if (mapLoader.objectContainer == null)
                    {
                        while (mapLoader.objectContainer == null)
                        {
                            ;
                        }
                    }

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
}