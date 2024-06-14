using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;

public class VRCam_Mov : NetworkBehaviour
{
    public OVRCameraRig vrCameraRig;

    void Start()
    {
        // Find the VR camera component in the player preset
        vrCameraRig = GetComponentInChildren<OVRCameraRig>();

        if (vrCameraRig != null)
        {
            // If it is a local player, activate the VR camera component, otherwise disable it
            vrCameraRig.enabled = isLocalPlayer;
        }
    }
}
