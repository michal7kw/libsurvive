import pysurvive
from pysurvive import *

import sys
import socket
import json
import time

HOST = 'localhost'
PORT = 12345

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
server_socket.bind((HOST, PORT))
server_socket.listen(1)

print(f"Server listening on {HOST}:{PORT}")

# Initialize the Survive context
actx = pysurvive.SimpleContext(sys.argv)

# Initialize the Survive context
ctx = init(sys.argv)

if actx.ptr:
    try:
        for obj in actx.Objects():
            print(str(obj.Name(), 'utf-8'))

        client_socket, address = server_socket.accept()
        print(f"Connected to client: {address}")

        while actx.Running():
            updated = actx.NextUpdated()
            if updated:
                poseObj = updated.Pose()
                poseData = poseObj[0]
                poseTimestamp = poseObj[1]

                data = {
                    'name': str(updated.Name(), 'utf-8'),
                    'timestamp': poseTimestamp,
                    'position': {
                        'x': poseData.Pos[0],
                        'y': poseData.Pos[1],
                        'z': poseData.Pos[2]
                    },
                    'rotation': {
                        'x': poseData.Rot[0],
                        'y': poseData.Rot[1],
                        'z': poseData.Rot[2],
                        'w': poseData.Rot[3]
                    }
                }

                json_data = json.dumps(data)

                print("Sending JSON data:", json_data)

                client_socket.send((json_data + '\n').encode('utf-8'))

            # Get the number of active base stations
            active_lighthouses = pysurvive.simple_get_object_count(actx.ptr)
            print("Number of active base stations:", active_lighthouses)

            # Initialize the Survive context
            # ctx = init(sys.argv)

            # Get the number of active base stations
            # active_lighthouses = ctx.contents.activeLighthouses
            # print("Number of active base stations:", active_lighthouses)

            # Iterate over the active base stations
            for i in range(active_lighthouses):
                base_station = ctx.contents.bsd[i]
                confidence = base_station.confidence
                print(f"Base Station {i+1} - Tracking Confidence: {confidence}")

            # Add a small delay to control the polling rate
            time.sleep(0.1)

    except KeyboardInterrupt:
        print("Exiting...")

    finally:
        # Clean up the Survive context
        client_socket.close()
        pysurvive.simple_close(actx.ptr)
else:
    print("Failed to initialize Survive context")