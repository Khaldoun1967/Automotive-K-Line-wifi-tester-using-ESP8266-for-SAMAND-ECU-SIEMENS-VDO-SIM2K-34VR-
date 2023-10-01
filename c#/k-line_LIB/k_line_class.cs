using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace k_line_LIB
{

    public enum Commands
    {
        Respond = 0x40,
        keep_alive = 0x3E,

        start_communication = 0x81,
        stop_communication = 0x82,
        ReadDTCByStatus = 0x18,
        ReadECUId = 0x1A,
        ReadDataByLocalId = 0x21,
        IOControlByLocalId = 0x30,
        ClearDTC = 0x14,
        ReadDTCCodes = 0x13,
        ReadStatusOfDTC = 0x17,


        Stream_Cmd = 0x01,

        
    };
  
    public enum ReadECUId
    {
        VIN = 0x90,
        Supplier_ECU_Hardware_Number = 0x92,        
        Boot_Software_Number = 0x8c,
        ECU_Software_Number = 0x8d,
        Calibration_Number = 0x8e,
        Programming_Date = 0x99
    }
    public enum IOControlByLocalId
    {
        All_off = 0x3F,
        Hi_Fan = 0x1A,
        Low_Fan = 0x1B,
        Fuel_pump = 0x12,
        AC = 0x13,
        dig_light = 0x10,
        Main_relay = 0x1C,
        Cansiter = 0x20,
        exhanst = 0x14,
        Stepper = 0x24,
        Ignition_Coil1 = 0x31,
        Ignition_Coil2 = 0x32,
        Injector1 = 0x3A,
        Injector2 = 0x3B,
        Injector3 = 0x3C,
        Injector4 = 0x3D
    }
    public class k_line_class
    {
        private byte Target;
        private byte Source;
        private byte CheckSum;
        private byte Header;
        private byte ECU_address;
        private byte Tester_address;

        public string Disconnect = "8111F18205";
        public string Read_DTC = "8411F11800FF009D";
        public string keepAlive = "8111F13EC1";
        public string ClearDTC = "8311F114000099";
        public byte[] par = new byte[100];
        public k_line_class(byte ecu_adres, byte tester_adres)
        {
            ECU_address = ecu_adres;
            Tester_address = tester_adres;
        }

        //Length = 2;
        //            Frame[4] = (byte)Commands.ReadDataByLocalId;
        //            Frame[5] = (byte)Commands.Stream_Cmd;
        public string Build_ReadDataByLocalId(byte cmd)
        {
            StringBuilder sb = new StringBuilder();
            Target = ECU_address;
            Source = Tester_address;
            Header = 0x80 + 2;
            byte CheckSum = Header;
            CheckSum += Target;
            CheckSum += Source;
            CheckSum += (byte)Commands.ReadDataByLocalId;
            CheckSum += cmd;
            sb.Append(Header.ToString("X2").ToUpper());
            sb.Append(Target.ToString("X2").ToUpper());
            sb.Append(Source.ToString("X2").ToUpper());
            sb.Append(((byte)Commands.ReadDataByLocalId).ToString("X2").ToUpper());
            sb.Append(cmd.ToString("X2").ToUpper());
            sb.Append(CheckSum.ToString("X2").ToUpper());
            return sb.ToString();
        }
     
        public string Build_ReadECUId(byte cmd)
        {
            StringBuilder sb = new StringBuilder();
            Target = ECU_address;
            Source = Tester_address;
            Header = 0x80 + 2;
            byte CheckSum = Header;
            CheckSum += Target;
            CheckSum += Source;           
            CheckSum += (byte)Commands.ReadECUId;
            CheckSum += cmd;
            sb.Append(Header.ToString("X2").ToUpper());
            sb.Append(Target.ToString("X2").ToUpper());
            sb.Append(Source.ToString("X2").ToUpper());
            sb.Append(((byte)Commands.ReadECUId).ToString("X2").ToUpper());
            sb.Append(cmd.ToString("X2").ToUpper());     
            sb.Append(CheckSum.ToString("X2").ToUpper());
            return sb.ToString();
        }
     
        public string Build_IOControlByLocalId(byte cmd)
        {
            bool off = false;
            if (cmd == (byte)IOControlByLocalId.All_off) off = true;
            StringBuilder sb = new StringBuilder();
            Target = ECU_address;
            Source = Tester_address;
            byte Low = 0x07;
            byte Hi = 0xFF;
            Header = 0x80 + 4;
            if (off == true) Header = 0x80 + 3;
            if (off == true) { Low = 0; Hi = 0; }
            byte CheckSum = Header;
            CheckSum += Target;
            CheckSum += Source;
            CheckSum += cmd;
            CheckSum += (byte)Commands.IOControlByLocalId;
            CheckSum += Low;
            CheckSum += Hi;
            sb.Append(Header.ToString("X2").ToUpper());
            sb.Append(Target.ToString("X2").ToUpper());
            sb.Append(Source.ToString("X2").ToUpper());
            sb.Append(((byte)Commands.IOControlByLocalId).ToString("X2").ToUpper());
            sb.Append(cmd.ToString("X2").ToUpper());
            sb.Append(Low.ToString("X2").ToUpper());
            if (off == false) sb.Append(Hi.ToString("X2").ToUpper());
            sb.Append(CheckSum.ToString("X2").ToUpper());
            return sb.ToString();
        }

        //80,F1,11,53,61,1,78,3D,A3,6D,A5,6C,11,0,0,11,CB,0,0,82,2B,0,0,75,0,0,0,37,4,FF,0,57,0,0,0,B0,B0,B0,B0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1D,0,80,FA,0,0,D1,96,D6,6,E1,F,7F,0,0,0,0,9A,9,0,7F,1,EB,3,0,39,DB,F,0,0,0,EA
        //80,F1,11,53,61,1,78,3D,A3,6D,A5,6C,11,0,0,11,CB,0,0,82,2B,0,0,75,0,0,0,37,4,FF,0,57,0,0,0,B0,B0,B0,B0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1D,0,80,FA,0,0,D1,96,D6,6,E1,F,7F,0,0,0,0,9A,9,0,7F,1,EB,3,0,39,DB,F,0,0,0,EA
        //80,F1,11,53,61,1,71,B5,A3,6D,A5,6C,11,0,0,11,CB,0,0,75,2B,0,0,75,0,0,0,37,4,FF,0,57,0,0,0,B0,B0,B0,B0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1D,0,80,FA,0,0,D1,96,D6,6,E1,F,7F,0,0,0,0,9A,9,0,7F,1,64,4,0,39,DB,F,0,0,0,C8
        //80,F1,11,53,61,1,81,BD,A3,6D,A6,6C,11,0,0,11,4F,7B,2,94,11,8F,19,75,0,A3,4,37,4,FF,0,59,0,0,0,9F,9F,9F,9F,26,5,1F,5,1D,5,1F,5,86,5,4C,0,0,0,0,0,0,1D,0,80,FA,6C,0,69,72,EA,5,E1,F,6A,0,0,0,0,9A,9,0,6A,4,8B,3,0,39,DB,F,0,0,0,52
        //80,F1,11,53,61,1,83,BD,A3,6D,A6,6C,11,0,0,11,54,65,2,86,12,DD,1A,75,0,42,4,37,4,FF,0,59,0,0,0,9F,9F,9F,9F,FB,4,F7,4,F3,4,FB,4,56,5,4D,0,0,0,0,0,0,1D,0,80,FA,6D,0,4F,6E,E8,3,E1,F,68,0,0,0,0,9A,9,0,68,4,72,3,0,39,DB,F,0,0,0,12

        public void decoding(string str)
        {
            if (str.Contains(",") == false) return;
            string[] ary = str.Split(',');
            byte[] arry = new byte[ary.Length];
            for (int i = 0; i < arry.Length; i++)
            {
                arry[i] = Convert.ToByte(ary[i], 16);
            }
            byte header = arry[0];
            byte target = arry[1];
            byte source = arry[2];
            byte length;
            byte frame = arry[4];
            if (header == 0x80) length = arry[3];
            else
            {
                length = arry[0];
                length &= (byte)0x3f;
            }
            byte mode1 = (byte)Commands.ReadDataByLocalId | (byte)Commands.Respond;
            byte mode2 = (byte)Commands.Stream_Cmd;
            if (arry[4] == ((byte)Commands.ReadDataByLocalId | (byte)Commands.Respond) && (arry[5] == (byte)Commands.Stream_Cmd))
            {
                for (int j = 0; j < length-5; j++)
                {
                    par[j] = arry[j+5];
                }
            }
        }
    }
}
