using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Event;
using Google.Maps;
using Google.Maps.Unity.Intersections;

public class CarComparer : IComparer<RoadLatticeNode>
{
    Vector3 pos;
    public CarComparer(Vector3 pos)
    {
        this.pos = pos;
    }
    public int Compare(RoadLatticeNode x, RoadLatticeNode y)
    {
        float dist1 = Vector3.Distance(new Vector3(x.Location.x, 0, x.Location.y), pos);
        float dist2 = Vector3.Distance(new Vector3(y.Location.x, 0, y.Location.y), pos);
        return (int)(dist2 - dist1);
    }
}

public class OutdoorNavigationScript : MonoBehaviour
{
    // Start is called before the first frame update
    private RoadLatticeNode startNavi;
    private RoadLatticeNode endNavi;
    private List<RoadLatticeNode> track;
    private List<RoadLatticeNode> lattices;
    private List<RoadLatticeNode> suitableTargets = new List<RoadLatticeNode>();
    public GameObject carObj;
    public MapsService mapsService;
    public GameObject prefab;
    public GameObject arrow;
    static public bool needToUnload = false;
    private List<GameObject> checkpoints = new List<GameObject>();
    public Camera cam;
    private int currentIndex = 0;
    private GameObjectOptions DefaultGameObjectOptions;
    private bool checkpointsReady = false;
    private bool checkpointsPlaced = false;
    private Vector3 finalCheckPos;
    private bool firstPass = false;
    private bool containsDest = false;
    private RoadLatticeNode finalDest = null;
    private RoadLatticeNode wanted = null;
    private ulong finalUid = 0;
    private bool lastPlaced = false;
    private List<ulong> uids = new List<ulong>();

    IEnumerator pathfinder()
    {
        for (int i = lattices.Count - 1; i >= 0; i--)
        {
            try
            {
                track = RoadLattice.FindPath(lattices[i], wanted, 10000, null);
                if (track.Count > 2)
                {
                    break;
                }
                else
                {
                    continue;
                }
            }
            catch
            {
                continue;
            }
        }
        yield return null;
    }

