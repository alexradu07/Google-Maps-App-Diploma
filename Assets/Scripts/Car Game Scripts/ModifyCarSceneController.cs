using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModifyCarSceneController : MonoBehaviour
{
    LinkedList<string> spoilers;
    LinkedList<string> fbumpers;
    LinkedList<string> bbumpers;
    static GameObject car;
    GameObject clickadd = null;
    string last_pressed = "";
    string temp_last_pressed = "";
    int temp_current_pos = 0;
    int current_pos = 0;
    public Transform carTrans;
    public Camera camera;
    GameObject lastAddedObj = null;
    public GameObject lockimg;
    // Start is called before the first frame update
    void Start()
    {
        spoilers = new LinkedList<string>();
        spoilers.AddLast("none");
        spoilers.AddLast("Spoiler_8");
        fbumpers = new LinkedList<string>();
        fbumpers.AddLast("Classic_16_Bumper_F_1");
        fbumpers.AddLast("Classic_16_Bumper_F_2");
        fbumpers.AddLast("Classic_16_Bumper_F_3");
        bbumpers = new LinkedList<string>();
        bbumpers.AddLast("Classic_16_Bumper_B_1");
        bbumpers.AddLast("Classic_16_Bumper_B_2");
        bbumpers.AddLast("Classic_16_Bumper_B_3");
    }

    // Update is called once per frame
    void Update()
    {
        car = GameObject.Find("Classic_16");
        if (current_pos != temp_current_pos)
        {
            if (clickadd != null)
            {
                Destroy(clickadd);
            }
            if (lastAddedObj != null)
            {
                Destroy(lastAddedObj);
                Debug.Log("intru aici");
            }
            temp_current_pos = current_pos;
            if (last_pressed.Equals("spoilers"))
            {
                if (current_pos != 0)
                {
                    GameObject objPrefab = Resources.Load("Low Poly Destructible 2Cars no. 8/Attachments/Spoilers/" + spoilers.ElementAt(current_pos)) as GameObject;
                    if (Globals.noOfCheckpoints < 5)
                    {
                        lockimg.SetActive(true);
                    } else
                    {
                        Globals.spoiler = 1;
                        GameObject obj = Instantiate(objPrefab) as GameObject;
                        lastAddedObj = obj;
                        obj.transform.parent = carTrans;
                        obj.transform.localScale = new Vector3(1, 1, 1);
                        obj.transform.localPosition = new Vector3(0, 1.57f, -1.5f);
                    }
                } else
                {
                    lockimg.SetActive(false);
                }
            }
            if (last_pressed.Equals("fbumper"))
            {
                lockimg.SetActive(false);
                foreach (string i in fbumpers)
                {
                    GameObject obj1 = GameObject.Find(i);
                    if (obj1 != null)
                    {
                        Destroy(obj1);
                    }
                    obj1 = GameObject.Find(i + "(Clone)");
                    if (obj1 != null)
                    {
                        Destroy(obj1);
                    }
                }
                GameObject objPrefab = Resources.Load("Low Poly Destructible 2Cars no. 8/Cars/16_Classic/Upgrades/Bumper_F/" + fbumpers.ElementAt(current_pos)) as GameObject;
                if (Globals.noOfCheckpoints < 5 && current_pos == 1)
                {
                    lockimg.SetActive(true);
                } else if (Globals.noOfFinishes < 3 && current_pos == 2)
                {
                    lockimg.SetActive(true);
                }
                else
                {
                    GameObject obj = Instantiate(objPrefab) as GameObject;
                    lastAddedObj = obj;
                    Globals.fbumper = current_pos;
                    obj.transform.parent = carTrans;
                    obj.transform.localScale = new Vector3(1, 1, 1);
                    obj.transform.localPosition = new Vector3(0, 0, 0);
                }
            }
            if (last_pressed.Equals("bbumper"))
            {
                lockimg.SetActive(false);
                foreach (string i in bbumpers)
                {
                    GameObject obj1 = GameObject.Find(i);
                    if (obj1 != null)
                    {
                        Destroy(obj1);
                    }
                    obj1 = GameObject.Find(i+"(Clone)");
                    if (obj1 != null)
                    {
                        Destroy(obj1);
                    }
                }
                GameObject objPrefab = Resources.Load("Low Poly Destructible 2Cars no. 8/Cars/16_Classic/Upgrades/Bumper_B/" + bbumpers.ElementAt(current_pos)) as GameObject;
                if (Globals.noOfCheckpoints < 5 && current_pos == 1)
                {
                    lockimg.SetActive(true);
                }
                else if (Globals.noOfFinishes < 3 && current_pos == 2)
                {
                    lockimg.SetActive(true);
                }
                else
                {
                    GameObject obj = Instantiate(objPrefab) as GameObject;
                    Globals.bbumper = current_pos;
                    lastAddedObj = obj;
                    obj.transform.parent = carTrans;
                    obj.transform.localScale = new Vector3(1, 1, 1);
                    obj.transform.localPosition = new Vector3(0, 0, 0);
                }
            }
            // other stuff
        }
        if (!last_pressed.Equals(temp_last_pressed))
        {
            temp_last_pressed = last_pressed;
            current_pos = 0;
            temp_current_pos = 0;
            lastAddedObj = null;
            return;
            // other stuff
        }
    }

    public void onPress(string button)
    {
        last_pressed = button;
        if (button.Equals("fbumper"))
        {
            CameraScript cam_comp = camera.GetComponent<CameraScript>();
            cam_comp.offset = new Vector3(8, 5, 10);
        }
        if(button.Equals("spoilers"))
        {
            CameraScript cam_comp = camera.GetComponent<CameraScript>();
            cam_comp.offset = new Vector3(0, 5, -15);
            current_pos = 0;
            temp_current_pos = 0;
        }
        if (button.Equals("bbumper"))
        {
            CameraScript cam_comp = camera.GetComponent<CameraScript>();
            cam_comp.offset = new Vector3(5, 5, -12);
        }
    }

    public void pressArrow(string arrow)
    {
        if (arrow.Equals("left"))
        {
            if (current_pos != 0)
            {
                current_pos--;
            }
        } else
        {
            if (last_pressed.Equals("spoilers"))
            {
                if (current_pos != spoilers.Count - 1)
                {
                    current_pos++;
                }
            }
            if (last_pressed.Equals("fbumper"))
            {
                if (current_pos != fbumpers.Count - 1)
                {
                    current_pos++;
                }
            }
            if (last_pressed.Equals("bbumper"))
            {
                if (current_pos != bbumpers.Count - 1)
                {
                    current_pos++;
                }
            }
        }
    }

    public void loads(string scene)
    {
        SceneManager.LoadScene(scene);
    }

}
