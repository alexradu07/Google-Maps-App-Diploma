using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    // Start is called before the first frame update
    public float timeLeft;
    public Text timerText;
    public GameObject gameOverPanel;
    public GameObject scorePanel;

    public bool isGameOver;
    private bool hasGameStarted;

    void Start()
    {
        isGameOver = false;
        hasGameStarted = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!timerText.IsActive() || !hasGameStarted)
        {
            // Timer should not start yet
            return;
        }
        if (isGameOver)
        {
            return;
        }

        timeLeft -= Time.deltaTime;
        timerText.text = "Time left: " + ((int)timeLeft).ToString() + "s";

        if (timeLeft <= 0)
        {
            // Game over
            isGameOver = true;
            timeLeft = 0;
            timerText.text = "Time left: " + ((int)timeLeft).ToString() + "s";
            gameOverPanel.SetActive(true);
            gameOverPanel.GetComponentInChildren<Text>().text = "Game Over\n"
                + "Total distance: " + scorePanel.GetComponentInChildren<Text>().text;
        }
    }

    public void StartGame()
    {
        hasGameStarted = true;
    }
}
