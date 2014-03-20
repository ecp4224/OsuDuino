#include <Servo.h> //Import servo library

//Ports                        
int leftClickPort = 10; //The port the servo is in

int motorXEnable = 9; //The enable port motor X is in
int motorX1 = 3; //The turn1 port for motor X
int motorX2 = 7; //The turn2 port for motor X

int motorYEnable = 11; //The enable port motor Y is in
int motorY1 = 5; //The turn1 port for motor Y
int motorY2 = 12;  //The turn2 port for motor Y

//Clicking vars
int leftClickDown = 35; //The angle the servo should be at for pressing down
int leftClickUp = 20; //The angle the servo should be at for lifting up
Servo leftClick; //The actual servo

void setup() {
  pinMode(motorXEnable, OUTPUT); //Set output for enable pin x
  pinMode(motorX1, OUTPUT); //Set output for turn1 x
  pinMode(motorX2, OUTPUT); //Set output for turn2 x

  pinMode(motorYEnable, OUTPUT); //Set output for enable pin y
  pinMode(motorY1, OUTPUT); //Set output for turn1 y
  pinMode(motorY2, OUTPUT); //Set output for turn2 y

  stopMotors(); //Stop all the motors  
  
  leftClick.attach(leftClickPort); //Setup the servo on leftClickPort
  leftClick.write(leftClickUp); //Tell the servo to lift up (stop clicking the mouse button)
  Serial.begin(9600); //Begin accepting data through USB
}

void loop() {
  byte opCode; //The instruction provided from the computer vis USB
  while ((opCode = Serial.read()) != -1) { //Read the next instruction and only run if it doesn't equal -1
    switch (opCode) { //Check which instruction was provided
    case 1: //Left down
      leftClick.write(leftClickDown); //Press down on the button
      break;
    case 2: //Left up
      leftClick.write(leftClickUp); //Let go of the button
      break;
    case 4: //Move X Motor Back
      moveX(150, 2); //Move the motor backwards with half the power
      break;
    case 8: //Move X Motor Foward
      moveX(150, 1); //Move the motor foward with half the power
      break;
    case 16: //Stop Motor X
      moveX(0, 0); //Stop the motor
      break;
    case 32: //Move Y Motor Foward
      moveY(150, 1); //Move the motor foward with half the power
      break;
    case 64: //Move Y Motor Back
      moveY(150, 2); //Move the motor backwards with half the power
      break;
    case 128: //Stop Motor Y
      moveY(0, 0); //Stop the motor
      break;

    } 
  }
}

void moveX(byte p, byte dir) { //A method for moving the motor
  int power = int(p); //Convert the byte p to an int
  if (dir != 1 && dir != 0) { //Do we NOT move foward and do we NOT stop?
    digitalWrite(motorX1, HIGH); //Then move backwards
    digitalWrite(motorX2, LOW); //Then move backwards
  } 
  else if (dir != 0) { //Otherwise, are we not stopping?
    digitalWrite(motorX1, LOW); //Then move foward
    digitalWrite(motorX2, HIGH); //The move foward
  } 
  else { //Otherwise
    digitalWrite(motorX1, LOW); //Stop
    digitalWrite(motorX2, LOW); //Stop
  }

  analogWrite(motorXEnable, power); //PMW the enable pin with frequency "power"
}

void moveY(byte p, byte dir) {
  int power = int(p); //Convert byte p to int
  if (dir != 1 && dir != 0) { //Are we not moving forward and are we not stopping?
    digitalWrite(motorY1, HIGH); //Then move backwards
    digitalWrite(motorY2, LOW); //Then move backwards
  } 
  else if (dir != 0) { //Otherwise, are we not stopping?
    digitalWrite(motorY1, LOW); //Then move forwards
    digitalWrite(motorY2, HIGH); //Then move forwards
  } 
  else { //Otherwise
    digitalWrite(motorY1, LOW); //Stop
    digitalWrite(motorY1, LOW); //Stop
  }  
  analogWrite(motorYEnable, power); //PMW the enable pin with frequency "power"
}

void stopMotors() { //A method to stop all the motors
  moveX(0, 0); //Stop motor X
  moveY(0, 0); //Stop motor Y
}




