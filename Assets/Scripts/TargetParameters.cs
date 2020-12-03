using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//0: look at the same rotation of the target
//1: look at the target
public enum DynamicRotationBehaviour { LookAs, LookAt }

public static class TargetParameters
{
    private static StaticTargetPositionParameters defaultStaticPositionParameters;
    private static StaticTargetRotationParameters defaultStaticRotationParameters;
    private static DynamicTargetPositionParameters defaultDynamicPositionParameters;
    private static DynamicTargetRotationParameters defaultDynamicRotationParameters;
    private static SteadyCamParameters defaultSteadyCamParameters;

    public static void SetDefaultStaticPositionParameters(CameraExecutionMode cameraExecutionMode, float lerpSpeed, float lerpThreshold, float duration, SteadyCamBehaviour steadyCamOnStart, SteadyCamBehaviour steadyCamOnEnd)
    {
        defaultStaticPositionParameters = new StaticTargetPositionParameters();
        defaultStaticPositionParameters.CameraExecutionMode = cameraExecutionMode;
        defaultStaticPositionParameters.LerpSpeed = lerpSpeed;
        defaultStaticPositionParameters.LerpThreshold = lerpThreshold;
        defaultStaticPositionParameters.Duration = duration;
        defaultStaticPositionParameters.SteadyCamOnStart = steadyCamOnStart;
        defaultStaticPositionParameters.SteadyCamOnEnd = steadyCamOnEnd;
        defaultStaticPositionParameters.StartSCParameters = GetDefaultSteadyCamParameters();
        defaultStaticPositionParameters.EndSCParameters = GetDefaultSteadyCamParameters();
    }

    public static void SetDefaultStaticRotationParameters(CameraExecutionMode cameraExecutionMode, float lerpSpeed, float lerpThreshold, float duration, SteadyCamBehaviour steadyCamOnStart, SteadyCamBehaviour steadyCamOnEnd)
    {
        defaultStaticRotationParameters = new StaticTargetRotationParameters();
        defaultStaticRotationParameters.CameraExecutionMode = cameraExecutionMode;
        defaultStaticRotationParameters.LerpSpeed = lerpSpeed;
        defaultStaticRotationParameters.LerpThreshold = lerpThreshold;
        defaultStaticRotationParameters.Duration = duration;
        defaultStaticRotationParameters.SteadyCamOnStart = steadyCamOnStart;
        defaultStaticRotationParameters.SteadyCamOnEnd = steadyCamOnEnd;
        defaultStaticRotationParameters.StartSCParameters = GetDefaultSteadyCamParameters();
        defaultStaticRotationParameters.EndSCParameters = GetDefaultSteadyCamParameters();
    }

    public static StaticTargetPositionParameters GetDefaultStaticPositionParameters()
    {
        return defaultStaticPositionParameters;
    }

    public static StaticTargetRotationParameters GetDefaultStaticRotationParameters()
    {
        return defaultStaticRotationParameters;
    }

    public static void SetDefaultDynamicPositionParameters(CameraExecutionMode cameraExecutionMode, float lerpSpeed, float lerpThreshold, Vector3 additiveOffset)
    {
        defaultDynamicPositionParameters = new DynamicTargetPositionParameters();
        defaultDynamicPositionParameters.CameraExecutionMode = cameraExecutionMode;
        defaultDynamicPositionParameters.AdditiveOffset = additiveOffset;
        defaultDynamicPositionParameters.LerpSpeed = lerpSpeed;
        defaultDynamicPositionParameters.LerpThreshold = lerpThreshold;
    }

    public static DynamicTargetPositionParameters GetDefaultDynamicPositionParameters()
    {
        return defaultDynamicPositionParameters;
    }

