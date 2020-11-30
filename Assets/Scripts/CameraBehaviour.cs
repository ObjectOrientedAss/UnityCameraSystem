using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraExecutionMode { Undefined, Immediate, ConstantLerp, NormalLerp, SmoothLerp }

public class CameraBehaviour : MonoBehaviour
{
    [HideInInspector] public Transform Camera; //Camera transform (must be child of this gameobject!)
    public bool UsingSteadyCam; //is the camera using the SteadyCam simulation?
    public bool Moving; //is the Camera moving?
    public bool Rotating; //is the Camera rotating?

    public Transform TEST_POSITION_TARGET;
    public Transform TEST_ROTATION_TARGET;

    private Coroutine SteadyCamCoroutine;

    public CameraExecutionMode CameraPositioningMode; //how will the camera move from A pos to B pos
    public CameraExecutionMode CameraRotatingMode; //how will the camera rotate from A rot to B rot

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

    public float SteadyCamLerpSpeed;

    private void Awake()
    {
        if (CameraPositioningMode == CameraExecutionMode.Undefined)
            CameraPositioningMode = CameraExecutionMode.Immediate;
        if (CameraRotatingMode == CameraExecutionMode.Undefined)
            CameraRotatingMode = CameraExecutionMode.Immediate;

        Camera = transform.GetChild(0);

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
        Moving = true;
        TargetPosition = position;
        if (SteadyCamCoroutine != null)
        {
            StopCoroutine(SteadyCamCoroutine);
            StartCoroutine(RecenterCamera());
        }

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
                StartCoroutine(NormalLerpPositioning());
                break;
            case CameraExecutionMode.SmoothLerp:
                StartCoroutine(SmoothLerpPositioning());
                break;
        }
    }

    private void SetCameraTargetRotation(Quaternion rotation)
    {
        Rotating = true;
        TargetRotation = rotation;

        if (SteadyCamCoroutine != null)
        {
            StopCoroutine(SteadyCamCoroutine);
            StartCoroutine(RecenterCamera());
        }

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
                StartCoroutine(NormalLerpRotating());
                break;
            case CameraExecutionMode.SmoothLerp:
                StartCoroutine(SmoothLerpRotating());
                break;
        }
    }

    /// <summary>
    /// The camera has reached the target position.
    /// </summary>
    private void CameraTargetPositionReached()
    {
        Moving = false;
        Debug.Log("POSITION REACHED");
        if (!Rotating)
            ActivateSteadyCam();
    }

    /// <summary>
    /// The camera has reached the target rotation.
    /// </summary>
    private void CameraTargetRotationReached()
    {
        Rotating = false;
        Debug.Log("ROTATION REACHED");
        if (!Moving)
            ActivateSteadyCam();
    }

    /// <summary>
    /// Move the camera towards the destination at constant speed, reaching in the given time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConstantLerpPositioning()
    {
        ElapsedPositioningLerpTime = 0;
        Vector3 startingPosition = transform.position;
        float step;
        while (ElapsedPositioningLerpTime < PositionDuration)
        {
            step = ElapsedPositioningLerpTime / PositionDuration;
            ElapsedPositioningLerpTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startingPosition, TargetPosition, step);
            yield return null;
        }

        CameraEventSystem.CameraTargetPositionReached();
    }

    /// <summary>
    /// Rotate the camera towards the destination rotation at constant speed, reaching in the given time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConstantLerpRotating()
    {
        ElapsedRotatingLerpTime = 0;
        Quaternion startingRotation = transform.rotation;
        float step;
        while (ElapsedRotatingLerpTime < RotationDuration)
        {
            step = ElapsedRotatingLerpTime / RotationDuration;
            ElapsedRotatingLerpTime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startingRotation, TargetRotation, step);
            yield return null;
        }

        CameraEventSystem.CameraTargetRotationReached();
    }

    /// <summary>
    /// Move the camera towards the destination, slowing down while approaching.
    /// </summary>
    /// <returns></returns>
    private IEnumerator NormalLerpPositioning()
    {
        while (Vector3.Distance(transform.position, TargetPosition) > 0.2f)
        {
            transform.position = Vector3.Lerp(transform.position, TargetPosition, PositionLerpSpeed * Time.deltaTime);
            yield return null;
        }

        CameraEventSystem.CameraTargetPositionReached();
    }

    /// <summary>
    /// Rotate the camera towards the destination rotation, slowing down while reaching it.
    /// </summary>
    /// <returns></returns>
    private IEnumerator NormalLerpRotating()
    {
        while (Quaternion.Angle(transform.rotation, TargetRotation) > 5)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, RotationLerpSpeed * Time.deltaTime);
            yield return null;
        }

        CameraEventSystem.CameraTargetRotationReached();
    }

    /// <summary>
    /// Move the camera towards the destination, starting slow, accelerating, decelerating near the end, reaching in the given time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SmoothLerpPositioning()
    {
        ElapsedPositioningLerpTime = 0;
        Vector3 startingPosition = transform.position;
        float step;
        while (ElapsedPositioningLerpTime < PositionDuration)
        {
            step = ElapsedPositioningLerpTime / PositionDuration;
            step = step * step * (3f - 2f * step);
            ElapsedPositioningLerpTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startingPosition, TargetPosition, step);
            yield return null;
        }

        CameraEventSystem.CameraTargetPositionReached();
    }

    /// <summary>
    /// Rotate the camera towards the destination rotation, starting slow, accelerating, decelerating near the end, reaching in the given time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SmoothLerpRotating()
    {
        ElapsedRotatingLerpTime = 0;
        Quaternion startingRotation = transform.rotation;
        float step;
        while (ElapsedRotatingLerpTime < RotationDuration)
        {
            step = ElapsedRotatingLerpTime / RotationDuration;
            step = step * step * (3f - 2f * step);
            ElapsedRotatingLerpTime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startingRotation, TargetRotation, step);
            yield return null;
        }

        CameraEventSystem.CameraTargetRotationReached();
    }

    public void ActivateSteadyCam()
    {
        UsingSteadyCam = true;
        SteadyCamCoroutine = StartCoroutine(SimulateSteadyCam());
    }

    /// <summary>
    /// Simulate the SteadyCam movement on the camera.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SimulateSteadyCam()
    {
        float steadyLerpSpeed = 0.5f;
        Vector3 randomPos;
        Vector3 destinationPosition;
        Vector3 startingPosition;
        float step;
        float duration;
        float elapsedSteadyTimeElapsed;
        int xSign = 0;
        int ySign = 0;
        while (true)
        {
            duration = Random.Range(3.5f, 4.5f);
            elapsedSteadyTimeElapsed = 0;
            randomPos = Random.insideUnitCircle;// * 0.5f;

            //Debug.Log("PRIMA ||| Segno X: " + xSign + "Segno Y: " + ySign);
            if ((int)Mathf.Sign(randomPos.x) == xSign)
                randomPos.x = -randomPos.x;
            if ((int)Mathf.Sign(randomPos.y) == ySign)
                randomPos.y = -randomPos.y;

            xSign = (int)Mathf.Sign(randomPos.x);
            ySign = (int)Mathf.Sign(randomPos.y);

            randomPos.x = Mathf.Clamp(randomPos.x, -0.1f, 0.1f);
            randomPos.y = Mathf.Clamp(randomPos.y, -0.1f, 0.1f);

            //Debug.Log("DOPO ||| Segno X: " + xSign + "Segno Y: " + ySign);
            Debug.Log("New Random Pos: " + randomPos);

            destinationPosition = transform.position + randomPos;
            startingPosition = Camera.position;
            while (elapsedSteadyTimeElapsed < duration)
            {
                step = elapsedSteadyTimeElapsed / duration;
                step = step * step * (3f - 2f * step);
                elapsedSteadyTimeElapsed += Time.deltaTime;
                Camera.position = Vector3.Lerp(startingPosition, destinationPosition, step);
                yield return null;
            }

            //while (Vector3.Distance(Camera.position, destinationPosition) > 0.1f)
            //{
            //    Camera.position = Vector3.Lerp(Camera.position, destinationPosition, steadyLerpSpeed * Time.deltaTime);
            //    yield return null;
            //}
            yield return null;
        }
    }

    /// <summary>
    /// Move back the camera child to the central position to match this gameobject.
    /// Useful if you are interrupting a SteadyCam movement for any reason.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RecenterCamera()
    {
        float recenterDuration = 1f;
        float recenterTimeElapsed = 0f;
        float step;
        Vector3 startingPos = Camera.position;
        while (recenterTimeElapsed < recenterDuration)
        {
            step = recenterTimeElapsed / recenterDuration;
            step = step * step * (3f - 2f * step);
            recenterTimeElapsed += Time.deltaTime;
            Camera.position = Vector3.Lerp(startingPos, transform.position, step);
            yield return null;
        }
        UsingSteadyCam = false;
    }
}