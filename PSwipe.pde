/*////////////////////////////////////////////////////////////////////////////
 rich.gg april 2018
 inspired by and ported from
 https://github.com/thebird/Swipe by Brad Birdsall
 under MIT licence
 *////////////////////////////////////////////////////////////////////////////


//////////////////////////////PARAMS///////////////////////////////////////
private int slidesCount = 5;          // amount of slides
private int startSlide = 4;           // slide to start from (must be between 0 and slidesCount -1 !)
private int speed = 300;              // time to move to next/prev slide
private boolean continuous = true;   // looping or not looping


SwipeController swiper;

void setup() {
  size(640, 380);
  noStroke();

  swiper = new SwipeController(slidesCount, startSlide, speed, continuous);
}

void draw() {
  background(200);

  swiper.run();
}


void mousePressed() {

  swiper.eventType = SwipeController.EVENT_STARTPRESS;
}

void mouseReleased() {

  swiper.eventType = SwipeController.EVENT_ENDPRESS;
}

void keyPressed() {

  if (keyCode == RIGHT) {
    swiper.next();
  }

  if (keyCode == LEFT) {
    swiper.prev();
  }
}
