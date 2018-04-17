public class Slide {

  private int id;
  private PVector pos;
  private PVector targetPos;
  private int bgcolor;

  //Shift loop timer
  private int clicStart ;
  private int clicNext  ;


  public Slide(int id) {
    this.id  = id;
    this.pos = new PVector(id * width, 0);
    this.targetPos = new PVector(0, 0);
    this.bgcolor = color(
      random(50, 100), 
      random(50, 100), 
      random(50, 100)
      );
  }

  public void run() {
    update();
    render();
  }

  private void update() {

    if (millis() <= clicNext) {

      float amt = map(millis(), clicStart, clicNext, 0.0f, 1.0f );

      pos.x = lerp(pos.x, targetPos.x, amt);
    }
  }


  private void render() {
    pushMatrix();
    translate(pos.x, pos.y);
    fill(bgcolor);
    ellipse(width/2, height/2, height, height);
    //rect(0, 0, width, height);    
    fill(200);
    textSize(250);
    textAlign(CENTER, CENTER);
    text(id+1, width/2, height/2-30);
    popMatrix();
  }


  public void moveIt(float _newX, int time) {

    if (time == 0) {
      pos.x = _newX;
    } else {
      targetPos.x = _newX;
      clicStart = millis();
      clicNext = clicStart + time;
    }
  }
}
