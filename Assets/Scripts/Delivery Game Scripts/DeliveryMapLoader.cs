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
public class DeliveryMapLoader : MonoBehaviour
{
    public MapsService mapsService;
    public PlacesApiQueryResponse objectContainer;
    public GameObjectOptions DefaultGameObjectOptions;
    public Camera minimapCameraTukTuk;
    public Camera minimapCameraDodge;
    public GameObject tuktuk;
    public GameObject dodge;
    public Camera defaultCamera;
    public GameObject cameraObjectTukTuk;
    public GameObject cameraObjectDodge;
    public GameObject groundPanel;
    public GameObject dropdownLabel;
    public GameObject vehicle;
    private Vector3 currentPosition;
    private Vector3 previousPosition;
    private LatLng latLng = new LatLng(0, 0);
    private bool queryNeeded;
    private bool firstQuery;

    // Start is called before the first frame update
    void Start()
    {
        if (dodge.activeSelf)
        {
            currentPosition = previousPosition = cameraObjectDodge.transform.position;
        }
        else
        {
            currentPosition = previousPosition = cameraObjectTukTuk.transform.position;
        }
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
        //if (Manager.gameStarted && Manager.dynamicLoadingUnlocked == 1)
        if (Manager.gameStarted) // for debug
        {
            Vector3 currentOffset = Vector3.zero;
            Vector3 previousOffset = Vector3.zero;
            if (dodge.activeSelf)
            {
                currentOffset = cameraObjectDodge.transform.position - currentPosition;
                previousOffset = cameraObjectDodge.transform.position - previousPosition;
            }
            else
            {
                currentOffset = cameraObjectTukTuk.transform.position - currentPosition;
                previousOffset = cameraObjectTukTuk.transform.position - previousPosition;
            }
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

        if (dodge.activeSelf)
        {
            mapsService.MakeMapLoadRegion().AddViewport(minimapCameraDodge, 200).Load(DefaultGameObjectOptions);
            groundPanel.transform.position = new Vector3(cameraObjectDodge.transform.position.x, -0.05f, cameraObjectDodge.transform.position.z);

            currentPosition = cameraObjectDodge.transform.position;
        }
        else
        {
            mapsService.MakeMapLoadRegion().AddViewport(minimapCameraTukTuk, 200).Load(DefaultGameObjectOptions);
            groundPanel.transform.position = new Vector3(cameraObjectTukTuk.transform.position.x, -0.05f, cameraObjectTukTuk.transform.position.z);

            currentPosition = cameraObjectTukTuk.transform.position;
        }
        getAsyncRestaurants();
        Debug.Log("dynamic loading part of scene");

        yield return new WaitForSeconds(1);
    }

    IEnumerator dynamicUnload()
    {
        if (dodge.activeSelf)
        {
            mapsService.MakeMapLoadRegion().AddCircle(minimapCameraDodge.transform.position, 300).UnloadOutside();
            previousPosition = cameraObjectDodge.transform.position;
        }
        else
        {
            mapsService.MakeMapLoadRegion().AddCircle(minimapCameraTukTuk.transform.position, 300).UnloadOutside();
            previousPosition = cameraObjectTukTuk.transform.position;
        }
        
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
                string locationString = dropdownLabel.GetComponent<Text>().text;
                switch (locationString)
                {
                    case "Grozavesti":
                        latLng = new LatLng(44.4433837, 26.0618934);
                        break;
                    case "Unirii":
                        latLng = new LatLng(44.426929, 26.1011807);
                        break;
                    case "Brasov":
                        latLng = new LatLng(45.6431122, 25.5858238);
                        break;
                    case "Current Location":
                        latLng = new LatLng(Manager.dynamicLatitude, Manager.dynamicLongitude);
                        break;
                    default:
                        break;
                }
                firstQuery = false;
            } else
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