using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider FR, FL, RR, RL;
    public Transform fr_tran, fl_tran, rr_tran, rl_tran;
    public Transform car_tran;
    public Rigidbody car_rigid;
    public float maxAngle = 30;
    public float angle = 0;
    public float power = 100.0f;
    public float cameraX;
    public float cameraY;
    public float cameraZ;
    public float velocity = 0;
    public Camera cam;
    // Start is called before the first frame update
    public void UpdateWheelPosition(WheelCollider col, Transform tran)
    {
        Vector3 position = tran.position;
        Quaternion rotation = tran.rotation;
        col.GetWorldPose(out position, out rotation);

        tran.position = position;
        tran.rotation = rotation;
    }
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        float tempVelocity = Mathf.Sqrt(Mathf.Pow(car_rigid.velocity.x, 2) + Mathf.Pow(car_rigid.velocity.x, 2));
        if (Input.GetKey(KeyCode.A))
        {
            if (angle > -maxAngle)
            {
                angle -= 2;
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (angle < maxAngle)
            {
                angle += 2;
            }
        }
        if (!Input.GetKey(KeyCode.Space))
        {
            RR.brakeTorque = 0;
            RL.brakeTorque = 0;
        }
        if (!Input.GetKey(KeyCode.S))
        {
            FR.brakeTorque = 0;
            FL.brakeTorque = 0;
            RR.brakeTorque = 0;
            RL.brakeTorque = 0;
        }
        if (Input.GetKey(KeyCode.W))
        {
            FR.motorTorque = power;
            FL.motorTorque = power;
            Debug.Log("accel");
            /*if (tempVelocity < velocity && angle == 0)
            {
                RR.brakeTorque = 1000;
                RL.brakeTorque = 1000;
                Debug.Log("braking while reversing");
            }*/
        }
        if (!Input.GetKey(KeyCode.W))
        {
            FR.motorTorque = 0;
            FL.motorTorque = 0;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Debug.Log("braking");
            FR.motorTorque = -1000;
            FL.motorTorque = -1000;
            if(Mathf.Sqrt(Mathf.Pow(car_rigid.velocity.x,2) + Mathf.Pow(car_rigid.velocity.x, 2)) > 2) 
            {
                RR.brakeTorque = 1000;
                RL.brakeTorque = 1000;
            }
        }
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("handbrake");
            RR.brakeTorque = 10000;
            RL.brakeTorque = 10000;
        }
        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            if (angle > 0)
            {
                angle -= 2;
            }
            else if (angle < 0)
            {
                angle += 2;
            }
        }
        FR.steerAngle = angle;
        FL.steerAngle = angle;
        //fr_tran.rotation = Quaternion.Euler(fr_tran.eulerAngles.x, car_tran.eulerAngles.y + angle, car_tran.eulerAngles.z);
        //fl_tran.rotation = Quaternion.Euler(fl_tran.eulerAngles.x, car_tran.eulerAngles.y + angle, car_tran.eulerAngles.z);
        //rr_tran.rotation = Quaternion.Euler(Mathf.Abs(car_rigid.velocity.z) + Mathf.Abs(car_rigid.velocity.x) + fr_tran.eulerAngles.x, car_tran.eulerAngles.y, 0);
        //rl_tran.rotation = Quaternion.Euler(Mathf.Abs(car_rigid.velocity.z) + Mathf.Abs(car_rigid.velocity.x) + fl_tran.eulerAngles.x, car_tran.eulerAngles.y, 0);
        //Debug.Log(car_rigid.velocity);
        //cam.transform.position = this.transform.position + new Vector3(cameraX, cameraY, cameraZ);
        UpdateWheelPosition(FR, fr_tran);
        UpdateWheelPosition(FL, fl_tran);
        UpdateWheelPosition(RR, rr_tran);
        UpdateWheelPosition(RL, rl_tran);
        velocity = Mathf.Sqrt(Mathf.Pow(car_rigid.velocity.x, 2) + Mathf.Pow(car_rigid.velocity.x, 2));
    }
}
