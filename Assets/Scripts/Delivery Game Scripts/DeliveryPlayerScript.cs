using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryPlayerScript : MonoBehaviour
{
    private Rigidbody rb;
    public Camera camera;
    public float cameraX;
    public float cameraY;
    public float cameraZ;
    public GameObject marker;
    public GameObject arrow;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
            rb.AddForce(Vector3.left * 10);
        if (Input.GetKey(KeyCode.D))
            rb.AddForce(Vector3.right * 10);
        if (Input.GetKey(KeyCode.W))
            rb.AddForce(new Vector3(0, 0, 10));
        if (Input.GetKey(KeyCode.S))
            rb.AddForce(new Vector3(0, 0, -10));

        camera.transform.position = this.transform.position + new Vector3(cameraX, cameraY, cameraZ);

        Vector3 arrowDirection = (marker.transform.position - this.transform.position).normalized;
        arrow.transform.position = this.transform.position + new Vector3(0, 4, 0);
        // arrow.transform.Rotate(new Vector3(0, Vector3.SignedAngle(new Vector3(1, 0 , 0), arrowDirection, Vector3.up), 0));
        arrow.transform.LookAt(marker.transform);
        arrow.transform.rotation = Quaternion.Euler(arrow.transform.eulerAngles.x, 
                arrow.transform.eulerAngles.y - 90,
                arrow.transform.eulerAngles.z); 
    }
}
