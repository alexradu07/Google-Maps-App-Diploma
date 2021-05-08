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
    private Vector3 finalCheckPos = new Vector3(0, 0, 0);
    private bool firstPass = false;
    private bool containsDest = false;
    private RoadLatticeNode finalDest = null;
    private RoadLatticeNode wanted = null;
    private ulong finalUid = 0;
    private bool lastPlaced = false;
    private List<ulong> uids = new List<ulong>();



    void Start()
    {
        mapsService = GameObject.Find("GoogleMaps").GetComponent<MapsService>();
        //mapsService.Events.MapEvents.Loaded.AddListener(Initialise);
    }

    // Update is called once per frame
    void Update()
    {
        //if (checkpoints.Count > 0 && currentIndex < checkpoints.Count)
        if (OutDoorCarSceneController.jsonResponse != null && checkpointsReady == false)
        {
            foreach (steps i in OutDoorCarSceneController.jsonResponse.routes[0].legs[0].steps)
            {
                GameObject check = (GameObject)Object.Instantiate(prefab, mapsService.Coords.FromLatLngToVector3(new Google.Maps.Coord.LatLng(i.end_location.lat,i.end_location.lng)), Quaternion.identity);
                checkpoints.Add(check);
            }
            checkpointsReady = true;
        }
        if (checkpoints.Count > 0)
        {
            Vector3 dirToLookAt = checkpoints[0].transform.position;
            dirToLookAt.y += 2;
            arrow.transform.LookAt(dirToLookAt);
            arrow.transform.Rotate(new Vector3(0, -90, 0));
            if (Vector3.Distance(checkpoints[currentIndex].transform.position, carObj.transform.position) < 15)
            {
                checkpoints[0].SetActive(false);
                checkpoints.RemoveAt(0);
            }
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
