#include <Servo.h>

//Ports
int leftClickPort = 9;
int rightClickPort = 10;

//Clicking vars
boolean leftClicking = false;
int leftClickStart = 0;
int leftClickDown = 3;
int leftClickUp = 15;

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
      performLeftClick();
      break;
  } 
 }
 
 if (leftClicking) {
   continueLeftClick();
 } else {
   leftClick.write(leftClickUp);
 }
}

void performLeftClick() {
  leftClick.write(leftClickDown);
  delay(80);
  leftClick.write(leftClickUp);
}

void beginLeftClick() {
 leftClickStart = 1;
 leftClicking = true;
 leftClick.write(leftClickDown);
}

void continueLeftClick() {
 leftClickStart++;
 if (leftClickStart > 2500) {
  leftClickStart = 0;
  leftClicking = false;
  leftClick.write(leftClickUp);
 } 
}
