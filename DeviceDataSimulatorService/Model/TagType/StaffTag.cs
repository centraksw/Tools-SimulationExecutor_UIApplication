using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DeviceDataSimulatorService.Model.TagType
{
    public class StaffTag
    {
        public List<string> tagIds { get; set; }
        public string tagType { get; set; } = string.Empty;
        public int rfReportRate { get; set; } = 0;
        public int wifiReportRate { get; set; } = 0;
        public int bleReportRate { get; set; } = 0;
        public int activeTime { get; set; } = 0;
        public int rfActiveReportRate { get; set; } = 0;
        public int wifiActiveReportRate { get; set; } = 0;
        public int bleActiveReportRate { get; set; } = 0;
        public int reloadConfigInterval { get; set; } = 0;
        public List<Locations> locations { get; set; } = new List<Locations>();
    }

    public class Locations
    {
        public string transport { get; set; } = string.Empty;
        public int starId { get; set; } = 0;
        public int roomId { get; set; } = 0;
        public int dwellTime { get; set; } = 0;
        public decimal rssi { get; set; } = 0;
        public List<string> keys { get; set; }
        public string campus { get; set; } = string.Empty;
        public string building { get; set; } = string.Empty;
        public string floor { get; set; } = string.Empty;
        public decimal x { get; set; } = 0;
        public decimal y { get; set; } = 0;
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
        public int latency { get; set; } = 0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PagingData3x
    {
        public byte Header { get; set; }
        public UInt32 DeviceId { get; set; }
        public ushort StarId { get; set; }
        public byte DeviceType { get; set; }
        public byte PageIdx { get; set; }
        public byte MD { get; set; }
        public byte PseudoSync { get; set; }
        public byte Info { get; set; }
        public ushort Version { get; set; }
        public ushort Miscellaneuos { get; set; }
        public byte HardwareMajorVersion { get; set; }
        public byte Rssi { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TagData
    {
        public UInt32 DeviceId;
        public byte Type;
        public ushort StarId;
        public ushort Command;
        public ushort RoomId;
        public byte Status;
        public byte Rssi;
        public byte LocPktType;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ALLOW_LIST
    {
        public UInt32 DeviceId;
        public ushort Profile1;
        public ushort Profile2;
        public UInt32 Profile3;
        public UInt32 Profile4;
        public ushort Profile5;
        public ushort Profile6;
        public ushort CmdValue;
        public byte command;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct WIFI_TAGINFO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] MacId; // BYTE MacId[6];

        public byte TelemetryType; // BYTE TelemetryType;
        public int ClientStatus; // int ClientStatus;

        public float X; // float X;
        public float Y; // float Y;
        public float Z; // float Z;
        public float ConfidenceFactor; // float confidenceFactor;
        public float CalcX; // float calcX;
        public float CalcY; // float calcY;
        public float Temp; // float Temp; 

        public long BatteryAge; // long batteryAge;
        public long FloorId; // long FloorId;
        public long CalcFloorId; // long calcFloorId;
        public long ChangedOn; // long ChangedOn;

        public int ObjectId; // int objectId;
        public int FloorWeight; // int floorWeight;
        public int IPAddr; // int IPAddr;
        public int NetworkStatus; // int networkStatus;    
        public int VendorId; // int vendorId; 
        public int Tolerance; // int tolerance;
        public int PercentRemaining; // int percentRemaining;
        public int DaysRemaining; // int daysRemaining;
        public int BatteryStatus; // int batteryStatus;

        public int Protocol; // int protocol;
        public int PolicyType; // int policyType;
        public int EapType; // int eapType;
        public int EncryptionCypher; // int encryptionCypher;

        public int FirmwareVersion; // int FirmwareVersion;
        public int WakeRegister; // int Wakeregister;
        public int LastBecSquNum; // int LastBecSquNum;

        public ushort WlanProfile; // WORD wlanProfile;
        public ushort WlanChannelMask; // WORD wlanChannelMask;
        public ushort WIFIReportingTime; // WORD WIFIReportingTime;
        public ushort WlanChannel; // WORD wlanChannel;

        public byte WlanBitRate; // BYTE wlanBitRate;
        public byte WlanRetry; // BYTE wlanRetry;
        public byte WlanPower; // BYTE wlanPower;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string BuildingName; // char BuildingName[32];	

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FloorName; // char FloorName[32];

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string CampusName; // char CampusName[32];

        public float Latitude; // float latitude;
        public float Longitude; // float longitude;
    }

    public static class TagDataValues
    {
        public static UInt32 tagId { get; set; }
        public static string tagType { get; set; } = string.Empty;
        public static int rfReportRate { get; set; } = 0;
        public static int wifiReportRate { get; set; } = 0;
        public static int bleReportRate { get; set; } = 0;
        public static int activeTime { get; set; } = 0;
        public static int rfActiveReportRate { get; set; } = 0;
        public static int wifiActiveReportRate { get; set; } = 0;
        public static int bleActiveReportRate { get; set; } = 0;
        public static int reloadConfigInterval { get; set; } = 0;

        public static string transport { get; set; } = string.Empty;
        public static int starId { get; set; } = 0;
        public static int roomId { get; set; } = 0;
        public static int dwellTime { get; set; } = 0;
        public static decimal rssi { get; set; } = 0;
        public static byte keys { get; set; } = 0;
        public static string campus { get; set; } = string.Empty;
        public static string building { get; set; } = string.Empty;
        public static string floor { get; set; } = string.Empty;
        public static decimal x { get; set; } = 0;
        public static decimal y { get; set; } = 0;
        public static decimal? latitude { get; set; }
        public static decimal? longitude { get; set; }
        public static int latency { get; set; } = 0;

        public static int wifiStarId { get; set; } = 0;
        public static byte wifiKeys { get; set; } = 0;

    }

    public class StaffTagCsv
    {
        public string tagIds { get; set; }
        public string tagType { get; set; } = string.Empty;
        public int rfReportRate { get; set; } = 0;
        public int wifiReportRate { get; set; } = 0;
        public int bleReportRate { get; set; } = 0;
        public int activeTime { get; set; } = 0;
        public int rfActiveReportRate { get; set; } = 0;
        public int wifiActiveReportRate { get; set; } = 0;
        public int bleActiveReportRate { get; set; } = 0;
        public int reloadConfigInterval { get; set; } = 0;
        public string transport { get; set; } = string.Empty;
        public int starId { get; set; } = 0;
        public int roomId { get; set; } = 0;
        public int dwellTime { get; set; } = 0;
        public decimal rssi { get; set; } = 0;
        public string keys { get; set; } = string.Empty;
        public string campus { get; set; } = string.Empty;
        public string building { get; set; } = string.Empty;
        public string floor { get; set; } = string.Empty;
        public decimal x { get; set; } = 0;
        public decimal y { get; set; } = 0;
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
        public int latency { get; set; } = 0;


        public static StaffTagCsv FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            StaffTagCsv deviceValues = new StaffTagCsv();

            deviceValues.tagIds = Convert.ToString(values[0]);
            deviceValues.tagType = Convert.ToString(values[1]);
            deviceValues.rfReportRate = Convert.ToInt32(values[2]);
            deviceValues.wifiReportRate = Convert.ToInt32(values[3]);
            deviceValues.bleReportRate = Convert.ToInt32(values[4]);
            deviceValues.activeTime = Convert.ToInt32(values[5]);
            deviceValues.rfActiveReportRate = Convert.ToInt32(values[6]);
            deviceValues.wifiActiveReportRate = Convert.ToInt32(values[7]);
            deviceValues.bleActiveReportRate = Convert.ToInt32(values[8]);
            deviceValues.reloadConfigInterval = Convert.ToInt32(values[9]);
            deviceValues.transport = Convert.ToString(values[10]);
            deviceValues.starId = Convert.ToInt32(values[11]);
            deviceValues.roomId = Convert.ToInt32(values[12]);
            deviceValues.dwellTime = Convert.ToInt32(values[13]);
            deviceValues.rssi = Convert.ToDecimal(values[14]);
            deviceValues.keys = Convert.ToString(values[15]);
            deviceValues.campus = Convert.ToString(values[16]);
            deviceValues.building = Convert.ToString(values[17]);
            deviceValues.floor = Convert.ToString(values[18]);

            decimal x = 0.0m;
            decimal y = 0.0m;
            decimal latitude = 0.0m;
            decimal longitude = 0.0m;
            int latency = 0;

            if (String.IsNullOrEmpty(values[19]))
            {
                deviceValues.x = x;
            }
            else
            {
                deviceValues.x = Convert.ToDecimal(values[19]);
            }

            if (String.IsNullOrEmpty(values[20]))
            {
                deviceValues.y = y;
            }
            else
            {
                deviceValues.y = Convert.ToDecimal(values[20]);
            }

            if (String.IsNullOrEmpty(values[21]))
            {
                deviceValues.latitude = latitude;
            }
            else
            {
                deviceValues.latitude = Convert.ToDecimal(values[21]);
            }

            if (String.IsNullOrEmpty(values[22]))
            {
                deviceValues.longitude = longitude;
            }
            else
            {
                deviceValues.longitude = Convert.ToDecimal(values[22]);
            }

            if (String.IsNullOrEmpty(values[23]))
            {
                deviceValues.latency = latency;
            }
            else
            {
                deviceValues.latency = Convert.ToInt32(values[23]);
            }

            return deviceValues;
        }

    }
}
