public class SwipeController {

  //GLOBAL PARAMS
  private ArrayList<Slide> slides;
  private ArrayList<PVector> slidesPos;

  private PVector deltaVect = new PVector(0, 0);

  ////////////////////////////PARAMS////////////////////////////////
  private int slidesCount;          // amount of slides
  private int startSlide;           // slide to start from
  private int speed;                // time to move to next/prev slide
  private boolean continuous;       // looping or not looping
  //////////////////////////////////////////////////////////////////

  public String eventType = ""; // check mouse/touch events starttouch/move etc...
  private int index = startSlide;
  private PVector startVect = new PVector(0, 0);
  private int startTime;

  private int UNDEFINED = 0;
  private int YES = 1;
  private int NOP = 2;
  private int isScrolling = UNDEFINED; //eliminate vertical scrolling noise

  ///////////////////////////////////////////////////////////////////////////

  //CONSTRUCTOR
  
  public SwipeController(int slidesCount, int startSlide, int speed, boolean continuous) {
    this.slidesCount = slidesCount;
    this.startSlide = startSlide;
    this.speed = speed;
    this.continuous = continuous;

    //create slides (aka windows)
    slides = new ArrayList<Slide>();
    for (int i = 0; i < slidesCount; i++) {
      slides.add(new Slide(i));
    }

    //create slide position arraylist
    slidesPos = new ArrayList<PVector>();
    for (int i = 0; i < slides.size(); i++) {
      //windowsPos.add(new PVector (i * width, 0));
      if (continuous) {

        if (i == 0) {
          slidesPos.add(new PVector (0, 0));
        } else if (i == slides.size()) {
          slidesPos.add(new PVector ( -width, 0));
        } else {
          slidesPos.add(new PVector (width, 0));
        }
      } else { 
        if (i == 0) {
          slidesPos.add(new PVector (0, 0));
        } else {
          slidesPos.add(new PVector (width, 0));
        }
      }
    }

    // reposition elements before and after index
    if (continuous ) {
      move(continuousIndex(index-1), -width, 0);
      move(continuousIndex(index+1), width, 0);
    }
  }


  //FUNCTIONS

   public void run() {
    //keep the slides running
    for (int i = 0; i < slides.size(); i++) {
      slides.get(i).run();
    }

    //monitor mouse/touch events
    eventHandler();
  }

  /////////////////////////////////////////////////////////

  private void eventHandler() {

    switch(eventType) {
    case "": 

      break;
    case "startpress": 

      isScrolling = UNDEFINED;
      starting();
      break;
    case "pressed": 

      moving();
      break;
    case "endpress": 

      ending();
      break;
    }

    //keep eventType to "pressed" ON 
    if (mousePressed) {
      eventType = "pressed";
    }
  }

  //////////////////////////////

  private void starting() {
    startVect.x = mouseX;
    startVect.y = mouseY;
    startTime = millis();
    deltaVect.x = 0;
    deltaVect.y = 0;
  }

  private void moving() {

    deltaVect.x = mouseX - startVect.x;
    deltaVect.y = mouseY - startVect.y;

    if (isScrolling == UNDEFINED) {
      //println("test");
      if (isScrolling == YES || Math.abs(deltaVect.x) < Math.abs(deltaVect.y)) {
        isScrolling = YES;
      } else {
        isScrolling = NOP;
      }
    }

    // if not scrolling vertically
    if (isScrolling == NOP) {

      if (continuous) { // we don't add resistance at the end
        //move the current, the one before and the one after
        translateSlide(continuousIndex(index-1), deltaVect.x + slidesPos.get(continuousIndex(index-1)).x, 0);
        translateSlide(index, deltaVect.x + slidesPos.get(continuousIndex(index)).x, 0);
        translateSlide(continuousIndex(index+1), deltaVect.x + slidesPos.get(continuousIndex(index+1)).x, 0);
      } else {

        // increase resistance if first or last slide
        if (index == 0  && deltaVect.x > 0 ) {                                //if first slide and sliding left

          deltaVect.x =  deltaVect.x / (Math.abs(deltaVect.x) / width + 1);
        } else if (index == slides.size()-1  && deltaVect.x < 0) {          //or if last slide and sliding right

          deltaVect.x =  deltaVect.x / (Math.abs(deltaVect.x) / width + 1);
        } else if (Math.abs(deltaVect.x) < 0) {                               //and if (not ?!) sliding at all

          deltaVect.x =  deltaVect.x / (Math.abs(deltaVect.x) / width + 1);
        } 

        if (index-1 > 0 ) {
          translateSlide(index-1, deltaVect.x + slidesPos.get(index-1).x, 0);
        }

        translateSlide(index, deltaVect.x + slidesPos.get(index).x, 0);

        if (index+1 < slides.size()) {
          translateSlide(index+1, deltaVect.x + slidesPos.get(index+1).x, 0);
        }
      }
    }
  }

