using DeviceDataSimulatorService.Model.DevicePackets;
using DeviceDataSimulatorService.Model.Settings;
using DeviceDataSimulatorService.Model.TagType;
using DeviceDataSimulatorService.Transport.TCPClient;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace DeviceDataSimulatorService
{
    public partial class SimulatorService : ServiceBase
    {
        List<REDIS_SUBSCRIBE_DATA> lstSubscribedData = new List<REDIS_SUBSCRIBE_DATA>();
        Thread thread_SendLocationAndPagingData = null;
        bool bStopSendingData = false;
        string pagingServerIP = "";
        int pagingServerPort = 5050;
        string locationSeverIP = "";
        int locationServerPort = 3030;
        string fileType = "";
        string fileData = "";
        
        public SimulatorService()
        {
            InitializeComponent();
        }

        public void StartDebug()
        {
            string[] a = new string[10];
            OnStart(a);
        }

        protected override void OnStart(string[] args)
        {
            Global.LoadSettingFields();
            StartRedisListener();          
        }

        private void StartRedisListener()
        {            
            ConfigurationOptions opt = new ConfigurationOptions
            {
                EndPoints =
                {
                    {Global.RedisSettings.Redis_ServerIP, Global.RedisSettings.Redis_Port}
                },
                KeepAlive = 3600,
                Password = "centrak123",
                AllowAdmin = true
            };

            ConnectionMultiplexer redis = null;
            ISubscriber sub = null;

            try
            {
                redis = ConnectionMultiplexer.Connect(opt);
                sub = redis.GetSubscriber();
            }
            catch (Exception ex)
            {
                Global.WriteLog(Global.ERROR_FILE, "StartRedisListener Exception: " + ex.Message);
                return;
            }                       

            sub = redis.GetSubscriber();
        
            try
            {
                string value = "{\"fileType\":\"JSON\",\"file\":\"data:application/json;base64,Ww0KDQogICAgew0KDQogICAgICAgICJ0YWdJZHMiOiBbIjU1MDU1MCJdLA0KDQogICAgICAgICJ0YWdUeXBlIjogInN0YWZmIiwNCg0KICAgICAgICAicmZSZXBvcnRSYXRlIjogMzAwLA0KDQogICAgICAgICJ3aWZpUmVwb3J0UmF0ZSI6IDQwMCwNCg0KICAgICAgICAiYmxlUmVwb3J0UmF0ZSI6IDMwMCwNCg0KICAgICAgICAiYWN0aXZlVGltZSI6IDMwLA0KDQogICAgICAgICJyZkFjdGl2ZVJlcG9ydFJhdGUiOiAzLA0KDQogICAgICAgICJ3aWZpQWN0aXZlUmVwb3J0UmF0ZSI6IDQsDQoNCiAgICAgICAgImJsZUFjdGl2ZVJlcG9ydFJhdGUiOiAzLA0KDQogICAgICAgICJyZWxvYWRDb25maWdJbnRlcnZhbCI6IDIwLA0KDQogICAgICAgICJsb2NhdGlvbnMiOiBbDQoNCiAgICAgICAgICAgIHsNCg0KICAgICAgICAgICAgICAgICJ0cmFuc3BvcnQiOiAiaXIiLA0KDQogICAgICAgICAgICAgICAgInN0YXJJZCI6IDEyLA0KDQogICAgICAgICAgICAgICAgInJvb21JZCI6IDIxMywNCg0KICAgICAgICAgICAgICAgICJkd2VsbFRpbWUiOiAzMCwNCg0KICAgICAgICAgICAgICAgICJyc3NpIjogLTc3LA0KICAgICAgICAgICAgICAgIA0KICAgICAgICAgICAgICAgICJrZXlzIjpudWxsDQoNCiAgICAgICAgICAgIH0sDQogICAgICAgICAgICB7DQoNCiAgICAgICAgICAgICAgICAidHJhbnNwb3J0IjogIndpZmkiLA0KDQogICAgICAgICAgICAgICAgImNhbXB1cyI6ICJNYWluIENhbXB1cyIsDQoNCiAgICAgICAgICAgICAgICAiYnVpbGRpbmciOiAiQnVpbGRpbmcgMSIsDQoNCiAgICAgICAgICAgICAgICAiZmxvb3IiOiAiQmFzZW1lbnQiLA0KDQogICAgICAgICAgICAgICAgIngiOiAyMy40LA0KDQogICAgICAgICAgICAgICAgInkiOiA1Ni4yLA0KDQogICAgICAgICAgICAgICAgImxhdGl0dWRlIjogbnVsbCwNCg0KICAgICAgICAgICAgICAgICJsb25naXR1ZGUiOiBudWxsLA0KDQogICAgICAgICAgICAgICAgImR3ZWxsVGltZSI6IDEyMCwNCg0KICAgICAgICAgICAgICAgICJrZXlzIjogWzFdLA0KDQogICAgICAgICAgICAgICAgImxhdGVuY3kiOiAyMA0KDQogICAgICAgICAgICB9DQoNCiAgICAgICAgXQ0KDQogICAgfSwNCiAgICB7DQogICAgDQogICAgICAgICAgICAidGFnSWRzIjogWyI1NTA1NTAiXSwNCiAgICANCiAgICAgICAgICAgICJ0YWdUeXBlIjogInN0YWZmIiwNCiAgICANCiAgICAgICAgICAgICJyZlJlcG9ydFJhdGUiOiAzMDAsDQogICAgDQogICAgICAgICAgICAid2lmaVJlcG9ydFJhdGUiOiA0MDAsDQogICAgDQogICAgICAgICAgICAiYmxlUmVwb3J0UmF0ZSI6IDMwMCwNCiAgICANCiAgICAgICAgICAgICJhY3RpdmVUaW1lIjogMzAsDQogICAgDQogICAgICAgICAgICAicmZBY3RpdmVSZXBvcnRSYXRlIjogMywNCiAgICANCiAgICAgICAgICAgICJ3aWZpQWN0aXZlUmVwb3J0UmF0ZSI6IDQsDQogICAgDQogICAgICAgICAgICAiYmxlQWN0aXZlUmVwb3J0UmF0ZSI6IDMsDQogICAgDQogICAgICAgICAgICAicmVsb2FkQ29uZmlnSW50ZXJ2YWwiOiAyMCwNCiAgICANCiAgICAgICAgICAgICJsb2NhdGlvbnMiOiBbDQogICAgDQogICAgICAgICAgICAgICAgew0KICAgIA0KICAgICAgICAgICAgICAgICAgICAidHJhbnNwb3J0IjogImlyIiwNCiAgICANCiAgICAgICAgICAgICAgICAgICAgInN0YXJJZCI6IDEyLA0KICAgIA0KICAgICAgICAgICAgICAgICAgICAicm9vbUlkIjogMjE0LA0KICAgIA0KICAgICAgICAgICAgICAgICAgICAiZHdlbGxUaW1lIjogMzAsDQogICAgDQogICAgICAgICAgICAgICAgICAgICJyc3NpIjogLTc3LA0KICAgIA0KICAgICAgICAgICAgICAgICAgICAia2V5cyI6IG51bGwNCiAgICANCiAgICAgICAgICAgICAgICB9LA0KICAgICAgICAgICAgICAgIHsNCiAgICANCiAgICAgICAgICAgICAgICAgICAgInRyYW5zcG9ydCI6ICJ3aWZpIiwNCiAgICANCiAgICAgICAgICAgICAgICAgICAgImNhbXB1cyI6ICJTdWIgQ2FtcHVzIiwNCiAgICANCiAgICAgICAgICAgICAgICAgICAgImJ1aWxkaW5nIjogIkJ1aWxkaW5nIDIiLA0KICAgIA0KICAgICAgICAgICAgICAgICAgICAiZmxvb3IiOiAiRmxvb3IgMSIsDQogICAgDQogICAgICAgICAgICAgICAgICAgICJ4IjogMzMuNCwNCiAgICANCiAgICAgICAgICAgICAgICAgICAgInkiOiA2NS4yLA0KICAgIA0KICAgICAgICAgICAgICAgICAgICAibGF0aXR1ZGUiOiBudWxsLA0KICAgIA0KICAgICAgICAgICAgICAgICAgICAibG9uZ2l0dWRlIjogbnVsbCwNCiAgICANCiAgICAgICAgICAgICAgICAgICAgImR3ZWxsVGltZSI6IDEyMCwNCiAgICANCiAgICAgICAgICAgICAgICAgICAgImtleXMiOiBbMV0sDQogICAgDQogICAgICAgICAgICAgICAgICAgICJsYXRlbmN5IjogMjANCiAgICANCiAgICAgICAgICAgICAgICB9DQogICAgDQogICAgICAgICAgICBdDQogICAgDQogICAgfQ0KDQpd\",\"serverName\":\"192.168.0.104\"}";
                DataContractJsonSerializer data = new DataContractJsonSerializer(typeof(REDIS_SUBSCRIBE_DATA));
                MemoryStream mStrm = new MemoryStream(Encoding.UTF8.GetBytes((string)value));
                REDIS_SUBSCRIBE_DATA cmd = (REDIS_SUBSCRIBE_DATA)data.ReadObject(mStrm);

                if (!string.IsNullOrEmpty(cmd.fileType))
                {
                    fileType = cmd.fileType;
                }

                if (!string.IsNullOrEmpty(cmd.file))
                {
                    string fileBase64String = "";

                    if (fileType == "CSV")
                    {
                        string prefix = "data:text/csv;base64,";
                        fileBase64String = cmd.file.Substring(prefix.Length);
                    }

                    if (fileType == "JSON")
                    {
                        string prefix = "data:application/json;base64,";
                        fileBase64String = cmd.file.Substring(prefix.Length);
                    }

                    byte[] bfileData = Convert.FromBase64String(fileBase64String);
                    fileData = Encoding.UTF8.GetString(bfileData);
                    Console.WriteLine("File Data: " + fileData);
                }

                if (!string.IsNullOrEmpty(cmd.serverName))
                {
                    pagingServerIP = cmd.serverName;
                    locationSeverIP = cmd.serverName;
                }

                if (fileType.Length > 0 && fileData.Length > 0)
                {
                    lstSubscribedData.Add(cmd);
                    thread_SendLocationAndPagingData = new Thread(new ThreadStart(_thread_LocationAndPagingDataListener));
                    thread_SendLocationAndPagingData.Start();
                }

                mStrm.Dispose();                            

                /*sub.Subscribe("SENDCOMMAND", (channel, value) =>
                {
                    if ((string)channel == "SENDCOMMAND")
                    {
                        DataContractJsonSerializer data = new DataContractJsonSerializer(typeof(REDIS_SUBSCRIBE_DATA));
                        MemoryStream mStrm = new MemoryStream(Encoding.UTF8.GetBytes((string)value));
                        REDIS_SUBSCRIBE_DATA cmd = (REDIS_SUBSCRIBE_DATA)data.ReadObject(mStrm);

                        Global.WriteLog(Global.LOG_FILE, "Received Value: " + cmd);

                        if (!string.IsNullOrEmpty(cmd.fileType))
                        {
                            fileType = cmd.fileType;
                        }

                        if (!string.IsNullOrEmpty(cmd.file))
                        {
                            string fileBase64String = "";

                            if (fileType == "CSV")
                            {
                                string prefix = "data:text/csv;base64,";
                                fileBase64String = cmd.file.Substring(prefix.Length);
                            }

                            if (fileType == "JSON")
                            {
                                string prefix = "data:application/json;base64,";
                                fileBase64String = cmd.file.Substring(prefix.Length);
                            }

                            byte[] bfileData = Convert.FromBase64String(fileBase64String);
                            fileData = Encoding.UTF8.GetString(bfileData);
                            Console.WriteLine("File Data: " + fileData);
                        }

                        if (!string.IsNullOrEmpty(cmd.serverName))
                        {
                            pagingServerIP = cmd.serverName;
                            locationSeverIP = cmd.serverName;
                        }

                        if (fileType.Length > 0 && fileData.Length > 0)
                        {
                            lstSubscribedData.Add(cmd);
                            thread_SendLocationAndPagingData = new Thread(new ThreadStart(_thread_LocationAndPagingDataListener));
                            thread_SendLocationAndPagingData.Start();
                        }

                        mStrm.Dispose();
                        Global.WriteLog(Global.LOG_FILE, "Receive Channel Subscribe :" + (string)value);
                    }
                });*/
            }
            catch (Exception ex)
            {
                Global.WriteLog(Global.ERROR_FILE, "StartRedisListener Exception: " + ex.Message);
                return;
            }
        }

        private void _thread_LocationAndPagingDataListener()
        {
            UInt32 TagId = 0;

            StaffTagPackets.staffTags = StaffTagPackets.GetTagDataFromFile(fileType,fileData);

            string TagIds = "";
            string[] arrTagIds = null;
            UInt32 startId = 0;
            UInt32 endId = 0;

            while (!bStopSendingData)
            {
                if (StaffTagPackets.staffTags != null)
                {

                    //for (UInt32 i = startId; i <= endId; i++) 
                    {
                        if (!bStopSendingData)
                        {
                            for (int ninc = 0; ninc < StaffTagPackets.staffTags.Count; ninc++)
                            {
                                TagIds = StaffTagPackets.staffTags[ninc].tagIds[0];

                                if (TagIds.IndexOf('-') > 0)
                                {
                                    arrTagIds = TagIds.Split('-');
                                }
                                else
                                {
                                    arrTagIds = new string[2];
                                    arrTagIds[0] = TagIds;
                                    arrTagIds[1] = TagIds;
                                }

                                startId = Convert.ToUInt32(arrTagIds[0]);
                                endId = Convert.ToUInt32(arrTagIds[1]);

                                for (UInt32 i = startId; i <= endId; i++)
                                {
                                    TagDataValues.tagId = i;
                                    TagDataValues.wifiActiveReportRate = StaffTagPackets.staffTags[ninc].wifiActiveReportRate;
                                    TagDataValues.rfActiveReportRate = StaffTagPackets.staffTags[ninc].rfActiveReportRate;

                                    for (int nidx = 0; nidx < StaffTagPackets.staffTags[ninc].locations.Count; nidx++)
                                    {
                                        string transport = StaffTagPackets.staffTags[ninc].locations[nidx].transport;

                                        if (transport == "wifi")
                                        {
                                            TagDataValues.campus = StaffTagPackets.staffTags[ninc].locations[nidx].campus;
                                            TagDataValues.building = StaffTagPackets.staffTags[ninc].locations[nidx].building;
                                            TagDataValues.floor = StaffTagPackets.staffTags[ninc].locations[nidx].floor;
                                            TagDataValues.x = StaffTagPackets.staffTags[ninc].locations[nidx].x;
                                            TagDataValues.y = StaffTagPackets.staffTags[ninc].locations[nidx].y;
                                            TagDataValues.wifiStarId = 1;

                                            if (StaffTagPackets.staffTags[ninc].locations[nidx].keys != null && StaffTagPackets.staffTags[ninc].locations[nidx].keys.Count > 0)
                                            {
                                                if (!String.IsNullOrEmpty(StaffTagPackets.staffTags[ninc].locations[nidx].keys[0]))
                                                    TagDataValues.wifiKeys = Convert.ToByte(StaffTagPackets.staffTags[ninc].locations[nidx].keys[0]);
                                            }
                                        }
                                        else if (transport == "ir")
                                        {

                                            if (StaffTagPackets.staffTags[ninc].locations[nidx].keys != null && StaffTagPackets.staffTags[ninc].locations[nidx].keys.Count > 0)
                                            {
                                                if (!String.IsNullOrEmpty(StaffTagPackets.staffTags[ninc].locations[nidx].keys[0]))
                                                    TagDataValues.wifiKeys = Convert.ToByte(StaffTagPackets.staffTags[ninc].locations[nidx].keys[0]);
                                            }

                                            TagDataValues.starId = StaffTagPackets.staffTags[ninc].locations[nidx].starId;
                                            TagDataValues.rssi = StaffTagPackets.staffTags[ninc].locations[nidx].rssi;
                                            TagDataValues.roomId = StaffTagPackets.staffTags[ninc].locations[nidx].roomId;
                                        }
                                    }

                                    var sendingData = new
                                    {
                                        tagId = TagDataValues.tagId,
                                        campus = "",
                                        building = "",
                                        floor = "",
                                        x = 0.0,
                                        y = 0.0,
                                        starId = TagDataValues.starId,
                                        rssi = TagDataValues.rssi,
                                        roomId = TagDataValues.roomId,
                                        keys = TagDataValues.keys
                                    };

                                    string tagSendingData = JsonConvert.SerializeObject(sendingData);
                                    Thread.Sleep(TagDataValues.rfActiveReportRate * 1000);

                                    SendPagingData();
                                    SendLocationData();

                                    Thread.Sleep(TagDataValues.wifiActiveReportRate * 1000);

                                    var sendingDataWiFi = new
                                    {
                                        tagId = TagDataValues.tagId,
                                        campus = TagDataValues.campus,
                                        building = TagDataValues.building,
                                        floor = TagDataValues.floor,
                                        x = TagDataValues.x,
                                        y = TagDataValues.y,
                                        starId = TagDataValues.wifiStarId,
                                        rssi = TagDataValues.rssi,
                                        roomId = TagDataValues.roomId,
                                        keys = TagDataValues.wifiKeys
                                    };

                                    tagSendingData = JsonConvert.SerializeObject(sendingDataWiFi);

                                    SendWiFiLocationData(TagId);
                                }
                            }
                        }
                        else
                            break;
                    }
                }
            }
        }
        private void SendPagingData()
        {
            Global.WriteLog("test.txt",StaffTagPackets.staffTags.ToString());
            byte[] buf = StaffTagPackets.PrepareStaffTagPagingData(StaffTagPackets.staffTags);

            Client.CreateTcpClient();
            Client.Connect(pagingServerIP, pagingServerPort);
            Client.SendData(buf);
            Client.Close();
        }

        private void SendLocationData()
        {
            Global.WriteLog("test.txt", StaffTagPackets.staffTags.ToString());
            byte[] buf = StaffTagPackets.PrepareStaffTagLocationData(StaffTagPackets.staffTags);

            Client.CreateTcpClient();
            Client.Connect(locationSeverIP, locationServerPort);
            Client.SendData(buf);
            Client.Close();


        }

        private void SendWiFiLocationData(UInt32 TagId)
        {
            Global.WriteLog("test.txt", StaffTagPackets.staffTags.ToString());
            byte[] buf = StaffTagPackets.PrepareWifiTagInfoEx(StaffTagPackets.staffTags, TagId);

            Client.CreateTcpClient();
            Client.Connect(locationSeverIP, Convert.ToInt32(3031));
            Client.SendData(buf);
            Client.Close();
        }

        protected override void OnStop()
        {
            try
            {
                if (thread_SendLocationAndPagingData != null)
                    thread_SendLocationAndPagingData.Abort();
            }
            catch (ThreadAbortException ex)
            {
                Global.WriteLog(Global.ERROR_FILE, "Thread Abort:" + ex.Message);
                if (ex.InnerException != null)
                {
                    Global.WriteLog(Global.ERROR_FILE, "Stop(): " + ex.InnerException.Message);
                }
            }
        }
    }
}
