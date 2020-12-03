using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTester : MonoBehaviour
{
    public enum TargetType { Static, Dynamic } //debug only
    public TargetType TreatTargetAs; //debug only
    public CameraTarget Target;
    private CameraBehaviour cameraTest;

    //public CameraTarget[] Targets;
    //private CameraTarget lastTarget;
    //private int targetIndex;

    private void Awake()
    {
        // GETTING THE CAMERA REFERENCE:
        cameraTest = FindObjectOfType<CameraBehaviour>();

        // SETTING THE DEFAULT PARAMETERS FOR STATIC AND DYNAMIC TARGETS AND FOR THE STEADYCAM
        TargetParameters.SetDefaultSteadyCamParameters(0.03f, 0.05f, 2f, 0.03f, 0.06f, 3f, 1f);
        TargetParameters.SetDefaultStaticPositionParameters(CameraExecutionMode.SmoothLerp, 0.5f, 0.5f, 25f, SteadyCamBehaviour.Ignore, SteadyCamBehaviour.Ignore);
        TargetParameters.SetDefaultStaticRotationParameters(CameraExecutionMode.SmoothLerp, 0.5f, 0.5f, 5f, SteadyCamBehaviour.Ignore, SteadyCamBehaviour.Ignore);
        TargetParameters.SetDefaultDynamicPositionParameters(CameraExecutionMode.NormalLerp, 1f, 0.05f, Vector3.zero);
        TargetParameters.SetDefaultDynamicRotationParameters(CameraExecutionMode.NormalLerp, DynamicRotationBehaviour.LookAs, 1f, 0.05f);

        //CameraEventSystem.CameraTargetPositionReachedEvent += CheckTarget;

        // SETTING CUSTOM METHOD FOR CAMERA MOVEMENT:
        cameraTest.SetCustomMethod(SmootherLerpPositioning);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (TreatTargetAs == TargetType.Static) //STATIC TARGET
            {
                TargetParameters.StaticTargetRotationParameters rotParams = TargetParameters.GetDefaultStaticRotationParameters();
                rotParams.Duration = 8f;
                cameraTest.SetStaticCameraTarget(Target, rotParams);

                TargetParameters.StaticTargetPositionParameters posParams = TargetParameters.GetDefaultStaticPositionParameters();
                posParams.CameraExecutionMode = CameraExecutionMode.CustomLerp;
                posParams.StartSCParameters.SinIntensityChangeTimer = 2f;
                posParams.StartSCParameters.CosIntensityChangeTimer = 2f;
                posParams.StartSCParameters.MinCosIntensity = 0.1f;
                posParams.StartSCParameters.MaxCosIntensity = 0.12f;
                posParams.StartSCParameters.MinSinIntensity = 0.1f;
                posParams.StartSCParameters.MaxSinIntensity = 0.11f;

                posParams.SteadyCamOnStart = SteadyCamBehaviour.TurnOnAndOverride;
                posParams.SteadyCamOnEnd = SteadyCamBehaviour.TurnOnAndOverride;

                cameraTest.SetStaticCameraTarget(Target, posParams);
            }
            else //DYNAMIC TARGET
            {
                TargetParameters.DynamicTargetPositionParameters dynamicPosParams = TargetParameters.GetDefaultDynamicPositionParameters();
                dynamicPosParams.LerpSpeed = 2f;
                cameraTest.SetDynamicCameraTarget(Target.transform.GetChild(0).GetComponent<CameraTarget>(), dynamicPosParams);

                TargetParameters.DynamicTargetRotationParameters dynamicRotParams = TargetParameters.GetDefaultDynamicRotationParameters();
                dynamicRotParams.DynamicRotationBehaviour = DynamicRotationBehaviour.LookAt;
                dynamicRotParams.LerpSpeed = 5f;
                cameraTest.SetDynamicCameraTarget(Target, dynamicRotParams);
            }
        }
        //else if (Input.GetKey(KeyCode.LeftControl))
        //{
        //    NextTarget();
        //}

    }

    //private void CheckTarget(CameraTarget target, TargetParameters.StaticTargetPositionParameters parameters)
    //{
    //    if (target == lastTarget)
    //    {
    //        NextTarget();
    //    }
    //}

    //private void NextTarget()
    //{
    //    if (Targets[++targetIndex])
    //    {
    //        TargetParameters.StaticTargetPositionParameters parameters = TargetParameters.GetDefaultStaticPositionParameters();
    //        parameters.Duration = Targets[targetIndex].Duration;
    //        CameraEventSystem.SetCameraStaticTargetPosition(Targets[targetIndex], parameters);
    //    }
    //}

    private IEnumerator SmootherLerpPositioning(CameraTarget target, TargetParameters.StaticTargetPositionParameters parameters)
    {
        float elapsedPositioningLerpTime = 0;
        Vector3 startingPosition = cameraTest.transform.position;
        float step;
        while (elapsedPositioningLerpTime < parameters.Duration)
        {
            step = elapsedPositioningLerpTime / parameters.Duration;
            step = step * step * step * (step * (6f * step - 15f) + 10f);
            elapsedPositioningLerpTime += Time.deltaTime;
            cameraTest.transform.position = Vector3.Lerp(startingPosition, target.transform.position, step);
            yield return null;
        }

        CameraEventSystem.CameraTargetPositionReached(target, parameters);
    }
}