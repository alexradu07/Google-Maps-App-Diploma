using UnityEngine;
using Google.Maps.Coord;
using Google.Maps.Event;
using Google.Maps;
using System.Net.Http;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using System.Collections;

[RequireComponent(typeof(MapsService))]
public class DeliveryOutdoorMapLoader : MonoBehaviour
{
    public MapsService mapsService;
    public PlacesApiQueryResponse objectContainer;
    public GameObjectOptions DefaultGameObjectOptions;
    public Camera minimapCamera;
    public Camera defaultCamera;
    public GameObject cameraObject;
    public GameObject groundPanel;
    public GameObject vehicle;
    private Vector3 currentPosition;
    private Vector3 previousPosition;
    private LatLng latLng = new LatLng(0, 0);
    private bool queryNeeded;
    private bool firstQuery;

    // Start is called before the first frame update
    void Start()
    {
        currentPosition = previousPosition = cameraObject.transform.position;
        // Get required MapsService component on this GameObject.
        mapsService = GetComponent<MapsService>();
        firstQuery = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (queryNeeded)
        {
            getAsyncRestaurants();
            queryNeeded = false;
        }
        if (Manager.gameStarted)
        {
            Vector3 currentOffset = cameraObject.transform.position - currentPosition;
            Vector3 previousOffset = cameraObject.transform.position - previousPosition;
            float currentDistance = currentOffset.sqrMagnitude;
            float previousDistance = previousOffset.sqrMagnitude;

            if (currentDistance > 300)
            {
                StartCoroutine(dynamicLoad());
            }

            if (previousDistance > 3000)
            {
                StartCoroutine(dynamicUnload());
            }
        }

    }

    private void AddCollidersToBuildings(MapLoadedArgs args)
    {
        GameObject[] buildingObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in buildingObjects)
        {
            if (obj.transform.parent != null && obj.transform.parent.name == "GoogleMaps")
            {
                obj.AddComponent<MeshCollider>();
            }
        }
    }

    IEnumerator dynamicLoad()
    {
        // if previously not set
        mapsService = GetComponent<MapsService>();

        mapsService.MakeMapLoadRegion().AddViewport(minimapCamera, 200).Load(DefaultGameObjectOptions);


        groundPanel.transform.position = new Vector3(cameraObject.transform.position.x, -0.05f, cameraObject.transform.position.z);

        currentPosition = cameraObject.transform.position;
        getAsyncRestaurants();
        Debug.Log("dynamic loading part of scene");

        yield return new WaitForSeconds(1);
    }

    IEnumerator dynamicUnload()
    {
        mapsService.MakeMapLoadRegion().AddCircle(minimapCamera.transform.position, 300).UnloadOutside();

        previousPosition = cameraObject.transform.position;
        getAsyncRestaurants();
        Debug.Log("dynamic unloading part of scene");

        yield return new WaitForSeconds(1);
    }

    public void LoadMap(double lat, double lng)
    {
        latLng = new LatLng(lat, lng);

        Debug.Log("Start start");

        // Set real-world location to load.
        mapsService.InitFloatingOrigin(latLng);

        // Load map with default options.
        DefaultGameObjectOptions = DefaultStyles.getDefaultStyles();
        mapsService.LoadMap(new Bounds(Vector3.zero, new Vector3(500, 0, 500)), DefaultGameObjectOptions);

        // Register a listener to be notified when the map is loaded.
        mapsService.Events.MapEvents.Loaded.AddListener(AddCollidersToBuildings);
        getAsyncRestaurants();
        Debug.Log("Start end");
    }

    async public void getAsyncRestaurants()
    {
        String apiKey = "AIzaSyDjifVlDD3A3XH1Zwj_fTJvKF4HGb5RBUg";
        try
        {
            Debug.Log("get Restaurants");
            LatLng latLng = new LatLng(0, 0);
            if (firstQuery)
            {
                latLng = new LatLng(Manager.dynamicLatitude, Manager.dynamicLongitude);
                firstQuery = false;
            }
            else
            {
                latLng = mapsService.Coords.FromVector3ToLatLng(vehicle.transform.position);
                Debug.Log("query from current position : " + latLng.ToString());
            }
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(String.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + latLng.Lat + "," + latLng.Lng + "&radius=500&fields=name&types=restaurant&key=" + apiKey));
                objectContainer = JsonConvert.DeserializeObject<PlacesApiQueryResponse>(response);
                Debug.Log("Object container size : " + objectContainer.results.Count);
                foreach (var restaurant in objectContainer.results)
                {
                    Debug.Log("restaurant : " + restaurant.name);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
            Debug.Log("Prind exceptie : " + ex.ToString());
        }


    }

    public void setQueryNeeded()
    {
        queryNeeded = true;
    }

    public void setQueryNotNeeded()
    {
        queryNeeded = false;
    }
}
