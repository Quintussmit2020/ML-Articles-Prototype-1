using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MagicLeapInputs;
using UnityEngine.InputSystem;
using UnityEngine.XR.MagicLeap;
using System.Runtime.CompilerServices;

public class DimmingController : MonoBehaviour
{
   

   public GameObject screenDimmer = null;

    public void Start()
    {
        MLSegmentedDimmer.Activate();
        screenDimmer.SetActive(false);
    }


    public void ActivateSegmentedDimming(bool active)
    {
        screenDimmer.SetActive(active);
    }
    public void SegmentedDimming(float DimmingValue)
    {        
        screenDimmer.GetComponent<MeshRenderer>().material.SetFloat("_DimmingValue", DimmingValue);        
    }

    public void ToggleGlobalDimming(bool isEnabled)
    {
        MLGlobalDimmer.SetValue(isEnabled ? 1 : 0);
    }
}
