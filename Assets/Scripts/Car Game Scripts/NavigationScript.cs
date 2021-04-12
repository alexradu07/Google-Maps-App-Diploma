using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Coord;
using Google.Maps.Event;
using Google.Maps;
using Google.Maps.Unity.Intersections;
using System.Runtime.ExceptionServices;
using System.IO;

public class Comparer : IComparer<RoadLatticeNode>
{
    Vector3 pos;
    public Comparer(Vector3 pos)
    {
        this.pos = pos;
    }
    public int Compare(RoadLatticeNode x, RoadLatticeNode y)
    {
        float dist1 = Vector3.Distance(new Vector3(x.Location.x,0,x.Location.y), pos);
        float dist2 = Vector3.Distance(new Vector3(y.Location.x, 0, y.Location.y), pos);
        return (int)(dist2 - dist1);
    }
}

public class NavigationScript : MonoBehaviour
{
    // Start is called before the first frame update
    public RoadLatticeNode startNavi;
    public RoadLatticeNode endNavi;
    public List<RoadLatticeNode> track;
    public List<RoadLatticeNode> lattices;
    public List<RoadLatticeNode> suitableTargets = new List<RoadLatticeNode>();
    public GameObject carObj;
    public MapsService mapsService;
    public GameObject prefab;
    public GameObject arrow;
    public List<GameObject> checkpoints = new List<GameObject>();
    public Camera cam;
    public int currentIndex = 0;
    private bool checkpointsReady = false;
    private bool checkpointsPlaced = false;
    private Vector3 finalCheckPos;
    private bool firstPass = false;
    private void Initialise(MapLoadedArgs args)
    {
        if (!checkpointsPlaced)
        {
            lattices = new List<RoadLatticeNode>(mapsService.RoadLattice.Nodes);
            Transform carPos = carObj.transform;
            float min = 1000;
            float max = 0;
            float max2 = 0;
            Object checkpoint = null;
            RoadLatticeNode temp = null;
            if (firstPass == false)
            {
                foreach (RoadLatticeNode i in lattices)
                {
                    if (Vector3.Distance(new Vector3(i.Location.x, 0, i.Location.y), carPos.position) > max2)
                    {
                        temp = i;
                        max2 = Vector3.Distance(new Vector3(i.Location.x, 0, i.Location.y), carPos.position);
                    }
                }
                //lattices.Sort()
                //remove outliers
                finalCheckPos = new Vector3(temp.Location.x, 0, temp.Location.y);
                checkpoint = Object.Instantiate(prefab, new Vector3(temp.Location.x, 0, temp.Location.y), Quaternion.identity);
            }
            Comparer comp = new Comparer(carPos.position);
            lattices.Sort(comp);
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
            RoadLatticeNode wanted = null;
            for (int i = 0; i < 200; i++)
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
            GameObject farthest = (GameObject)Object.Instantiate(prefab, new Vector3(wanted.Location.x, 0, wanted.Location.y), Quaternion.identity);
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
            for (int i = lattices.Count - 1; i >= 0; i--)
            {
                try {
                    track = RoadLattice.FindPath(lattices[i], wanted, 10000, null);
                    if (track.Count > 2)
                    {
                        break;
                    } else
                    {
                        continue;
                    }
                } catch
                {
                    continue;
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
            
            foreach (RoadLatticeNode i in track)
            {
                if (Vector3.Distance(initCoord, new Vector3(i.Location.x, 0, i.Location.y)) > 20)
                {
                    initCoord = new Vector3(i.Location.x, 0, i.Location.y);
                    checkpoints.Add(Object.Instantiate(prefab, new Vector3(i.Location.x, 0, i.Location.y), Quaternion.identity));
                }
                if (checkpoints.Count == 1)
                {
                    carObj.transform.LookAt(checkpoints[0].transform.position);
                }
                //Object.Instantiate(prefab, new Vector3(i.Location.x, 0, i.Location.y), Quaternion.identity);
            }
            checkpoints.Add(farthest);
            checkpointsReady = true;
            //Object.Instantiate(prefab, new Vector3(startNavi.Location.x, 0, startNavi.Location.y), Quaternion.identity);
            checkpointsPlaced = true;
            firstPass = true;
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
        if (checkpoints.Count > 0 && currentIndex < checkpoints.Count)
        {
            Vector3 dirToLookAt = checkpoints[currentIndex].transform.position;
            dirToLookAt.y += 2;
            arrow.transform.LookAt(dirToLookAt);
            arrow.transform.Rotate(new Vector3(0, -90, 0));
            if (Vector3.Distance(checkpoints[currentIndex].transform.position, carObj.transform.position) < 3)
            {
                checkpoints[currentIndex].SetActive(false);
                currentIndex++;
            }
        }
        if (currentIndex == checkpoints.Count && checkpointsReady == true)
        {
            CarController.gameEnded = true;
            checkpoints.Clear();
            checkpointsPlaced = false;
            currentIndex = 0;
            //arrow.SetActive(false);
        }

    }

}
