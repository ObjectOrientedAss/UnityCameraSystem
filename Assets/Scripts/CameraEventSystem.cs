using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnSetCameraTargetPosition(Vector3 position);
public delegate void OnSetCameraTargetRotation(Quaternion rotation);
public delegate void OnCameraTargetPositionReached();
public delegate void OnCameraTargetRotationReached();

public class CameraEventSystem
{
    public static event OnSetCameraTargetPosition SetCameraTargetPositionEvent;
    public static event OnSetCameraTargetRotation SetCameraTargetRotationEvent;
    public static event OnCameraTargetPositionReached CameraTargetPositionReachedEvent;
    public static event OnCameraTargetRotationReached CameraTargetRotationReachedEvent;

    public static void SetCameraTargetPosition(Vector3 position)
    {
        SetCameraTargetPositionEvent.Invoke(position);
    }

    public static void SetCameraTargetRotation(Quaternion rotation)
    {
        SetCameraTargetRotationEvent.Invoke(rotation);
    }

    public static void CameraTargetPositionReached()
    {
        CameraTargetPositionReachedEvent.Invoke();
    }

    public static void CameraTargetRotationReached()
    {
        CameraTargetRotationReachedEvent.Invoke();
    }
}
