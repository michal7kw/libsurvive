import pysurvive
import sys
import json
print(pysurvive.__file__)

actx = pysurvive.SimpleContext(sys.argv)

for obj in actx.Objects():
    print(str(obj.Name(), 'utf-8'))

while actx.Running():
    updated = actx.NextUpdated()
    if updated:
        
        print("Lighthouses seeing", updated.Name())
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