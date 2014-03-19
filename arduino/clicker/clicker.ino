#include <Servo.h>

//Ports                        
int leftClickPort = 10;

int motorXEnable = 9;
int motorX1 = 3;
int motorX2 = 7;

int motorYEnable = 11;
int motorY1 = 5;
int motorY2 = 12;

//Clicking vars
int leftClickDown = 35;
int leftClickUp = 20;
Servo leftClick;

void setup() {
  pinMode(motorXEnable, OUTPUT);
  pinMode(motorX1, OUTPUT);
  pinMode(motorX2, OUTPUT);

  pinMode(motorYEnable, OUTPUT);
  pinMode(motorY1, OUTPUT);
  pinMode(motorY2, OUTPUT);

  stopMotors();

  leftClick.attach(leftClickPort);
  leftClick.write(leftClickUp);
  Serial.begin(9600);
}

void loop() {
  byte opCode;
  while ((opCode = Serial.read()) != -1) {
    switch (opCode) {
    case 1: //Left down
      leftClick.write(leftClickDown);
      break;
    case 2: //Left up
      leftClick.write(leftClickUp);
      break;
    case 4: //Move X Motor Back
      moveX(150, 2);
      break;
    case 8: //Move X Motor Foward
      moveX(150, 1);
      break;
    case 16: //Stop Motor X
      moveX(0, 0);
      break;
    case 32: //Move Y Motor Foward
      moveY(150, 1);
      break;
    case 64: //Move Y Motor Back
      moveY(150, 2);
      break;
    case 128: //Stop Motor Y
      moveY(0, 0);
      break;

    } 
  }
}

void moveX(byte p, byte dir) {
  int power = int(p);
  if (dir != 1 && dir != 0) {
    digitalWrite(motorX1, HIGH);
    digitalWrite(motorX2, LOW);
  } 
  else if (dir != 0) {
    digitalWrite(motorX1, LOW);
    digitalWrite(motorX2, HIGH);
  } 
  else {
    digitalWrite(motorX1, LOW);
    digitalWrite(motorX2, LOW);
  }

  analogWrite(motorXEnable, power);
}

void moveY(byte p, byte dir) {
  int power = int(p);
  if (dir != 1 && dir != 0) {
    digitalWrite(motorY1, HIGH);
    digitalWrite(motorY2, LOW);
  } 
  else if (dir != 0) {
    digitalWrite(motorY1, LOW);
    digitalWrite(motorY2, HIGH);
  } 
  else {
    digitalWrite(motorY1, LOW);
    digitalWrite(motorY1, LOW);
  }  
  analogWrite(motorYEnable, power);
}

void stopMotors() {
  moveX(0, 0);
  moveY(0, 0);
}




