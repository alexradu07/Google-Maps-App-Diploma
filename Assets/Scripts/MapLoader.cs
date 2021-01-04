using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Coord;
using Google.Maps.Event;
using Google.Maps;

//namespace Diploma
//{

[RequireComponent(typeof(MapsService))]
public class MapLoader : MonoBehaviour
{
    private LatLng latLng = new LatLng(0, 0);

    //public MapsService MapsService;
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