  private void ending() {
    // measure duration
    int duration = millis() - startTime;

    // determine if slide attempt triggers next/prev slide
    boolean isValidSlide =
      duration < 250 &&         // if slide duration is less than 250ms
      Math.abs(deltaVect.x) > 20 ||         // and if slide amt is greater than 20px
      Math.abs(deltaVect.x) > width/2;      // or if slide amt is greater than half the width

    // determine if slide attempt is past start and end
    boolean isPastBounds =
      index == 0 && deltaVect.x > 0 ||                      // if first slide and slide amt is greater than 0
      index == slides.size() - 1 && deltaVect.x < 0;   // or if last slide and slide amt is less than 0
    if (continuous) {
      isPastBounds = false;
    }

    // OLD determine direction of swipe (true:right, false:left)
    // determine direction of swipe (1: backward, -1: forward)
    float direction = Math.abs(deltaVect.x) / deltaVect.x;

    // if not scrolling vertically
    if (isScrolling == NOP) {

      if (isValidSlide && !isPastBounds) {

        if (direction < 0) {// if we're moving -----------------------------------RIGHT-----------------------------------

          if (continuous) { // we need to get the next in this direction in place
            move(continuousIndex(index-1), -width, 0);
            move(continuousIndex(index+2), width, 0);
          } else {

            if (index-1 > 0 ) {
              move(index-1, -width, 0);
            }
          }

          move(index, slidesPos.get(index).x-width, speed);
          move(continuousIndex(index+1), slidesPos.get(continuousIndex(index+1)).x-width, speed);
          index = continuousIndex(index+1);
        } else {           // if we're moving -----------------------------------LEFT-----------------------------------

          if (continuous) { // we need to get the next in this direction in place

            move(continuousIndex(index+1), width, 0);
            move(continuousIndex(index-2), -width, 0);
          } else {

            if (index+1 < slides.size()) {
              move(index+1, width, 0);
            }
          }

          move(index, slidesPos.get(index).x+width, speed);
          move(continuousIndex(index-1), slidesPos.get(continuousIndex(index-1)).x+width, speed);
          index = continuousIndex(index-1);
        }

        // runCallback(getPos(), slides[index], direction); //didn't get this one...
      } else {             // if we -----------------------------------SET BACK-----------------------------------

        if (continuous) {
          move(continuousIndex(index-1), -width, speed);
          move(index, 0, speed);
          move(continuousIndex(index+1), width, speed);
        } else {

          if (index-1 > 0 ) {
            move(index-1, -width, speed);
          }

          move(index, 0, speed);

          if (index+1 < slides.size()) {
            move(index+1, width, speed);
          }
        }
      }
    }

    //reset evenType
    eventType = "";
  }


  //////////////////////////////

  private int continuousIndex(int index) {
    // used to loop the index in "continuous"
    // a simple positive modulo using slides.length
    return (slidesCount + (index % slidesCount)) % slidesCount;
  }

  private void move(int index, float dist, int speed) {

    translateSlide(index, dist, speed);

    slidesPos.get(index).x = dist;
  }

  void translateSlide(int index, float dist, int speed) {

    slides.get(index).moveIt(dist, speed);
  }

  /////////////////////////////////BUTTON CONTROLS///////////////////////////////////

   public void next() {
    if (index != slides.size() - 1 || continuous) {

      if (continuous) { // we need to get the next in this direction in place
        move(continuousIndex(index-1), -width, 0);
        move(continuousIndex(index+2), width, 0);
      } else {

        if (index-1 > 0 ) {
          move(index-1, -width, 0);
        }
      }

      move(index, slidesPos.get(index).x-width, speed);
      move(continuousIndex(index+1), slidesPos.get(continuousIndex(index+1)).x-width, speed);
      index = continuousIndex(index+1);
    }
  }

  public void prev() {
    if (index != 0 || continuous) {

      if (continuous) { // we need to get the next in this direction in place

        move(continuousIndex(index+1), width, 0);
        move(continuousIndex(index-2), -width, 0);
      } else {

        if (index+1 < slides.size()) {
          move(index+1, width, 0);
        }
      }

      move(index, slidesPos.get(index).x+width, speed);
      move(continuousIndex(index-1), slidesPos.get(continuousIndex(index-1)).x+width, speed);
      index = continuousIndex(index-1);
    }
  }
}
