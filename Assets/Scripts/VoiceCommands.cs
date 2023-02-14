using System;
using System.Collections;
using System.Collections.Generic;
using MagicLeap.Core;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.MagicLeap;

public class VoiceCommands : MonoBehaviour
{
    private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();
    public Transform mediaPlayerRoot;
    public MLMediaPlayerBehavior mlMediaPlayer;


    public PlayerPlacement playerPlacement;

    DimmingController dimmingController;
    public GameObject screenDimmer;

    public MLVoiceIntentsConfiguration VoiceIntentsConfiguration;

    private void Awake()
    {
        permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
    }

    private void OnDestroy()
    {
        permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;
    }

    private void Start()
    {         
        MLPermissions.RequestPermission(MLPermission.VoiceInput, permissionCallbacks);
        dimmingController = new DimmingController();
        dimmingController.screenDimmer = screenDimmer;
    }


    private void Update()
    {

    }



    private void OnPermissionGranted(string permission)
    {

        if (permission == MLPermission.VoiceInput)
            InitializeVoiceInput();
    }

    private void InitializeVoiceInput()
    {
        bool isVoiceEnabled = MLVoice.VoiceEnabled;
        if (isVoiceEnabled)
        {
            var result = MLVoice.SetupVoiceIntents(VoiceIntentsConfiguration);
            if (result.IsOk)
            {
                MLVoice.OnVoiceEvent += MLVoiceOnOnVoiceEvent;
            }
            else
            {
                Debug.LogError("Voice could not initialize:" + result);
            }
        }
        else
        {
            UnityEngine.XR.MagicLeap.SettingsIntentsLauncher.LaunchSystemVoiceInputSettings();
            Application.Quit();
        }
    }


    private void MLVoiceOnOnVoiceEvent(in bool wassuccessful, in MLVoice.IntentEvent voiceevent)
    {
        if (wassuccessful)
        {
            if (voiceevent.EventID == 101)
            {
                Debug.Log("Show Global Dimmer");
                dimmingController.ToggleGlobalDimming(true);
            }
            if (voiceevent.EventID == 102)
            {
                Debug.Log("Hide Global Dimmer");
                dimmingController.ToggleGlobalDimming(false);
            }
            if (voiceevent.EventID == 103)
            {
                Debug.Log("Show Segmented Dimmer");
                dimmingController.ActivateSegmentedDimming(true);
            }
            if (voiceevent.EventID == 104)
            {
                Debug.Log("Hide Segmented Dimmer");
                dimmingController.ActivateSegmentedDimming(false);
            }
            if (voiceevent.EventID == 105)
            {
                Debug.Log("Playing video");
                mlMediaPlayer.Play(); ;
            }
            if (voiceevent.EventID == 106)
            {
                Debug.Log("Pausing video");
                mlMediaPlayer.Pause(); ;
            }
            if (voiceevent.EventID == 107)
            {
                Debug.Log("Close media player");
                playerPlacement.ExitMediaPlayer();
            }
        }
    }

    private void ToggleGlobalDimming(bool isEnabled)
    {
        MLGlobalDimmer.SetValue(isEnabled ? 1 : 0);
    }

    private void OnPermissionDenied(string permission)
    {
        Debug.LogError($"Failed to create Planes Subsystem due to missing or denied {MLPermission.SpatialMapping} permission. Please add to manifest. Disabling script.");
        enabled = false;
    }

}
