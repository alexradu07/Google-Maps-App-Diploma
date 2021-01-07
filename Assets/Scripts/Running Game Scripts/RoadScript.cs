using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Unity.Intersections;
using Google.Maps;
using Google.Maps.Event;

public class RoadScript : MonoBehaviour
{
    public float movementSpeed;

    private RoadLatticeNode startNode;
    private RoadLatticeNode endNode;
    private float slope;

    private List<RoadLatticeNode> visitedNodes = new List<RoadLatticeNode>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (startNode == null)
        {
            return;
        }

        MapsService mapsService = GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().mapsService;
        GameObject player = GameObject.Find("Player");
        Camera mainCamera = Camera.main;

        // player.GetComponent<Rigidbody>().AddForce(new Vector3(endNode.Location.x - startNode.Location.x, 0, endNode.Location.y - startNode.Location.y), ForceMode.Force);
        player.GetComponent<Rigidbody>().velocity = new Vector3(endNode.Location.x - startNode.Location.x, 0, endNode.Location.y - startNode.Location.y).normalized * movementSpeed;

        
        if ((endNode.Location.x < startNode.Location.x && endNode.Location.y > startNode.Location.y)
            || (endNode.Location.x < startNode.Location.x && endNode.Location.y < startNode.Location.y))
        {
            player.GetComponent<Rigidbody>().velocity = new Vector3(endNode.Location.x - startNode.Location.x, 0, endNode.Location.y - startNode.Location.y).normalized * movementSpeed;

            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(player.transform.position.x + 5 * Mathf.Cos(Mathf.Atan(slope)),
                                                            mainCamera.transform.position.y,
                                                            player.transform.position.z + 5 * Mathf.Sin(Mathf.Atan(slope))), 0.1f);
            mainCamera.transform.eulerAngles = Vector3.Lerp(new Vector3(mainCamera.transform.eulerAngles.x,
                                                        -90-45 - Mathf.Atan(slope) * 180 / Mathf.PI,
                                                        mainCamera.transform.eulerAngles.z), mainCamera.transform.eulerAngles, 0.1f);
        } else
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(player.transform.position.x - 5 * Mathf.Cos(Mathf.Atan(slope)),
                                                        mainCamera.transform.position.y,
                                                        player.transform.position.z - 5 * Mathf.Sin(Mathf.Atan(slope))), 0.1f);
            mainCamera.transform.eulerAngles = Vector3.Lerp(mainCamera.transform.eulerAngles, new Vector3(mainCamera.transform.eulerAngles.x,
                                                        90 - Mathf.Atan(slope) * 180 / Mathf.PI,
                                                        mainCamera.transform.eulerAngles.z), 0.1f);
        }


        // if (Input.GetKeyDown(KeyCode.N))
        if ((endNode.Location.x > startNode.Location.x && endNode.Location.y > startNode.Location.y
            && player.transform.position.x > endNode.Location.x && player.transform.position.z > endNode.Location.y)
            || (endNode.Location.x < startNode.Location.x && endNode.Location.y < startNode.Location.y
            && player.transform.position.x < endNode.Location.x && player.transform.position.z < endNode.Location.y)
            || (endNode.Location.x > startNode.Location.x && endNode.Location.y < startNode.Location.y
            && player.transform.position.x > endNode.Location.x && player.transform.position.z < endNode.Location.y)
            || (endNode.Location.x < startNode.Location.x && endNode.Location.y > startNode.Location.y
            && player.transform.position.x < endNode.Location.x && player.transform.position.z > endNode.Location.y))
        {
            startNode = endNode;
            player.transform.position = new Vector3(startNode.Location.x, player.transform.position.y, startNode.Location.y);

            visitedNodes.Add(startNode);

            List<RoadLatticeNode> neighbors = new List<RoadLatticeNode>(startNode.Neighbors);
            endNode = neighbors[neighbors.Count - 1];
            if (visitedNodes.Contains(endNode))
            {
                endNode = neighbors[neighbors.Count - 2];
            }

            slope = (startNode.Location.y - endNode.Location.y) / (startNode.Location.x - endNode.Location.x);
            Debug.Log(slope);
            /*
            Debug.Log(startNode.Location.x);
            Debug.Log(startNode.Location.y);
            Debug.Log(endNode.Location.x);
            Debug.Log(endNode.Location.y);
            */
            
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

        slope = (startNode.Location.y - endNode.Location.y) / (startNode.Location.x - endNode.Location.x);
        Debug.Log(slope);
        /*
        Debug.Log(startNode.Location.x);
        Debug.Log(startNode.Location.y);
        Debug.Log(endNode.Location.x);
        Debug.Log(endNode.Location.y);
        */

        mainCamera.transform.position = new Vector3(player.transform.position.x - 5 * Mathf.Cos(Mathf.Atan(slope)),
                                                    mainCamera.transform.position.y,
                                                    player.transform.position.z - 5 * Mathf.Sin(Mathf.Atan(slope)));
        mainCamera.transform.eulerAngles = new Vector3(mainCamera.transform.eulerAngles.x,
                                                    90 - Mathf.Atan(slope) * 180 / Mathf.PI,
                                                    mainCamera.transform.eulerAngles.z);

        // player.GetComponent<Rigidbody>().AddForce(new Vector3(endNode.Location.x - startNode.Location.x, 0, endNode.Location.y - startNode.Location.y), ForceMode.Force);
        player.GetComponent<Rigidbody>().velocity = new Vector3(endNode.Location.x - startNode.Location.x, 0, endNode.Location.y - startNode.Location.y).normalized * movementSpeed;

    }
}
