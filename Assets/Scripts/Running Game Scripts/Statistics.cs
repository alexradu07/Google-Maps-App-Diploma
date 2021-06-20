using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Statistics : MonoBehaviour
{
    public Text lastDistanceText;
    public Text totalDistanceText;
    public Text lastAverageSpeed;
    public Text maxAverageSpeed;

    // Start is called before the first frame update
    void Start()
    {
        lastDistanceText.text += " " + PlayerPrefs.GetFloat("lastDistance", 0) + " m ( ~" + (int)(PlayerPrefs.GetFloat("lastDistance", 0) * 1.35f) + " steps" + " )";
        totalDistanceText.text += " " + PlayerPrefs.GetFloat("totalDistance", 0) + " m ( ~" + (int)(PlayerPrefs.GetFloat("totalDistance", 0) * 1.35f) + " steps" + " )";
        lastAverageSpeed.text += " " + PlayerPrefs.GetFloat("lastAvgSpeed", 0) + " km/h";
        maxAverageSpeed.text += " " + PlayerPrefs.GetFloat("maxAvgSpeed", 0) + " km/h";
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
        {
            // Baack button pressed
            SceneManager.LoadScene("IORunningScene");
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene("IORunningScene");
    }
}
