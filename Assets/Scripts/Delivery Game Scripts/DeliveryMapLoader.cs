using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Coord;
using Google.Maps.Event;
using Google.Maps;
using System.Net.Http;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

//namespace Diploma
//{

[RequireComponent(typeof(MapsService))]
public class DeliveryMapLoader : MonoBehaviour
{
    private LatLng latLng = new LatLng(0, 0);
    private static MapsService mapsService;
    // public static Renderer renderer;
    private static GameObject cylinder;

    //public MapsService MapsService;
    public GameObjectOptions DefaultGameObjectOptions;

    // Start is called before the first frame update
    void Start()
    {
        // renderer = GetComponent<Renderer>();
        cylinder = GameObject.Find("Marker");
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
        mapsService.LoadMap(new Bounds(Vector3.zero, new Vector3(500, 0, 500)), DefaultGameObjectOptions);
        // mapsService.InitFloatingOrigin(new LatLng(lat, lng));
        mapsService.Events.MapEvents.Loaded.AddListener(AddCollidersToBuildings);
        getAsyncRestaurants();
        Debug.Log("Start end");
    }

    static async public void getAsyncRestaurants()
    {
        String apiKey = "AIzaSyDjifVlDD3A3XH1Zwj_fTJvKF4HGb5RBUg";
        try
        {
            LatLng latLng = new LatLng(0, 0);
            string locationString = GameObject.Find("Canvas/LocationDropdownSelector/Label").GetComponent<Text>().text;
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
                default:
                    break;
            }
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(String.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + latLng.Lat + "," + latLng.Lng + "&radius=500&fields=name&types=restaurant&key=" + apiKey));

                PlacesApiQueryResponse objectContainer = JsonConvert.DeserializeObject<PlacesApiQueryResponse>(response);
                foreach (var result in objectContainer.results)
                {
                    Debug.Log(result.name);
                    if (result.name.Equals("Spartan"))
                    {
                        Debug.Log("Spartan latitude : " + result.geometry.location.lat);
                        Debug.Log("Spartan longitude : " + result.geometry.location.lng);
                        Vector3 restaurantPosition = mapsService.Coords.FromLatLngToVector3(new LatLng(result.geometry.location.lat, result.geometry.location.lng));
                        // GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        cylinder.transform.position = restaurantPosition + new Vector3(0, 100.5f, 0);
                        // cylinder.transform.position = new Vector3(-2, 1, 0);
                        // renderer.material.SetColor("_Color", Color.red);
                    }
                }

            }

        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }


    }
}

//}
