// Data received by this program are send by `server_socket.py` script.

using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class PositionReceiver : MonoBehaviour
{
    private const string HOST = "localhost";
    private const int PORT = 12345;
    private const int AVERAGE_WINDOW_SIZE = 100;
    private const int VELO = 100;

    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer;
    private StringBuilder jsonBuilder;

    public GameObject boxObject;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isInitialPositionSet = false;

    private Queue<Vector3> positionQueue = new Queue<Vector3>();
    private Queue<Quaternion> rotationQueue = new Queue<Quaternion>();

    private void Start()
    {
        client = new TcpClient();
        client.Connect(HOST, PORT);

        if (client.Connected)
        {
            stream = client.GetStream();
            Debug.Log("Connected to server: " + HOST + ":" + PORT);

            buffer = new byte[client.ReceiveBufferSize];
            jsonBuilder = new StringBuilder();
        }
        else
        {
            Debug.LogError("Failed to connect to server: " + HOST + ":" + PORT);
        }
    }

    private void Update()
    {
        if (stream != null && stream.DataAvailable)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                // Connection closed by the server
                return;
            }

            jsonBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

            string jsonData = jsonBuilder.ToString();
            int newLineIndex;

            while ((newLineIndex = jsonData.IndexOf('\n')) != -1)
            {
                string jsonObject = jsonData.Substring(0, newLineIndex);
                jsonData = jsonData.Substring(newLineIndex + 1);

                // Parse the JSON data
                JObject data = JObject.Parse(jsonObject);

                // Access the position data
                float posX = (float)data["position"]["x"];
                float posY = (float)data["position"]["y"];
                float posZ = (float)data["position"]["z"];
                float rotX = (float)data["rotation"]["x"];
                float rotY = (float)data["rotation"]["y"];
                float rotZ = (float)data["rotation"]["z"];
                float rotW = (float)data["rotation"]["w"];

                Vector3 receivedPosition = new Vector3(posX, posY, posZ);
                Quaternion receivedRotation = new Quaternion(rotX, rotY, rotZ, rotW);

                positionQueue.Enqueue(receivedPosition);
                rotationQueue.Enqueue(receivedRotation);

                if (positionQueue.Count > AVERAGE_WINDOW_SIZE)
                {
                    positionQueue.Dequeue();
                    rotationQueue.Dequeue();
                }

                if (!isInitialPositionSet && positionQueue.Count == AVERAGE_WINDOW_SIZE)
                {
                    initialPosition = CalculateAveragePosition();
                    Debug.Log("initialPosition: " + initialPosition);
                    initialRotation = CalculateAverageRotation();
                    isInitialPositionSet = true;
                }

                if (isInitialPositionSet)
                {
                    Vector3 averagePosition = CalculateAveragePosition();
                    Debug.Log("averagePosition: " + averagePosition);
                    Quaternion averageRotation = CalculateAverageRotation();

                    Vector3 relativePosition = averagePosition - initialPosition;

                    // Update the box object's position and rotation
                    boxObject.transform.position = relativePosition * VELO;
                    boxObject.transform.rotation = averageRotation;
                }
            }

            jsonBuilder.Clear();
            jsonBuilder.Append(jsonData);
        }
    }

    private Vector3 CalculateAveragePosition()
    {
        Vector3 sum = Vector3.zero;

        foreach (Vector3 position in positionQueue)
        {
            sum += position;
        }

        return sum / positionQueue.Count;
    }

    private Quaternion CalculateAverageRotation()
    {
        Quaternion average = rotationQueue.Peek();
        float weight = 1f / rotationQueue.Count;

        foreach (Quaternion rotation in rotationQueue)
        {
            average = Quaternion.Slerp(average, rotation, weight);
        }

        return average;
    }

    private void OnDestroy()
    {
        // Close the connection
        if (stream != null)
        {
            stream.Close();
        }

        if (client != null)
        {
            client.Close();
        }
    }
}