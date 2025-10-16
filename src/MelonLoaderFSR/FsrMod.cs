using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MelonLoaderFSR;

public sealed class FsrMod : MelonMod
{
    private MelonPreferences_Category? _preferences;
    private MelonPreferences_Entry<bool>? _enabledEntry;
    private MelonPreferences_Entry<float>? _scaleEntry;
    private MelonPreferences_Entry<bool>? _persistEntry;

    private float _lastAppliedScale = float.NaN;
    private bool _lastEnabledState;
    private float _lastDynamicCheck;

    private const float MinScale = 0.5f;
    private const float MaxScale = 1.0f;

    public override void OnInitializeMelon()
    {
        _preferences = MelonPreferences.CreateCategory("FSRLikeUpscaler", "FSR-like Upscaler");
        _enabledEntry = _preferences.CreateEntry("Enabled", true, "Master Switch",
            "Turn the internal resolution downscaling on or off without removing the mod.");
        _scaleEntry = _preferences.CreateEntry("RenderScale", 0.7f, "Internal Render Scale",
            "Multiplier applied to the 3D render targets. 1 = native, lower values downscale for better performance.");
        _persistEntry = _preferences.CreateEntry("RefreshEveryFrame", false, "Force Refresh",
            "When enabled the mod reapplies the scale every frame to fight games that reset dynamic resolution.");

        _preferences.SaveToFile(false);

        SceneManager.sceneLoaded += HandleSceneLoaded;
        MelonLogger.Msg("FSR-like upscaler initialized. Adjust settings with the Melon Preferences manager.");
    }

    public override void OnDeinitializeMelon()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        ResetScale();
    }

    public override void OnLateUpdate()
    {
        if (_enabledEntry == null || _scaleEntry == null || _persistEntry == null)
            return;

        if (_enabledEntry.Value)
        {
            if (_persistEntry.Value)
            {
                EnsureDynamicResolutionAllowed(true);
            }

            ApplyScaleIfNeeded();
        }
        else if (_lastEnabledState)
        {
            ResetScale();
        }

        _lastEnabledState = _enabledEntry.Value;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MelonLogger.Msg($"Scene '{scene.name}' loaded. Reapplying internal render scale.");
        _lastAppliedScale = float.NaN;
        EnsureDynamicResolutionAllowed(false);
        ApplyScaleIfNeeded();
    }

    private void ApplyScaleIfNeeded()
    {
        if (_scaleEntry == null)
            return;

        var desiredScale = Mathf.Clamp(_scaleEntry.Value, MinScale, MaxScale);
        if (float.IsNaN(_lastAppliedScale) || !Mathf.Approximately(desiredScale, _lastAppliedScale))
        {
            ApplyScale(desiredScale);
        }
    }

    private void ApplyScale(float scale)
    {
        MelonLogger.Msg($"Applying internal render scale {scale:P0}.");
        try
        {
            QualitySettings.resolutionScalingFixedDPIFactor = scale;
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"Failed to set QualitySettings.resolutionScalingFixedDPIFactor: {ex.Message}");
        }

        try
        {
            ScalableBufferManager.ResizeBuffers(scale, scale);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"Failed to resize scalable buffers: {ex.Message}");
        }

        EnsureDynamicResolutionAllowed(false);
        _lastAppliedScale = scale;
    }

    private void ResetScale()
    {
        MelonLogger.Msg("Resetting internal render scale to native.");
        try
        {
            QualitySettings.resolutionScalingFixedDPIFactor = 1f;
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"Failed to reset QualitySettings.resolutionScalingFixedDPIFactor: {ex.Message}");
        }

        try
        {
            ScalableBufferManager.ResizeBuffers(1f, 1f);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"Failed to reset scalable buffers: {ex.Message}");
        }

        _lastAppliedScale = 1f;
    }

    private void EnsureDynamicResolutionAllowed(bool everyFrame)
    {
        if (!everyFrame)
        {
            if (Time.unscaledTime - _lastDynamicCheck < 1f)
                return;
        }

        _lastDynamicCheck = Time.unscaledTime;

        foreach (var camera in Camera.allCameras)
        {
            if (camera == null)
                continue;

            try
            {
                if (!camera.allowDynamicResolution)
                {
                    camera.allowDynamicResolution = true;
                    MelonLogger.Msg($"Enabled dynamic resolution on camera '{camera.name}'.");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"Failed to enable dynamic resolution on camera '{camera?.name}': {ex.Message}");
            }
        }
    }
}
