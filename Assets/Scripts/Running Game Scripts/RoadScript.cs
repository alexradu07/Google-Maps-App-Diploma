using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Maps.Unity.Intersections;
using Google.Maps;
using Google.Maps.Event;

public class RoadScript : MonoBehaviour
{
    public GameObject player;
    public float movementSpeed;
    public float lateralMovementAmount;
    public List<GameObject> obstacles;
    public float distanceBetweenObstacles;
    public float cameraLerpPos;
    public float cameraLerpRot;

    public int powerupsRate;
    public GameObject speedPowerup;

    private MapsService mapsService;

    public GameObject roadNamePanel;
    public GameObject scorePanel;
    public GameObject timerPanel;
    public GameObject startButton;
    public GameObject miniMap;
    private string roadName;

    private RoadLatticeNode startNode;
    private RoadLatticeNode endNode;
    private RoadLatticeNode prevNode;
    private RoadLatticeNode nextNode;
    //private float slope;

    private List<RoadLatticeNode> visitedNodes = new List<RoadLatticeNode>();
    private Vector3 runningDirection;
    private Vector3 nextRunningDirection;
    private float cameraAngle;
    //public int iterations = 0;
    //public int maxIterations;
    //public int maxIt;

    /*
     * -1 -> left
     *  0 -> center
     *  1 -> right
     */
    private int roadPosition;
    private int prevRoadPosition;

    public string difficulty;
    private bool hasGameStarted;
    private float movementSpeedIncrease;
    private Vector3 playerCenterPosition;

    // Start is called before the first frame update
    void Start()
    {
        hasGameStarted = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (startNode == null || !hasGameStarted)
        {
            return;
        }
        if (this.GetComponent<TimerScript>().isGameOver)
        {
            // Game over
            return;
        }
        //GameObject player = GameObject.Find("Player");

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveRight();
        }

