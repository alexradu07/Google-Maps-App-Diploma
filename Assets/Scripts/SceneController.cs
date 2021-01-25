using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SelectLocation()
    {
        string locationString = GameObject.Find("Canvas/LocationDropdownSelector/Label").GetComponent<Text>().text;
        switch (locationString)
        {
            case "Grozavesti":
                GameObject.Find("GoogleMaps").GetComponent<MapLoader>().LoadMap(44.4433837, 26.0618934);
                break;
            case "Unirii":
                GameObject.Find("GoogleMaps").GetComponent<MapLoader>().LoadMap(44.426929, 26.1011807);
                break;
            case "Brasov":
                GameObject.Find("GoogleMaps").GetComponent<MapLoader>().LoadMap(45.6431122, 25.5858238);
                break;
            default:
                break;
        }

        GameObject.Find("Canvas/Panel").SetActive(false);
        GameObject.Find("Canvas/LocationDropdownSelector").SetActive(false);
        GameObject.Find("Canvas/LocationButton").SetActive(false);
        GameObject.Find("Canvas/Speedo/needle").SetActive(true);
        GameObject.Find("Canvas/Speedo/speedo").SetActive(true);
        GameObject.Find("Canvas/Speedo/needle2").SetActive(true);
        GameObject.Find("Canvas/Speedo/revs").SetActive(true);
        GameObject.Find("Canvas/Text").SetActive(true);
    }
}
