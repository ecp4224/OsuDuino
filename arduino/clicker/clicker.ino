#include <Servo.h>

//Ports                        
int leftClickPort = 9                                                                                                                                                                         ;
int rightClickPort = 10;

//Clicking vars
int leftClickDown = 35;
int leftClickUp = 15;
int rightClickDown = 35;
int rightClickUp = 25;
Servo leftClick; Servo rightClick;

void setup() {
  leftClick.attach(leftClickPort);
  rightClick.attach(rightClickPort);
  leftClick.write(leftClickUp);
  Serial.begin(9600);
}

void loop() {
  while (Serial.available() > 0) {
  byte opCode = Serial.read();
  switch (opCode) {
    case 1: //Left down
      leftClick.write(leftClickDown);
      break;
    case 2: //Right down
      rightClick.write(rightClickDown);
      break;
    case 4: //Left up
      leftClick.write(leftClickUp);
      break;
    case 8: //Right up
      rightClick.write(rightClickUp);
      break;
  } 
 }
}
