using System;
using System.Linq;
using UnityEngine;
using Websocket.Client;

public class PositionReceiver : MonoBehaviour
{
    private const int MAX_LOSS = 100;
    private const int AVERAGE_WINDOW_SIZE = 100;
    private const int VELO = 100;

    private WebsocketClient websocket;

    public GameObject boxObject;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isInitialPositionSet = false;

    private Queue<Vector3> positionQueue = new Queue<Vector3>();
    private Queue<Quaternion> rotationQueue = new Queue<Quaternion>();

    private void Start()
    {
        websocket = new WebsocketClient(new Uri("ws://localhost:8080/ws"));
        websocket.ReconnectTimeout = TimeSpan.FromSeconds(30);
        websocket.ReconnectionHappened.Subscribe(info =>
            Debug.Log($"Reconnection happened, type: {info.Type}"));

        websocket.MessageReceived.Subscribe(msg =>
        {
            if (msg.Text != null)
            {
                var message = msg.Text;
                var data = message.Trim().Split();

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
                    // ...
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
                    // ...
                }
                else
                {
                    // ...
                }
            }
        });

        websocket.StartOrFail();
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
        if (websocket != null)
        {
            websocket.Stop(WebSocketCloseStatus.NormalClosure, string.Empty);
            websocket.Dispose();
        }
    }
}