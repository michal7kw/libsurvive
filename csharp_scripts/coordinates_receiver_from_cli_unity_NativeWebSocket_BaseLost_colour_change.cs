using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using UnityEngine.Rendering;

public class PositionReceiver : MonoBehaviour
{
    private const int MAX_LOSS = 200;
    private const int AVERAGE_WINDOW_SIZE = 100;
    private const int VELO = 20;

    private WebSocket websocket;
    private bool[] baseLoss = new bool[4];
    private int[] baseLossBuffer = new int[4];

    public GameObject boxObject;
    private Renderer boxRenderer;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isInitialPositionSet = false;

    private Queue<Vector3> positionQueue = new Queue<Vector3>();
    private Queue<Quaternion> rotationQueue = new Queue<Quaternion>();

    private async void Start()
    {
        websocket = new WebSocket("ws://192.168.8.108:8080/ws");
        
        boxRenderer = boxObject.GetComponent<Renderer>();

        for (int i = 0; i < 4; i++)
        {
            baseLoss[i] = false;
            baseLossBuffer[i] = MAX_LOSS;
        }

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            var data = message.Trim().Split();

            for (int i = 0; i < 4; i++)
            {
                Debug.Log($"Base {i} Loss: {baseLossBuffer[i]}");
                if (baseLoss[i])
                {
                    Debug.Log($"!!!! Base {i} Lost: {baseLossBuffer[i]} !!!!");
                }
            }

            if (data[2] == "POSE")
            {
                string trackerId = data[1];
                Vector3 position = new Vector3(float.Parse(data[3]), float.Parse(data[4]), float.Parse(data[5]));
                Quaternion rotation = new Quaternion(float.Parse(data[6]), float.Parse(data[7]), float.Parse(data[8]), float.Parse(data[9]));

                positionQueue.Enqueue(position);
                rotationQueue.Enqueue(rotation);

                if (positionQueue.Count > AVERAGE_WINDOW_SIZE)
                {
                    positionQueue.Dequeue();
                    rotationQueue.Dequeue();
                }

                if (!isInitialPositionSet && positionQueue.Count == AVERAGE_WINDOW_SIZE)
                {
                    initialPosition = CalculateAveragePosition();
                    initialRotation = CalculateAverageRotation();
                    isInitialPositionSet = true;
                }

                if (isInitialPositionSet)
                {
                    Vector3 averagePosition = CalculateAveragePosition();
                    Quaternion averageRotation = CalculateAverageRotation();

                    Vector3 relativePosition = averagePosition - initialPosition;

                    boxObject.transform.position = relativePosition * VELO;
                    boxObject.transform.rotation = averageRotation;
                }
            }
            else if (data[1] == "WM0")
            {
                if (data[2] == "W")
                {
                    int baseIndex = int.Parse(data[3]);
                    if (baseIndex >= 0 && baseIndex < 4)
                    {
                        UpdateBaseLossStatus(baseIndex);
                    }
                }
            }

            UpdateBoxColor();
        };

        InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

        await websocket.Connect();
    }

    private void UpdateBaseLossStatus(int currentBase)
    {
        if (baseLoss[currentBase])
        {
            baseLossBuffer[currentBase] = MAX_LOSS;
            baseLoss[currentBase] = false;
        }
        else
        {
            if (baseLossBuffer[currentBase] < MAX_LOSS)
            {
                baseLossBuffer[currentBase]++;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (i != currentBase)
            {
                baseLossBuffer[i]--;
                if (baseLossBuffer[i] < 0)
                {
                    baseLoss[i] = true;
                }
            }
        }
    }

    private void UpdateBoxColor()
    {
        if (baseLoss.Any(loss => loss))
        {
            boxRenderer.material.color = Color.red;
        }
        else
        {
            boxRenderer.material.color = Color.white;
        }
    }

    private void SendWebSocketMessage()
    {
        if (websocket.State == WebSocketState.Open)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Hello");
            websocket.Send(buffer);
        }
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
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
}