using System.Collections.Generic;
using Google.Maps;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OutdoorCarController : MonoBehaviour
{
    public WheelCollider FR, FL, RR, RL;
    public Transform fr_tran, fl_tran, rr_tran, rl_tran;
    public Transform car_tran;
    public Rigidbody car_rigid;
    public float maxAngle = 30;
    public float angle = 0;
    private float power = 500;
    public float cameraX;
    public float cameraY;
    public float cameraZ;
    public GameObject needle;
    public GameObject needle2;
    public float baseRot = -133;
    public float velocity = 0;
    public Camera cam;
    private float rpm = 0;
    private int currentGear = 0;
    private float currentPower = 0;
    private List<float> ratios = new List<float>();
    public Text timeElapsed;
    private float time = 0;
    private float elapsedSinceLastUpdate = 0;
    static public bool gameEnded = false;
    private bool firstWPressed = false;
    private float airResistance = 0;
    private float tempVelocity = 0;
    private bool accel = false;
    private bool brake = false;

    // Start is called before the first frame update
    public void UpdateWheelPosition(WheelCollider col, Transform tran)
    {
        Vector3 position = tran.position;
        Quaternion rotation = tran.rotation;
        col.GetWorldPose(out position, out rotation);

        tran.position = position;
        tran.rotation = rotation;
    }
    void Start()
    {
        ratios.Add(0.00171428f);
        ratios.Add(0.0024285f);
        ratios.Add(0.0034285f);
        ratios.Add(0.0048571f);
        ratios.Add(0.0062857f);
        currentPower = power * (ratios[2] / ratios[currentGear]);
    }

    // Update is called once per frame
    void Update()
    {
        if (OutDoorCarSceneController.canTakeControl)
        {
            time += Time.deltaTime;
            elapsedSinceLastUpdate += Time.deltaTime;
            if (elapsedSinceLastUpdate > 1 && Input.location.status != LocationServiceStatus.Failed && OutdoorCarMapLoader.initSet)
            {
                LocationInfo current = Input.location.lastData;
                MapsService mapsService = OutdoorCarMapLoader.mapsService;
                car_tran.position = mapsService.Coords.FromLatLngToVector3(new Google.Maps.Coord.LatLng(current.latitude, current.longitude));
            }
            timeElapsed.text = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(time / 60), Mathf.FloorToInt(time % 60));
        }
    }

    public void onAccelPress(BaseEventData eventData)
    {
        accel = true;
    }

    public void onAccelRelease(BaseEventData eventData)
    {
        accel = false;
        //Debug.Log("accelreleased");
    }

    public void onBrakePress(BaseEventData eventData)
    {
        brake = true;
    }

    public void onBrakeRelease(BaseEventData eventData)
    {
        brake = false;
    }

    void changeUp()
    {
        currentGear += 1;
        currentPower = power * (ratios[2] / ratios[currentGear]);
    }
    void changeDown()
    {
        currentGear -= 1;
        currentPower = power * (ratios[2] / ratios[currentGear]);
    }
}
