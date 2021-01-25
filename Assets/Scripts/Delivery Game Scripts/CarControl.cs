using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControl : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject frontWheel;
    public GameObject rearWheel;
    public WheelCollider frontWheelCollider;
    public WheelCollider rearWheelCollider;
    public GameObject motorcycle;
    private float angleOz; // left increase
    private float maxAngleOz;
    public Camera playerCamera;
    public float cameraX;
    public float cameraY;
    public float cameraZ;
    // Start is called before the first frame update
    void Start()
    {
        angleOz = .0f;
        maxAngleOz = 45;
    }

    // Update is called once per frame
    void Update()
    {
        rearWheelCollider.motorTorque = 0;
        if (angleOz < 0)
        {
            angleOz += .5f;
        }
        else
        {
            angleOz -= .5f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (angleOz + 1 > maxAngleOz)
            {
                angleOz = maxAngleOz;
            }
            else
            {
                angleOz += 1;
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (angleOz - 1 < -maxAngleOz)
            {
                angleOz = -maxAngleOz;
            }
            else
            {
                angleOz -= 1;
            }
        }
        if (Input.GetKey(KeyCode.W))
        {
            rearWheelCollider.motorTorque = 200;
        }
        if (Input.GetKey(KeyCode.S))
        {
            rearWheelCollider.brakeTorque = 300;
            frontWheelCollider.brakeTorque = 300;
        }
        //motorcycle.transform.eulerAngles = new Vector3(motorcycle.transform.eulerAngles.x, motorcycle.transform.eulerAngles.y, angleOz);
        playerCamera.transform.position = motorcycle.transform.position + new Vector3(cameraX, cameraY, cameraZ);
    }
}