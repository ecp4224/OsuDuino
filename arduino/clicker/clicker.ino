#include <Servo.h>

//Ports                        
int leftClickPort = 9                                                                                                                                                                         ;
int rightClickPort = 10;

int motorXEnable = 6;
int motorX1 = 5;
int motorX2 = 2;

int motorYEnable = 3;
int motorY1 = 4;
int motorY2 = 1;

//Clicking vars
int leftClickDown = 25;
int leftClickUp = 15;
int rightClickDown = 35;
int rightClickUp = 25;
Servo leftClick; Servo rightClick;

void setup() {
  pinMode(motorXEnable, OUTPUT);
  pinMode(motorX1, OUTPUT);
  pinMode(motorX2, OUTPUT);
  
  pinMode(motorYEnable, OUTPUT);
  pinMode(motorY1, OUTPUT);
  pinMode(motorY2, OUTPUT);
  
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
    case 16: //Move X Motor
      byte power = Serial.read();
      byte dir = Serial.read();
      moveX(power, dir);
      break;
    case 32: //Move Y Motor
      byte power = Serial.read();
      byte dir = Serial.read();
      moveY(power, dir);
      break;
    case 64: //Stop
      stopMotors();
      break;
  } 
 }
}

void moveX(byte p, byte dir) {
 int power = int(p);
 boolean reverse = false;
 if (dir == 1) reverse = true;
 analogWrite(motorXEnable, power);
 digitalWrite(motorX1, !reverse);
 digitalWrite(motorX2, reverse);
}

void moveY(byte p, byte dir) {
 int power = int(p);
 boolean reverse = false;
 if (dir == 1) reverse = true;
 analogWrite(motorYEnable, power);
 digitalWrite(motorY1, !reverse);
 digitalWrite(motorY2, reverse);
}

void stopMotors() {
  moveX(0, 0);
  moveY(0, 0);
}
