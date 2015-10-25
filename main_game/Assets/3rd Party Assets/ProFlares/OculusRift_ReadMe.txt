
This version of ProFlares was tested with - OVR V0.5.0.1


To add Oculus Rift Support to ProFlares. 

1 - Import the latest Oculus Rift SDK to your project.

2 - Then Unzip the OculusRift.zip, This will add an extra option to your ProFlares Menu.

3 - To use, select your OVRCameraRig then use option. : 'Window->ProFlares->Create VR Setup On Selected OVR Camera'

    This will add a ProFlares VR setup, on the LeftEyeAnchor.

4 - Connect a ProFlareAtlas to the created ProFlareBatch.
    
    This is the basic setup complete, you can now add flares to your scene.


VR Flare Depth - 
This is a new setting on a ProFlareBatch that is setup in VR mode. It used to control the depth of that is added to a flare. Zero is completely flat, 0.2 is Default. Any higher than 0.5 may cause to much depth and cause eye strain.

NOTE - If modifying this value make sure that both the left and right ProFlareBatch have the same Value!

NOTE - If using OVRPlayerController take note that the collider attached to the OVRPlayerController can occlude flares, use the layer mask feature to mask the OVRPlayerController collider.
