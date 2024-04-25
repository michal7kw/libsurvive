from pysurvive import *
import sys

# Initialize the Survive context
ctx = init(sys.argv)

if ctx:
    try:
        # Start the Survive thread
        startup(ctx)

        while poll(ctx) == 0:
            # Get the number of active base stations
            active_lighthouses = ctx.contents.activeLighthouses
            print("Number of active base stations:", active_lighthouses)
            active_lighthouses = ctx.contents.light_pulse_max_call_time
            print("Max call time:", active_lighthouses)

            # You can add a small delay to control the polling rate
            # time.sleep(0.1)

    except KeyboardInterrupt:
        print("Exiting...")

    finally:
        # Clean up the Survive context
        close(ctx)
else:
    print("Failed to initialize Survive context")