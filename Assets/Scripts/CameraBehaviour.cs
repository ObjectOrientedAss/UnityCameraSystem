using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//0: Immediately match target's position/rotation
//1: Lerp at constant speed to reach target's position/rotation
//2: Lerp faster when far, and slower when near, to reach target's position/rotation
//3: Lerp smoothly to reach target's position/rotation
//4: Custom lerp to reach target's position/rotation. Remember to act on the main camera transform!
public enum CameraExecutionMode { Immediate, ConstantLerp, NormalLerp, SmoothLerp, CustomLerp }

//0: If steadycam is not active, activate it AND set its parameters. If steadycam is active, only override its parameters.
//1: If steadycam is on, turn it off.
//2: If steadycam is on, override its parameters.
//3: Totally ignore the steadycam.
public enum SteadyCamBehaviour { TurnOnAndOverride, TurnOff, OverrideIfOn, Ignore }

public class CameraBehaviour : MonoBehaviour
{
    [HideInInspector] public Transform Camera; //Camera transform (MUST be child of this gameobject!)

    [HideInInspector] public bool UsingSteadyCam; //is the camera using the SteadyCam simulation?
    [HideInInspector] public bool Moving; //is the Camera moving?
    [HideInInspector] public bool Rotating; //is the Camera rotating?

    //STEADYCAM PARAMS--- TODO: move them in a steadycam configuration struct
    //public float MinFloatingDuration;
    //public float MaxFloatingDuration;
    //public float MinXClamp;
    //public float MaxXClamp;
    //public float MinYClamp;
    //public float MaxYClamp;
    public float RecenterDuration;
    private Coroutine steadyCamCoroutine;
    private Coroutine positionCoroutine;
    private Coroutine rotationCoroutine;
    private TargetParameters.SteadyCamParameters steadyCamParameters;
    //-----------

    System.Func<CameraTarget, TargetParameters.StaticTargetPositionParameters, IEnumerator> customStaticPositioningLerp;
    System.Func<CameraTarget, TargetParameters.StaticTargetRotationParameters, IEnumerator> customStaticRotatingLerp;
    System.Func<GameObject, TargetParameters.DynamicTargetPositionParameters, IEnumerator> customDynamicPositioningLerp;
    System.Func<GameObject, TargetParameters.DynamicTargetRotationParameters, IEnumerator> customDynamicRotatingLerp;

    private void Awake()
    {
        Camera = transform.GetChild(0);
        if (Camera == null)
            Debug.LogError("Error: the transform with the Camera component must be a child of " + gameObject.name);

        CameraEventSystem.SetCameraStaticTargetPositionEvent += SetCameraStaticTargetPosition;
        CameraEventSystem.SetCameraStaticTargetRotationEvent += SetCameraStaticTargetRotation;

        CameraEventSystem.CameraTargetPositionReachedEvent += CameraTargetPositionReached;
        CameraEventSystem.CameraTargetRotationReachedEvent += CameraTargetRotationReached;

        CameraEventSystem.SetCameraDynamicTargetPositionEvent += SetCameraDynamicTargetPosition;
        CameraEventSystem.SetCameraDynamicTargetRotationEvent += SetCameraDynamicTargetRotation;

        CameraEventSystem.CameraSteadyCamActivationEvent += ActivateSteadyCam;

        steadyCamParameters = TargetParameters.GetDefaultSteadyCamParameters();
    }

    public void SetStaticCameraTarget(CameraTarget cameraTarget, TargetParameters.StaticTargetPositionParameters parameters)
    {
        CameraEventSystem.SetCameraStaticTargetPosition(cameraTarget, parameters);
    }

    public void SetStaticCameraTarget(CameraTarget cameraTarget, TargetParameters.StaticTargetRotationParameters parameters)
    {
        CameraEventSystem.SetCameraStaticTargetRotation(cameraTarget, parameters);
    }

    public void SetStaticCameraTarget(CameraTarget cameraTarget, TargetParameters.StaticTargetPositionParameters posParameters, TargetParameters.StaticTargetRotationParameters rotParameters)
    {
        CameraEventSystem.SetCameraStaticTargetPosition(cameraTarget, posParameters);
        CameraEventSystem.SetCameraStaticTargetRotation(cameraTarget, rotParameters);
    }

