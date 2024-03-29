﻿using UnityEngine;
using UnityEngine.UI;

public static class Globals
{
    public static int spoiler = 0;
    public static int bbumper = 0;
    public static int fbumper = 0;
    public static int noOfCheckpoints = 0;
    public static int noOfFinishes = 0;
}

public class MainMenuController : MonoBehaviour
{
    public Button carGameButton;
    public Button deliveryGameButton;
    public Button runningGameButton;

    private int width;
    private int height;

    public float f1;
    public float f2;

    // Start is called before the first frame update
    void Start()
    {
        width = Screen.width;
        height = Screen.height;

        Debug.Log(width);
        Debug.Log(height);
        
        carGameButton.GetComponent<RectTransform>().sizeDelta = new Vector2(width / 3, height);
        deliveryGameButton.GetComponent<RectTransform>().sizeDelta = new Vector2(width / 3, height);
        runningGameButton.GetComponent<RectTransform>().sizeDelta = new Vector2(width / 3, height);

        deliveryGameButton.transform.position = new Vector3(width / 3, deliveryGameButton.transform.position.y, deliveryGameButton.transform.position.z);
        runningGameButton.transform.position = new Vector3(2 * width / 3, deliveryGameButton.transform.position.y, deliveryGameButton.transform.position.z);
        
    }

    // Update is called once per frame
    void Update()
    {
        // carGameButton.GetComponent<RectTransform>().sizeDelta = new Vector2(f1, f2);
    }
}
