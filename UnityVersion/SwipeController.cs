using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeController : MonoBehaviour
{

    private int millis;

    //SCREEN SPACE
    private Vector2 screenBounds;
    private float spaceZeroX, spaceMaxX, spaceZeroY, spaceMaxY, spaceWidth, spaceHeight;
    private Vector3 mouseInSpace;

    //OBJECT PICKER
    private GameObject pickedObj = null;
    private Plane pickedObjPlane;
    Vector3 mouseOffset;


    //Don't know why this cannot be in start
    public Transform slidePos;
    public GameObject slide;

    ////////////////////////////PARAMS////////////////////////////////
    public int slidesCount;            // amount of slides
    public int startSlide;             // slide to start from
    public int speed;                  // (300) time to move to next/prev slide
    public bool continuous;            // looping or not looping
    //////////////////////////////////////////////////////////////////

    private GameObject[] slides;
    private Vector3[] slidesPos;

    private SlideScript slideScript;

    ////////////////////////////RUNNING PARAMS/////////////////////////
    private const int UNDEFINED = 0;
    private const int EVENT_STARTPRESS = 1;
    private const int EVENT_PRESSED = 2;
    private const int EVENT_ENDPRESS = 3;

    private int eventType = 0; // check mouse/touch events starttouch/move etc...
    private int YES = 1;
    private int NOP = 2;
    private int isScrolling = UNDEFINED; //eliminate vertical scrolling noise

    public bool Continuous { get => continuous; set => continuous = value; }

    private Vector2 deltaVect = new Vector2(0, 0);
    private int index;
    private Vector2 startVect = new Vector2(0, 0);
    private int startTime;

    // Start is called before the first frame update
    void Start()
    {


        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        spaceWidth = screenBounds.x * -2f;
        spaceHeight = screenBounds.y * -2f;
        //  Debug.Log(spaceWidth + " <-> " + spaceHeight);
        spaceZeroX = screenBounds.x;
        spaceMaxX = -screenBounds.x;
        spaceZeroY = -screenBounds.y;
        spaceMaxY = screenBounds.y;

        slides = new GameObject[slidesCount];
        slidesPos = new Vector3[slidesCount];

        //create slide position
        for (int i = 0; i < slidesCount; i++)
        {
            slidesPos[i] = new Vector3(i * spaceWidth, 0, 0);
        }

        //create slides
        for (int i = 0; i < slidesCount; i++)
        {
            //Vector2 newPos = new Vector2(spaceZeroX, spaceMaxY );

            slidePos.position = slidesPos[i];

            //slides.get(i).setInitPos(slidesPos.get(i));
            slides[i] = Instantiate(slide, slidePos.position, slidePos.rotation);
            slides[i].GetComponent<SlideScript>().initPos = slidePos.position;
            slides[i].GetComponent<SlideScript>().SlideId = (i + 1);
        }

        //Instantiate(slide, slidePos.position, slidePos.rotation);

    }

    // Update is called once per frame
    void Update()
    {
        millis = (int)(Time.time * 1000);
        // //keep the slides running
        // for (Slide s : slides) { 
        //   s.run ();
        // }

        //RayPicking
        GenerateMouseRay();
        RayPicker();

        //monitor mouse/touch events
        eventHandler();

        // mouseInSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // //  Debug.Log("Input.mousePosition " + Input.mousePosition);
        // Debug.Log("mouseInSpace " + mouseInSpace);
    }


    /////////////////////////////////////////////////////////

    Ray GenerateMouseRay()
    {
        Vector3 mousePosFar = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane);
        Vector3 mousePosNear = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);

        Vector3 mousePosF = Camera.main.ScreenToWorldPoint(mousePosFar);
        Vector3 mousePosN = Camera.main.ScreenToWorldPoint(mousePosNear);

        Ray mr = new Ray(mousePosN, mousePosF - mousePosN);
        return mr;
    }

    void RayPicker()
    {
        //onCLick
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = GenerateMouseRay();
            RaycastHit hit;

            if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit))
            {
                pickedObj = hit.transform.gameObject;
                pickedObjPlane = new Plane(Camera.main.transform.forward * -1, pickedObj.transform.position);

                //calc mouse offset
                Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                float rayDistance;
                pickedObjPlane.Raycast(mRay, out rayDistance);
                mouseOffset = pickedObj.transform.position - mRay.GetPoint(rayDistance);
            }
        }
        //clicked
        else if (Input.GetMouseButton(0))
        {
            Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            float rayDistance;
            if (pickedObjPlane.Raycast(mRay, out rayDistance))
            {
                mouseInSpace = mRay.GetPoint(rayDistance) + mouseOffset;
            }
        }
        //clickrealease
        else if (Input.GetMouseButtonUp(0) && pickedObj)
        {
            pickedObj = null;
        }
    }

    /////////////////////////////////////////////////////////



    private void eventHandler()
    {
        //  Debug.Log("eventType:" + eventType);
        switch (eventType)
        {
            case 0:

                break;
            case EVENT_STARTPRESS:

                isScrolling = UNDEFINED;

                starting();


                break;

            case EVENT_PRESSED:

                moving();

                break;

            case EVENT_ENDPRESS:

                ending();
                break;
        }

        //keep eventType to "pressed" ON 
        if (Input.GetMouseButton(0))
        {
            eventType = EVENT_PRESSED;
        }

        if (Input.GetMouseButtonDown(0))
        {
            eventType = EVENT_STARTPRESS;
        }

        if (Input.GetMouseButtonUp(0))
        {
            eventType = EVENT_ENDPRESS;
        }

    }

    //////////////////////////////

    private void starting()
    {
        startVect.x = mouseInSpace.x;
        startVect.y = mouseInSpace.y;
        startTime = millis;
        deltaVect.x = 0;
        deltaVect.y = 0;
    }

    private void moving()
    {
        deltaVect.x = mouseInSpace.x - startVect.x;
        deltaVect.y = mouseInSpace.y - startVect.y;
        // Debug.Log("time:" + (int)(Time.time * 1000));
        if (isScrolling == UNDEFINED)
        {
            //println("test");
            if (isScrolling == YES || Mathf.Abs(deltaVect.x) < Mathf.Abs(deltaVect.y))
            {
                isScrolling = YES;
            }
            else
            {
                isScrolling = NOP;
            }
        }


        // if not scrolling vertically
        if (isScrolling == NOP)
        {

            if (continuous)
            { // we don't add resistance at the end
              //move the current, the one before and the one after
                translateSlide(getLoopedIndex(index - 1), deltaVect.x + slidesPos[getLoopedIndex(index - 1)].x, 0);
                translateSlide(index, deltaVect.x + slidesPos[getLoopedIndex(index)].x, 0);
                translateSlide(getLoopedIndex(index + 1), deltaVect.x + slidesPos[getLoopedIndex(index + 1)].x, 0);
            }
            else
            {

                // increase resistance if first or last slide
                if (index == 0 && deltaVect.x > 0)
                {                                //if first slide and sliding left

                    deltaVect.x = deltaVect.x / (Mathf.Abs(deltaVect.x) / spaceWidth + 1);
                }
                else if (index == slides.Length - 1 && deltaVect.x < 0)
                {          //or if last slide and sliding right

                    deltaVect.x = deltaVect.x / (Mathf.Abs(deltaVect.x) / spaceWidth + 1);
                }
                else if (Mathf.Abs(deltaVect.x) < 0)
                {                               //and if (not ?!) sliding at all

                    deltaVect.x = deltaVect.x / (Mathf.Abs(deltaVect.x) / spaceWidth + 1);
                }

                if (index - 1 >= 0)
                {
                    translateSlide(index - 1, deltaVect.x + slidesPos[index - 1].x, 0);
                }

                translateSlide(index, deltaVect.x + slidesPos[index].x, 0);

                if (index + 1 < slides.Length)
                {
                    translateSlide(index + 1, deltaVect.x + slidesPos[index + 1].x, 0);
                }
            }
        }



    }

    private void ending()
    {
        // measure duration
        int duration = millis - startTime;

        // determine if slide attempt triggers next/prev slide
        bool isValidSlide =
          duration < 250 &&         // if slide duration is less than 250ms
          Mathf.Abs(deltaVect.x) > 20 ||         // and if slide amt is greater than 20px
          Mathf.Abs(deltaVect.x) > spaceWidth / 10;      // or if slide amt is greater than half the width

        // determine if slide attempt is past start and end
        bool isPastBounds =
          index == 0 && deltaVect.x > 0 ||                      // if first slide and slide amt is greater than 0
          index == slides.Length - 1 && deltaVect.x < 0;   // or if last slide and slide amt is less than 0
        if (continuous)
        {
            isPastBounds = false;
        }

        // OLD determine direction of swipe (true:right, false:left)
        // determine direction of swipe (1: backward, -1: forward)
        float direction = Mathf.Abs(deltaVect.x) / deltaVect.x;

        // if not scrolling vertically
        if (isScrolling == NOP)
        {

            if (isValidSlide && !isPastBounds)
            {

                if (direction < 0)
                {// if we're moving -----------------------------------RIGHT--------COUNT UP-----------------

                    if (continuous)
                    { // we need to get the next in this direction in place
                        move(getLoopedIndex(index - 1), -spaceWidth, 0);
                        move(getLoopedIndex(index + 2), spaceWidth, 0);
                    }
                    else
                    {

                        if (index - 1 >= 0)
                        {
                            move(index - 1, -spaceWidth, 0);
                        }
                    }

                    move(index, slidesPos[index].x - spaceWidth, speed);
                    if (continuous)
                    {
                        move(getLoopedIndex(index + 1), slidesPos[getLoopedIndex(index + 1)].x - spaceWidth, speed);
                    }
                    else
                    {
                        for (int i = getLoopedIndex(index + 1); i < slidesCount; i++)
                        {
                            move(i, slidesPos[i].x - spaceWidth, speed);
                        }
                    }

                    index = getLoopedIndex(index + 1);

                }
                else
                {           // if we're moving -----------------------------------LEFT----------COUNT DOWN------------------

                    if (continuous)
                    { // we need to get the next in this direction in place

                        move(getLoopedIndex(index + 1), spaceWidth, 0);
                        move(getLoopedIndex(index - 2), -spaceWidth, 0);
                    }
                    else
                    {
                        //if you've reached the end
                        if (index + 1 < slides.Length)
                        {
                            move(index + 1, spaceWidth, 0);
                        }
                    }

                    move(index, slidesPos[index].x + spaceWidth, speed);
                    if (continuous)
                    {
                        move(getLoopedIndex(index - 1), slidesPos[getLoopedIndex(index - 1)].x + spaceWidth, speed);
                    }
                    else
                    {
                        for (int i = getLoopedIndex(index - 1); i < slidesCount; i++)
                        {
                            if (i != index)
                            {
                                move(i, slidesPos[i].x + spaceWidth, speed);
                            }
                        }
                    }

                    index = getLoopedIndex(index - 1);
                }

                // runCallback(getPos(), slides[index], direction); //didn't get this one...
            }
            else
            {             // if we -----------------------------------SET BACK-----------------------------------

                if (continuous)
                {
                    move(getLoopedIndex(index - 1), -spaceWidth, speed);
                    move(index, 0, speed);
                    move(getLoopedIndex(index + 1), spaceWidth, speed);
                }
                else
                {

                    if (index - 1 > 0)
                    {
                        move(index - 1, -spaceWidth, speed);
                    }

                    move(index, 0, speed);

                    if (index + 1 < slides.Length)
                    {
                        move(index + 1, spaceWidth, speed);
                    }
                }
            }
        }

        //reset evenType
        eventType = 0;
    }


    //////////////////////////////////////////////////////////////////////////////////////////


    private int getLoopedIndex(int index)
    {
        // used to loop the index in "continuous"
        // a simple positive modulo using slides.length
        return (slidesCount + (index % slidesCount)) % slidesCount;
    }

    private void move(int index, float dist, int speed)
    {

        translateSlide(index, dist, speed);

        slidesPos[index].x = dist;
    }

    void translateSlide(int index, float dist, int speed)
    {
        // slides[index].moveIt(dist, speed);
        slides[index].GetComponent<SlideScript>().moveIt(dist, speed);
    }

}
