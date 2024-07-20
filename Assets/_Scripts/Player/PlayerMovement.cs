using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private CharacterController characterController;
	[SerializeField] private Transform playerOrientation;
	[SerializeField] private Transform cameraTransform;
	[SerializeField] private Transform groundCheckTransform;
	[SerializeField] private LayerMask groundLayers;	

	[Header("Base Movement Variables")]
	[SerializeField] private float movementSpeed;
	[SerializeField] private float slideDistance;
	[SerializeField] private float slideHeight = 0.5f;

	[Header("Gravity Variables")]
	[SerializeField] private float gravity = -9.31f;
	[SerializeField] private float groundedRadius = 0.5f;

	[Header("Debug Variables")]
	[SerializeField] private bool drawGizmos = false;

	public EventHandler<PlayerMovementDirectionChangedEventArgs> OnPlayerMovementDirectionChanged;
	public class PlayerMovementDirectionChangedEventArgs : EventArgs{
		public Vector3 rawDirection;
		public PlayerMovementDirectionChangedEventArgs(Vector3 _rawDirection){
			rawDirection = _rawDirection;
		}
	}

	public EventHandler OnPlayerMovementStopped;

	private const float terminalVelocity = -53f;

	private float xInput;
	private float yInput;
	private float verticalVelocity;
	private float currentSpeed;
	private float standingHeight;

	private bool grounded;

	private Vector2 previousMoveInput;
	private Vector2 playerMoveInput;
	private Vector3 moveDirection;
	private Vector3 initialCameraPosition;
	private Vector3 initialGroundCheckPosition;

	private IEnumerator currentSlideCooldown;

	private void Start() {
		standingHeight = characterController.height;
		initialCameraPosition = cameraTransform.localPosition;
		initialGroundCheckPosition = groundCheckTransform.localPosition;
	}

	private void OnDestroy() {
		StopAllCoroutines();
	}

	private void Update() {
		
	}

	//  public void MoveInput(InputAction.CallbackContext context){
	// 	xInput = context.ReadValue<Vector2>().x;
	// 	yInput = context.ReadValue<Vector2>().y;

	// 	playerMoveInput.x = xInput;
	// 	playerMoveInput.y = yInput;
	// }
}
