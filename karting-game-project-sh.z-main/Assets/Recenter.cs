using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Unity.XR;
using Unity.XR.CoreUtils;
public class Recenter : MonoBehaviour
{
    [SerializeField] private GameObject recenterLocation;

    // Start is called before the first frame update
    void Awake()
    {
        Transform target = recenterLocation.transform;
        XROrigin xROrigin = GetComponent<XROrigin>();
        xROrigin.MoveCameraToWorldLocation(target.localToWorldMatrix.GetPosition());
        xROrigin.MatchOriginUpCameraForward(target.up,target.forward);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
