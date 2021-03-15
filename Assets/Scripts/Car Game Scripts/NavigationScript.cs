using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Maps.Coord;
using Google.Maps.Event;
using Google.Maps;
using Google.Maps.Unity.Intersections;
using System.Runtime.ExceptionServices;

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
    public int currentIndex = 0;
    private bool checkpointsReady = false;
    private bool checkpointsPlaced = false;
    private void Initialise(MapLoadedArgs args)
    {
        if (!checkpointsPlaced)
        {
            lattices = new List<RoadLatticeNode>(mapsService.RoadLattice.Nodes);
            Transform carPos = carObj.transform;
            float min = 1000;
            float max = 0;
            foreach (RoadLatticeNode i in lattices)
            {
                Vector2 nodeLoc = i.Location;
                Vector3 nodeLoc3 = new Vector3(nodeLoc.x, 0, nodeLoc.y);
                float dist = Vector3.Distance(nodeLoc3, carPos.position);
                if (dist < min)
                {
                    min = dist;
                    startNavi = i;
                }
                if (dist > 200)
                {
                    //max = dist;
                    //endNavi = i;
                    suitableTargets.Add(i);
                }
            }
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
            checkpointsReady = true;
            //Object.Instantiate(prefab, new Vector3(startNavi.Location.x, 0, startNavi.Location.y), Quaternion.identity);
            checkpointsPlaced = true;
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
            arrow.SetActive(false);
        }

    }

}