    public void SetDynamicCameraTarget(CameraTarget cameraTarget, TargetParameters.DynamicTargetPositionParameters parameters)
    {
        CameraEventSystem.SetCameraDynamicTargetPosition(cameraTarget.gameObject, parameters);
    }

    public void SetDynamicCameraTarget(CameraTarget cameraTarget, TargetParameters.DynamicTargetRotationParameters parameters)
    {
        CameraEventSystem.SetCameraDynamicTargetRotation(cameraTarget.gameObject, parameters);
    }

    public void SetDynamicCameraTarget(CameraTarget cameraTarget, TargetParameters.DynamicTargetPositionParameters posParameters, TargetParameters.DynamicTargetRotationParameters rotParameters)
    {
        CameraEventSystem.SetCameraDynamicTargetPosition(cameraTarget.gameObject, posParameters);
        CameraEventSystem.SetCameraDynamicTargetRotation(cameraTarget.gameObject, rotParameters);
    }

    private void SetCameraDynamicTargetPosition(GameObject target, TargetParameters.DynamicTargetPositionParameters parameters)
    {
        if (positionCoroutine != null)
            StopCoroutine(positionCoroutine);

        switch (parameters.CameraExecutionMode)
        {
            case CameraExecutionMode.Immediate:
                //ImmediatePositioning(position, parameters);
                break;
            case CameraExecutionMode.ConstantLerp:
                //StartCoroutine(ConstantLerpPositioning(position, parameters));
                break;
            case CameraExecutionMode.NormalLerp:
                positionCoroutine = StartCoroutine(FollowPosition(target, parameters));
                break;
            case CameraExecutionMode.SmoothLerp:
                //StartCoroutine(SmoothLerpPositioning(position, parameters));
                break;
            case CameraExecutionMode.CustomLerp:
                positionCoroutine = StartCoroutine(customDynamicPositioningLerp(target, parameters));
                break;
        }
    }

    private void SetCameraDynamicTargetRotation(GameObject target, TargetParameters.DynamicTargetRotationParameters parameters)
    {
        if (rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);

        switch (parameters.CameraExecutionMode)
        {
            case CameraExecutionMode.Immediate:
                //ImmediatePositioning(position, parameters);
                break;
            case CameraExecutionMode.ConstantLerp:
                //StartCoroutine(ConstantLerpPositioning(position, parameters));
                break;
            case CameraExecutionMode.NormalLerp:
                rotationCoroutine = StartCoroutine(FollowRotation(target, parameters));
                break;
            case CameraExecutionMode.SmoothLerp:
                //StartCoroutine(SmoothLerpPositioning(position, parameters));
                break;
            case CameraExecutionMode.CustomLerp:
                rotationCoroutine = StartCoroutine(customDynamicRotatingLerp(target, parameters));
                break;
        }
    }

    private void SetCameraStaticTargetPosition(CameraTarget target, TargetParameters.StaticTargetPositionParameters parameters)
    {
        if (positionCoroutine != null)
            StopCoroutine(positionCoroutine);

        switch (parameters.CameraExecutionMode)
        {
            case CameraExecutionMode.Immediate:
                ImmediatePositioning(target, parameters);
                break;
            case CameraExecutionMode.ConstantLerp:
                positionCoroutine = StartCoroutine(ConstantLerpPositioning(target, parameters));
                break;
            case CameraExecutionMode.NormalLerp:
                positionCoroutine = StartCoroutine(NormalLerpPositioning(target, parameters));
                break;
            case CameraExecutionMode.SmoothLerp:
                positionCoroutine = StartCoroutine(SmoothLerpPositioning(target, parameters));
                break;
            case CameraExecutionMode.CustomLerp:
                positionCoroutine = StartCoroutine(customStaticPositioningLerp(target, parameters));
                break;
        }

        Moving = true;

        switch (parameters.SteadyCamOnStart)
        {
            case SteadyCamBehaviour.Ignore:
                return;
            case SteadyCamBehaviour.TurnOnAndOverride:
                if (steadyCamCoroutine == null)
                    ActivateSteadyCam(parameters.StartSCParameters);
                else
                    steadyCamParameters = parameters.StartSCParameters;
                break;
            case SteadyCamBehaviour.TurnOff:
                if (steadyCamCoroutine != null)
                {
                    StopCoroutine(steadyCamCoroutine);
                    StartCoroutine(RecenterCamera());
                }
                break;
            case SteadyCamBehaviour.OverrideIfOn:
                if (steadyCamCoroutine != null)
                    steadyCamParameters = parameters.StartSCParameters;
                break;
        }
    }

