using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace LAN_project
{
    public class MotoComUDP
    {
        private IPAddress _IP_Address;
        private int _port;
        private int _localPort;

        UdpClient sendingUdpClient;

        IPEndPoint ipEndPointSending;
        IPEndPoint ipEndPointReceiving;
        
        const string ON_SERVO_CMD = "59 45 52 43 20 00 04 00 03 01 00 01 00 00 00 00 39 39 39 39 39 39 39 39 83 00 02 00 01 10 00 00 01 00 00 00";
        const string OFF_SERVO_CMD = "59 45 52 43 20 00 04 00 03 01 00 01 00 00 00 00 39 39 39 39 39 39 39 39 83 00 02 00 01 10 00 00 02 00 00 00";
        const string GET_POS_CMD = "59 45 52 43 20 00 00 00 03 01 00 00 00 00 00 00 39 39 39 39 39 39 39 39 75 00 65 00 00 01 00 00";
        const string GET_POS_PULSE = "59 45 52 43 20 00 00 00 03 01 00 00 00 00 00 00 39 39 39 39 39 39 39 39 75 00 01 00 00 01 00 00";
        const string Write_Register = "59 45 52 43 20 01 00 00 03 01 00 00 00 00 00 00 39 39 39 39 39 39 39 39 79 00";
        const string Read_Register = "59 45 52 43 20 00 00 00 03 01 00 00 00 00 00 00 39 39 39 39 39 39 39 39 79 00";
        const string Write_IO = "59 45 52 43 20 01 00 00 03 01 00 00 00 00 00 00 39 39 39 39 39 39 39 39 78 00";
        const string Read_IO = "59 45 52 43 20 00 00 00 03 01 00 00 00 00 00 00 39 39 39 39 39 39 39 39 78 00";
        const string pos_write_type = "59 45 52 43 20 0D 00 00 03 01 00 00 00 00 00 00 39 39 39 39 39 39 39 39 7F 00 00 00 01 02 00 00 10 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00";
        const string pos_read_type = "59 45 52 43 20 00 00 00 03 01 00 00 00 00 00 00 39 39 39 39 39 39 39 39 7F 00 00 00 01 01 00 00";
        const string job_select = "59 45 52 43 20 00 04 00 03 01 00 01 00 00 00 00 39 39 39 39 39 39 39 39 87 00 01 00 01 02 00 00";
        public byte[] temper = new byte[] { 89, 69, 82, 67, 32, 0, 0, 0, 3, 1, 0, 0, 0, 0, 0, 0, 57, 57, 57, 57, 57, 57, 57, 57, 117, 0, 1, 0, 0, 1, 0, 0 };
        string MOVE_POS_CMD;

        string receivedData;
        Byte[] receiveBytes;
        
        public string ReceivedData { get; set; }
        public byte[] ReceiveBytes { get; set; }
        //public byte[] testing { get; set; }
        public MotoComUDP(IPAddress ip_address, int port, int localPort)
        {
            _IP_Address = ip_address;
            _port = port;
            _localPort = localPort;
        }
        public bool ConnectMotoman()
        {
            ipEndPointReceiving = new IPEndPoint(IPAddress.Any, 0);
            ipEndPointSending = new IPEndPoint(_IP_Address, _port);
            sendingUdpClient = new UdpClient();
            return true;
        }

        public bool CloseMotoman()
        {
            sendingUdpClient.Close();          
            return true;
        }
        
        public string SendCommand(Byte[] command)
        {
            sendingUdpClient.Send(command, command.Length, ipEndPointSending);
            return ByteArrayToHex(command);
        }

        public void ReceiveDataThread()
        {
            ReceiveBytes = sendingUdpClient.Receive(ref ipEndPointReceiving);
            ReceivedData = ByteArrayToHex(ReceiveBytes);
        }
        public void ReceiveDataThread_direct()
        {
            ReceiveBytes = sendingUdpClient.Receive(ref ipEndPointReceiving);
        }
        public byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", string.Empty);

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        public string ByteArrayToHex(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString().ToUpper();
        }

        public byte[] Int32ToByteArray(Int32 value)
        {
            byte[] intBytes = BitConverter.GetBytes(value);
            return intBytes;
        }
        public int ByteArrayToInt32(byte[] intBytes)
        {
            int value = BitConverter.ToInt32(intBytes, 0);
            return value;
        }

        public T[] SubArray<T>(T[] array, int start, int lenght)
        {

            return array.Skip(start).Take(lenght).ToArray();
        }
        public T[] ConcatArrays<T>(params T[][] list)
        {
            var result = new T[list.Sum(a => a.Length)];
            int offset = 0;
            for (int x = 0; x < list.Length; x++)
            {
                list[x].CopyTo(result, offset);
                offset += list[x].Length;
            }
            return result;
        }
        public bool TurnOnServo()
        {

            SendCommand(StringToByteArray(ON_SERVO_CMD));
            return true;
        }

        public bool TurnOffServo()
        {
            SendCommand(StringToByteArray(OFF_SERVO_CMD));
            return true;
        }

        public bool GetPosition()
        {
            SendCommand(StringToByteArray(GET_POS_CMD));
            return true;
        }
        public bool GetPosition_pulse()
        {
            SendCommand(StringToByteArray(GET_POS_PULSE));
            return true;
        }

        public bool MoveJoint(Int32 motionSpeed,
                              Int32 X_cordVal, Int32 Y_cordVal, Int32 Z_cordVal,
                              Int32 Rx_angle, Int32 Ry_angle, Int32 Rz_angle,
                              Int32 numTool = 0)
        {
            byte[] moveJCmd = StringToByteArray("59 45 52 43 20 00 68 00 03 01 00 01 00 00 00 00 39 39 39 39 39 39 39 39 8A 00 01 00 01 02 00 00 01 00 00 00 00 00 00 00 00 00 00 00");

            moveJCmd = ConcatArrays(moveJCmd, Int32ToByteArray(motionSpeed), StringToByteArray("10 00 00 00"),
                                    Int32ToByteArray(X_cordVal), Int32ToByteArray(Y_cordVal), Int32ToByteArray(Z_cordVal),
                                    Int32ToByteArray(Rx_angle), Int32ToByteArray(Ry_angle), Int32ToByteArray(Rz_angle),
                                    StringToByteArray("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00"));

            SendCommand(moveJCmd);

            MOVE_POS_CMD = ByteArrayToHex(moveJCmd);
            return true;
        }
        public bool MovePulse(Int32 motionSpeed, Int32 one, Int32 two, Int32 three, Int32 four, Int32 five, Int32 six)
        {
            byte[] movePCmd = StringToByteArray("59 45 52 43 20 00 58 00 03 01 00 01 00 00 00 00 39 39 39 39 39 39 39 39 8B 00 01 00 01 02 00 00 01 00 00 00 00 00 00 00 00 00 00 00");
            movePCmd = ConcatArrays(movePCmd, Int32ToByteArray(motionSpeed),
                                    Int32ToByteArray(one), Int32ToByteArray(two), Int32ToByteArray(three),
                                    Int32ToByteArray(four), Int32ToByteArray(five), Int32ToByteArray(six),
                                    StringToByteArray("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00"));
            SendCommand(movePCmd);
            MOVE_POS_CMD = ByteArrayToHex(movePCmd);
            return true;
        }
        public bool Write_Read_Register(int number, Int32 value)
        {
            byte[] conv = BitConverter.GetBytes(number);
            byte[] Cmd = StringToByteArray(Write_Register);
            Cmd = ConcatArrays(Cmd, conv, StringToByteArray("01 01 00 00"), Int32ToByteArray(value));
            SendCommand(Cmd);
            return true;
        }
        public bool Write_Read_Register(int number)
        {
            byte[] conv = BitConverter.GetBytes(number);
            byte[] Cmd = StringToByteArray(Read_Register);
            Cmd = ConcatArrays(Cmd, conv, StringToByteArray("01 0E 00 00"));
            SendCommand(Cmd);
            return true;
        }
        public bool Write_Read_IO(int number, Int32 value)
        {
            byte[] conv = BitConverter.GetBytes(number);
            byte[] Cmd = StringToByteArray(Write_IO);
            Cmd = ConcatArrays(Cmd, conv, StringToByteArray("01 01 00 00"), Int32ToByteArray(value));
            SendCommand(Cmd);
            return true;
        }
        public bool Write_Read_IO(int number)
        {
            byte[] conv = BitConverter.GetBytes(number);
            byte[] Cmd = StringToByteArray(Read_IO);
            Cmd = ConcatArrays(Cmd, conv, StringToByteArray("01 0E 00 00"));
            SendCommand(Cmd);
            return true;
        }
        public bool Execute_job(String jobname)
        {
            byte[] data_send = StringToByteArray(job_select + jobname);
            data_send[5] = Convert.ToByte(jobname.Length);
            SendCommand(data_send);
            return true;
        }

        public string[] Get_feedback_staus()
        {
            String[] status_addedstatus = new String[2];
            char[] data = ReceivedData.ToArray();
            status_addedstatus[0] = "0x" + data[50] + data[51];
            status_addedstatus[1] = "0x" + data[56] + data[57] + data[58] + data[59];
            return status_addedstatus;
        }
        /*
        public string GetCommandString(string commandName)
        {
            switch (commandName)
            {
                case "TurnOnServo":
                    return "TurnOnServo: " + ON_SERVO_CMD;
                    break;
                case "TurnOffServo":
                    return "TurnOffServo: " + OFF_SERVO_CMD;
                    break;
                case "GetPosition":
                    return "GetPosition: " + GET_POS_CMD;
                    break;
                case "MoveJoint":
                    return "MoveJoint: " + MOVE_POS_CMD;
                    break;
                default:
                    return "Wrong command name!!!";
                    break;
            }
        }*/
    }
}
