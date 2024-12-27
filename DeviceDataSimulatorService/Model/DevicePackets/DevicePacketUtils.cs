using System;

namespace DeviceDataSimulatorService.Model.DevicePackets
{
    public class DevicePacketUtils
    {
        public static byte CheckSum(byte[] buf, int offset, int len)
        {
            byte checksum = 0;

            for (int idx = 0; idx < len; idx++)
            {
                checksum += buf[offset + idx]; // Using the offset
            }

            return checksum;
        }

        public static byte GetRandomRssi(int Rssi)
        {
            if (Rssi <= 0)
                Rssi = 1;

            Random random = new Random();
            byte randomrssi = Convert.ToByte(Rssi);
            randomrssi = Convert.ToByte(randomrssi % Rssi + 1);

            return randomrssi;
        }

        public static byte[] ConvertMacStringToByteArray(string mac)
        {
            // Split the MAC address into parts
            string[] macParts = mac.Split(':');
            byte[] macBytes = new byte[macParts.Length];

            for (int i = 0; i < macParts.Length; i++)
            {
                // Convert each hex pair to a byte
                macBytes[i] = Convert.ToByte(macParts[i], 16);
            }

            return macBytes;
        }
    }
}
