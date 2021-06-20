using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Google.Maps.Coord;
using Google.Maps.Event;
using Google.Maps;

//namespace Diploma
//{

[RequireComponent(typeof(MapsService))]
public class MapLoaderRunningGame : MonoBehaviour
{
    private LatLng latLng = new LatLng(0, 0);

    //public MapsService MapsService;
    public MapsService mapsService;
    public GameObjectOptions DefaultGameObjectOptions;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
        mapsService = GetComponent<MapsService>();

        // Set real-world location to load.
        mapsService.InitFloatingOrigin(latLng);

        // Register a listener to be notified when the map is loaded.
        // mapsService.Events.MapEvents.Loaded.AddListener(OnLoaded);

        // Load map with default options.
        DefaultGameObjectOptions = DefaultStyles.getDefaultStyles();
        if (SceneManager.GetActiveScene().name == "RunningScene")
        {
            mapsService.LoadMap(new Bounds(Vector3.zero, new Vector3(500, 0, 500)), DefaultGameObjectOptions);
        } else
        {
            mapsService.LoadMap(new Bounds(Vector3.zero, new Vector3(5000, 0, 5000)), DefaultGameObjectOptions);
        }

        mapsService.Events.MapEvents.Loaded.AddListener(AddCollidersToBuildings);
        // mapsService.Events.MapEvents.Loaded.AddListener(RoadScript.GenerateRoad);
        if (SceneManager.GetActiveScene().name == "RunningScene")
        {
            mapsService.Events.MapEvents.Loaded.AddListener(GameObject.Find("RoadController").GetComponent<RoadScript>().GenerateRoad);
        }

        Debug.Log("Start end");
    }
}

//}
