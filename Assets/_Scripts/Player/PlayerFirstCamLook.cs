using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System;

public class PlayerFirstCamLook : MonoBehaviour{
	[Header("Cam Variables")]
	[SerializeField, Range(0.1f, 100)] private float camSens;
	[SerializeField] private bool lockCursor;

	[Header("Tilt Variables")]
	[SerializeField] private float tiltTime = 0.25f;
	[SerializeField] private float playerMovementTiltAmount = 2f;
	[SerializeField] private float wallRideTiltAmount = 4f;

	[Header("Headbob Variables")]
	[SerializeField, Range(0, 0.1f)] private float bobbingAmplitude = 0.003f;
	[SerializeField, Range(0, 30f)] private float bobbingFrequency = 7.0f;

	[SerializeField, Range(1f, 5f)] private float headBobResetSpeed = 3f;

	[Header("Required Reference")]
	[SerializeField] private Transform cameraRoot;
	[SerializeField] private Transform cameraTransform;
	[SerializeField] private Transform characterOrientation;

	private const float YClamp = 80f;
	private const string MOUSE_SENS = "MouseSens";

	private float lastSinValue;
	private bool isMovingUpwards = true;

	private float camX;
	private float camY;
	private float currentAmplitude;
	private float currentFrequency;

	private Vector3 startPos;
	private Vector3 currentMovementVectorNormalized;
	private Vector3 currentTiltVector;

	private PlayerMovement playerMovement;

	// public EventHandler<TerrainStepEventArgs> OnTerrainStep; 
	// public class TerrainStepEventArgs : EventArgs{
	// 	public TerrainType terrainType;
	// 	public TerrainStepEventArgs(TerrainType _terrainType){
	// 		terrainType = _terrainType;
	// 	}
	// }

	private void Awake(){
        startPos = cameraTransform.localPosition;
        currentAmplitude = bobbingAmplitude;
        currentFrequency = bobbingFrequency;

        UpdateCameraSens();
    }

    public void UpdateCameraSens(){
        if (PlayerPrefs.HasKey(MOUSE_SENS)){
            camSens = PlayerPrefs.GetFloat(MOUSE_SENS);
        }
        else{
            camSens = 10f;
        }
    }

    private void Start() {
		if(TryGetComponent(out playerMovement)){
			playerMovement.OnPlayerMovementDirectionChanged += UpdateCameraTiltOnPlayerMovementDir;
			playerMovement.OnPlayerMovementStopped += ResetCameraTilt;
			playerMovement.OnStartWallRide += StartWallTilt;
			playerMovement.OnStopWallRide += StopWallTilt;
		}

		Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
	}

	private void OnDestroy() {
		if(playerMovement != null){
			playerMovement.OnPlayerMovementDirectionChanged -= UpdateCameraTiltOnPlayerMovementDir;
			playerMovement.OnPlayerMovementStopped -= ResetCameraTilt;
			playerMovement.OnStartWallRide -= StartWallTilt;
			playerMovement.OnStopWallRide -= StopWallTilt;
		}
	}

    private void StartWallTilt(bool isLeft){
		ResetCameraTilt(null, EventArgs.Empty);
		
		Vector3 wallRideTiltVector = isLeft ? -transform.right : transform.right;

		ApplyCameraTilt(wallRideTiltVector);
    }

    private void StopWallTilt(){
		ResetCameraTilt(null, EventArgs.Empty);
		ApplyCameraTilt(playerMovement.GetMoveDirection());
	}

	public void Update(){
		cameraRoot.localRotation = Quaternion.Euler(camY, camX, 0);

		characterOrientation.transform.localRotation = Quaternion.Euler(0, camX, 0);

		CheckMotion();
		ResetPosition();
	}

	public void OnLookInput(InputAction.CallbackContext context){
		var inputVector = context.ReadValue<Vector2>();
		camX += inputVector.x * camSens * Time.deltaTime;
		camY -= inputVector.y * camSens * Time.deltaTime;
		camY = Mathf.Clamp(camY, -YClamp, YClamp);
	}

	public void LookInputInjected(Vector2 inputVector){
		camX += inputVector.x;
		camY -= inputVector.y;
		camY = Mathf.Clamp(camY, -YClamp, YClamp);
	}

	private void CheckMotion(){
		if(!playerMovement.IsMoving() || !playerMovement.IsGrounded() || playerMovement.IsSliding() || playerMovement.IsWallRiding()) return;
		PlayMotion(StepMotionCalculation());
	}

	private void PlayMotion(Vector3 motion){
		cameraTransform.localPosition += motion;
	}

	private void ResetPosition(){
		if (cameraTransform.localPosition == startPos) return;
		cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, startPos, headBobResetSpeed * Time.deltaTime);
	}

	private Vector3 StepMotionCalculation(){
		Vector3 pos = Vector3.zero;
		pos.y += Mathf.Sin(Time.time * currentFrequency) * currentAmplitude;
		pos.x += Mathf.Cos(Time.time * currentFrequency / 2) * currentAmplitude * 2;
		
		if ((pos.y > 0 && !isMovingUpwards) || (pos.y < 0 && isMovingUpwards)){
            // Footstep sound should play at the peak of the sin wave
            // if (lastSinValue < 0 && pos.y >= 0){
            //     OnTerrainStep?.Invoke(this, new TerrainStepEventArgs(playerMovement.GetCurrentTerrainType()));
            // }
            isMovingUpwards = pos.y >= 0;
        }
		
		lastSinValue = pos.y;
		return pos;
	}

    private void ResetCameraTilt(object sender, EventArgs e){
		if(playerMovement.IsWallRiding()) return;
		
		cameraTransform.DOLocalRotate(Vector3.zero, tiltTime);
    }

    private void UpdateCameraTiltOnPlayerMovementDir(object sender, PlayerMovement.PlayerMovementDirectionChangedEventArgs e){
        if (playerMovement.IsWallRiding()) return;
        ApplyCameraTilt(e.rawDirection);
    }

    private void ApplyCameraTilt(Vector3 rawDirection){
        currentMovementVectorNormalized = rawDirection.normalized;

		float tiltAmount = playerMovement.IsWallRiding() ? wallRideTiltAmount : playerMovementTiltAmount;

        Vector3 tiltDirection = currentMovementVectorNormalized * tiltAmount;

        currentTiltVector = new Vector3(tiltDirection.y, 0, -tiltDirection.x);

        cameraTransform.DOLocalRotate(currentTiltVector, tiltTime);
    }
}