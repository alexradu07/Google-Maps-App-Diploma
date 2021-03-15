using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Coord;
using Google.Maps.Event;
using Google.Maps;
//namespace Diploma
//{

[RequireComponent(typeof(MapsService))]
public class CarMapLoader : MonoBehaviour
{
    private LatLng latLng = new LatLng(0, 0);
    public GameObject cameraObj;
    //public MapsService MapsService;
    public GameObjectOptions DefaultGameObjectOptions;
    public GameObject groundPlane;
    private Vector3 oldPos;
    private bool canLoad = false;
    private bool canUnload = false;
    private MapsService mapsService;

    // Start is called before the first frame update
    void Start()
    {
        oldPos = cameraObj.transform.position;
        mapsService = GetComponent<MapsService>();
        startCoroutine();
    }

    private void startCoroutine()
    {
        StartCoroutine(unload());
    }

    private IEnumerator unload()
    {
        while (true)
        {
            if (canUnload == true)
            {
                mapsService.MakeMapLoadRegion()
                     .AddCircle(Camera.main.transform.position, 1000)
                     .UnloadOutside();
                canUnload = false;
            }
            Debug.Log("penis");
        }
        yield return new WaitForSeconds(1);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 offset = cameraObj.transform.position - oldPos;
        float dist = offset.sqrMagnitude;
        Debug.Log(dist);
        if (dist > 1000)
        {
            mapsService = GetComponent<MapsService>();
            mapsService.LoadMap(new Bounds(cameraObj.transform.position, new Vector3(50, 0, 50)), DefaultGameObjectOptions);
            groundPlane.transform.position = new Vector3(cameraObj.transform.position.x, 0, cameraObj.transform.position.z);
            oldPos = cameraObj.transform.position;
            canUnload = true;
        }
    }

    private void AddCollidersToBuildings(MapLoadedArgs args)
    {
        GameObject[] buildingObjects = GameObject.FindObjectsOfType<GameObject>();
        // Debug.Log(buildingObjects.Length);
        foreach (GameObject obj in buildingObjects)
        {
            // if (obj.name.StartsWith("ExtrudedStructure "))
            if (obj.transform.parent != null && obj.transform.parent.name == "GoogleMaps")
            {
                obj.AddComponent<MeshCollider>();
            }
        }
    }

    public void LoadMap(double lat, double lng)
    {
        latLng = new LatLng(lat, lng);

        Debug.Log("Start start");
        // Get required MapsService component on this GameObject.
        MapsService mapsService = GetComponent<MapsService>();

        // Set real-world location to load.
        mapsService.InitFloatingOrigin(latLng);

        // Register a listener to be notified when the map is loaded.
        // mapsService.Events.MapEvents.Loaded.AddListener(OnLoaded);

        // Load map with default options.
        DefaultGameObjectOptions = DefaultStyles.getDefaultStyles();
        mapsService.LoadMap(new Bounds(Vector3.zero, new Vector3(500, 0, 500)), DefaultGameObjectOptions);

        mapsService.Events.MapEvents.Loaded.AddListener(AddCollidersToBuildings);

        Debug.Log("Start end");
    }
}

//}