    private void SetCameraStaticTargetRotation(CameraTarget target, TargetParameters.StaticTargetRotationParameters parameters)
    {
        if (rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);

        switch (parameters.CameraExecutionMode)
        {
            case CameraExecutionMode.Immediate:
                ImmediateRotating(target, parameters);
                break;
            case CameraExecutionMode.ConstantLerp:
                rotationCoroutine = StartCoroutine(ConstantLerpRotating(target, parameters));
                break;
            case CameraExecutionMode.NormalLerp:
                rotationCoroutine = StartCoroutine(NormalLerpRotating(target, parameters));
                break;
            case CameraExecutionMode.SmoothLerp:
                rotationCoroutine = StartCoroutine(SmoothLerpRotating(target, parameters));
                break;
            case CameraExecutionMode.CustomLerp:
                rotationCoroutine = StartCoroutine(customStaticRotatingLerp(target, parameters));
                break;
        }

        Rotating = true;

        switch (parameters.SteadyCamOnStart)
        {
            case SteadyCamBehaviour.Ignore:
                return;
            case SteadyCamBehaviour.TurnOnAndOverride:
                if (steadyCamCoroutine == null)
                    ActivateSteadyCam(parameters.StartSCParameters);
                else
                    steadyCamParameters = parameters.StartSCParameters;
                break;
            case SteadyCamBehaviour.TurnOff:
                if (steadyCamCoroutine != null)
                {
                    StopCoroutine(steadyCamCoroutine);
                    StartCoroutine(RecenterCamera());
                }
                break;
            case SteadyCamBehaviour.OverrideIfOn:
                if (steadyCamCoroutine != null)
                    steadyCamParameters = parameters.StartSCParameters;
                break;
        }
    }

    /// <summary>
    /// The camera has reached the target position.
    /// </summary>
    private void CameraTargetPositionReached(CameraTarget target, TargetParameters.StaticTargetPositionParameters parameters)
    {
        Moving = false;
        Debug.Log("POSITION REACHED");

        switch (parameters.SteadyCamOnEnd)
        {
            case SteadyCamBehaviour.Ignore:
                return;
            case SteadyCamBehaviour.TurnOnAndOverride:
                if (steadyCamCoroutine == null)
                    ActivateSteadyCam(parameters.EndSCParameters);
                else
                    steadyCamParameters = parameters.EndSCParameters;
                break;
            case SteadyCamBehaviour.TurnOff:
                if (steadyCamCoroutine != null)
                {
                    StopCoroutine(steadyCamCoroutine);
                    StartCoroutine(RecenterCamera());
                }
                break;
            case SteadyCamBehaviour.OverrideIfOn:
                if (steadyCamCoroutine != null)
                    steadyCamParameters = parameters.EndSCParameters;
                break;
        }
    }

    /// <summary>
    /// The camera has reached the target rotation.
    /// </summary>
    private void CameraTargetRotationReached(CameraTarget target, TargetParameters.StaticTargetRotationParameters parameters)
    {
        Rotating = false;
        Debug.Log("ROTATION REACHED");

        switch (parameters.SteadyCamOnEnd)
        {
            case SteadyCamBehaviour.Ignore:
                return;
            case SteadyCamBehaviour.TurnOnAndOverride:
                if (steadyCamCoroutine == null)
                    ActivateSteadyCam(parameters.EndSCParameters);
                else
                    steadyCamParameters = parameters.EndSCParameters;
                break;
            case SteadyCamBehaviour.TurnOff:
                if (steadyCamCoroutine != null)
                {
                    StopCoroutine(steadyCamCoroutine);
                    StartCoroutine(RecenterCamera());
                }
                break;
            case SteadyCamBehaviour.OverrideIfOn:
                if (steadyCamCoroutine != null)
                    steadyCamParameters = parameters.EndSCParameters;
                break;
        }

        Debug.Log(rotationCoroutine != null);
    }

