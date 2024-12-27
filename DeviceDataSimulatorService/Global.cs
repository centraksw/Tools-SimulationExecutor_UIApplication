using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace DeviceDataSimulatorService
{
    public class RedisSettingsFields
    {
        public string Redis_ServerIP = "";
        public int Redis_Port = 0;
    }
    
    class Global
    {
        public static string LOG_FILE = "SimulatorServiceLog.txt";
        public static string ERROR_FILE = "SimulatorServiceError.txt";
        static Object lockWriteLog = new object();

        public static RedisSettingsFields RedisSettings = new RedisSettingsFields();

        public static void LoadSettingFields()
        {
            string json = "";
            var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            try
            {
                if (File.Exists(filePath))
                {
                    json = File.ReadAllText(filePath);

                    object JsonSettings = JsonConvert.DeserializeObject(json);
                    JObject obj = JObject.Parse(JsonSettings.ToString());

                    if (obj != null && obj["RedisSettings"] != null)
                    {
                        dynamic settingsField = JsonConvert.DeserializeObject<RedisSettingsFields>(obj["RedisSettings"].ToString());
                        if (settingsField != null)
                        {
                            RedisSettings.Redis_ServerIP = settingsField.Redis_ServerIP;
                            RedisSettings.Redis_Port = settingsField.Redis_Port;
                        }
                    }
                }

                if (RedisSettings.Redis_ServerIP == "")
                {
                    RedisSettings.Redis_ServerIP = Global.GetLocalIP();
                }

                if (RedisSettings.Redis_Port == 0)
                {
                    RedisSettings.Redis_Port = 6379;
                }

                WriteLog(LOG_FILE, "Redis configuration loaded successfully!");
            }
            catch (Exception ex)
            {
                WriteLog(ERROR_FILE, "ERROR in LoadSettingFields:" + ex.Message.ToString());
                if (ex.InnerException != null)
                {
                    Global.WriteLog(Global.ERROR_FILE, "LoadSettingFields(): " + ex.InnerException.Message);
                }
            }
        }
        public static void WriteLog(String strFilename, String strLog)
        {
            strFilename = GetFileName(strFilename, "");

            lock (lockWriteLog)
            {
                DeleteBackUpFile(strFilename);
                try
                {
                    StreamWriter sw = new StreamWriter(strFilename, true);
                    sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] - " + strLog);
                    sw.Flush();
                    sw.Close();
                    sw = null;
                }
                catch (Exception ex)
                {
                    WriteLog(ERROR_FILE, "ERROR: WriteLog - " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        Global.WriteLog(Global.ERROR_FILE, "WriteLog(): " + ex.InnerException.Message);
                    }
                }
            }
        }

        public static void DeleteBackUpFile(String strFileName)
        {
            String strBackupFilename;
            long size = 0;
            try
            {
                size = GetLocalFilesize(strFileName);
                if (size >= 25)
                {
                    strBackupFilename = strFileName.Substring(0, strFileName.Length - 4) + "-old.txt";
                    if (File.Exists(strBackupFilename))
                        File.Delete(strBackupFilename);
                    File.Move(strFileName, strBackupFilename);
                }
            }
            catch (Exception ex)
            {
                WriteLog(ERROR_FILE, "ERROR: DeleteBackUpFile - " + ex.Message);
                if (ex.InnerException != null)
                {
                    Global.WriteLog(Global.ERROR_FILE, "DeleteBackUpFile(): " + ex.InnerException.Message);
                }
            }
        }

        public static long GetLocalFilesize(String strfullpath)
        {
            long size = 0;
            try
            {
                if (File.Exists(strfullpath))
                {
                    FileInfo f = new FileInfo(strfullpath);
                    size = f.Length / (1024 * 1024);
                }
            }
            catch (Exception ex)
            {
                WriteLog(ERROR_FILE, "ERROR: GetLocalFilesize - " + ex.Message);
                if (ex.InnerException != null)
                {
                    Global.WriteLog(Global.ERROR_FILE, "GetLocalFilesize(): " + ex.InnerException.Message);
                }
            }
            return size;
        }

        public static String GetFileName(String strFilename, String strSubDir)
        {
            string appPath = Global.GetAppPath();

            if (strSubDir.Length <= 0)
            {
                appPath += strFilename;
            }
            else
            {
                appPath += strSubDir + "\\" + strFilename;
            }

            return appPath;
        }

        public static string GetAppPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetLocalIP()
        {
            try
            {
                String strHostName = Dns.GetHostName();
                IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
                foreach (IPAddress ip in iphostentry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ERROR_FILE, "Failed to GetLocalIP" + ex.Message);
                if (ex.InnerException != null)
                {
                    Global.WriteLog(Global.ERROR_FILE, "GetLocalIP(): " + ex.InnerException.Message);
                }
            }
            return "";
        }
    }
}
