using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Unity.Intersections;
using Google.Maps;
using Google.Maps.Event;

public class RoadScript : MonoBehaviour
{
    private RoadLatticeNode startNode;
    private RoadLatticeNode endNode;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MapsService mapsService = GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().mapsService;
        GameObject player = GameObject.Find("Player");
        Camera mainCamera = Camera.main;

        if (Input.GetKeyDown(KeyCode.N))
        {
            startNode = endNode;
            player.transform.position = new Vector3(startNode.Location.x, player.transform.position.y, startNode.Location.y);

            List<RoadLatticeNode> neighbors = new List<RoadLatticeNode>(startNode.Neighbors);
            endNode = neighbors[neighbors.Count - 1];

            float slope = (startNode.Location.y - endNode.Location.y) / (startNode.Location.x - endNode.Location.x);
            Debug.Log(slope);
            Debug.Log(startNode.Location.x);
            Debug.Log(startNode.Location.y);
            Debug.Log(endNode.Location.x);
            Debug.Log(endNode.Location.y);

            mainCamera.transform.position = new Vector3(player.transform.position.x - 5 * Mathf.Cos(Mathf.Atan(slope)),
                                                        mainCamera.transform.position.y,
                                                        player.transform.position.z - 5 * Mathf.Sin(Mathf.Atan(slope)));
            mainCamera.transform.eulerAngles = new Vector3(mainCamera.transform.eulerAngles.x,
                                                        90 - Mathf.Atan(slope) * 180 / Mathf.PI,
                                                        mainCamera.transform.eulerAngles.z);
        }
    }

    public void GenerateRoad(MapLoadedArgs args)
    {
        MapsService mapsService = GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().mapsService;
        GameObject player = GameObject.Find("Player");
        Camera mainCamera = Camera.main;

        List<RoadLatticeNode> nodes = new List<RoadLatticeNode>(mapsService.RoadLattice.Nodes);

        float minX = float.MaxValue;
        ulong startPosUID = 0;
        foreach (RoadLatticeNode node in nodes)
        {
            if (node.Location.x < minX)
            {
                minX = node.Location.x;
                startPosUID = node.LocationUID;
            }
        }

        startNode = mapsService.RoadLattice.FindNodeAt(startPosUID);
        player.transform.position = new Vector3(startNode.Location.x, player.transform.position.y, startNode.Location.y);

        List<RoadLatticeNode> neighbors = new List<RoadLatticeNode>(startNode.Neighbors);
        endNode = neighbors[0];

        float slope = (startNode.Location.y - endNode.Location.y) / (startNode.Location.x - endNode.Location.x);
        Debug.Log(slope);
        Debug.Log(startNode.Location.x);
        Debug.Log(startNode.Location.y);
        Debug.Log(endNode.Location.x);
        Debug.Log(endNode.Location.y);

        mainCamera.transform.position = new Vector3(player.transform.position.x - 5 * Mathf.Cos(Mathf.Atan(slope)),
                                                    mainCamera.transform.position.y,
                                                    player.transform.position.z - 5 * Mathf.Sin(Mathf.Atan(slope)));
        mainCamera.transform.eulerAngles = new Vector3(mainCamera.transform.eulerAngles.x,
                                                    90 - Mathf.Atan(slope) * 180 / Mathf.PI,
                                                    mainCamera.transform.eulerAngles.z);

    }
}