    private void Initialise(MapLoadedArgs args)
    {
        Debug.Log(containsDest);
        if (!lastPlaced)
        {
            if (!checkpointsPlaced)
            {
                lattices = new List<RoadLatticeNode>(mapsService.RoadLattice.Nodes);
                if (firstPass && !containsDest)
                {
                    foreach (RoadLatticeNode i in lattices)
                    {
                        if (i.LocationUID == finalUid)
                        {
                            containsDest = true;
                        }
                    }
                }
                Transform carPos = carObj.transform;
                float min = 1000;
                float max = 0;
                float max2 = 0;
                Object checkpoint = null;
                GameObject farthest = null;
                if (!containsDest)
                {
                    if (firstPass == false)
                    {
                        foreach (RoadLatticeNode i in lattices)
                        {
                            if (Vector3.Distance(new Vector3(i.Location.x, 0, i.Location.y), carPos.position) > max2)
                            {
                                finalDest = i;
                                max2 = Vector3.Distance(new Vector3(i.Location.x, 0, i.Location.y), carPos.position);
                            }
                        }
                        //lattices.Sort()
                        //remove outliers
                        finalCheckPos = new Vector3(finalDest.Location.x, 0, finalDest.Location.y);
                        finalUid = finalDest.LocationUID;
                        Debug.Log("finalUID" + finalUid);
                        //checkpoint = Object.Instantiate(prefab, new Vector3(finalDest.Location.x, 0, finalDest.Location.y), Quaternion.identity);
                    }
                    /*mapsService.MakeMapLoadRegion()
                     .AddCircle(cam.transform.position, 700)
                     .UnloadOutside();*/
                    needToUnload = true;
                    CarComparer comp = new CarComparer(carPos.position);
                    lattices.Sort(comp);
                    /*for (int i = 0; i < lattices.Count; i++)
                    {
                        float dist = Vector3.Distance(new Vector3(lattices[i].Location.x, 0, lattices[i].Location.y), carPos.position);
                        Debug.Log(dist);
                    }*/
                    for (int i = 0; i < lattices.Count - 1; i++)
                    {
                        float dist1 = Vector3.Distance(new Vector3(lattices[i].Location.x, 0, lattices[i].Location.y), carPos.position);
                        float dist2 = Vector3.Distance(new Vector3(lattices[i + 1].Location.x, 0, lattices[i + 1].Location.y), carPos.position);
                        if (dist1 - dist2 > 100f)
                        {
                            lattices.RemoveRange(0, i + 1);
                            break;
                        }
                    }
                    float minDiff = 100000;
                    for (int i = 0; i < lattices.Count * 3 / 4; i++)
                    {
                        float distCarToLattice = Vector3.Distance(new Vector3(lattices[i].Location.x, 0, lattices[i].Location.y), carPos.position);
                        float distLatticeToFinal = Vector3.Distance(new Vector3(lattices[i].Location.x, 0, lattices[i].Location.y), finalCheckPos);
                        float distanceCarToFinal = Vector3.Distance(carPos.position, finalCheckPos);

                        if (distCarToLattice + distLatticeToFinal - distanceCarToFinal < minDiff)
                        {
                            minDiff = distCarToLattice + distLatticeToFinal - distanceCarToFinal;
                            wanted = lattices[i];
                        }
                    }
                    min = 10000;
                    farthest = (GameObject)Object.Instantiate(prefab, new Vector3(wanted.Location.x, 0, wanted.Location.y), Quaternion.identity);
                    /*foreach (RoadLatticeNode i in lattices)
                    {
                        Vector2 nodeLoc = i.Location;
                        Vector3 nodeLoc3 = new Vector3(nodeLoc.x, 0, nodeLoc.y);
                        float dist = Vector3.Distance(nodeLoc3, carPos.position);
                        if (dist < min)
                        {
                            min = dist;
                            startNavi = i;
                        }
                    }*/
                    if (!firstPass)
                    {
                        StartCoroutine(pathfinder());
                    }
                    else
                    {
                        lattices = new List<RoadLatticeNode>(mapsService.RoadLattice.Nodes);
                        CarComparer comp2 = new CarComparer(checkpoints[checkpoints.Count - 1].transform.position);
                        lattices.Sort(comp2);
                        StartCoroutine(pathfinder());
                    }
                }
                else
                {
                    if (!lastPlaced)
                    {
                        lattices = new List<RoadLatticeNode>(mapsService.RoadLattice.Nodes);
                        CarComparer comp = new CarComparer(new Vector3(wanted.Location.x, 0, wanted.Location.y));
                        lattices.Sort(comp);
                        StartCoroutine(pathfinder());
                        lastPlaced = true;
                        foreach (GameObject i in checkpoints)
                        {
                            i.SetActive(false);
                        }
                        checkpoints.Clear();
                    }
                }
                Debug.Log(track.Count);

                /*
                int x = Random.Range(0, suitableTargets.Count - 1);
                try
                {
                    track = new List<RoadLatticeNode>(RoadLattice.FindPath(startNavi, suitableTargets[x], 10000, null));
                }
                catch
                {
                    x = Random.Range(0, suitableTargets.Count - 1);
                    track = new List<RoadLatticeNode>(RoadLattice.FindPath(startNavi, suitableTargets[x], 10000, null));
                }
                */
                Vector3 initCoord = carPos.position;

                for (int i = 0; i < track.Count; i++)
                {
                    bool needToContinue = true;
                    if (track[i].NeighborCount == 1)
                    {
                        continue;
                    }
                    for (int j = 0; j < checkpoints.Count; j++)
                    {
                        if (Vector3.Distance(checkpoints[j].transform.position, new Vector3(track[i].Location.x, 0, track[i].Location.y)) < 50)
                        {
                            needToContinue = false;
                            break;
                        }
                    }
                    if (needToContinue == false)
                    {
                        continue;
                    }
                    if (Vector3.Distance(initCoord, new Vector3(track[i].Location.x, 0, track[i].Location.y)) > 100)
                    {
                        initCoord = new Vector3(track[i].Location.x, 0, track[i].Location.y);
                        if (!uids.Contains(track[i].LocationUID))
                        {
                            checkpoints.Add(Object.Instantiate(prefab, new Vector3(track[i].Location.x, 0, track[i].Location.y), Quaternion.identity));
                            uids.Add(track[i].LocationUID);
                        }
                    }
                    if (checkpoints.Count == 1 && !firstPass)
                    {
                        carObj.transform.LookAt(checkpoints[0].transform.position);
                    }
                    //Object.Instantiate(prefab, new Vector3(i.Location.x, 0, i.Location.y), Quaternion.identity);
                }
                if (!containsDest)
                {
                    checkpoints.Add(farthest);
                }
                checkpointsReady = true;
                //Object.Instantiate(prefab, new Vector3(startNavi.Location.x, 0, startNavi.Location.y), Quaternion.identity);
                checkpointsPlaced = true;
                firstPass = true;
                for (int i = 0; i < checkpoints.Count - 1; i++)
                {
                    if (Vector3.Distance(checkpoints[i].transform.position, checkpoints[i + 1].transform.position) < 100)
                    {
                        checkpoints[i + 1].SetActive(false);
                        checkpoints.RemoveAt(i + 1);
                        i--;
                    }
                }
            }
        }
    }

    void Start()
    {
        mapsService = GameObject.Find("GoogleMaps").GetComponent<MapsService>();
        mapsService.Events.MapEvents.Loaded.AddListener(Initialise);
    }

    // Update is called once per frame
    void Update()
    {
        //if (checkpoints.Count > 0 && currentIndex < checkpoints.Count)
        if (checkpoints.Count > 0)
        {
            Vector3 dirToLookAt = checkpoints[0].transform.position;
            dirToLookAt.y += 2;
            arrow.transform.LookAt(dirToLookAt);
            arrow.transform.Rotate(new Vector3(0, -90, 0));
            if (Vector3.Distance(checkpoints[currentIndex].transform.position, carObj.transform.position) < 3)
            {
                checkpoints[0].SetActive(false);
                checkpoints.RemoveAt(0);
            }
        }
        //if (currentIndex == checkpoints.Count && checkpointsReady == true)
        if (checkpoints.Count < 3 && !containsDest)
        {
            checkpointsPlaced = false;
        }
        if (checkpoints.Count == 0 && checkpointsReady == true)
        {
            OutdoorCarController.gameEnded = true;
            checkpoints.Clear();
            checkpointsPlaced = false;
            currentIndex = 0;
            arrow.SetActive(false);
            Input.location.Stop();
        }

    }

}
