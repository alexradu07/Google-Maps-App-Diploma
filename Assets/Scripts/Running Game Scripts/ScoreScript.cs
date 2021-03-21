using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Maps;
using Google.Maps.Coord;

public class ScoreScript : MonoBehaviour
{
    public GameObject player;
    public GameObject scorePanel;

    private double score;
    private const int earthRadius = 6371000;
    private double deltaLat;
    private double deltaLng;
    private double a;
    private double c;

    private MapsService mapsService;
    private LatLng currLatLng;
    private LatLng prevLatLng;

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        mapsService = GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().mapsService;
    }

    // Update is called once per frame
    void Update()
    {
        if (mapsService == null)
        {
            mapsService = GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().mapsService;
            return;
        }
        
        if (currLatLng.Lat == 0 && currLatLng.Lng == 0)
        {
            // First coordinates
            currLatLng = mapsService.Coords.FromVector3ToLatLng(player.transform.position);
            return;
        }

        prevLatLng = currLatLng;
        currLatLng = mapsService.Coords.FromVector3ToLatLng(player.transform.position);

        deltaLat = (currLatLng.Lat - prevLatLng.Lat) * Mathf.PI/180;
        deltaLng = (currLatLng.Lng - prevLatLng.Lng) * Mathf.PI/180;

        a = System.Math.Sin(deltaLat / 2) * System.Math.Sin(deltaLat / 2)
            + System.Math.Cos(prevLatLng.Lat * Mathf.PI/180) * System.Math.Cos(currLatLng.Lat * Mathf.PI / 180)
            * System.Math.Sin(deltaLng / 2) * System.Math.Sin(deltaLng / 2);
        c = 2 * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1 - a));

        if (earthRadius * c > 1)
        {
            // Movement is too big and should not be registered
            return;
        }
        score += earthRadius * c;

        scorePanel.GetComponentInChildren<Text>().text = ((int)score).ToString() + " m";
    }
}
