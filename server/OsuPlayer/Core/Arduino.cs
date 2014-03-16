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
            if (port == null) return;
            if (!port.IsOpen) return;
            port.Write(data, 0, data.Length);
        }

        public static void RequestLeftDown()
        {
            SendCommand(new byte[] { (byte)OpCode.LeftDown });
        }

        public static void RequestLeftUp()
        {
            SendCommand(new byte[] { (byte)OpCode.LeftUp });
        }

        public static void RequestRightDown()
        {
            SendCommand(new byte[] { (byte)OpCode.RightDown });
        }

        public static void RequestRightUp()
        {
            SendCommand(new byte[] { (byte)OpCode.RightUp });
        }

        public static void StopMotors()
        {
            SendCommand(new byte[] { (byte)OpCode.Stop });
        }

        public static void MoveX(Direction direction, byte power = 255)
        {
            SendCommand(new byte[] { (byte)OpCode.SetMotorX, power, (byte)direction });
        }

        public static void MoveY(Direction direction, byte power = 255)
        {
            SendCommand(new byte[] { (byte)OpCode.SetMotorY, power, (byte)direction });
        }

        public enum Direction : byte
        {
            Foward = 1,
            Back = 2
        }

        public enum OpCode : byte
        {
            LeftDown = 1,
            RightDown = 2,
            LeftUp = 4,
            RightUp = 8,
            SetMotorX = 16,
            SetMotorY = 32,
            Stop = 64
        }
    }
}
