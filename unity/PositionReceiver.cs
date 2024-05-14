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
    private bool base0Loss = false;
    private bool base1Loss = false;
    private int base0LossBuffer = MAX_LOSS;
    private int base1LossBuffer = MAX_LOSS;

    public GameObject boxObject;
    private Renderer boxRenderer;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isInitialPositionSet = false;

    private Queue<Vector3> positionQueue = new Queue<Vector3>();
    private Queue<Quaternion> rotationQueue = new Queue<Quaternion>();

    private EllipsoidRenderer ellipsoidRenderer;

    private const float ROTATION_SMOOTH_TIME = 0.1f;
    private Quaternion targetRotation;
    private Quaternion currentRotation;

    private async void Start()
    {
        // ####### Local connection #######
        // websocket = new WebSocket("ws://localhost:8080/ws");
        
        // ####### Remote connection #######
        // ip Huawei portable wifi: 192.168.8.108
        // websocket = new WebSocket("ws://192.168.8.108:8080/ws");
        websocket = new WebSocket("ws://127.0.0.1:8080/ws");
        // ip Enel wifi: 192.168.1.55
        // websocket = new WebSocket("ws://192.168.1.55:8080/ws");
        
        boxRenderer = boxObject.GetComponent<Renderer>();
        ellipsoidRenderer = boxObject.GetComponent<EllipsoidRenderer>();

        currentRotation = Quaternion.identity;
        targetRotation = Quaternion.identity;

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

            Debug.Log($"Base 0 Loss: {base0LossBuffer}");
            Debug.Log($"Base 0 Loss: {base0LossBuffer}");
            if (base0Loss)
            {
                Debug.Log($"!!!! Base 0 Lost: {base0LossBuffer} !!!!");
            }

            if (base1Loss)
            {
                Debug.Log($"!!!! Base 1 Lost {base1LossBuffer} !!!!");
            }

           if (data[2] == "POSE")
        {
            string trackerId = data[1];
            Vector3 position = new Vector3(float.Parse(data[3]), float.Parse(data[4]), float.Parse(data[5]));
            Quaternion rotation = new Quaternion(float.Parse(data[6]), float.Parse(data[7]), float.Parse(data[8]), float.Parse(data[9]));

            if (!isInitialPositionSet)
            {
                initialPosition = position;
                initialRotation = rotation;
                isInitialPositionSet = true;
                currentRotation = rotation;
                targetRotation = rotation;
            }
            else
            {
                targetRotation = rotation;
            }

            Vector3 relativePosition = position - initialPosition;
            boxObject.transform.position = relativePosition * VELO;
        }
            else if (data[2] == "VELOCITY")
            {
                // ...
            }
            else if (data[2] == "FULL_STATE")
            {
                // ...
            }
            else if (data[2] == "FULL_COVARIANCE")
            {
                string name = data[1];
                int rows, cols;

                if (int.TryParse(data[4], out rows) && int.TryParse(data[5], out cols))
                {
                    float[] values = new float[rows * cols];

                    for (int i = 6; i < data.Length; i++)
                    {
                        float value;
                        if (float.TryParse(data[i], out value))
                        {
                            values[i - 6] = value;
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to parse float value at index {i}: {data[i]}");
                        }
                    }

                   UpdateEllipsoid(name, values);
                }
                else
                {
                    Debug.LogWarning($"Failed to parse rows or cols: {data[4]}, {data[5]}");
                }
            }
            else if (data[2] == "LH_UP")
            {
                Debug.Log(string.Join(" ", data));
            }
            else if (data[2] == "DISCONNECT")
            {
                Debug.Log(string.Join(" ", data));
            }
            else if (data[1] == "WM0")
            {
                if (data[2] == "W")
                {
                    if (data[3] == "0")
                    {
                        if (base0Loss)
                        {
                            base0LossBuffer = MAX_LOSS;
                            base0Loss = false;
                            if (!base1Loss)
                            {
                                base1LossBuffer--;
                            }
                        }
                        else
                        {
                            if (base0LossBuffer < MAX_LOSS){
                                base0LossBuffer++;
                            }
                            if (!base1Loss)
                            {
                                base1LossBuffer--;
                            }
                        }
                    }
                    else if (data[3] == "1")
                    {
                        if (base1Loss)
                        {
                            base1LossBuffer = MAX_LOSS;
                            base1Loss = false;
                            if (!base0Loss)
                            {
                                base0LossBuffer--;
                            }
                        }
                        else
                        {
                            if (base1LossBuffer < MAX_LOSS){
                                base1LossBuffer++;
                            }
                            if (!base0Loss)
                            {
                                base0LossBuffer--;
                            }
                        }
                    }

                    if (base0LossBuffer < 0)
                    {
                        base0Loss = true;
                    }
                    if (base1LossBuffer < 0)
                    {
                        base1Loss = true;
                    }
                }
            }
            else
            {
                // ...
            }

            // Change box color based on base station loss
            if (base0Loss || base1Loss)
            {
                boxRenderer.material.color = Color.red;
            }
            else
            {
                boxRenderer.material.color = Color.white;
            }
        };

        // Keep sending messages at every 0.3s
        InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

        // waiting for messages
        await websocket.Connect();
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
 // Smoothly interpolate rotation using SLERP
        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, ROTATION_SMOOTH_TIME);
        boxObject.transform.rotation = currentRotation;
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
     private void UpdateEllipsoid(string name, float[] values)
{
    if (name.StartsWith(boxObject.name))
    {
        Matrix4x4 u = Matrix4x4.identity;
        Vector3 s = Vector3.zero;
        Matrix4x4 v = Matrix4x4.identity;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                u[i, j] = values[i * 3 + j];
                v[i, j] = values[i * 3 + j + 9];
            }
            s[i] = values[i + 6];
        }

        ellipsoidRenderer.UpdateEllipsoid(u, s, v);
    }
}
}