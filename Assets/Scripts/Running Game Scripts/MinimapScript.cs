using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapScript : MonoBehaviour
{
    public GameObject player;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Vector3.Lerp(this.transform.position,
            player.transform.position + new Vector3(0, 100, 0), 0.1f);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation,
            Quaternion.Euler(90, player.transform.rotation.eulerAngles.y, 0), 0.1f);
    }
}