        Camera mainCamera = Camera.main;
        if ((endNode.Location.x < startNode.Location.x && endNode.Location.y > startNode.Location.y)
            || (endNode.Location.x < startNode.Location.x && endNode.Location.y < startNode.Location.y))
        {
            //player.GetComponent<Rigidbody>().velocity = runningDirection.normalized * movementSpeed;

            /*
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(player.transform.position.x + 5 * Mathf.Cos(Mathf.Atan(slope)),
                                                            mainCamera.transform.position.y,
                                                            player.transform.position.z + 5 * Mathf.Sin(Mathf.Atan(slope))), 0.1f);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, Quaternion.Euler(new Vector3(mainCamera.transform.eulerAngles.x,
                                                        -90 - Mathf.Atan(slope) * 180 / Mathf.PI,
                                                        mainCamera.transform.eulerAngles.z)), 0.1f);
            */

            Vector3 cameraPos = player.transform.position - runningDirection.normalized * 5;
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(cameraPos.x, 2, cameraPos.z), cameraLerpPos);
            //mainCamera.transform.position += new Vector3(0, 1.5f, 0);
            //mainCamera.transform.LookAt(player.transform);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, Quaternion.Euler(new Vector3(mainCamera.transform.eulerAngles.x,
                                                        cameraAngle,
                                                        mainCamera.transform.eulerAngles.z)), cameraLerpRot);
        }
        else
        {
            /*
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(player.transform.position.x - 5 * Mathf.Cos(Mathf.Atan(slope)),
                                                        mainCamera.transform.position.y,
                                                        player.transform.position.z - 5 * Mathf.Sin(Mathf.Atan(slope))), 0.1f);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, Quaternion.Euler(new Vector3(mainCamera.transform.eulerAngles.x,
                                                        90 - Mathf.Atan(slope) * 180 / Mathf.PI,
                                                        mainCamera.transform.eulerAngles.z)), 0.1f);
            */

            Vector3 cameraPos = player.transform.position - runningDirection.normalized * 5;
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(cameraPos.x, 2, cameraPos.z), cameraLerpPos);
            //mainCamera.transform.position += new Vector3(0, 1.5f, 0);
            //mainCamera.transform.LookAt(player.transform);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, Quaternion.Euler(new Vector3(mainCamera.transform.eulerAngles.x,
                                                        cameraAngle,
                                                        mainCamera.transform.eulerAngles.z)), cameraLerpRot);
        }
        // Calculate player center position
        playerCenterPosition = player.transform.position;
        if (roadPosition == -1)
        {
            playerCenterPosition = player.transform.position
                    - Vector3.Cross(runningDirection.normalized * lateralMovementAmount, Vector3.up);
        }
        else if (roadPosition == 1)
        {
            playerCenterPosition = player.transform.position
                    + Vector3.Cross(runningDirection.normalized * lateralMovementAmount, Vector3.up);
        }
        // Check if player passed the endNode, so a new segment is used
        nextNode = FindNextNode();
        /*if ((endNode.Location.x >= startNode.Location.x && endNode.Location.y >= startNode.Location.y
            && playerCenterPosition.x >= endNode.Location.x && playerCenterPosition.z >= endNode.Location.y)
            || (endNode.Location.x <= startNode.Location.x && endNode.Location.y <= startNode.Location.y
            && playerCenterPosition.x <= endNode.Location.x && playerCenterPosition.z <= endNode.Location.y)
            || (endNode.Location.x >= startNode.Location.x && endNode.Location.y <= startNode.Location.y
            && playerCenterPosition.x >= endNode.Location.x && playerCenterPosition.z <= endNode.Location.y)
            || (endNode.Location.x <= startNode.Location.x && endNode.Location.y >= startNode.Location.y
            && playerCenterPosition.x <= endNode.Location.x && playerCenterPosition.z >= endNode.Location.y))
        {*/
        if (DidPassEndnode())
        {
            prevNode = startNode;
            startNode = endNode;
            endNode = nextNode;
            // Choose neighbor and update runningDirection
            //List<RoadLatticeNode> neighbors = new List<RoadLatticeNode>(startNode.Neighbors);
            //float angle;
            //float minAngle = 180;
            //float maxAngle = -180;
            //float closestToZeroAngle = 180;
            //if (neighbors.Count == 1)
            //{
            //    // Dead end road
            //    endNode = prevNode;
            //}
            //else if (neighbors.Count == 2)
            //{
            //    // Straight road
            //    foreach (RoadLatticeNode neigh in neighbors)
            //    {
            //        if (neigh != prevNode)
            //        {
            //            endNode = neigh;
            //        }
            //    }
            //}
            //else
            //{
            //    // Intersection
            //    if (roadPosition == 0)
            //    {
            //        // Center position
            //        /*
            //        if (neighbors.Count == 3)
            //        {
            //            foreach (RoadLatticeNode neigh in neighbors)
            //            {
            //                angle = Vector3.SignedAngle(runningDirection, new Vector3(neigh.Location.x, 0, neigh.Location.y) - player.transform.position, Vector3.up);
            //                if (angle < minAngle)
            //                {
            //                    minAngle = angle;
            //                    endNode = neigh;
            //                }
            //            }
            //        }
            //        */
            //        //else if (neighbors.Count >= 3)
            //        //{
            //        foreach (RoadLatticeNode neigh in neighbors)
            //        {
            //            if (neigh == prevNode)
            //            {
            //                continue;
            //            }
            //            angle = Vector3.SignedAngle(runningDirection,
            //                new Vector3(neigh.Location.x - startNode.Location.x, 0, neigh.Location.y - startNode.Location.y), Vector3.up);
            //            if (Mathf.Abs(angle) <= closestToZeroAngle)
            //            {
            //                closestToZeroAngle = Mathf.Abs(angle);
            //                endNode = neigh;
            //            }
            //        }
            //        //}
            //    }
            //    else if (roadPosition == -1)
            //    {
            //        // Left position
            //        foreach (RoadLatticeNode neigh in neighbors)
            //        {
            //            if (neigh == prevNode)
            //            {
            //                continue;
            //            }
            //            //angle = Vector3.SignedAngle(runningDirection,
            //            //    new Vector3(neigh.Location.x - prevNode.Location.x, 0, neigh.Location.y - prevNode.Location.y), Vector3.up);
            //            angle = Vector3.SignedAngle(runningDirection,
            //                new Vector3(neigh.Location.x - startNode.Location.x, 0, neigh.Location.y - startNode.Location.y), Vector3.up);
            //            if (angle < minAngle)
            //            {
            //                minAngle = angle;
            //                endNode = neigh;
            //            }
            //        }
            //    }
            //    else if (roadPosition == 1)
            //    {
            //        // Right position
            //        foreach (RoadLatticeNode neigh in neighbors)
            //        {
            //            if (neigh == prevNode)
            //            {
            //                continue;
            //            }
            //            //angle = Vector3.SignedAngle(runningDirection,
            //            //    new Vector3(neigh.Location.x - prevNode.Location.x, 0, neigh.Location.y - prevNode.Location.y), Vector3.up);
            //            angle = Vector3.SignedAngle(runningDirection,
            //                new Vector3(neigh.Location.x - startNode.Location.x, 0, neigh.Location.y - startNode.Location.y), Vector3.up);
            //            if (angle > maxAngle)
            //            {
            //                maxAngle = angle;
            //                endNode = neigh;
            //            }
            //        }
            //    }
            //}
            runningDirection = new Vector3(endNode.Location.x - startNode.Location.x, 0, endNode.Location.y - startNode.Location.y);
            //runningDirection = new Vector3(endNode.Location.x - player.transform.position.x, 0, endNode.Location.y - player.transform.position.z);
            cameraAngle = mainCamera.transform.eulerAngles.y + Vector3.SignedAngle(new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z),
                                                                                    runningDirection.normalized, Vector3.up);

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


            //slope = (startNode.Location.y - endNode.Location.y) / (startNode.Location.x - endNode.Location.x);
            // Debug.Log(slope);

            // Generate obstacles
            //GenerateObstacles();

            // Update player rotation
            player.transform.LookAt(new Vector3(endNode.Location.x, player.transform.position.y, endNode.Location.y));

            // Update road name
            roadName = startNode.EdgeTo(endNode).Segment.MapFeatureMetadata.Name;
            roadNamePanel.GetComponentInChildren<Text>().text = roadName;
        }
    }

    void FixedUpdate()
    {
        if (startNode == null || !hasGameStarted)
        {
            return;
        }
        if (this.GetComponent<TimerScript>().isGameOver)
        {
            // Game over
            return;
        }

        Camera mainCamera = Camera.main;

        // player.GetComponent<Rigidbody>().AddForce(new Vector3(endNode.Location.x - startNode.Location.x, 0, endNode.Location.y - startNode.Location.y), ForceMode.Force);
        runningDirection = new Vector3(endNode.Location.x - startNode.Location.x, 0, endNode.Location.y - startNode.Location.y);

        if (movementSpeed < 10)
        {
            // Accelerate faster because the speed is low
            movementSpeed += 0.2f;
        }
        else
        {
            // Accelerate normally
            movementSpeed += movementSpeedIncrease;
        }

        player.GetComponent<Rigidbody>().velocity = runningDirection.normalized * movementSpeed;

        
        // ALL THE LOGIC COPIED FROM BELOW TO UPDATE FUNCTION
        
    }

    public void GenerateRoad(MapLoadedArgs args)
    {
        mapsService = GameObject.Find("GoogleMaps").GetComponent<MapLoaderRunningGame>().mapsService;
        //GameObject player = GameObject.Find("Player");
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

        //slope = (startNode.Location.y - endNode.Location.y) / (startNode.Location.x - endNode.Location.x);
        //Debug.Log(slope);
        /*
        Debug.Log(startNode.Location.x);
        Debug.Log(startNode.Location.y);
        Debug.Log(endNode.Location.x);
        Debug.Log(endNode.Location.y);
        */

        /*
        mainCamera.transform.position = new Vector3(player.transform.position.x - 5 * Mathf.Cos(Mathf.Atan(slope)),
                                                    mainCamera.transform.position.y,
                                                    player.transform.position.z - 5 * Mathf.Sin(Mathf.Atan(slope)));
        mainCamera.transform.eulerAngles = new Vector3(mainCamera.transform.eulerAngles.x,
                                                    90 - Mathf.Atan(slope) * 180 / Mathf.PI,
                                                    mainCamera.transform.eulerAngles.z);
        */
        runningDirection = new Vector3(endNode.Location.x - startNode.Location.x, 0, endNode.Location.y - startNode.Location.y);
        //mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, player.transform.position - runningDirection.normalized * 5, cameraLerpPos);
        mainCamera.transform.position = player.transform.position - runningDirection.normalized * 5;
        mainCamera.transform.position += new Vector3(0, 1.5f, 0);
        cameraAngle = mainCamera.transform.eulerAngles.y + Vector3.SignedAngle(new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z),
                                                                                    runningDirection.normalized, Vector3.up);

        // player.GetComponent<Rigidbody>().AddForce(new Vector3(endNode.Location.x - startNode.Location.x, 0, endNode.Location.y - startNode.Location.y), ForceMode.Force);
        player.GetComponent<Rigidbody>().velocity = new Vector3(endNode.Location.x - startNode.Location.x, 0, endNode.Location.y - startNode.Location.y).normalized * movementSpeed;
        
        // Update player rotation
        player.transform.LookAt(new Vector3(endNode.Location.x, player.transform.position.y, endNode.Location.y));

        // Set difficulty settings
        SetDifficultySettings();

        // Generate all obstacles
        GenerateAllObstacles(startNode, endNode);

        // Update road name
        roadName = startNode.EdgeTo(endNode).Segment.MapFeatureMetadata.Name;
        roadNamePanel.SetActive(true);
        roadNamePanel.GetComponentInChildren<Text>().text = roadName;

        // Make minimap visible
        miniMap.SetActive(true);

        // Make score, timer and start button visible
        scorePanel.SetActive(true);
        timerPanel.SetActive(true);
        startButton.SetActive(true);
    }

    private void GenerateAllObstacles(RoadLatticeNode start, RoadLatticeNode end)
    {
        Vector3 runningDir = new Vector3(end.Location.x - start.Location.x, 0, end.Location.y - start.Location.y);
        Vector3 currentObstaclePos = new Vector3(start.Location.x, 0.5f, start.Location.y);
        currentObstaclePos += runningDir.normalized * distanceBetweenObstacles;

        visitedNodes.Add(start);
        
        /*
        iterations++;
        if (iterations >= maxIterations)
        {
            return;
        }
        */

        // int it = 0;

        // Debug.Log("Start " + start);
        // Debug.Log("End " + end);

        while (true)
        {
            // Check if currentObstaclePos passed the end node, so generating obstacles stops
            if ((end.Location.x >= start.Location.x && end.Location.y >= start.Location.y
                && currentObstaclePos.x >= end.Location.x && currentObstaclePos.z >= end.Location.y)
                || (end.Location.x <= start.Location.x && end.Location.y <= start.Location.y
                && currentObstaclePos.x <= end.Location.x && currentObstaclePos.z <= end.Location.y)
                || (end.Location.x >= start.Location.x && end.Location.y <= start.Location.y
                && currentObstaclePos.x >= end.Location.x && currentObstaclePos.z <= end.Location.y)
                || (end.Location.x <= start.Location.x && end.Location.y >= start.Location.y
                && currentObstaclePos.x <= end.Location.x && currentObstaclePos.z >= end.Location.y))
            {
                break;
            }

            /*
            it++;
            Debug.Log(it);
            if (it > maxIt)
            {
                return;
            }
            */
            
            if (Vector3.Distance(currentObstaclePos, new Vector3(start.Location.x, 0.5f, start.Location.y)) < 8
                || Vector3.Distance(currentObstaclePos, new Vector3(end.Location.x, 0.5f, end.Location.y)) < 8)
            {
                // Obstacle is too close to road edge
                currentObstaclePos += runningDir.normalized * distanceBetweenObstacles;
                continue;
            }

            // Maybe generate powerup
            if (Random.Range(0, 100/powerupsRate) == 0)
            {
                Object.Instantiate(speedPowerup, currentObstaclePos + runningDir.normalized * distanceBetweenObstacles / 2, Quaternion.Euler(0, 0, 0));
            }

            // Generate new obstacle
            GameObject currentObstacle = Object.Instantiate(obstacles[Random.Range(0, obstacles.Count)], currentObstaclePos, Quaternion.Euler(0, 0, 0));
            currentObstacle.transform.LookAt(new Vector3(start.Location.x, 0.5f, start.Location.y));
            currentObstacle.transform.Rotate(0, 90, 0);
            currentObstaclePos += runningDir.normalized * distanceBetweenObstacles;

            int laneNr = Random.Range(0, 3);
            if (laneNr == 0)
            {
                // Generate obstacle on left lane
                currentObstacle.transform.position += Vector3.Cross(runningDir.normalized * lateralMovementAmount, Vector3.up);
            }
            else if (laneNr == 2)
            {
                // Right obstacle on right lane
                currentObstacle.transform.position -= Vector3.Cross(runningDir.normalized * lateralMovementAmount, Vector3.up);
            }
        }

        List<RoadLatticeNode> endNeighbors = new List<RoadLatticeNode>(end.Neighbors);
        foreach (RoadLatticeNode neigh in endNeighbors)
        {
            if (!visitedNodes.Contains(neigh))
            {
                GenerateAllObstacles(end, neigh);
            }
        }
    }

    IEnumerator MovePlayer(GameObject player, Vector3 targetPos)
    {
        while (Vector3.Distance(player.transform.position, targetPos) > 0.1f) {
            player.transform.position = Vector3.Lerp(player.transform.position, player.transform.position + Vector3.Cross(runningDirection.normalized * lateralMovementAmount,
                                                        Vector3.up), 0.1f);
            yield return null;
        }  
    }

    public void MoveLeft()
    {
        // Move to the left
        // player.GetComponent<Rigidbody>().velocity = new Vector3(endNode.Location.x - startNode.Location.x, 0, startNode.Location.y - endNode.Location.y).normalized * movementSpeed;
        // player.transform.position = player.transform.position + new Vector3(endNode.Location.x - startNode.Location.x, 0, startNode.Location.y - endNode.Location.y).normalized;
        prevRoadPosition = roadPosition;
        if (roadPosition > -1)
        {
            player.transform.position = Vector3.Lerp(player.transform.position, player.transform.position + Vector3.Cross(runningDirection.normalized * lateralMovementAmount,
                                                Vector3.up), 1.0f);
            //StartCoroutine(MovePlayer(player, player.transform.position + Vector3.Cross(runningDirection.normalized * lateralMovementAmount, Vector3.up)));
            roadPosition--;
        }
    }

    public void MoveRight()
    {
        // Move to the right
        // player.GetComponent<Rigidbody>().velocity = new Vector3(startNode.Location.x - endNode.Location.x, 0, endNode.Location.y - startNode.Location.y).normalized * movementSpeed;
        // player.transform.position = player.transform.position + new Vector3(startNode.Location.x - endNode.Location.x, 0, endNode.Location.y - startNode.Location.y).normalized;
        prevRoadPosition = roadPosition;
        if (roadPosition < 1)
        {
            player.transform.position = Vector3.Lerp(player.transform.position, player.transform.position - Vector3.Cross(runningDirection.normalized * lateralMovementAmount,
                                                Vector3.up), 1.0f);
            roadPosition++;
        }
    }

    public void StartGame()
    {
        hasGameStarted = true;
        startButton.SetActive(false);
    }

    private void SetDifficultySettings()
    {
        switch (difficulty)
        {
            case "Easy":
                movementSpeed = 10;
                distanceBetweenObstacles = 15;
                movementSpeedIncrease = 0.001f;
                break;
            case "Medium":
                movementSpeed = 10;
                distanceBetweenObstacles = 10;
                movementSpeedIncrease = 0.002f;
                break;
            case "Hard":
                movementSpeed = 10;
                distanceBetweenObstacles = 8;
                movementSpeedIncrease = 0.003f;
                break;
        }
    }

    public void CarCollision()
    {
        movementSpeed = 0;

        if (roadPosition == -1)
        {
            player.transform.position = playerCenterPosition;
            roadPosition++;
        } else if (roadPosition == 1)
        {
            player.transform.position = playerCenterPosition;
            roadPosition--;
        } else
        {
            player.transform.position = playerCenterPosition + Vector3.Cross(runningDirection.normalized * lateralMovementAmount, Vector3.up);
            roadPosition--;
        }

    }

    private bool DidPassEndnode()
    {
        nextRunningDirection = new Vector3(nextNode.Location.x - endNode.Location.x, 0, nextNode.Location.y - endNode.Location.y);
        Vector3 endNodePos = new Vector3(endNode.Location.x, 0, endNode.Location.y);
        Vector3 endNodeCenterPos = new Vector3(endNode.Location.x, 0, endNode.Location.y);
        float angle = Vector3.SignedAngle(runningDirection, nextRunningDirection, Vector3.up);
        float angle1 = 180 - angle;
        float angle2 = Mathf.Abs(90 - angle);
        //Debug.Log(angle);
        if (angle2 == 90)
        {
            angle2 = 89;
        } /*else if (angle2 == -90)
        {
            angle2 = -89;
        }*/
        angle2 = Mathf.Deg2Rad * angle2;
        //Debug.Log("RUN " + runningDirection);
        //Debug.Log("NEXT " + nextRunningDirection);
        if (runningDirection == nextRunningDirection
            || runningDirection == -nextRunningDirection)
        {
            // Straight or dead end road
            endNodePos -= roadPosition * Vector3.Cross(runningDirection.normalized * lateralMovementAmount, Vector3.up);
        }
        else if (roadPosition == -1)
        {
            // Left position
            endNodePos += runningDirection.normalized * (lateralMovementAmount / (float)System.Math.Cos((double)angle2));
            endNodePos -= nextRunningDirection.normalized * (lateralMovementAmount / (float)System.Math.Cos((double)angle2));
        }
        else if (roadPosition == 1)
        {
            // Right position
            endNodePos -= runningDirection.normalized * (lateralMovementAmount / (float)System.Math.Cos((double)angle2));
            endNodePos += nextRunningDirection.normalized * (lateralMovementAmount / (float)System.Math.Cos((double)angle2));
        }
        //Debug.Log(endNodeCenterPos + " " + endNodePos);
        // -V- FOR DEBUG PURPOSES -V- 
        //Object.Instantiate(speedPowerup, endNodePos, Quaternion.Euler(0, 0, 0));
        if ((endNodeCenterPos.x >= startNode.Location.x && endNodeCenterPos.z >= startNode.Location.y
            && player.transform.position.x >= endNodePos.x && player.transform.position.z >= endNodePos.z)
            || (endNodeCenterPos.x <= startNode.Location.x && endNodeCenterPos.z <= startNode.Location.y
            && player.transform.position.x <= endNodePos.x && player.transform.position.z <= endNodePos.z)
            || (endNodeCenterPos.x >= startNode.Location.x && endNodeCenterPos.z <= startNode.Location.y
            && player.transform.position.x >= endNodePos.x && player.transform.position.z <= endNodePos.z)
            || (endNodeCenterPos.x <= startNode.Location.x && endNodeCenterPos.z >= startNode.Location.y
            && player.transform.position.x <= endNodePos.x && player.transform.position.z >= endNodePos.z))
        {
            return true;
        }

        return false;
    }

    private RoadLatticeNode FindNextNode()
    {
        RoadLatticeNode nextNode = null;

        // Choose neighbor
        List<RoadLatticeNode> neighbors = new List<RoadLatticeNode>(endNode.Neighbors);
        float angle;
        float minAngle = 180;
        float maxAngle = -180;
        float closestToZeroAngle = 180;
        if (neighbors.Count == 1)
        {
            // Dead end road
            nextNode = startNode;
        }
        else if (neighbors.Count == 2)
        {
            // Straight road
            foreach (RoadLatticeNode neigh in neighbors)
            {
                if (neigh != startNode)
                {
                    nextNode = neigh;
                }
            }
        }
        else
        {
            // Intersection
            if (roadPosition == 0)
            {
                // Center position
                foreach (RoadLatticeNode neigh in neighbors)
                {
                    if (neigh == startNode)
                    {
                        continue;
                    }
                    angle = Vector3.SignedAngle(runningDirection,
                        new Vector3(neigh.Location.x - endNode.Location.x, 0, neigh.Location.y - endNode.Location.y), Vector3.up);
                    if (Mathf.Abs(angle) <= closestToZeroAngle)
                    {
                        closestToZeroAngle = Mathf.Abs(angle);
                        nextNode = neigh;
                    }
                }
                //}
            }
            else if (roadPosition == -1)
            {
                // Left position
                foreach (RoadLatticeNode neigh in neighbors)
                {
                    if (neigh == startNode)
                    {
                        continue;
                    }
                    //angle = Vector3.SignedAngle(runningDirection,
                    //    new Vector3(neigh.Location.x - prevNode.Location.x, 0, neigh.Location.y - prevNode.Location.y), Vector3.up);
                    angle = Vector3.SignedAngle(runningDirection,
                        new Vector3(neigh.Location.x - endNode.Location.x, 0, neigh.Location.y - endNode.Location.y), Vector3.up);
                    if (angle < minAngle)
                    {
                        minAngle = angle;
                        nextNode = neigh;
                    }
                }
            }
            else if (roadPosition == 1)
            {
                // Right position
                foreach (RoadLatticeNode neigh in neighbors)
                {
                    if (neigh == startNode)
                    {
                        continue;
                    }
                    //angle = Vector3.SignedAngle(runningDirection,
                    //    new Vector3(neigh.Location.x - prevNode.Location.x, 0, neigh.Location.y - prevNode.Location.y), Vector3.up);
                    angle = Vector3.SignedAngle(runningDirection,
                        new Vector3(neigh.Location.x - endNode.Location.x, 0, neigh.Location.y - endNode.Location.y), Vector3.up);
                    if (angle > maxAngle)
                    {
                        maxAngle = angle;
                        nextNode = neigh;
                    }
                }
            }
        }

        return nextNode;
    }
}
