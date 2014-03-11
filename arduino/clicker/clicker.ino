#include <Servo.h>

//Ports
int leftClickPort = 9;
int rightClickPort = 10;

//Clicking vars
boolean leftClicking = false;
int leftClickStart = 0;
int leftClickDown = 10;
int leftClickUp = 0;

boolean rightClicking = false;
int rightClickStart = 0;
int rightClickDown = 10;
int rightClickUp = 0;

Servo leftClick; Servo rightClick;

void setup() {
  leftClick.attach(leftClickPort);
  rightClick.attach(rightClickPort);
  
  Serial.begin(9600);
}

void loop() {
 if (Serial.available() > 0) {
  byte opCode = Serial.read();
  switch (opCode) {
    case 1: //Left click
      beginLeftClick();
      break;
  } 
 }
 
 if (leftClicking) {
   continueLeftClick();
 }
}

void beginLeftClick() {
 leftClickStart = 1;
 leftClicking = true;
 leftClick.write(leftClickDown);
}

void continueLeftClick() {
 leftClickStart++;
 if (leftClickStart > 100) {
  leftClickStart = 0;
  leftClicking = false;
  leftClick.write(leftClickUp);
 } 
}
