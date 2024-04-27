using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Websocket.Client;

class Program
{
    static async Task ProcessData(WebsocketClient websocket)
    {
        int maxLoss = 100;
        bool base0Loss = false;
        bool base1Loss = false;
        int base0LossBuffer = maxLoss;
        int base1LossBuffer = maxLoss;

        websocket.MessageReceived.Subscribe(msg =>
        {
            if (msg.Text != null)
            {
                var message = msg.Text;
                var data = message.Trim().Split();

                if (base0Loss)
                {
                    Console.WriteLine($"Base 0 Loss: {base0LossBuffer}");
                }

                if (base1Loss)
                {
                    Console.WriteLine($"Base 1 Loss {base1LossBuffer}");
                }

                if (data[2] == "POSE")
                {
                    string trackerId = data[1];
                    float[] position = { float.Parse(data[3]), float.Parse(data[4]), float.Parse(data[5]) };
                    float[] quaternion = { float.Parse(data[6]), float.Parse(data[7]), float.Parse(data[8]), float.Parse(data[9]) };
                    // Process position and quaternion data
                    Console.WriteLine($"Pose Data - Tracker: {trackerId}");
                    Console.WriteLine($"Position: [{string.Join(", ", position)}]");
                    Console.WriteLine($"Quaternion: [{string.Join(", ", quaternion)}]");
                    Console.WriteLine();
                }
                else if (data[2] == "VELOCITY")
                {
                    // string trackerId = data[1];
                    // float[] linearVelocity = { float.Parse(data[3]), float.Parse(data[4]), float.Parse(data[5]) };
                    // float[] angularVelocity = { float.Parse(data[6]), float.Parse(data[7]), float.Parse(data[8]) };
                    // // Process velocity data
                    // Console.WriteLine($"Velocity Data - Tracker: {trackerId}");
                    // Console.WriteLine($"Linear Velocity: [{string.Join(", ", linearVelocity)}]");
                    // Console.WriteLine($"Angular Velocity: [{string.Join(", ", angularVelocity)}]");
                    // Console.WriteLine();
                }
                else if (data[2] == "FULL_STATE")
                {
                    // string trackerId = data[1];
                    // float[] position = { float.Parse(data[3]), float.Parse(data[4]), float.Parse(data[5]) };
                    // float[] quaternion = { float.Parse(data[6]), float.Parse(data[7]), float.Parse(data[8]), float.Parse(data[9]) };
                    // float[] linearVelocity = { float.Parse(data[10]), float.Parse(data[11]), float.Parse(data[12]) };
                    // float[] angularVelocity = { float.Parse(data[13]), float.Parse(data[14]), float.Parse(data[15]) };
                    // float[] acceleration = { float.Parse(data[16]), float.Parse(data[17]), float.Parse(data[18]) };
                    // float scaleFactor = float.Parse(data[19]);
                    // float[] imuCorrection = { float.Parse(data[20]), float.Parse(data[21]), float.Parse(data[22]), float.Parse(data[23]) };
                    // float[] accelerationBias = { float.Parse(data[24]), float.Parse(data[25]), float.Parse(data[26]) };
                    // float[] gyroscopeBias = { float.Parse(data[27]), float.Parse(data[28]), float.Parse(data[29]) };
                    // // Process full state data
                    // Console.WriteLine($"Full State Data - Tracker: {trackerId}");
                    // Console.WriteLine($"Position: [{string.Join(", ", position)}]");
                    // Console.WriteLine($"Quaternion: [{string.Join(", ", quaternion)}]");
                    // Console.WriteLine($"Linear Velocity: [{string.Join(", ", linearVelocity)}]");
                    // Console.WriteLine($"Angular Velocity: [{string.Join(", ", angularVelocity)}]");
                    // Console.WriteLine($"Acceleration: [{string.Join(", ", acceleration)}]");
                    // Console.WriteLine($"Scale Factor: {scaleFactor}");
                    // Console.WriteLine($"IMU Correction: [{string.Join(", ", imuCorrection)}]");
                    // Console.WriteLine($"Acceleration Bias: [{string.Join(", ", accelerationBias)}]");
                    // Console.WriteLine($"Gyroscope Bias: [{string.Join(", ", gyroscopeBias)}]");
                    // Console.WriteLine();
                }
                else if (data[2] == "FULL_COVARIANCE")
                {
                    string trackerId = data[1];
                    float[] covarianceMatrix = data.Skip(3).Select(float.Parse).ToArray();
                    Console.WriteLine($"Full Covariance Data - Tracker: {trackerId}");
                    Console.WriteLine($"Covariance Matrix: [{string.Join(", ", covarianceMatrix)}]");
                    Console.WriteLine();
                }
                else if (data[2] == "LH_UP")
                {
                    Console.WriteLine(string.Join(" ", data));
                }
                else if (data[2] == "DISCONNECT")
                {
                    Console.WriteLine(string.Join(" ", data));
                }
                else if (data[1] == "WM0")
                {
                    if (data[2] == "BUTTON")
                    {
                        Console.WriteLine("Button pressed");
                        Console.WriteLine();
                    }
                    else if (data[2] == "W")
                    {
                        // Console.WriteLine(string.Join(" ", data));
                        if (data[3] == "0")
                        {
                            if (base0Loss)
                            {
                                base0Loss = false;
                                base0LossBuffer = maxLoss;
                            }
                            else if (base0LossBuffer < maxLoss)
                            {
                                base0LossBuffer++;
                            }
                            else
                            {
                                // do nothing
                            }

                            base1LossBuffer--;
                        }
                        else // data[3] == "1"
                        {
                            if (base1Loss)
                            {
                                base1Loss = false;
                                base1LossBuffer = maxLoss;
                            }
                            else if (base1LossBuffer < maxLoss)
                            {
                                base1LossBuffer++;
                            }
                            else
                            {
                                // do nothing
                            }

                            base0LossBuffer--;
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
                    else
                    {
                        // Console.WriteLine(string.Join(" ", data));
                        // Console.WriteLine();
                    }
                }
                else
                {
                    // Handle informational logs and other messages
                    // Console.WriteLine($"Informational Message: {string.Join(" ", data)}");
                }
            }
        });

        while (websocket.IsRunning)
        {
            await Task.Delay(1000);
        }
    }

    static async Task Main()
    {
        using var websocket = new WebsocketClient(new Uri("ws://localhost:8080/ws"));
        websocket.ReconnectTimeout = TimeSpan.FromSeconds(30);
        websocket.ReconnectionHappened.Subscribe(info =>
            Console.WriteLine($"Reconnection happened, type: {info.Type}"));

        await websocket.StartOrFail();

        await ProcessData(websocket);
    }
}