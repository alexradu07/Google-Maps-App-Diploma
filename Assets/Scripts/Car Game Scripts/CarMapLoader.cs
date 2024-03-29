﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Coord;
using Google.Maps.Event;
using Google.Maps;
using UnityEngine.Events;
using System;
using System.Threading;
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
    private Vector3 oldPos1;
    private Vector3 oldPos2;
    private MapsService mapsService;
    public Camera cam;
    public Camera carCam;


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
        if (NavigationScript.needToUnload == true)
        {
            StartCoroutine(deleteAsync());
            NavigationScript.needToUnload = false;
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
        mapsService.MakeMapLoadRegion().AddCircle(cam.transform.position, 400).Load(DefaultGameObjectOptions);
        groundPlane.transform.position = new Vector3(cameraObj.transform.position.x, -0.01f, cameraObj.transform.position.z);
        oldPos1 = cameraObj.transform.position;
        yield return null;
    }
    IEnumerator addColliders()
    {
        GameObject[] buildingObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in buildingObjects)
        {
            if (obj.transform.parent != null && !obj.name.Contains("Segment") && !obj.name.Contains("Region") && !obj.name.Contains("AreaWater") && obj.transform.parent.name == "GoogleMaps")
            {
                obj.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            }
            if (obj.transform.parent != null && obj.transform.parent.name == "GoogleMaps" && !obj.name.Contains("Segment"))
            {
                obj.AddComponent<MeshCollider>();
            }
        }
        yield return null;
    }
    IEnumerator deleteAsync()
    {
        mapsService.MakeMapLoadRegion()
                     .AddCircle(cam.transform.position, 400)
                     .UnloadOutside();
        oldPos2 = cameraObj.transform.position;
        yield return null;
    }

    private void AddCollidersToBuildings(MapLoadedArgs args)
    {
        StartCoroutine(addColliders());
    }

    public void LoadMap(double lat, double lng, double lat1, double lng1)
    {
        latLng = new LatLng(lat, lng);

        Debug.Log("Start start");
        MapsService mapsService = GetComponent<MapsService>();

        mapsService.InitFloatingOrigin(new LatLng(lat1, lng1));
        DefaultGameObjectOptions = DefaultStyles.getDefaultStyles();
        mapsService.LoadMap(new Bounds(Vector3.zero, new Vector3(300, 0, 300)), DefaultGameObjectOptions);
        mapsService.MoveFloatingOrigin(new LatLng(lat, lng), null);
        mapsService.LoadMap(new Bounds(Vector3.zero, new Vector3(300, 0, 300)), DefaultGameObjectOptions);


        mapsService.Events.MapEvents.Loaded.AddListener(AddCollidersToBuildings);

        Debug.Log("Start end");
    }
}

//}
