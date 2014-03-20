using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace OsuPlayer.Core
{
    public class Arduino
    {
        private static SerialPort port;
        private static OpCode lastX, lastY;
        public static void Init(string portName)
        {
            port = new SerialPort(portName, 9600); //Create a new SerialPort object
            port.Open(); //And open the port so we can write data
        }

        public static void SendCommand(byte[] data)
        {
            if (port == null) return; //If there is no port (wat), don't do anything
            if (!port.IsOpen) return; //If the port isn't open, don't do anything
            port.Write(data, 0, data.Length); //Write the data provided to the arduino
        }

        /// <summary>
        /// Send the byte 1 to the Arduino to tell it to press down
        /// </summary>
        public static void RequestLeftDown()
        {
            SendCommand(new byte[] { (byte)OpCode.LeftDown });
        }

        /// <summary>
        /// Send the byte 2 to the Arduino to tell it to lift up
        /// </summary>
        public static void RequestLeftUp()
        {
            SendCommand(new byte[] { (byte)OpCode.LeftUp });
        }

        /// <summary>
        /// Send the byte 4 to the Arduino to tell it to move left
        /// </summary>
        public static void MoveLeft()
        {
            if (lastX == OpCode.MoveLeft) return;
            else lastX = OpCode.MoveLeft;
            SendCommand(new byte[] { (byte)OpCode.MoveLeft });
        }

        /// <summary>
        /// Send the byte 8 to the Arduino to tell it to move right
        /// </summary>
        public static void MoveRight()
        {
            if (lastX == OpCode.MoveRight) return;
            else lastX = OpCode.MoveRight;
            SendCommand(new byte[] { (byte)OpCode.MoveRight });
        }

        /// <summary>
        /// Send the byte 32 to the Arduino to tell it to move up
        /// </summary>
        public static void MoveUp()
        {
            if (lastY == OpCode.MoveUp) return;
            else lastY = OpCode.MoveUp;
            SendCommand(new byte[] { (byte)OpCode.MoveUp });
        }

        /// <summary>
        /// Send the byte 64 to the Arduino to tell it to move down
        /// </summary>
        public static void MoveDown()
        {
            if (lastY == OpCode.MoveDown) return;
            else lastY = OpCode.MoveDown;
            SendCommand(new byte[] { (byte)OpCode.MoveDown });
        }

        /// <summary>
        /// Send the byte 16 to the Arduino to tell it to stop the X motor
        /// </summary>
        public static void StopX()
        {
            if (lastX == OpCode.StopX) return;
            else lastX = OpCode.StopX;
            SendCommand(new byte[] { (byte)OpCode.StopX });
        }

        /// <summary>
        /// Send the byte 128 to the Arduino to tell it to stop the Y motor
        /// </summary>
        public static void StopY()
        {
            if (lastY == OpCode.StopY) return;
            else lastY = OpCode.StopY;
            SendCommand(new byte[] { (byte)OpCode.StopY });
        }

        //All possible OpCode's for the Arduino
        public enum OpCode : byte
        {
            LeftDown = 1,
            LeftUp = 2,
            MoveLeft = 4,
            MoveRight = 8,
            StopX = 16,
            MoveUp = 32,
            MoveDown = 64,
            StopY = 128
        }
    }
}