    /// <summary>
    /// Immediately move the camera to match the target position.
    /// </summary>
    private void ImmediatePositioning(CameraTarget target, TargetParameters.StaticTargetPositionParameters parameters)
    {
        transform.position = target.transform.position;
        CameraEventSystem.CameraTargetPositionReached(target, parameters);
    }

    /// <summary>
    /// Immediately rotate the camera to match the target rotation.
    /// </summary>
    private void ImmediateRotating(CameraTarget target, TargetParameters.StaticTargetRotationParameters parameters)
    {
        transform.rotation = target.transform.rotation;
        CameraEventSystem.CameraTargetRotationReached(target, parameters);
    }

    /// <summary>
    /// Move the camera towards the destination at constant speed, reaching in the given time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConstantLerpPositioning(CameraTarget target, TargetParameters.StaticTargetPositionParameters parameters)
    {
        float elapsedPositioningLerpTime = 0;
        Vector3 startingPosition = transform.position;
        float step;
        while (elapsedPositioningLerpTime < parameters.Duration)
        {
            step = elapsedPositioningLerpTime / parameters.Duration;
            elapsedPositioningLerpTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startingPosition, target.transform.position, step);
            yield return null;
        }

        CameraEventSystem.CameraTargetPositionReached(target, parameters);
    }

    /// <summary>
    /// Rotate the camera towards the destination rotation at constant speed, reaching in the given time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConstantLerpRotating(CameraTarget target, TargetParameters.StaticTargetRotationParameters parameters)
    {
        float elapsedRotatingLerpTime = 0;
        Quaternion startingRotation = transform.rotation;
        float step;
        while (elapsedRotatingLerpTime < parameters.Duration)
        {
            step = elapsedRotatingLerpTime / parameters.Duration;
            elapsedRotatingLerpTime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startingRotation, target.transform.rotation, step);
            yield return null;
        }

        CameraEventSystem.CameraTargetRotationReached(target, parameters);
    }

    /// <summary>
    /// Move the camera towards the destination, slowing down while approaching.
    /// </summary>
    /// <returns></returns>
    private IEnumerator NormalLerpPositioning(CameraTarget target, TargetParameters.StaticTargetPositionParameters parameters)
    {
        while (Vector3.Distance(transform.position, target.transform.position) > parameters.LerpThreshold)
        {
            transform.position = Vector3.Lerp(transform.position, target.transform.position, parameters.LerpSpeed * Time.deltaTime);
            yield return null;
        }

        CameraEventSystem.CameraTargetPositionReached(target, parameters);
    }

    /// <summary>
    /// Rotate the camera towards the destination rotation, slowing down while reaching it.
    /// </summary>
    /// <returns></returns>
    private IEnumerator NormalLerpRotating(CameraTarget target, TargetParameters.StaticTargetRotationParameters parameters)
    {
        while (Quaternion.Angle(transform.rotation, target.transform.rotation) > parameters.LerpThreshold)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, target.transform.rotation, parameters.LerpSpeed * Time.deltaTime);
            yield return null;
        }

        CameraEventSystem.CameraTargetRotationReached(target, parameters);
    }

    /// <summary>
    /// Move the camera towards the destination, starting slow, accelerating, decelerating near the end, reaching in the given time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SmoothLerpPositioning(CameraTarget target, TargetParameters.StaticTargetPositionParameters parameters)
    {
        float elapsedPositioningLerpTime = 0;
        Vector3 startingPosition = transform.position;
        float step;
        while (elapsedPositioningLerpTime < parameters.Duration)
        {
            step = elapsedPositioningLerpTime / parameters.Duration;
            step = step * step * (3f - 2f * step);
            elapsedPositioningLerpTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startingPosition, target.transform.position, step);
            yield return null;
        }

        CameraEventSystem.CameraTargetPositionReached(target, parameters);
    }

    /// <summary>
    /// Rotate the camera towards the destination rotation, starting slow, accelerating, decelerating near the end, reaching in the given time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SmoothLerpRotating(CameraTarget target, TargetParameters.StaticTargetRotationParameters parameters)
    {
        float elapsedRotatingLerpTime = 0;
        Quaternion startingRotation = transform.rotation;
        float step;
        while (elapsedRotatingLerpTime < parameters.Duration)
        {
            step = elapsedRotatingLerpTime / parameters.Duration;
            step = step * step * (3f - 2f * step);
            elapsedRotatingLerpTime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startingRotation, target.transform.rotation, step);
            yield return null;
        }

        CameraEventSystem.CameraTargetRotationReached(target, parameters);
    }

    /// <summary>
    /// Activate the SteadyCam simulation on the camera.
    /// </summary>
    private void ActivateSteadyCam(TargetParameters.SteadyCamParameters parameters)
    {
        UsingSteadyCam = true;
        steadyCamParameters = parameters;
        steadyCamCoroutine = StartCoroutine(SimulateSteadyCamMoving());
    }

    //----- OLD STEADYCAM IN PLACE | DON'T REMOVE IT, COULD BE USEFUL IN THE FUTURE. -----
    /// <summary>
    /// Simulate the SteadyCam movement on the camera.
    /// </summary>
    /// <returns></returns>
    //private IEnumerator SimulateSteadyCamInPlace()
    //{
    //    //float steadyLerpSpeed = 0.5f;
    //    Vector3 randomPos;
    //    Vector3 destinationPosition;
    //    Vector3 startingPosition;
    //    float step;
    //    float duration;
    //    float elapsedSteadyTimeElapsed;
    //    int xSign = 0;
    //    int ySign = 0;
    //    while (true)
    //    {
    //        duration = Random.Range(MinFloatingDuration, MaxFloatingDuration);
    //        elapsedSteadyTimeElapsed = 0;
    //        randomPos = Random.insideUnitCircle;// * 0.5f;

    //        if ((int)Mathf.Sign(randomPos.x) == xSign)
    //            randomPos.x = -randomPos.x;
    //        if ((int)Mathf.Sign(randomPos.y) == ySign)
    //            randomPos.y = -randomPos.y;

    //        xSign = (int)Mathf.Sign(randomPos.x);
    //        ySign = (int)Mathf.Sign(randomPos.y);

    //        randomPos.x = Mathf.Clamp(randomPos.x, MinXClamp, MaxXClamp);
    //        randomPos.y = Mathf.Clamp(randomPos.y, MinYClamp, MaxYClamp);

    //        destinationPosition = transform.position + randomPos;
    //        startingPosition = Camera.position;
    //        while (elapsedSteadyTimeElapsed < duration)
    //        {
    //            step = elapsedSteadyTimeElapsed / duration;
    //            step = step * step * (3f - 2f * step);
    //            elapsedSteadyTimeElapsed += Time.deltaTime;
    //            Camera.position = Vector3.Lerp(startingPosition, destinationPosition, step);
    //            yield return null;
    //        }

    //        yield return null;
    //    }
    //}

    private IEnumerator SimulateSteadyCamMoving()
    {
        float xInt = 0;
        float yInt = 0;
        Vector3 destination;

        float xIntChangeTimer = steadyCamParameters.CosIntensityChangeTimer;
        float yIntChangeTimer = steadyCamParameters.SinIntensityChangeTimer;
        float xElapsedIntChangeTimer = xIntChangeTimer;
        float yElapsedIntChangeTimer = yIntChangeTimer;

        while (true)
        {
            if (xElapsedIntChangeTimer < xIntChangeTimer)
                xElapsedIntChangeTimer += Time.deltaTime;
            else
            {
                xElapsedIntChangeTimer = 0;
                xInt = Random.Range(steadyCamParameters.MinCosIntensity, steadyCamParameters.MaxCosIntensity);
            }
            if (yElapsedIntChangeTimer < yIntChangeTimer)
                yElapsedIntChangeTimer += Time.deltaTime;
            else
            {
                yElapsedIntChangeTimer = 0;
                yInt = Random.Range(steadyCamParameters.MinSinIntensity, steadyCamParameters.MaxSinIntensity);
            }

            destination = new Vector3(Mathf.Cos(Time.time) * xInt, Mathf.Sin(Time.time * 2) * yInt, Camera.localPosition.z);
            Camera.localPosition = Vector3.Lerp(Camera.localPosition, destination, Time.deltaTime * steadyCamParameters.LerpSpeed);

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
        //float recenterDuration = 1f;
        float recenterTimeElapsed = 0f;
        float step;
        Vector3 startingPos = Camera.position;
        while (recenterTimeElapsed < RecenterDuration)
        {
            step = recenterTimeElapsed / RecenterDuration;
            step = step * step * (3f - 2f * step);
            recenterTimeElapsed += Time.deltaTime;
            Camera.position = Vector3.Lerp(startingPos, transform.position, step);
            yield return null;
        }
        UsingSteadyCam = false;
    }

    /// <summary>
    /// Follow the dynamic target position.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private IEnumerator FollowPosition(GameObject target, TargetParameters.DynamicTargetPositionParameters parameters)
    {
        Vector3 destination;
        while (true)
        {
            destination = target.transform.position + parameters.AdditiveOffset;
            if (Vector3.Distance(transform.position, destination) > parameters.LerpThreshold)
            {
                Moving = true;
                transform.position = Vector3.Lerp(transform.position, destination, parameters.LerpSpeed * Time.deltaTime);
            }
            else
                Moving = false;

            yield return null;
        }
    }

    /// <summary>
    /// Follow the dynamic target rotation.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private IEnumerator FollowRotation(GameObject target, TargetParameters.DynamicTargetRotationParameters parameters)
    {
        if(parameters.DynamicRotationBehaviour == DynamicRotationBehaviour.LookAs)
        {
            while(true)
            {
                if (Quaternion.Angle(transform.rotation, target.transform.rotation) > parameters.LerpThreshold)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, target.transform.rotation, parameters.LerpSpeed * Time.deltaTime);
                    Rotating = true;
                }
                else
                    Rotating = false;

                yield return null;
            }
        }
        else
        {
            Rotating = true;
            Vector3 direction;
            while(true)
            {
                direction = target.transform.position - transform.position;
                if (Vector3.Angle(transform.forward, direction) > parameters.LerpThreshold)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), parameters.LerpSpeed * Time.deltaTime);
                else
                    transform.LookAt(target.transform);

                yield return null;
            }
        }
    }

    /// <summary>
    /// Set a customized method to reach a static target position.
    /// </summary>
    /// <param name="customLerp"></param>
    public void SetCustomMethod(System.Func<CameraTarget, TargetParameters.StaticTargetPositionParameters, IEnumerator> customLerp)
    {
        customStaticPositioningLerp = customLerp;
    }

    /// <summary>
    /// Set a customized method to reach a static target rotation.
    /// </summary>
    /// <param name="customLerp"></param>
    public void SetCustomMethod(System.Func<CameraTarget, TargetParameters.StaticTargetRotationParameters, IEnumerator> customLerp)
    {
        customStaticRotatingLerp = customLerp;
    }

    /// <summary>
    /// Set a customized method to follow a dynamic target position.
    /// </summary>
    /// <param name="customLerp"></param>
    public void SetCustomMethod(System.Func<GameObject, TargetParameters.DynamicTargetPositionParameters, IEnumerator> customLerp)
    {
        customDynamicPositioningLerp = customLerp;
    }

    /// <summary>
    /// Set a customized method to follow a dynamic target rotation.
    /// </summary>
    /// <param name="customLerp"></param>
    public void SetCustomMethod(System.Func<GameObject, TargetParameters.DynamicTargetRotationParameters, IEnumerator> customLerp)
    {
        customDynamicRotatingLerp = customLerp;
    }
}