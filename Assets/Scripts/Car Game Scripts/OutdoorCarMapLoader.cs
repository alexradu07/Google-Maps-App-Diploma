using System.Collections;
using UnityEngine;
using Google.Maps.Coord;
using Google.Maps.Event;
using Google.Maps;
//namespace Diploma
//{

[RequireComponent(typeof(MapsService))]
public class OutdoorCarMapLoader : MonoBehaviour
{
    private LatLng latLng = new LatLng(0, 0);
    public GameObject cameraObj;
    //public MapsService MapsService;
    public GameObjectOptions DefaultGameObjectOptions;
    public GameObject groundPlane;
    private Vector3 oldPos1;
    private Vector3 oldPos2;
    public static MapsService mapsService;
    public Camera cam;
    public Camera carCam;
    public static bool initSet = false;


    // Start is called before the first frame update
    void Start()
    {
        oldPos1 = cameraObj.transform.position;
        oldPos2 = cameraObj.transform.position;
        mapsService = GetComponent<MapsService>();
    }



    // Update is called once per frame
    void Update()
    {
        Vector3 offset1 = cameraObj.transform.position - oldPos1;
        Vector3 offset2 = cameraObj.transform.position - oldPos2;
        float dist1 = offset1.sqrMagnitude;
        float dist2 = offset2.sqrMagnitude;
        //Debug.Log(dist);
        if (OutdoorNavigationScript.needToUnload == true)
        {
            StartCoroutine(deleteAsync());
            OutdoorNavigationScript.needToUnload = false;
        }
        if (dist1 > 8000)
        {
            StartCoroutine(loadAsync());
        }
        if (dist2 > 20000)
        {
            StartCoroutine(deleteAsync());
        }
    }
    IEnumerator loadAsync()
    {
        mapsService = GetComponent<MapsService>();
        //mapsService.LoadMap(new Bounds(cameraObj.transform.position, new Vector3(200, 0, 200)), DefaultGameObjectOptions);
        //mapsService.MakeMapLoadRegion().AddCircle(cam.transform.position, 100).Load(DefaultGameObjectOptions);
        mapsService.MakeMapLoadRegion().AddCircle(cam.transform.position, 400).Load(DefaultGameObjectOptions);
        groundPlane.transform.position = new Vector3(cameraObj.transform.position.x, -0.01f, cameraObj.transform.position.z);
        oldPos1 = cameraObj.transform.position;
        //canUnload = true;
        Debug.Log("dau load");
        yield return null;
    }
    IEnumerator deleteAsync()
    {
        mapsService.MakeMapLoadRegion()
                     .AddCircle(cam.transform.position, 400)
                     .UnloadOutside();
        Debug.Log("dau unload");
        oldPos2 = cameraObj.transform.position;
        yield return null;
    }

    private void AddCollidersToBuildings(MapLoadedArgs args)
    {
        GameObject[] buildingObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in buildingObjects)
        {
            if (obj.transform.parent != null && !obj.name.Contains("Segment") && !obj.name.Contains("Region") && !obj.name.Contains("AreaWater") && obj.transform.parent.name == "GoogleMaps")
            {
                obj.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            }
            if (obj.transform.parent != null && obj.transform.parent.name == "GoogleMaps")
            {
                obj.AddComponent<MeshCollider>();
            }
        }
    }

    public void LoadMap(double lat, double lng, double lat1, double lng1)
    {
        latLng = new LatLng(lat, lng);

        Debug.Log("Start start");
        // Get required MapsService component on this GameObject.
        MapsService mapsService = GetComponent<MapsService>();

        // Set real-world location to load.
        mapsService.InitFloatingOrigin(new LatLng(lat1, lng1));

        // Register a listener to be notified when the map is loaded.
        // mapsService.Events.MapEvents.Loaded.AddListener(OnLoaded);

        // Load map with default options.
        DefaultGameObjectOptions = DefaultStyles.getDefaultStyles();
        mapsService.LoadMap(new Bounds(Vector3.zero, new Vector3(300, 0, 300)), DefaultGameObjectOptions);

        mapsService.MoveFloatingOrigin(new LatLng(lat, lng), null);
        mapsService.LoadMap(new Bounds(Vector3.zero, new Vector3(300, 0, 300)), DefaultGameObjectOptions);
        initSet = true;

        mapsService.Events.MapEvents.Loaded.AddListener(AddCollidersToBuildings);

        Debug.Log("Start end");
    }
}

//}
