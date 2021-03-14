using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleScript : MonoBehaviour
{
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(this.transform.position, player.transform.position) > 100)
        {
            this.GetComponent<MeshRenderer>().enabled = false;
            this.GetComponent<BoxCollider>().enabled = false;
            MeshRenderer[] meshRendereres = this.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRendereres)
            {
                meshRenderer.enabled = false;
            }
        } else
        {
            this.GetComponent<MeshRenderer>().enabled = true;
            this.GetComponent<BoxCollider>().enabled = true;
            MeshRenderer[] meshRendereres = this.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRendereres)
            {
                meshRenderer.enabled = true;
            }
        }
    }
}
