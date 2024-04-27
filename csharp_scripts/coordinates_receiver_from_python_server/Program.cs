using System;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;

class Program
{
    private const string HOST = "localhost"; 
    private const int PORT = 12345;

    static void Main(string[] args)
    {
        TcpClient client = new TcpClient();
        client.Connect(HOST, PORT);
        NetworkStream stream = client.GetStream();

        Console.WriteLine("Connected to server: " + HOST + ":" + PORT);

        byte[] buffer = new byte[client.ReceiveBufferSize];
        StringBuilder jsonBuilder = new StringBuilder();

        while (true)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }

                jsonBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                string jsonData = jsonBuilder.ToString();
                int newLineIndex;

                while ((newLineIndex = jsonData.IndexOf('\n')) != -1)
                {
                    string jsonObject = jsonData.Substring(0, newLineIndex);
                    jsonData = jsonData.Substring(newLineIndex + 1);

                    JObject data = JObject.Parse(jsonObject);

                    string name = data["name"].ToString();
                    float timestamp = (float)data["timestamp"];
                    float posX = (float)data["position"]["x"];
                    float posY = (float)data["position"]["y"];
                    float posZ = (float)data["position"]["z"];
                    float rotX = (float)data["rotation"]["x"];
                    float rotY = (float)data["rotation"]["y"];
                    float rotZ = (float)data["rotation"]["z"];
                    float rotW = (float)data["rotation"]["w"];

                    Console.WriteLine($"Received data - Name: {name}, Timestamp: {timestamp}");
                    Console.WriteLine($"Position: ({posX}, {posY}, {posZ})");
                    Console.WriteLine($"Rotation: ({rotX}, {rotY}, {rotZ}, {rotW})");
                }

                jsonBuilder.Clear();
                jsonBuilder.Append(jsonData);
            }
            catch (IOException)
            {
                break;
            }
        }

        Console.WriteLine("Connection closed by the server.");

        stream.Close();
        client.Close();
    }
}