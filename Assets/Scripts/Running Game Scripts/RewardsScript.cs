using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RewardsScript : MonoBehaviour
{
    public Text parisUnlockText;
    public Text londonUnlockText;
    public Text newYorkUnlockText;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetFloat("totalDistance", 0) < 1000)
        {
            parisUnlockText.text += " ( " + PlayerPrefs.GetFloat("totalDistance", 0) + " m so far )";
        } else
        {
            parisUnlockText.text += " ( completed )";
        }

        if (PlayerPrefs.GetFloat("totalDistance", 0) < 10000)
        {
            londonUnlockText.text += " ( " + PlayerPrefs.GetFloat("totalDistance", 0) + " m so far )";
        }
        else
        {
            londonUnlockText.text += " ( completed )";
        }

        if (PlayerPrefs.GetFloat("totalDistance", 0) < 20000)
        {
            newYorkUnlockText.text += " ( " + PlayerPrefs.GetFloat("totalDistance", 0) + " m so far )";
        }
        else
        {
            newYorkUnlockText.text += " ( completed )";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
        {
            // Baack button pressed
            SceneManager.LoadScene("RunningScene");
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene("RunningScene");
    }
}
