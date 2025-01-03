using System.Runtime.Serialization;

namespace DeviceDataSimulatorService.Model.Settings
{
    [System.Runtime.Serialization.DataContract]
    public struct REDIS_SUBSCRIBE_DATA
    {
        [DataMember]
        public string fileType;
        [DataMember]
        public string file;
        [DataMember]
        public string serverName;
       
        public void Initialize()
        {
            fileType = "";
            file = "";
            serverName = "";
        }
    }

    public class SettingFields
    {       
        public static uint TagBaseId = 0x80000000;

        public static byte StaffTagHeader = 19;

        public static int MaxBuffer = 1024 * 40;

        public static byte Header_Page_Info = 20;

        public static int WiFiLocationDataPort = 3031;
    }
}
