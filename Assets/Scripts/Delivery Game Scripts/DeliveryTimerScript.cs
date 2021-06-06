using UnityEngine;
using UnityEngine.UI;

public class DeliveryTimerScript : MonoBehaviour
{
    static float timer = 0.0f;
    public Text text_box;
    private bool isDelivering;

    // Start is called before the first frame update
    void Start()
    {
        ;
    }

    // Update is called once per frame
    void Update()
    {
        if (Manager.gameStarted)
        {
            if (isDelivering)
            {
                timer += Time.deltaTime;
                text_box.text = timer.ToString("0.00");
            }
        }
    }

    public void StartTimer()
    {
        Debug.Log("Start delivering timer");
        isDelivering = true;
    }

    public void StopTimer()
    {
        Debug.Log("Stop delivering timer");

        isDelivering = false;
    }

    public void ResetTimer()
    {
        Debug.Log("Reset delivering timer");

        timer = .0f;
    }

    public float getCurrentDeliveryTime()
    {
        return timer;
    }
}
