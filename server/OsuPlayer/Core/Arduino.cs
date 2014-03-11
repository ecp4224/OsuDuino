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
        public static void Init(string portName)
        {
            port = new SerialPort(portName, 9600);
            port.Open();
        }
        
        public static void SendCommand(byte[] data)
        {
            if (!port.IsOpen)
                throw new InvalidOperationException("The Arduino port is not open!");
            port.Write(data, 0, data.Length);
        }

        public static void RequestLeftClick()
        {
            SendCommand(new byte[] { (byte)OpCode.LeftClick });
        }

        public static void RequestRightClick()
        {
            SendCommand(new byte[] { (byte)OpCode.RightClick });
        }

        public static void StopMotors()
        {
            SendCommand(new byte[] { (byte)OpCode.Stop });
        }

        public static void MoveX(byte power = 255)
        {
            SendCommand(new byte[] { (byte)OpCode.SetMotorX, power });
        }

        public static void MoveY(byte power = 255)
        {
            SendCommand(new byte[] { (byte)OpCode.SetMotorY, power });
        }

        public enum OpCode : byte
        {
            LeftClick = 1,
            RightClick = 2,
            SetMotorX = 4,
            SetMotorY = 8,
            Stop = 16
        }
    }
}
