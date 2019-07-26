using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideScript : MonoBehaviour
{
    private int millis;

    public int SlideId;


    public string SlideText;
    public TextMesh name_text;


    private MeshRenderer slideRenderer;

    private Vector2 pos;
    public Vector2 initPos;
    private Vector2 targetPos;

    //Shift loop timer
    private int clicStart;
    private int clicNext;

    // Start is called before the first frame update
    void Start()
    {
        //TEXT
        SlideText = SlideId.ToString();
        if (SlideText != null)
        {
            name_text.text = SlideText;
        }
        else
        {
            name_text.text = "0";
        }

        //COLOR
        slideRenderer = GetComponent<MeshRenderer>();

        if (slideRenderer != null)
        {
            Color sliderColor = new Color(
                Random.value,
                Random.value,
                Random.value
            );

            slideRenderer.material.color = sliderColor;
        }

        //POSITION
        this.pos = new Vector2(0, 0);
        this.targetPos = new Vector2(0, 0);
        this.pos = new Vector2(initPos.x, initPos.y);

    }

    // public void setInitPos(Vector2 initPos)
    // {
    //     this.pos = new Vector2(initPos.x, initPos.y);
    // }

    // Update is called once per frame
    void Update()
    {
        millis = (int)(Time.time * 1000);
 
        if (millis <= clicNext)
        {
            float amt = Map(0.0f, 1.0f, clicStart, clicNext, millis);
            pos.x = Mathf.Lerp(pos.x, targetPos.x, amt);
        }

        this.transform.position = new Vector3(pos.x, pos.y, this.transform.position.z);

    }


    public void moveIt(float _newX, int time)
    {

        if (time == 0)
        {
            pos.x = _newX;
        }
        else
        {
            targetPos.x = _newX;
            clicStart = millis;
            clicNext = clicStart + time;
        }
    }

    ///////////////////////////////////TOOLS///////////////////////////////////////

    public float Map(float from, float to, float from2, float to2, float value)
    {
        if (value <= from2)
        {
            return from;
        }
        else if (value >= to2)
        {
            return to;
        }
        else
        {
            return (to - from) * ((value - from2) / (to2 - from2)) + from;
        }
    }
}