    public static void SetDefaultDynamicRotationParameters(CameraExecutionMode cameraExecutionMode, DynamicRotationBehaviour dynamicRotationBehaviour, float lerpSpeed, float lerpThreshold)
    {
        defaultDynamicRotationParameters = new DynamicTargetRotationParameters();
        defaultDynamicRotationParameters.CameraExecutionMode = cameraExecutionMode;
        defaultDynamicRotationParameters.DynamicRotationBehaviour = dynamicRotationBehaviour;
        defaultDynamicRotationParameters.LerpSpeed = lerpSpeed;
        defaultDynamicRotationParameters.LerpThreshold = lerpThreshold;
    }

    public static DynamicTargetRotationParameters GetDefaultDynamicRotationParameters()
    {
        return defaultDynamicRotationParameters;
    }

    public static void SetDefaultSteadyCamParameters(float minCosIntensity, float maxCosIntensity, float cosIntensityChangeTimer, float minSinIntensity, float maxSinIntensity, float sinIntensityChangeTimer, float lerpSpeed)
    {
        defaultSteadyCamParameters = new SteadyCamParameters();
        defaultSteadyCamParameters.MinCosIntensity = minCosIntensity;
        defaultSteadyCamParameters.MaxCosIntensity = maxCosIntensity;
        defaultSteadyCamParameters.CosIntensityChangeTimer = cosIntensityChangeTimer;
        defaultSteadyCamParameters.MinSinIntensity = minSinIntensity;
        defaultSteadyCamParameters.MaxSinIntensity = maxSinIntensity;
        defaultSteadyCamParameters.SinIntensityChangeTimer = sinIntensityChangeTimer;
        defaultSteadyCamParameters.LerpSpeed = lerpSpeed;
    }

    public static SteadyCamParameters GetDefaultSteadyCamParameters()
    {
        return defaultSteadyCamParameters;
    }

    public class StaticTargetPositionParameters
    {
        public CameraExecutionMode CameraExecutionMode; //how do you want to reach the target?
        public float LerpSpeed; //automatically used if CameraExecutionMode is NormalLerp
        public float LerpThreshold; //the threshold at which the lerp will be considered finished in NormalLerp
        public float Duration; //automatically used if CameraExecutionMode is ConstantLerp or SmoothLerp
        public SteadyCamBehaviour SteadyCamOnStart; //steadycam behaviour when the target is set
        public SteadyCamBehaviour SteadyCamOnEnd; //steadycam behaviour when the target is reached
        public SteadyCamParameters StartSCParameters; //the steadycam params to use if SC on start is set
        public SteadyCamParameters EndSCParameters; //the steadycam params to use if SC on end is set
    }

    public class StaticTargetRotationParameters
    {
        public CameraExecutionMode CameraExecutionMode; //how do you want to reach the target?
        public float LerpSpeed; //automatically used if CameraExecutionMode is NormalLerp
        public float LerpThreshold; //the threshold at which the lerp will be considered finished in NormalLerp
        public float Duration; //automatically used if CameraExecutionMode is ConstantLerp or SmoothLerp
        public SteadyCamBehaviour SteadyCamOnStart; //steadycam behaviour when the target is set
        public SteadyCamBehaviour SteadyCamOnEnd; //steadycam behaviour when the target is reached
        public SteadyCamParameters StartSCParameters; //the steadycam params to use if SC on start is set
        public SteadyCamParameters EndSCParameters; //the steadycam params to use if SC on end is set
    }

    public class DynamicTargetPositionParameters
    {
        public CameraExecutionMode CameraExecutionMode;
        public float LerpSpeed;
        public float LerpThreshold;
        public Vector3 AdditiveOffset;
    }

    public class DynamicTargetRotationParameters
    {
        public CameraExecutionMode CameraExecutionMode;
        public DynamicRotationBehaviour DynamicRotationBehaviour;
        public float LerpSpeed;
        public float LerpThreshold;
    }

    public struct SteadyCamParameters
    {
        public float MinCosIntensity;
        public float MaxCosIntensity;
        public float CosIntensityChangeTimer;
        public float MinSinIntensity;
        public float MaxSinIntensity;
        public float SinIntensityChangeTimer;
        public float LerpSpeed;
    }
}
