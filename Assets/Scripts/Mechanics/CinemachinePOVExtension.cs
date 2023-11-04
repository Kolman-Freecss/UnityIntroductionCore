using UnityEngine;
using Cinemachine;

public class CinemachinePOVExtension : CinemachineExtension
{




    #region Auxiliary data
    [Header("Sesitivity mouse")]
    [SerializeField] private float _horizontalSpeed = 1.0f;

    [SerializeField] private float _verticalSpeed = 1.0f;

    [SerializeField] private float _clampAngle = 80.0f;
    private PlayerInput playerInput;
    private Vector3 startingRotation;
    #endregion

    protected override void Awake()
    {
       // playerInput = PlayerInput.Instance; 
        base.Awake();
    }
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (vcam.Follow)
        {
            if (stage == CinemachineCore.Stage.Aim)
            {
                if (startingRotation == null) startingRotation = transform.localRotation.eulerAngles;
              //  Vector2 delta = playerInput.GetMouseDelta();//ver como coger las 3 acciones
                startingRotation.x += delta.x * Time.deltaTime;
                startingRotation.y += delta.y * Time.deltaTime;
                startingRotation.y = Mathf.Clamp(startingRotation.y, -_clampAngle, _clampAngle);
                state.RawOrientation = Quaternion.Euler(-startingRotation.y, startingRotation.x, 0f);
            }
        }
    }
}
