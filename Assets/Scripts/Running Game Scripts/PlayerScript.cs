using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private Rigidbody rb;
    public Camera mainCamera;
    public float cameraX;
    public float cameraY;
    public float cameraZ;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Substring(0, 3) == "Car"
            || collision.gameObject.name.Substring(0, 6) == "Police")
        {
            GameObject.Find("RoadController").GetComponent<RoadScript>().CarCollision();
        }
    }
}
