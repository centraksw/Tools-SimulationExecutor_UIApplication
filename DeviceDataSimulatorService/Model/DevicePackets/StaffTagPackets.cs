using DeviceDataSimulatorService.Model.TagType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DeviceDataSimulatorService.Model.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeviceDataSimulatorService.Model.DevicePackets
{
    public class StaffTagPackets
    {
        public static List<StaffTag> staffTags = null;
        public static byte TagPageIdx = 0;
        public static byte[] PrepareStaffTagPagingData(List<StaffTag> staffTags)
        {
            int offset = 0;
            int tagPktSize = 0;
            int totalPktSize = 0;

            byte[] buf = new byte[SettingFields.MaxBuffer];
            ushort starId = 1;

            if (staffTags != null && staffTags.Count > 0)
            {
                //for (int idx = 0; idx < staffTags.Count; idx++)
                {
                    PagingData3x pageReqList = new PagingData3x();

                    Buffer.BlockCopy(BitConverter.GetBytes(totalPktSize), 0, buf, offset, 2);
                    offset += 2;

                    buf[offset++] = SettingFields.Header_Page_Info;  //Header

                    Buffer.BlockCopy(BitConverter.GetBytes(totalPktSize), 0, buf, offset, 2);
                    offset += 2;

                    Buffer.BlockCopy(BitConverter.GetBytes(starId), 0, buf, offset, 2);
                    offset += 2;

                    buf[offset++] = 0; //Reserved

                    pageReqList.Header = SettingFields.StaffTagHeader;
                    pageReqList.DeviceId = Convert.ToUInt32(TagDataValues.tagId) + SettingFields.TagBaseId;
                    pageReqList.DeviceType = TagTypes.StaffTag;
                    pageReqList.StarId = (ushort)TagDataValues.starId;
                    pageReqList.PageIdx = TagPageIdx++;
                    pageReqList.Rssi = DevicePacketUtils.GetRandomRssi(Convert.ToInt32(TagDataValues.rssi));
                    pageReqList.Info = 0;
                    pageReqList.Version = 48;
                    pageReqList.Miscellaneuos = 0;
                    pageReqList.HardwareMajorVersion = 1;
                    pageReqList.MD = 1;
                    pageReqList.PseudoSync = 0;

                    int size = Marshal.SizeOf(typeof(PagingData3x));
                    IntPtr ptr = Marshal.AllocHGlobal(size);

                    TagPageIdx %= 16;

                    try
                    {
                        Marshal.StructureToPtr(pageReqList, ptr, false);
                        Marshal.Copy(ptr, buf, offset, size);
                        offset += size;
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }

                tagPktSize = offset - 1;
                Buffer.BlockCopy(BitConverter.GetBytes(tagPktSize), 0, buf, 0, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(tagPktSize), 0, buf, 3, 2);

                totalPktSize = tagPktSize;

                buf[offset] = DevicePacketUtils.CheckSum(buf, 2, totalPktSize - 1);
            }

            return buf;
        }

        public static byte[] PrepareStaffTagLocationData(List<StaffTag> staffTags)
        {
            int StarId = 1;

            bool blnSPLPkt = false;
            const uint TAG_BASE_ID_3X = 0x80000000;

            int dataIdx = 1;
            byte MonitorType = 0;
            byte FwType = 0;
            byte TagPktType = 2;
            byte MonitorPktType = 0;

            int idx = 0;
            int StartId = 0;
            int EndId = 0;
            int TagPktSize = 0;
            int TotalPktSize = 0;
            ushort EMTagPktSize = 0;
            int MonitorPktSize = 0;
            int OfflineTempCnt = 0;

            byte Pwrlvl = 0;
            byte Profile1 = 0;
            byte Profile2 = 0;
            byte undefined = 1;

            ushort offset = 0;
            ushort EmOffset = 0;
            ushort CurRoomId = 0;

            ushort TotProfile = 0;
            ushort RawHumidity = 0, RawTemp = 0;
            int Command = 0;
            string Str = "";
            byte[] buf = new byte[1020 * 40];
            byte[] Embuf = new byte[1020 * 40];

            TagData data = new TagData();

            data.DeviceId = 0;
            data.Type = 0;
            data.StarId = 0;
            data.Command = 0;
            data.RoomId = 0;
            data.Status = 0;
            data.Rssi = 0;
            data.LocPktType = 0;

            ALLOW_LIST list = new ALLOW_LIST();

            list.DeviceId = 0;
            list.Profile1 = 0;
            list.Profile2 = 0;
            list.Profile3 = 0;
            list.Profile4 = 0;
            list.Profile5 = 0;
            list.Profile6 = 0;
            list.CmdValue = 0;
            list.command = 0;

            RawTemp = 2366;
            RawHumidity = 3000;
            RawTemp <<= 2; // 9464
            RawHumidity <<= 2; // 12000
            RawTemp |= 1 << 12; // 13560
            RawHumidity |= 1 << 12; //16096

            data.Rssi = DevicePacketUtils.GetRandomRssi(Convert.ToInt32(TagDataValues.rssi)); // 2

            Buffer.BlockCopy(BitConverter.GetBytes(TotalPktSize), 0, buf, offset, 2);
            offset += 2;

            buf[offset++] = 23;

            Buffer.BlockCopy(BitConverter.GetBytes(TotalPktSize), 0, buf, offset, 2);
            offset += 2;

            Buffer.BlockCopy(BitConverter.GetBytes(TagDataValues.starId), 0, buf, offset, 2);
            offset += 2;

            buf[offset++] = 0; //Reserved

            buf[offset++] = 0xD; //PktLen-Reserved

            Int32 RF_HDR_LOCATION_INFO_3X_EX = 5;
            Int32 RF_TAG_IRID_PACKET = 5;

            buf[offset++] = Convert.ToByte((((RF_HDR_LOCATION_INFO_3X_EX & 0x7) << 5) | (RF_TAG_IRID_PACKET & 0xF)));  //SubHeader

            data.DeviceId = Convert.ToUInt32(TagDataValues.tagId) + TAG_BASE_ID_3X;

            Buffer.BlockCopy(BitConverter.GetBytes(data.DeviceId), 0, buf, offset, 4);
            offset += 4;

            buf[offset++] = TagTypes.StaffTag; //Type

            Buffer.BlockCopy(BitConverter.GetBytes(TagDataValues.starId), 0, buf, offset, 2);
            offset += 2;

            data.Status = Convert.ToByte((dataIdx & 0xF) << 4);

            data.Status |= 0x4;
            data.Status |= 0x1;

            //if (settings.Alive) data.Status |= 0x8;
            buf[offset++] = data.Status;

            CurRoomId = 1;

            Buffer.BlockCopy(BitConverter.GetBytes(TagDataValues.roomId), 0, buf, offset, 2);
            offset += 2;

            buf[offset++] = (byte)2; // data.Rssi;

            TagPktSize = offset - 1;

            Buffer.BlockCopy(BitConverter.GetBytes(TagPktSize), 0, buf, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(TagPktSize), 0, buf, 3, 2);

            TotalPktSize = TagPktSize;

            int StarReportIndex = TagDataValues.starId;

            Buffer.BlockCopy(BitConverter.GetBytes(StarReportIndex), 0, buf, 5, 2);

            buf[offset] = DevicePacketUtils.CheckSum(buf, 2, TotalPktSize - 1);

            return buf;
        }

        public static byte[] PrepareWifiTagInfoEx(List<StaffTag> staffTags, UInt32 TagId)
        {
            int PktLen = 0;
            int offset = 0;
            int RoomIdx = 0;
            int StartId = 0;
            int VendorLen = 0;
            int MacCount = 0;
            int TagDataIdx = 0;
            int WifiStarId = 0;
            int DataOffset = 13;
            int LastBeaconSuqNum = 1;
            float floorX = 0;
            float floorY = 0;
            byte[] buf = new byte[130];
            byte[] Vendorbuf = new byte[1024];
            byte[] databuffer = new byte[(1024 * 80)];

            byte TagType = 8;
            byte TagPktType = 2;
            UInt32 DeviceId = 0;

            int dataIdx = 0;

            int CycleCount = 0;

            DateTime currentTime = DateTime.Now;
            // If you need to convert it to a Unix timestamp (equivalent to time_t)
            long unixTimestamp = new DateTimeOffset(currentTime).ToUnixTimeSeconds();

            WIFI_TAGINFO WifiTagInfo = new WIFI_TAGINFO();

            //int GDDType = cmbGDDType.GetItemData(cmbGDDType.GetCurSel());

            //00:06:66:78:00:00

            byte[] WiFiMacId = DevicePacketUtils.ConvertMacStringToByteArray("00:06:66:78:00:00");
            byte[] MacId = new byte[6];
            Array.Clear(WiFiMacId, 0, MacId.Length);
            Array.Copy(WiFiMacId, MacId, 6);

            ++dataIdx;
            dataIdx %= 16;

            WifiStarId = 1;
            StartId = 10000;

            databuffer[DataOffset++] = 3;

            MacId[5] = Convert.ToByte(WiFiMacId[5] + 0);

            MacCount++;

            Buffer.BlockCopy(MacId, 0, databuffer, DataOffset, 6);
            DataOffset += 6;

            WifiTagInfo.FloorId = 5;

            byte[] floorIdBytes = BitConverter.GetBytes(WifiTagInfo.FloorId);
            Buffer.BlockCopy(floorIdBytes, 0, databuffer, DataOffset, 4);
            DataOffset += 4;

            floorX = (float)TagDataValues.x;// WifiTagInfo.X;
            floorY = (float)TagDataValues.y;//WifiTagInfo.Y;

            //if (settings.RandowXY)
            //{
            //    floorX = GetRandomXY(WifiTagInfo.X, 10);
            //    floorY = GetRandomXY(WifiTagInfo.Y, 10);
            //}

            byte[] floorXBytes = BitConverter.GetBytes(floorX);
            Buffer.BlockCopy(floorXBytes, 0, databuffer, DataOffset, 4);
            DataOffset += 4;

            byte[] floorYBytes = BitConverter.GetBytes(floorY);
            Buffer.BlockCopy(floorYBytes, 0, databuffer, DataOffset, 4);
            DataOffset += 4;

            byte[] zBytes = BitConverter.GetBytes(WifiTagInfo.Z);
            Buffer.BlockCopy(zBytes, 0, databuffer, DataOffset, 4);
            DataOffset += 4;

            byte[] objectIdBytes = BitConverter.GetBytes(WifiTagInfo.ObjectId);
            Buffer.BlockCopy(objectIdBytes, 0, databuffer, DataOffset, 4);
            DataOffset += 4;

            byte[] confidenceFactorBytes = BitConverter.GetBytes(WifiTagInfo.ConfidenceFactor);
            Buffer.BlockCopy(confidenceFactorBytes, 0, databuffer, DataOffset, 4);
            DataOffset += 4;

            byte[] ipAddrBytes = BitConverter.GetBytes(WifiTagInfo.IPAddr);
            Buffer.BlockCopy(ipAddrBytes, 0, databuffer, DataOffset, 4);
            DataOffset += 4;

            WifiTagInfo.ChangedOn = unixTimestamp;

            byte[] changedOn = BitConverter.GetBytes(WifiTagInfo.ChangedOn);
            Buffer.BlockCopy(changedOn, 0, databuffer, DataOffset, 4);
            DataOffset += 4;

            WifiTagInfo.CampusName = TagDataValues.campus;
            WifiTagInfo.BuildingName = TagDataValues.building;
            WifiTagInfo.FloorName = TagDataValues.floor;

            string strbuf = string.Format("{0}>{1}>{2}", WifiTagInfo.CampusName, WifiTagInfo.BuildingName, WifiTagInfo.FloorName);

            byte[] strbufBytes = Encoding.UTF8.GetBytes(strbuf);

            int bytesToCopy = Math.Min(128, strbufBytes.Length);
            Buffer.BlockCopy(strbufBytes, 0, databuffer, DataOffset, bytesToCopy);

            /*if (bytesToCopy < 128)
            {
                Array.Clear(databuffer, DataOffset + bytesToCopy, 128 - bytesToCopy);
            }*/

            DataOffset += 128;

            byte[] vendorIdBytes = BitConverter.GetBytes(WifiTagInfo.VendorId);
            Buffer.BlockCopy(vendorIdBytes, 0, databuffer, DataOffset, 4);
            DataOffset += 4;

            Array.Clear(Vendorbuf, 0, Vendorbuf.Length);
            VendorLen = 34;

            byte[] vendorLenBytes = BitConverter.GetBytes(VendorLen);
            Buffer.BlockCopy(vendorLenBytes, 0, databuffer, DataOffset, 2);
            DataOffset += 2;

            DeviceId = Convert.ToUInt32(TagDataValues.tagId) + SettingFields.TagBaseId;

            int value = 0;
            int vendoroffset = 2;

            Vendorbuf[vendoroffset++] = 36;  //PacketLength

            Vendorbuf[vendoroffset++] = 61;

            Vendorbuf[vendoroffset++] = 8;

            vendoroffset++;	//LocPacketType

            byte[] deviceIdBytes = BitConverter.GetBytes(DeviceId);
            Buffer.BlockCopy(deviceIdBytes, 0, Vendorbuf, vendoroffset, 4);
            vendoroffset += 4;

            vendoroffset += 2;

            value = (ushort)((0x5 << 12) | 10);
            byte[] valueBytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(valueBytes, 0, Vendorbuf, vendoroffset, 3);
            vendoroffset += 3;

            value = dataIdx;
            Vendorbuf[vendoroffset++] = (byte)value;

            TagDataIdx++;
            TagDataIdx %= 16;

            int RoomId = TagDataValues.roomId;

            value = (ushort)(RoomId | (1 << 12));
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, Vendorbuf, vendoroffset, 2);
            vendoroffset += 2;

            byte Keys = TagDataValues.wifiKeys;
            bool Alive = false;
            bool Motion = true;

            value = (Keys << 4) & 0xF0;
            value |= (1 & 0x3);

            if (Alive) value |= 0x8;
            if (Motion) value |= 0x4;

            if (Motion)
                Vendorbuf[vendoroffset++] = (byte)value;
            else
                vendoroffset++;

            int TagLBIValue = 1;

            Buffer.BlockCopy(BitConverter.GetBytes(TagLBIValue), 0, Vendorbuf, vendoroffset, 2);
            vendoroffset += 2;

            Buffer.BlockCopy(BitConverter.GetBytes(WifiTagInfo.WlanProfile), 0, Vendorbuf, vendoroffset, 2);
            vendoroffset += 2;

            value = (WifiTagInfo.WlanChannelMask & 0xFFF) | ((WifiTagInfo.WIFIReportingTime & 0xF) << 12);
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, Vendorbuf, vendoroffset, 2);
            vendoroffset += 2;

            byte TagVersion = 1;

            value = (ushort)(TagVersion & 0xFF);
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, Vendorbuf, vendoroffset, sizeof(ushort));
            vendoroffset += 2;

            vendoroffset += 12;

            Vendorbuf[1] = Convert.ToByte(vendoroffset - 2); //TotalPacketLen

            Vendorbuf[vendoroffset++] = Convert.ToByte((WifiTagInfo.FirmwareVersion >> 8) & 0xFF);
            Vendorbuf[vendoroffset++] = Convert.ToByte((WifiTagInfo.FirmwareVersion) & 0xFF);

            if (VendorLen > 0)
            {
                Buffer.BlockCopy(Vendorbuf, 0, databuffer, DataOffset, VendorLen);
                DataOffset += VendorLen;
            }

            PktLen = DataOffset - 2; // PacketLen
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)PktLen), 0, databuffer, offset, 2);
            offset += 2;

            // Add Header
            databuffer[offset++] = 19;

            // Copy WifiStarId
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)WifiStarId), 0, databuffer, offset, 2);
            offset += 2;

            // Copy CycleCount
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)CycleCount), 0, databuffer, offset, 2);
            offset += 2;

            // Copy tm (assuming it's an int or uint)
            Buffer.BlockCopy(BitConverter.GetBytes(unixTimestamp), 0, databuffer, offset, 4);
            offset += 4;

            // Recalculate Packet Length
            PktLen = DataOffset - 13;
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)PktLen), 0, databuffer, offset, 2);
            offset += 2;


            return databuffer;
        }

        public static List<StaffTag> GetStaffTagData(string fileName)
        {
            string deviceData = "";

            List<StaffTag> staffTags = new List<StaffTag>();

            string fileNameOnly = Path.GetFileName(fileName);

            if (fileNameOnly.ToLower().IndexOf("csv") > 0)
            {
                List<StaffTagCsv> staffTagCsv = File.ReadAllLines(fileName)
                                           .Skip(1)
                                           .Select(v => StaffTagCsv.FromCsv(v))
                                           .ToList();

                if (staffTagCsv != null && staffTagCsv.Count > 0)
                {

                    for (int idx = 0; idx < staffTagCsv.Count; idx++)
                    {

                        StaffTag staffTag = new StaffTag();

                        List<string> TagIds = staffTagCsv[idx].tagIds.Split(',').ToList();

                        staffTag.tagIds = TagIds;
                        staffTag.tagType = staffTagCsv[idx].tagType;
                        staffTag.rfReportRate = staffTagCsv[idx].rfReportRate;
                        staffTag.wifiReportRate = staffTagCsv[idx].wifiReportRate;
                        staffTag.bleReportRate = staffTagCsv[idx].bleReportRate;
                        staffTag.activeTime = staffTagCsv[idx].activeTime;
                        staffTag.rfActiveReportRate = staffTagCsv[idx].rfActiveReportRate;
                        staffTag.wifiActiveReportRate = staffTagCsv[idx].wifiActiveReportRate;
                        staffTag.bleActiveReportRate = staffTagCsv[idx].bleActiveReportRate;
                        staffTag.reloadConfigInterval = staffTagCsv[idx].reloadConfigInterval;


                        Locations locations = new Locations();

                        locations.transport = staffTagCsv[idx].transport;
                        locations.starId = staffTagCsv[idx].starId;
                        locations.roomId = staffTagCsv[idx].roomId;
                        locations.dwellTime = staffTagCsv[idx].dwellTime;
                        locations.rssi = staffTagCsv[idx].rssi;
                        locations.campus = staffTagCsv[idx].campus;
                        locations.building = staffTagCsv[idx].building;
                        locations.floor = staffTagCsv[idx].floor;
                        locations.x = staffTagCsv[idx].x;
                        locations.y = staffTagCsv[idx].y;
                        locations.latitude = staffTagCsv[idx].latitude;
                        locations.longitude = staffTagCsv[idx].longitude;
                        locations.latency = staffTagCsv[idx].latency;

                        List<string> keys = staffTagCsv[idx].keys.Split(',').ToList();
                        locations.keys = keys;

                        staffTag.locations.Add(locations);

                        staffTags.Add(staffTag);

                    }
                }
            }
            else
            {
                using (StreamReader r = new StreamReader(fileName))
                {
                    deviceData = r.ReadToEnd();
                    staffTags = JsonConvert.DeserializeObject<List<StaffTag>>(deviceData);
                }
            }

            return staffTags;
        }

        public static List<StaffTag> GetTagDataFromFile(string fileType,string fileData)
        {
            string deviceData = "";

            List<StaffTag> staffTags = new List<StaffTag>();
          
            if (fileType.ToLower() == "csv")
            {
                string[] lines = fileData.Split('\n');
                List<string> linesList = new List<string>();

                for (int i = 1; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        linesList.Add(lines[i]);
                    }
                }
                 
                if (linesList.Count > 0)
                {
                    List<StaffTagCsv> staffTagCsv = ConvertToObjectList(linesList);

                    if (staffTagCsv != null && staffTagCsv.Count > 0)
                    {
                        for (int idx = 0; idx < staffTagCsv.Count; idx++)
                        {
                            StaffTag staffTag = new StaffTag();

                            List<string> TagIds = staffTagCsv[idx].tagIds.Split(',').ToList();

                            staffTag.tagIds = TagIds;
                            staffTag.tagType = staffTagCsv[idx].tagType;
                            staffTag.rfReportRate = staffTagCsv[idx].rfReportRate;
                            staffTag.wifiReportRate = staffTagCsv[idx].wifiReportRate;
                            staffTag.bleReportRate = staffTagCsv[idx].bleReportRate;
                            staffTag.activeTime = staffTagCsv[idx].activeTime;
                            staffTag.rfActiveReportRate = staffTagCsv[idx].rfActiveReportRate;
                            staffTag.wifiActiveReportRate = staffTagCsv[idx].wifiActiveReportRate;
                            staffTag.bleActiveReportRate = staffTagCsv[idx].bleActiveReportRate;
                            staffTag.reloadConfigInterval = staffTagCsv[idx].reloadConfigInterval;

                            Locations locations = new Locations();

                            locations.transport = staffTagCsv[idx].transport;
                            locations.starId = staffTagCsv[idx].starId;
                            locations.roomId = staffTagCsv[idx].roomId;
                            locations.dwellTime = staffTagCsv[idx].dwellTime;
                            locations.rssi = staffTagCsv[idx].rssi;
                            locations.campus = staffTagCsv[idx].campus;
                            locations.building = staffTagCsv[idx].building;
                            locations.floor = staffTagCsv[idx].floor;
                            locations.x = staffTagCsv[idx].x;
                            locations.y = staffTagCsv[idx].y;
                            locations.latitude = staffTagCsv[idx].latitude;
                            locations.longitude = staffTagCsv[idx].longitude;
                            locations.latency = staffTagCsv[idx].latency;

                            List<string> keys = staffTagCsv[idx].keys.Split(',').ToList();
                            locations.keys = keys;

                            staffTag.locations.Add(locations);

                            staffTags.Add(staffTag);

                        }
                    }
                }
            }
            else
            {
                staffTags = JsonConvert.DeserializeObject<List<StaffTag>>(fileData);             
            }

            return staffTags;
        }

        public static List<StaffTagCsv> ConvertToObjectList(List<string> fileData)
        {
            List<StaffTagCsv> staffTagData = fileData.Select(item =>
            {

                var parts = item.Split(',');
                return new StaffTagCsv
                {
                    tagIds = parts[0],
                    tagType = parts[1],
                    rfReportRate = int.Parse(string.IsNullOrEmpty(parts[2]) ? "0" : parts[2]),
                    wifiReportRate = int.Parse(string.IsNullOrEmpty(parts[3]) ? "0" : parts[3]),
                    bleReportRate = int.Parse(string.IsNullOrEmpty(parts[4]) ? "0" : parts[4]),
                    activeTime = int.Parse(string.IsNullOrEmpty(parts[5]) ? "0" : parts[5]),
                    rfActiveReportRate = int.Parse(string.IsNullOrEmpty(parts[6]) ? "0" : parts[6]),
                    wifiActiveReportRate = int.Parse(string.IsNullOrEmpty(parts[7]) ? "0" : parts[7]),
                    bleActiveReportRate = int.Parse(string.IsNullOrEmpty(parts[8]) ? "0" : parts[8]),
                    reloadConfigInterval = int.Parse(string.IsNullOrEmpty(parts[9]) ? "0" : parts[9]),
                    transport = parts[10],
                    starId = int.Parse(string.IsNullOrEmpty(parts[11]) ? "0" : parts[11]),
                    roomId = int.Parse(string.IsNullOrEmpty(parts[12]) ? "0" : parts[12]),
                    dwellTime = int.Parse(string.IsNullOrEmpty(parts[13]) ? "0" : parts[13]),
                    rssi = decimal.Parse(string.IsNullOrEmpty(parts[14]) ? "0.0" : parts[14]),
                    keys = parts[15],
                    campus = parts[16],
                    building = parts[17],
                    floor = parts[18],
                    x = decimal.Parse(string.IsNullOrEmpty(parts[19]) ? "0.0" : parts[19]),
                    y = decimal.Parse(string.IsNullOrEmpty(parts[20]) ? "0.0" : parts[20]),
                    latitude = decimal.Parse(string.IsNullOrEmpty(parts[21]) ? "0.0" : parts[21]),
                    longitude = decimal.Parse(string.IsNullOrEmpty(parts[22]) ? "0.0" : parts[22]),
                    latency = int.Parse(string.IsNullOrEmpty(parts[23]) ? "0" : parts[23])
                };
            }).ToList();
            return staffTagData;
        }
    }
}
