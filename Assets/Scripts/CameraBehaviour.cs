using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraExecutionMode { Undefined, Immediate, ConstantLerp, NormalLerp, SmoothLerp }
public enum CameraInPlaceBehaviour { Undefined, Normal, SteadyCam }

public class CameraBehaviour : MonoBehaviour
{
    public Transform TEST_POSITION_TARGET;
    public Transform TEST_ROTATION_TARGET;

    public CameraExecutionMode CameraPositioningMode = CameraExecutionMode.Undefined;
    public CameraExecutionMode CameraRotatingMode = CameraExecutionMode.Undefined;
    public CameraInPlaceBehaviour CameraInPlaceBehaviour = CameraInPlaceBehaviour.Undefined;

    //Normal Lerp
    public float PositionLerpSpeed;
    public float RotationLerpSpeed;
    //-----------

    //Constant / Smooth Lerp---
    public float PositionDuration;
    [HideInInspector] public float ElapsedPositioningLerpTime;

    public float RotationDuration;
    [HideInInspector] public float ElapsedRotatingLerpTime;
    //-----------

    [HideInInspector] public Vector3 TargetPosition;
    [HideInInspector] public Quaternion TargetRotation;

    private void Awake()
    {
        CameraEventSystem.SetCameraTargetPositionEvent += SetCameraTargetPosition;
        CameraEventSystem.SetCameraTargetRotationEvent += SetCameraTargetRotation;
        CameraEventSystem.CameraTargetPositionReachedEvent += CameraTargetPositionReached;
        CameraEventSystem.CameraTargetRotationReachedEvent += CameraTargetRotationReached;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CameraEventSystem.SetCameraTargetPosition(TEST_POSITION_TARGET.position);
            CameraEventSystem.SetCameraTargetRotation(TEST_ROTATION_TARGET.rotation);
        }
    }

    private void SetCameraTargetPosition(Vector3 position)
    {
        TargetPosition = position;

        switch (CameraPositioningMode)
        {
            case CameraExecutionMode.Undefined:
                Debug.LogError("Camera Positioning Mode is UNDEFINED.", this);
                break;
            case CameraExecutionMode.Immediate:
                transform.position = TargetPosition;
                CameraEventSystem.CameraTargetPositionReached();
                break;
            case CameraExecutionMode.ConstantLerp:
                StartCoroutine(ConstantLerpPositioning());
                break;
            case CameraExecutionMode.NormalLerp:
                StartCoroutine(VariableLerpPositioning());
                break;
            case CameraExecutionMode.SmoothLerp:
                StartCoroutine(SmoothLerpPositioning());
                break;
        }
    }

    private void SetCameraTargetRotation(Quaternion rotation)
    {
        TargetRotation = rotation;

        switch (CameraRotatingMode)
        {
            case CameraExecutionMode.Undefined:
                Debug.LogError("Camera Rotating Mode is UNDEFINED.", this);
                break;
            case CameraExecutionMode.Immediate:
                transform.rotation = TargetRotation;
                CameraEventSystem.CameraTargetRotationReached();
                break;
            case CameraExecutionMode.ConstantLerp:
                StartCoroutine(ConstantLerpRotating());
                break;
            case CameraExecutionMode.NormalLerp:
                StartCoroutine(VariableLerpRotating());
                break;
            case CameraExecutionMode.SmoothLerp:
                StartCoroutine(SmoothLerpRotating());
                break;
        }
    }

    private void CameraTargetPositionReached()
    {
        Debug.Log("POSITION REACHED");
    }

    private void CameraTargetRotationReached()
    {
        Debug.Log("ROTATION REACHED");
    }

    private IEnumerator ConstantLerpPositioning()
    {
        ElapsedPositioningLerpTime = 0;
        Vector3 StartingPosition = transform.position;
        float step;
        while (ElapsedPositioningLerpTime < PositionDuration)
        {
            step = ElapsedPositioningLerpTime / PositionDuration;
            ElapsedPositioningLerpTime += Time.deltaTime;
            transform.position = Vector3.Lerp(StartingPosition, TargetPosition, step);
            yield return null;
        }

        CameraEventSystem.CameraTargetPositionReached();
    }

    private IEnumerator ConstantLerpRotating()
    {
        ElapsedRotatingLerpTime = 0;
        Quaternion StartingRotation = transform.rotation;
        float step;
        while (ElapsedRotatingLerpTime < RotationDuration)
        {
            step = ElapsedRotatingLerpTime / RotationDuration;
            ElapsedRotatingLerpTime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(StartingRotation, TargetRotation, step);
            yield return null;
        }

        CameraEventSystem.CameraTargetRotationReached();
    }

    private IEnumerator VariableLerpPositioning()
    {
        while (Vector3.Distance(transform.position, TargetPosition) > 0.5f)
        {
            transform.position = Vector3.Lerp(transform.position, TargetPosition, PositionLerpSpeed * Time.deltaTime);
            yield return null;
        }

        CameraEventSystem.CameraTargetPositionReached();
    }

    private IEnumerator VariableLerpRotating()
    {
        while (Quaternion.Angle(transform.rotation, TargetRotation) > 5)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, RotationLerpSpeed * Time.deltaTime);
            yield return null;
        }

        CameraEventSystem.CameraTargetRotationReached();
    }

    private IEnumerator SmoothLerpPositioning()
    {
        ElapsedPositioningLerpTime = 0;
        Vector3 StartingPosition = transform.position;
        float step;
        while (ElapsedPositioningLerpTime < PositionDuration)
        {
            step = ElapsedPositioningLerpTime / PositionDuration;
            step = step * step * (3f - 2f * step);
            ElapsedPositioningLerpTime += Time.deltaTime;
            transform.position = Vector3.Lerp(StartingPosition, TargetPosition, step);
            yield return null;
        }

        CameraEventSystem.CameraTargetPositionReached();
    }

    private IEnumerator SmoothLerpRotating()
    {
        ElapsedRotatingLerpTime = 0;
        Quaternion StartingRotation = transform.rotation;
        float step;
        while (ElapsedRotatingLerpTime < RotationDuration)
        {
            step = ElapsedRotatingLerpTime / RotationDuration;
            step = step * step * (3f - 2f * step);
            ElapsedRotatingLerpTime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(StartingRotation, TargetRotation, step);
            yield return null;
        }

        CameraEventSystem.CameraTargetRotationReached();
    }
}