using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wvr;
using WaveVR_Log;

public class Drawing : MonoBehaviour
{
    // Start is called before the first frame update

    Color color;

    public GameObject drawingSphere;
    public GameObject eraser;

    WaveVR_Controller.Device c_dom;
    WaveVR_Controller.Device c_non_dom;

    Transform dom_t;
    Transform non_dom_t;

    Transform previous;
    float width;
    LineRenderer lr;
    GameObject go;
    bool prev_press;
    int num;

    ArrayList arr = new ArrayList();

    void Start()
    {

        color = new Color(255, 0, 0);

        c_dom = WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.Dominant);
        c_non_dom = WaveVR_Controller.Input(WaveVR_Controller.EDeviceType.NonDominant);

        dom_t = transform.GetChild(3);
        non_dom_t = transform.GetChild(4);

        width = 0.005f;
        prev_press = false;
        num = 0;

        drawingSphere.GetComponent<MeshRenderer>().enabled = true;
        eraser.GetComponent<MeshRenderer>().enabled = false;

        moveInput();
        scaleInput();

    }

    // Update is called once per frame
    void Update()
    {
        pickerInput();
        bool _press = c_dom.GetPress(WVR_InputId.WVR_InputId_Alias1_Trigger);
        if (_press && !prev_press)
        {
            drawingSphere.GetComponent<MeshRenderer>().enabled = true;
            eraser.GetComponent<MeshRenderer>().enabled = false;
            go = new GameObject();
            go.SetActive(false);
            arr.Add(go);
            lr = go.AddComponent<LineRenderer>();
            lr.startWidth = width;
            lr.endWidth = width;
            lr.useWorldSpace = false;

            Log.i("drawing", "drawing with colors:", true);
            Log.i("drawing", "r: " + (int)color.r, true);
            Log.i("drawing", "g: " + (int)color.g, true);
            Log.i("drawing", "b: " + (int)color.b, true);
            lr.material = new Material(Shader.Find("Standard"));
            lr.material.color = color;
            lr.SetColors(color, color);
            num = 0;
        }
        else if (_press)
        {
            go.SetActive(true);
            lr.positionCount = num + 1;
            lr.SetPosition(num, drawingSphere.transform.position);
            num++;
        }
        previous = dom_t;
        prev_press = _press;
    }

    Quaternion start;
    Color start_color;
    float H;
    bool _prev = false;
    public GameObject selectSphere;
    void pickerInput()
    {
        bool _press = c_non_dom.GetPress(WVR_InputId.WVR_InputId_Alias1_Touchpad);
        if (_press && !_prev)
        {
            Log.i("drawing", "starting input", true);
            start = non_dom_t.rotation;
            start_color =  new Color(color.r, color.g, color.b);
            float S, V;

            Color.RGBToHSV(start_color, out H, out S, out V);

            selectSphere.SetActive(true);
            /*
            selectSphere.transform.position = non_dom_t.position;
            selectSphere.transform.rotation = non_dom_t.rotation;*/
            selectSphere.GetComponent<MeshRenderer>().material.SetColor("_Color", color);

        } else if (_press)
        {
            /*
            selectSphere.transform.position = non_dom_t.position;
            selectSphere.transform.rotation = non_dom_t.rotation;*/
            selectSphere.GetComponent<MeshRenderer>().material.SetColor("_Color", color);

            Quaternion current_rotation = non_dom_t.rotation;
            float rotation_h = Quaternion.Angle(start, current_rotation) / 180.0f;

            rotation_h += H;

            if (rotation_h > 1)
            {
                rotation_h -= 1;
            }
            if (rotation_h < 0)
            {
                rotation_h += 1;
            }

            color = Color.HSVToRGB(rotation_h, 1.0f, 1.0f);

        }
        _prev = _press;

        pickerInput1();
        clearInput();
        //undoInput();
        moveInput();
        scaleInput();
        rotationInput();
    }

    GameObject temp;
    LineRenderer templr;
    Vector3 start_pos;
    bool _prev_pull = false;
    void pickerInput1()
    {
        bool _press = c_non_dom.GetPress(WVR_InputId.WVR_InputId_Alias1_Trigger);
        if (_press && !_prev_pull)
        {
            start_pos = non_dom_t.position;

            temp = new GameObject();
            temp.SetActive(false);
            templr = temp.AddComponent<LineRenderer>();
            templr.SetPosition(0, start_pos);

            templr.material = new Material(Shader.Find("Standard"));
            templr.material.color = color;
        }
        else if (_press)
        {
            temp.SetActive(true);
            templr.startWidth = 0.03f;
            templr.endWidth = 0.03f;
            float distance = (float)Math.Min(Vector3.Distance(non_dom_t.position, start_pos), 0.06);
            float scale = distance / 0.06f * 0.12f;
            templr.SetPosition(1, new Vector3(start_pos[0] + (non_dom_t.position[0] - start_pos[0]) * scale, start_pos[1] + 
                (non_dom_t.position[1] - start_pos[1]) * scale, start_pos[2] + (non_dom_t.position[2] - start_pos[2]) * scale));
        } else if (_prev_pull)
        {
            float distance = (float)Vector3.Distance(non_dom_t.position, start_pos);

            width = (float)(Math.Min(distance, 0.06) / 0.06 * 0.06);

            Destroy(temp);
        }
        _prev_pull = _press;
    }

    int counter = 0;
    void clearInput()
    {
        bool _press = c_dom.GetPress(WVR_InputId.WVR_InputId_Alias1_Touchpad);
        if (_press)
        {
            drawingSphere.GetComponent<MeshRenderer>().enabled = false;
            eraser.GetComponent<MeshRenderer>().enabled = true;
            counter++;
            if (counter >= 1)
            {
                foreach (GameObject stroke in arr)
                {
                    Vector3[] positions = new Vector3[stroke.GetComponent<LineRenderer>().positionCount];
                    stroke.GetComponent<LineRenderer>().GetPositions(positions);
                    foreach (Vector3 position in positions)
                    {
                        if (Vector3.Distance(position, dom_t.position) < 0.1)
                        {
                            arr.Remove(stroke);
                            Destroy(stroke);
                            break;
                        }
                    }
                }
            }
            if (counter > 60)
            {
                foreach (GameObject stroke in arr)
                {
                    arr.Remove(stroke);
                    Destroy(stroke);
                }
                counter = 0;
            }
        }
        else
        {
            counter = 0;
        }
    }
    /*int cooldown = 0;
    void undoInput()
    {
        bool _press = c_dom.GetPress(WVR_InputId.WVR_InputId_Alias1_Menu);

        if (_press)
        {
            if (cooldown <= 0)
            {
                GameObject to_del = (GameObject)arr[arr.Count - 1];
                arr.Remove(to_del);
                Destroy(to_del);
                cooldown = 10;
            }
            else
            {
                cooldown--;
            }
        } else
        {
            cooldown = 0;
        }
    }*/
    int cooldown2 = 0;
    bool _cheat_ = false;
    void moveInput()
    {
        bool _press = c_non_dom.GetPress(WVR_InputId.WVR_InputId_Alias1_Menu);
        if (_cheat_ || _press)
        {
            if (cooldown2 <= 0)
            {
                
                transform.position = transform.GetChild(4).GetChild(1).position;
                
                cooldown2 = 20;
            } else
            {
                cooldown2--;
            }
        } else
        {
            cooldown2 = 0;
        }
    }
    int cooldown3 = 0;
    void scaleInput()
    {
        bool _press = c_dom.GetPress(WVR_InputId.WVR_InputId_Alias1_Grip);
        if (_cheat_ || _press && cooldown3 <= 0)
        {
            Log.i("drawing", "DOM GRIP", true);

            foreach(GameObject stroke in arr)
            {
                LineRenderer tempr = stroke.GetComponent<LineRenderer>();
                stroke.transform.SetParent(transform.GetChild(0), true);
                for (int i = 0; i < tempr.positionCount; i++)
                {
                    tempr.SetPosition(i, Vector3.Scale(tempr.GetPosition(i), new Vector3(0.96154f, 0.96154f, 0.96154f)));
                    tempr.startWidth = tempr.startWidth * 1.0001f;
                    tempr.endWidth = tempr.endWidth * 1.0001f;
                }
                stroke.transform.SetParent(null, true);
            }
            cooldown3 = 5;
            _cheat_ = false;
        }
        _press = c_non_dom.GetPress(WVR_InputId.WVR_InputId_Alias1_Grip);
        if (_press && cooldown3 <= 0)
        {
            Log.i("drawing", "NON DOM GRIP", true);

            foreach (GameObject stroke in arr)
            {
                LineRenderer tempr = stroke.GetComponent<LineRenderer>();
                stroke.transform.SetParent(transform.GetChild(0), true);
                for (int i = 0; i < tempr.positionCount; i++)
                {
                    tempr.SetPosition(i, Vector3.Scale(tempr.GetPosition(i), new Vector3(1.04f, 1.04f, 1.04f)));
                    tempr.startWidth = tempr.startWidth * 0.9999f;
                    tempr.endWidth = tempr.endWidth * 0.9999f;
                }
                stroke.transform.SetParent(null, true);
            }
            cooldown3 = 5;
        }
        cooldown3--;
    }
    int cooldown4 = 0;
    void rotationInput()
    {
        bool _press = c_dom.GetPress(WVR_InputId.WVR_InputId_Alias1_Menu);
        if (_press)
        {
            Debug.Log("here");
            if (cooldown4 <= 0) {
                foreach (GameObject stroke in arr)
                {
                    LineRenderer tempr = stroke.GetComponent<LineRenderer>();
                    for (int i = 0; i < tempr.positionCount; i++)
                    {
                        Vector3 pos = tempr.GetPosition(i);
                        float x = pos[0];
                        float y = pos[1];
                        float z = pos[2];
                        if (x >= 0 && z >= 0)
                        {
                            tempr.SetPosition(i, new Vector3(pos[0] - 0.03f, pos[1], pos[2] + 0.03f));
                        }
                        else if (x >= 0 && z < 0)
                        {
                            tempr.SetPosition(i, new Vector3(pos[0] + 0.03f, pos[1], pos[2] + 0.03f));
                        }
                        else if (x < 0 && z >= 0)
                        {
                            tempr.SetPosition(i, new Vector3(pos[0] - 0.03f, pos[1], pos[2] - 0.03f));
                        }
                        else
                        {
                            tempr.SetPosition(i, new Vector3(pos[0] + 0.03f, pos[1], pos[2] - 0.03f));
                        }
                    }
                }
                cooldown4 = 5;
            } else
            {
                cooldown4--;
            }
        } else
        {
            cooldown4 = 0;
        }
    }
}
