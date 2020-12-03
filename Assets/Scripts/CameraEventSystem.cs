using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//BIG PAPPA QUESTION: do i really need to pass a CameraTarget object to these little boys?
//consider just passing a transform...
//Anyway, CameraTarget solution fits more, change the dynamic kiddos to get CameraTargets instead of GOs :)

public delegate void OnSetCameraStaticTargetPosition(CameraTarget targetPosition, TargetParameters.StaticTargetPositionParameters parameters);
public delegate void OnSetCameraStaticTargetRotation(CameraTarget targetRotation, TargetParameters.StaticTargetRotationParameters parameters);

public delegate void OnCameraTargetPositionReached(CameraTarget reachedTarget, TargetParameters.StaticTargetPositionParameters parameters);
public delegate void OnCameraTargetRotationReached(CameraTarget reachedTarget, TargetParameters.StaticTargetRotationParameters parameters);

public delegate void OnSetCameraDynamicTargetPosition(GameObject target, TargetParameters.DynamicTargetPositionParameters parameters);
public delegate void OnSetCameraDynamicTargetRotation(GameObject target, TargetParameters.DynamicTargetRotationParameters parameters);

public delegate void OnCameraSteadyCamActivation(TargetParameters.SteadyCamParameters parameters);

public class CameraEventSystem
{
    public static event OnSetCameraStaticTargetPosition SetCameraStaticTargetPositionEvent;
    public static event OnSetCameraStaticTargetRotation SetCameraStaticTargetRotationEvent;

    public static event OnCameraTargetPositionReached CameraTargetPositionReachedEvent;
    public static event OnCameraTargetRotationReached CameraTargetRotationReachedEvent;

    public static event OnSetCameraDynamicTargetPosition SetCameraDynamicTargetPositionEvent;
    public static event OnSetCameraDynamicTargetRotation SetCameraDynamicTargetRotationEvent;

    public static event OnCameraSteadyCamActivation CameraSteadyCamActivationEvent;

    public static void SetCameraStaticTargetPosition(CameraTarget targetPosition, TargetParameters.StaticTargetPositionParameters parameters)
    {
        SetCameraStaticTargetPositionEvent.Invoke(targetPosition, parameters);
    }

    public static void SetCameraStaticTargetRotation(CameraTarget targetRotation, TargetParameters.StaticTargetRotationParameters parameters)
    {
        SetCameraStaticTargetRotationEvent.Invoke(targetRotation, parameters);
    }

    public static void CameraTargetPositionReached(CameraTarget reachedTarget, TargetParameters.StaticTargetPositionParameters parameters)
    {
        CameraTargetPositionReachedEvent.Invoke(reachedTarget, parameters);
    }

    public static void CameraTargetRotationReached(CameraTarget reachedTarget, TargetParameters.StaticTargetRotationParameters parameters)
    {
        CameraTargetRotationReachedEvent.Invoke(reachedTarget, parameters);
    }

    public static void SetCameraDynamicTargetPosition(GameObject target, TargetParameters.DynamicTargetPositionParameters parameters)
    {
        SetCameraDynamicTargetPositionEvent.Invoke(target, parameters);
    }

    public static void SetCameraDynamicTargetRotation(GameObject target, TargetParameters.DynamicTargetRotationParameters parameters)
    {
        SetCameraDynamicTargetRotationEvent.Invoke(target, parameters);
    }

    public static void CameraSteadyCamActivation(TargetParameters.SteadyCamParameters parameters)
    {
        CameraSteadyCamActivationEvent.Invoke(parameters);
    }
}
