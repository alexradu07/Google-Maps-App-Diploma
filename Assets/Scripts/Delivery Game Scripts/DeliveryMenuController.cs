using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryMenuController : MonoBehaviour
{
    public Button indoor;
    public Button outdoor;

    private int width;
    private int height;

    // Start is called before the first frame update
    void Start()
    {
        width = Screen.width;
        height = Screen.height;


        indoor.GetComponent<RectTransform>().sizeDelta = new Vector2(width / 2, height);
        outdoor.GetComponent<RectTransform>().sizeDelta = new Vector2(width / 2, height);

        indoor.transform.position = new Vector3(0, indoor.transform.position.y, indoor.transform.position.z);
        outdoor.transform.position = new Vector3(width / 2, outdoor.transform.position.y, outdoor.transform.position.z);

    }

    // Update is called once per frame
    void Update()
    {
        // carGameButton.GetComponent<RectTransform>().sizeDelta = new Vector2(f1, f2);
    }
}
