using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Recenter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var inputSubsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances(inputSubsystems);
        Debug.Log($"input system: {inputSubsystems.Count}");
        foreach (var subsystem in inputSubsystems)
        {
            subsystem.TryRecenter();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
