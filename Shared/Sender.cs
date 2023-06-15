﻿using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

#pragma warning disable

namespace Shared
{
    public class Sender
    {
        public byte[] SerializePacket(Packet packet)
        {
            string jsonString = JsonSerializer.Serialize(packet);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        public void SendData(string serverIp, int serverPort, byte[] data, int packetSize)
        {
            try
            {
                var end_point = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

                using (UdpClient udp_client = new UdpClient())
                {
                    var lastno = data.Length / packetSize + (data.Length % packetSize > 0 ? 1 : 0);
                    var offset = 0;

                    for (int i = 0; i < lastno; i++)
                    {
                        var size = Math.Min(packetSize, data.Length - offset);
                        var packet_data = new byte[size];
                        Array.Copy(data, offset, packet_data, 0, size);

                        offset += size;

                        var packet = new Packet { seqno = i, size = packetSize, lastno = lastno, data = packet_data };
                        var send_data = SerializePacket(packet);
                        udp_client.Send(send_data, send_data.Length, end_point);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public byte[] CaptureScreen()
        {
            var bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            }

            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Jpeg);
                return memoryStream.ToArray();
            }
        }
    }
}