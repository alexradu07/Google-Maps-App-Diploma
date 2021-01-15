using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Unity.Intersections;
using Google.Maps;
using Google.Maps.Event;

public class RoadScript : MonoBehaviour
{
    public float movementSpeed;
    public float lateralMovementAmount;

    private RoadLatticeNode startNode;
    private RoadLatticeNode endNode;
    private RoadLatticeNode prevNode;
    private float slope;

    private List<RoadLatticeNode> visitedNodes = new List<RoadLatticeNode>();
    private Vector3 runningDirection;

    /*
     * -1 -> left
     *  0 -> center
     *  1 -> right
     */
    private int roadPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GameObject player = GameObject.Find("Player");

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // Move to the left
            // player.GetComponent<Rigidbody>().velocity = new Vector3(endNode.Location.x - startNode.Location.x, 0, startNode.Location.y - endNode.Location.y).normalized * movementSpeed;
            // player.transform.position = player.transform.position + new Vector3(endNode.Location.x - startNode.Location.x, 0, startNode.Location.y - endNode.Location.y).normalized;
            
            if (roadPosition > -1)
            {
                player.transform.position = player.transform.position + Vector3.Cross(runningDirection.normalized * lateralMovementAmount,
                                                    Vector3.up);
                roadPosition--;
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // Move to the right
            // player.GetComponent<Rigidbody>().velocity = new Vector3(startNode.Location.x - endNode.Location.x, 0, endNode.Location.y - startNode.Location.y).normalized * movementSpeed;
            // player.transform.position = player.transform.position + new Vector3(startNode.Location.x - endNode.Location.x, 0, endNode.Location.y - startNode.Location.y).normalized;
            
            if (roadPosition < 1)
            {
                player.transform.position = player.transform.position - Vector3.Cross(runningDirection.normalized * lateralMovementAmount,
                                                    Vector3.up);
                roadPosition++;
            }
        }
    }

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
        runningDirection = new Vector3(endNode.Location.x - startNode.Location.x, 0, endNode.Location.y - startNode.Location.y);

        player.GetComponent<Rigidbody>().velocity = runningDirection.normalized * movementSpeed;

        if ((endNode.Location.x < startNode.Location.x && endNode.Location.y > startNode.Location.y)
            || (endNode.Location.x < startNode.Location.x && endNode.Location.y < startNode.Location.y))
        {
            player.GetComponent<Rigidbody>().velocity = runningDirection.normalized * movementSpeed;

            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(player.transform.position.x + 5 * Mathf.Cos(Mathf.Atan(slope)),
                                                            mainCamera.transform.position.y,
                                                            player.transform.position.z + 5 * Mathf.Sin(Mathf.Atan(slope))), 0.1f);
            /*
            mainCamera.transform.eulerAngles = Vector3.Lerp(new Vector3(mainCamera.transform.eulerAngles.x,
                                                        -90-45 - Mathf.Atan(slope) * 180 / Mathf.PI,
                                                        mainCamera.transform.eulerAngles.z), mainCamera.transform.eulerAngles, 0.1f);
                                                        */
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, Quaternion.Euler(new Vector3(mainCamera.transform.eulerAngles.x,
                                                        -90 - Mathf.Atan(slope) * 180 / Mathf.PI,
                                                        mainCamera.transform.eulerAngles.z)), 0.1f);
        } else
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(player.transform.position.x - 5 * Mathf.Cos(Mathf.Atan(slope)),
                                                        mainCamera.transform.position.y,
                                                        player.transform.position.z - 5 * Mathf.Sin(Mathf.Atan(slope))), 0.1f);
            /*
            mainCamera.transform.eulerAngles = Vector3.Lerp(mainCamera.transform.eulerAngles, new Vector3(mainCamera.transform.eulerAngles.x,
                                                        90 - Mathf.Atan(slope) * 180 / Mathf.PI,
                                                        mainCamera.transform.eulerAngles.z), 0.1f);
                                                        */
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, Quaternion.Euler(new Vector3(mainCamera.transform.eulerAngles.x,
                                                        90 - Mathf.Atan(slope) * 180 / Mathf.PI,
                                                        mainCamera.transform.eulerAngles.z)), 0.1f);
        }

        // Calculate player center position
        Vector3 playerCenterPosition = player.transform.position;
        if (roadPosition == -1)
        {
            playerCenterPosition = player.transform.position
                    - Vector3.Cross(runningDirection.normalized * lateralMovementAmount, Vector3.up);
        } else if (roadPosition == 1) {
            playerCenterPosition = player.transform.position
                    + Vector3.Cross(runningDirection.normalized * lateralMovementAmount, Vector3.up);
        }

        // Player passed the endNode, new segment is used
        if ((endNode.Location.x > startNode.Location.x && endNode.Location.y > startNode.Location.y
            && playerCenterPosition.x >= endNode.Location.x && playerCenterPosition.z >= endNode.Location.y)
            || (endNode.Location.x < startNode.Location.x && endNode.Location.y < startNode.Location.y
            && playerCenterPosition.x <= endNode.Location.x && playerCenterPosition.z <= endNode.Location.y)
            || (endNode.Location.x > startNode.Location.x && endNode.Location.y < startNode.Location.y
            && playerCenterPosition.x >= endNode.Location.x && playerCenterPosition.z <= endNode.Location.y)
            || (endNode.Location.x < startNode.Location.x && endNode.Location.y > startNode.Location.y
            && playerCenterPosition.x <= endNode.Location.x && playerCenterPosition.z >= endNode.Location.y))
        {
            prevNode = startNode;
            startNode = endNode;
            visitedNodes.Add(startNode);

            // Choose neighbor and update runningDirection
            List<RoadLatticeNode> neighbors = new List<RoadLatticeNode>(startNode.Neighbors);
            /*
            endNode = neighbors[neighbors.Count - 1];
            if (visitedNodes.Contains(endNode))
            {
                endNode = neighbors[neighbors.Count - 2];
            }
            */
            float angle;
            float minAngle = 0;
            float maxAngle = 0;
            float closestToZeroAngle = 180;
            if (neighbors.Count == 1)
            {
                // Dead end road
                endNode = prevNode;
            }
            else if (neighbors.Count == 2)
            {
                // Straight road
                foreach (RoadLatticeNode neigh in neighbors)
                {
                    if (neigh != prevNode)
                    {
                        endNode = neigh;
                    }
                }
            }
            else
            {
                // Intersection
                if (roadPosition == 0)
                {
                    // Center position
                    /*
                    if (neighbors.Count == 3)
                    {
                        foreach (RoadLatticeNode neigh in neighbors)
                        {
                            angle = Vector3.SignedAngle(runningDirection, new Vector3(neigh.Location.x, 0, neigh.Location.y) - player.transform.position, Vector3.up);
                            if (angle < minAngle)
                            {
                                minAngle = angle;
                                endNode = neigh;
                            }
                        }
                    }
                    */
                    //else if (neighbors.Count >= 3)
                    //{
                    foreach (RoadLatticeNode neigh in neighbors)
                    {
                        if (neigh == prevNode)
                        {
                            continue;
                        }
                        angle = Vector3.SignedAngle(runningDirection,
                            new Vector3(neigh.Location.x - startNode.Location.x, 0, neigh.Location.y - startNode.Location.y), Vector3.up);
                        if (Mathf.Abs(angle) <= closestToZeroAngle)
                        {
                            closestToZeroAngle = Mathf.Abs(angle);
                            endNode = neigh;
                        }
                    }
                    //}
                }
                else if (roadPosition == -1)
                {
                    // Left position
                    foreach (RoadLatticeNode neigh in neighbors)
                    {
                        angle = Vector3.SignedAngle(runningDirection,
                            new Vector3(neigh.Location.x - prevNode.Location.x, 0, neigh.Location.y - prevNode.Location.y), Vector3.up);
                        if (angle < minAngle)
                        {
                            minAngle = angle;
                            endNode = neigh;
                        }
                    }
                }
                else if (roadPosition == 1)
                {
                    // Right position
                    foreach (RoadLatticeNode neigh in neighbors)
                    {
                        angle = Vector3.SignedAngle(runningDirection,
                            new Vector3(neigh.Location.x - prevNode.Location.x, 0, neigh.Location.y - prevNode.Location.y), Vector3.up);
                        if (angle > maxAngle)
                        {
                            maxAngle = angle;
                            endNode = neigh;
                        }
                    }
                }
            }
            runningDirection = new Vector3(endNode.Location.x - startNode.Location.x, 0, endNode.Location.y - startNode.Location.y);

            // Keep lane when new segment is used
            if (roadPosition == 0)
            {
                // Center position
                player.transform.position = new Vector3(startNode.Location.x, player.transform.position.y, startNode.Location.y);
            }
            else if (roadPosition == 1)
            {
                // Right position
                //player.transform.position = new Vector3(startNode.Location.x, player.transform.position.y, startNode.Location.y)
                //    + new Vector3(endNode.Location.x - startNode.Location.x, 0, startNode.Location.y - endNode.Location.y).normalized;
                player.transform.position = new Vector3(startNode.Location.x, player.transform.position.y, startNode.Location.y)
                    - Vector3.Cross(runningDirection.normalized * lateralMovementAmount, Vector3.up);
            }
            else if (roadPosition == -1)
            {
                // Left position
                //player.transform.position = new Vector3(startNode.Location.x, player.transform.position.y, startNode.Location.y)
                //    + new Vector3(startNode.Location.x - endNode.Location.x, 0, endNode.Location.y - startNode.Location.y).normalized;
                player.transform.position = new Vector3(startNode.Location.x, player.transform.position.y, startNode.Location.y)
                    + Vector3.Cross(runningDirection.normalized * lateralMovementAmount, Vector3.up);
            }
            // player.transform.position = Vector3.Lerp(player.transform.position, new Vector3(startNode.Location.x, 0, startNode.Location.y), 0.1f);


            slope = (startNode.Location.y - endNode.Location.y) / (startNode.Location.x - endNode.Location.x);
            // Debug.Log(slope);
            
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
