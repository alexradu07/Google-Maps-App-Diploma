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
        //Debug.Log(collision.gameObject.name);
        if (collision.gameObject.name.Substring(0, 3) == "Car"
            || collision.gameObject.name.Substring(0, 6) == "Police")
        {
            GameObject.Find("RoadController").GetComponent<RoadScript>().CarCollision();
        } else if (collision.gameObject.name.Length >= 12
            && collision.gameObject.name.Substring(0, 12) == "SpeedPowerup")
        {
            this.GetComponent<ParticleSystem>().Play();
            GameObject.Find("RoadController").GetComponent<RoadScript>().movementSpeed += 5;
            collision.gameObject.SetActive(false);
        }
    }
}
