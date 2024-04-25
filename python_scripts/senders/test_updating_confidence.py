from pysurvive import *
import sys

# Initialize the Survive context
ctx = init(sys.argv)

if ctx:
    # Get the number of active base stations
    active_lighthouses = ctx.contents.activeLighthouses
    print("Number of active base stations:", active_lighthouses)

    # Iterate over the active base stations
    for i in range(active_lighthouses):
        base_station = ctx.contents.bsd[i]
        confidence = base_station.confidence
        print(f"Base Station {i+1} - Tracking Confidence: {confidence}")

    # Clean up the Survive context
    close(ctx)
else:
    print("Failed to initialize Survive context")