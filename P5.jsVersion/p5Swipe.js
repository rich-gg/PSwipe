/*////////////////////////////////////////////////////////////////////////////
 ------------------------------P5.JS version----------------------------------
 rich.gg april 2018
 ported from
 https://github.com/rich-gg/PSwipe by rich.gg
 itself inspired by and ported from
 https://github.com/thebird/Swipe by Brad Birdsall
 under MIT licence
*/ ///////////////////////////////////////////////////////////////////////////
//////////////////////////////PARAMS///////////////////////////////////////
var slidesCount = 5; // amount of slides
var startSlide = 0; // slide to start from (must be between 0 and slidesCount -1 !)
var speed = 300; // time to move to next/prev slide
var continuous = true; // looping or not looping
var swiper;

function setup() {
  createCanvas(640, 380);
  noStroke();

  swiper = new SwipeController(slidesCount, startSlide, speed, continuous);
}

function draw() {
  background(200);

  swiper.run();
}

//////////////////////////////////////////////////////////////////////////

function mousePressed() {

  swiper.eventHandler(1);

}

function mouseReleased() {

  swiper.eventHandler(3);
}

function keyPressed() {

  if (keyCode === RIGHT_ARROW) {
    swiper.nextSlide();
  }

  if (keyCode === LEFT_ARROW) {
    swiper.prevSlide();
  }
}

/////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////OBJECTS SLIDE
/////////////////////////////////////////////////////////////////////////////

function Slide(id) {
  this.id = id;
  this.pos = createVector(0, 0);
  this.targetPos = createVector(0, 0);
  this.bgcolor = color(
    random(50, 100),
    random(50, 100),
    random(50, 100)
  );
  //Shift loop timer
  var clicStart;
  var clicNext;

  //////////////FUNCTIONS
  this.setInitPos = function(initPos) {

    this.pos = createVector(initPos.x, initPos.y);
  }

  this.run = function() {
    this.update();
    this.render();
  }

  this.update = function() {

    if (millis() <= clicNext) {

      var amt = map(millis(), clicStart, clicNext, 0.0, 1.0);

      this.pos.x = lerp(this.pos.x, this.targetPos.x, amt);
    }
  }

  this.render = function() {
    push();
    translate(this.pos.x, this.pos.y);
    fill(this.bgcolor);
    ellipse(width / 2, height / 2, height, height);
    //rect(0, 0, width, height);    
    fill(200);
    textSize(250);
    textAlign(CENTER, CENTER);
    text(this.id + 1, width / 2, height / 2);
    pop();
  }

  this.moveIt = function(_newX, time) {

    if (time === 0) {
      this.pos.x = _newX;
    } else {
      this.targetPos.x = _newX;
      clicStart = millis();
      clicNext = clicStart + time;
    }
  }
}

////////////////////////////////////////////////////////////////OBJECTS SWIPECONTROLLER

