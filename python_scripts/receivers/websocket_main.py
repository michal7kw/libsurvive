import asyncio
import websockets
import json

async def process_data(websocket):

    max_loss = 100
    base0_loss = False
    base1_loss = False
    base0_loss_buffer = max_loss
    base1_loss_buffer = max_loss

    async for message in websocket:
        data = message.strip().split()
        
        if base0_loss:
            print(f"Base 0 Loss: {base0_loss_buffer}")

        if base1_loss:
            print(f"Base 1 Loss {base1_loss_buffer}")

        if data[2] == 'POSE':
            tracker_id = data[1]
            position = [float(data[3]), float(data[4]), float(data[5])]
            quaternion = [float(data[6]), float(data[7]), float(data[8]), float(data[9])]
            # Process position and quaternion data
            print(f"Pose Data - Tracker: {tracker_id}")
            print(f"Position: {position}")
            print(f"Quaternion: {quaternion}")
            print()
        
        elif data[2] == 'VELOCITY':
            pass
            # tracker_id = data[1]
            # linear_velocity = [float(data[3]), float(data[4]), float(data[5])]
            # angular_velocity = [float(data[6]), float(data[7]), float(data[8])]
            # # Process velocity data
            # print(f"Velocity Data - Tracker: {tracker_id}")
            # print(f"Linear Velocity: {linear_velocity}")
            # print(f"Angular Velocity: {angular_velocity}")
            # print()
        
        elif data[2] == 'FULL_STATE':
            pass
            # tracker_id = data[1]
            # position = [float(data[3]), float(data[4]), float(data[5])]
            # quaternion = [float(data[6]), float(data[7]), float(data[8]), float(data[9])]
            # linear_velocity = [float(data[10]), float(data[11]), float(data[12])]
            # angular_velocity = [float(data[13]), float(data[14]), float(data[15])]
            # acceleration = [float(data[16]), float(data[17]), float(data[18])]
            # scale_factor = float(data[19])
            # imu_correction = [float(data[20]), float(data[21]), float(data[22]), float(data[23])]
            # acceleration_bias = [float(data[24]), float(data[25]), float(data[26])]
            # gyroscope_bias = [float(data[27]), float(data[28]), float(data[29])]
            # # Process full state data
            # print(f"Full State Data - Tracker: {tracker_id}")
            # print(f"Position: {position}")
            # print(f"Quaternion: {quaternion}")
            # print(f"Linear Velocity: {linear_velocity}")
            # print(f"Angular Velocity: {angular_velocity}")
            # print(f"Acceleration: {acceleration}")
            # print(f"Scale Factor: {scale_factor}")
            # print(f"IMU Correction: {imu_correction}")
            # print(f"Acceleration Bias: {acceleration_bias}")
            # print(f"Gyroscope Bias: {gyroscope_bias}")
            # print()
        
        elif data[2] == "FULL_COVARIANCE":
            tracker_id = data[1]
            covariance_matrix = [float(x) for x in data[3:]]
            print(f"Full Covariance Data - Tracker: {tracker_id}")
            print(f"Covariance Matrix: {covariance_matrix}")
            print()
        
        elif data[2] == "LH_UP":
            print(data)
        
        elif data[2] == "DISCONNECT":
            print(data)

        elif data[1] in ["WM0"]:
            if data[2] == 'BUTTON':
                print("Button pressed")
                print()
            elif data[2] == 'W':
                # print(data)
                if data[3] == '0':
                    if base0_loss:
                        base0_loss = False
                        base0_loss_buffer = max_loss
                    elif base0_loss_buffer < max_loss:
                        base0_loss_buffer += 1
                    else:
                        pass

                    base1_loss_buffer -= 1
                else: #data[3] == '1':
                    if base1_loss:
                        base1_loss = False
                        base1_loss_buffer = max_loss
                    elif base1_loss_buffer < max_loss:
                        base1_loss_buffer += 1
                    else:
                        pass
                    base0_loss_buffer -= 1
                
                if base0_loss_buffer < 0:
                    base0_loss = True
                if base1_loss_buffer < 0:
                    base1_loss = True
            else:
                pass
                # print(data)
                # print()
        else:
            pass
            # Handle informational logs and other messages
            # print(f"Informational Message: {' '.join(data)}")

async def main():
    async with websockets.connect('ws://localhost:8080/ws') as websocket:
        await process_data(websocket)

asyncio.run(main())