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

        public static void MoveLeft()
        {
            if (lastX == OpCode.MoveLeft) return;
            else lastX = OpCode.MoveLeft;
            SendCommand(new byte[] { (byte)OpCode.MoveLeft });
        }

        public static void MoveRight()
        {
            if (lastX == OpCode.MoveRight) return;
            else lastX = OpCode.MoveRight;
            SendCommand(new byte[] { (byte)OpCode.MoveRight });
        }

        public static void MoveUp()
        {
            if (lastY == OpCode.MoveUp) return;
            else lastY = OpCode.MoveUp;
            SendCommand(new byte[] { (byte)OpCode.MoveUp });
        }

        public static void MoveDown()
        {
            if (lastY == OpCode.MoveDown) return;
            else lastY = OpCode.MoveDown;
            SendCommand(new byte[] { (byte)OpCode.MoveDown });
        }

        public static void StopX()
        {
            if (lastX == OpCode.StopX) return;
            else lastX = OpCode.StopX;
            SendCommand(new byte[] { (byte)OpCode.StopX });
        }

        public static void StopY()
        {
            if (lastY == OpCode.StopY) return;
            else lastY = OpCode.StopY;
            SendCommand(new byte[] { (byte)OpCode.StopY });
        }

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