function SwipeController(slidesCount, startSlide, speed, continuous) {
  //GLOBAL PARAMS
  var init = false;
  var deltaVect = createVector(0, 0);
  //
  var EVENT_STARTPRESS = 1;
  var EVENT_PRESSED = 2;
  var EVENT_ENDPRESS = 3;
  var eventType = 0; // check mouse/touch events starttouch/move etc...
  //
  var startVect = createVector(0, 0);
  var startTime;
  //
  var UNDEFINED = 0;
  var YES = 1;
  var NOP = 2;
  var isScrolling = UNDEFINED; //eliminate vertical scrolling noise

  //CONSTRUCT PARAMS
  this.slidesCount = slidesCount;
  this.speed = speed;
  this.continuous = continuous;
  this.index = startSlide;

  //create slides (aka windows)
  var slides = new Array(slidesCount);
  var slidesPos = new Array(slidesCount);

  for (var i = 0; i < slidesCount; i++) {
    slides[i] = new Slide(i);

    if (this.continuous) {

      if (i == this.index) {
        slidesPos[i] = createVector(0, 0);
      } else if (i == slidesCount - 1) {

        slidesPos[i] = createVector(-width, 0);
      } else if (i != slidesCount && i > this.index) {

        slidesPos[i] = createVector(width, 0);
      } else if (i != this.index && i < this.index) {
        slidesPos[i] = createVector(-width, 0);
      }
    } else {

      if (i == this.index) {
        slidesPos[i] = createVector(0, 0);
      } else if (i > this.index) {
        slidesPos[i] = createVector(width, 0);
      } else if (i < this.index) {
        slidesPos[i] = createVector(-width, 0);
      }
    }

    slides[i].setInitPos(slidesPos[i]);
  }


  //////////////FUNCTIONS

  this.run = function() {
    //keep the slides running
    for (var i = 0; i < slidesCount; i++) {
      slides[i].run();
    }

    //monitor mouse/touch events
    //keep eventType to "pressed" ON 
    if (mouseIsPressed) {
      this.eventHandler(EVENT_PRESSED);
    }
  }


  this.eventHandler = function(eventType) {

    switch (eventType) {

      case 0:

        break;
      case EVENT_STARTPRESS:

        isScrolling = UNDEFINED;
        this.starting();
        break;

      case EVENT_PRESSED:

        this.moving();
        break;

      case EVENT_ENDPRESS:

        this.ending();
        break;
    }

  }



  //////////////////////////////

  this.starting = function() {
    startVect.x = mouseX;
    startVect.y = mouseY;
    startTime = millis();
    deltaVect.x = 0;
    deltaVect.y = 0;
  }

  this.moving = function() {

    deltaVect.x = mouseX - startVect.x;
    deltaVect.y = mouseY - startVect.y;

    if (isScrolling == UNDEFINED) {

      if (isScrolling == YES || Math.abs(deltaVect.x) < Math.abs(deltaVect.y)) {
        isScrolling = YES;
      } else {
        isScrolling = NOP;
      }
    }

    // if not scrolling vertically
    if (isScrolling == NOP) {

      if (this.continuous) { // we don't add resistance at the end
        //move the current, the one before and the one after
        this.translateSlide(this.getLoopedIndex(this.index - 1), deltaVect.x + slidesPos[this.getLoopedIndex(this.index - 1)].x, 0);
        this.translateSlide(this.index, deltaVect.x + slidesPos[this.getLoopedIndex(this.index)].x, 0);
        this.translateSlide(this.getLoopedIndex(this.index + 1), deltaVect.x + slidesPos[this.getLoopedIndex(this.index + 1)].x, 0);
      } else {

        // increase resistance if first or last slide
        if (this.index === 0 && deltaVect.x > 0) { //if first slide and sliding left

          deltaVect.x = deltaVect.x / (Math.abs(deltaVect.x) / width + 1);
        } else if (this.index == slidesCount - 1 && deltaVect.x < 0) { //or if last slide and sliding right

          deltaVect.x = deltaVect.x / (Math.abs(deltaVect.x) / width + 1);
        } else if (Math.abs(deltaVect.x) < 0) { //and if (not ?!) sliding at all

          deltaVect.x = deltaVect.x / (Math.abs(deltaVect.x) / width + 1);
        }

        if (this.index - 1 >= 0) {
          this.translateSlide(this.index - 1, deltaVect.x + slidesPos[this.index - 1].x, 0);
        }


        this.translateSlide(this.index, deltaVect.x + slidesPos[this.index].x, 0);

        if (this.index + 1 < slidesCount) {
          this.translateSlide(this.index + 1, deltaVect.x + slidesPos[this.index + 1].x, 0);
        }
      }
    }
  }

  this.ending = function() {
    // measure duration
    var duration = millis() - startTime;

    // determine if slide attempt triggers next/prev slide
    var isValidSlide =
      duration < 250 && // if slide duration is less than 250ms
      Math.abs(deltaVect.x) > 20 || // and if slide amt is greater than 20px
      Math.abs(deltaVect.x) > width / 2; // or if slide amt is greater than half the width

    // determine if slide attempt is past start and end
    var isPastBounds =
      this.index === 0 && deltaVect.x > 0 || // if first slide and slide amt is greater than 0
      this.index === slidesCount - 1 && deltaVect.x < 0; // or if last slide and slide amt is less than 0

    if (continuous) {
      isPastBounds = false;
    }


    // OLD determine direction of swipe (true:right, false:left)
    // determine direction of swipe (1: backward, -1: forward)
    var direction = Math.abs(deltaVect.x) / deltaVect.x;

    // if not scrolling vertically
    if (isScrolling == NOP) {

      if (isValidSlide && !isPastBounds) {

        if (direction < 0) { // if we're moving -----------------------------------RIGHT-----------------------------------

          if (continuous) { // we need to get the next in this direction in place
            this.move(this.getLoopedIndex(this.index - 1), -width, 0);
            this.move(this.getLoopedIndex(this.index + 2), width, 0);
          } else {

            if (this.index - 1 > 0) {
              this.move(this.index - 1, -width, 0);
            }
          }

          this.move(this.index, slidesPos[this.index].x - width, speed);
          this.move(this.getLoopedIndex(this.index + 1), slidesPos[this.getLoopedIndex(this.index + 1)].x - width, speed);
          this.index = this.getLoopedIndex(this.index + 1);
        } else { // if we're moving -----------------------------------LEFT-----------------------------------

          if (continuous) { // we need to get the next in this direction in place

            this.move(this.getLoopedIndex(this.index + 1), width, 0);
            this.move(this.getLoopedIndex(this.index - 2), -width, 0);
          } else {

            if (this.index + 1 < slidesCount) {
              this.move(this.index + 1, width, 0);
            }
          }

          this.move(this.index, slidesPos[this.index].x + width, speed);
          this.move(this.getLoopedIndex(this.index - 1), slidesPos[this.getLoopedIndex(this.index - 1)].x + width, speed);
          this.index = this.getLoopedIndex(this.index - 1);
        }

        // runCallback(getPos(), slides[index], direction); //didn't get this one...
      } else { // if we -----------------------------------SET BACK-----------------------------------

        if (continuous) {
          this.move(this.getLoopedIndex(this.index - 1), -width, speed);
          this.move(this.index, 0, speed);
          this.move(this.getLoopedIndex(this.index + 1), width, speed);
        } else {

          if (this.index - 1 > 0) {
            this.move(this.index - 1, -width, speed);
          }

          this.move(this.index, 0, speed);

          if (this.index + 1 < slidesCount) {
            this.move(this.index + 1, width, speed);
          }
        }
      }
    }

    //reset evenType
    eventType = 0;
  }

  //////////////////////////////

  this.getLoopedIndex = function(index) {
    // used to loop the index in "continuous"
    // a simple positive modulo using slides.length
    return (slidesCount + (index % slidesCount)) % slidesCount;
  }

  this.move = function(index, distance, speed) {

    this.translateSlide(index, distance, speed);

    slidesPos[index].x = distance;
  }

  this.translateSlide = function(index, distance, speed) {

    slides[index].moveIt(distance, speed);
  }

  /////////////////////////////////BUTTON CONTROLS///////////////////////////////////

  this.nextSlide = function() {
    if (this.index != slidesCount - 1 || continuous) {

      if (continuous) { // we need to get the next in this direction in place
        this.move(this.getLoopedIndex(this.index - 1), -width, 0);
        this.move(this.getLoopedIndex(this.index + 2), width, 0);
      } else {

        if (this.index - 1 > 0) {
          this.move(this.index - 1, -width, 0);
        }
      }

      this.move(this.index, slidesPos[this.index].x - width, speed);
      this.move(this.getLoopedIndex(this.index + 1), slidesPos[this.getLoopedIndex(this.index + 1)].x - width, speed);
      this.index = this.getLoopedIndex(this.index + 1);
    }
  }

  this.prevSlide = function() {
    if (this.index !== 0 || this.continuous) {

      if (this.continuous) { // we need to get the next in this direction in place

        this.move(this.getLoopedIndex(this.index + 1), width, 0);
        this.move(this.getLoopedIndex(this.index - 2), -width, 0);
      } else {

        if (this.index + 1 < slidesCount) {
          this.move(this.index + 1, width, 0);
        }
      }

      this.move(this.index, slidesPos[this.index].x + width, speed);
      this.move(this.getLoopedIndex(this.index - 1), slidesPos[this.getLoopedIndex(this.index - 1)].x + width, speed);
      this.index = this.getLoopedIndex(this.index - 1);
    }
  }


}