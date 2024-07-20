using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour{
	[Header("Required References")]
	[SerializeField] private CharacterController characterController;
	[SerializeField] private Transform playerOrientation;
	[SerializeField] private Transform cameraTransform;
	[SerializeField] private Transform groundCheckTransform;
	[SerializeField] private LayerMask groundLayers;	

	[Header("Base Movement Variables")]
	[SerializeField] private float movementSpeed;

	[Header("Jumping Variables")]
	[SerializeField] private float jumpHeight = 0.3f;
	[SerializeField] private float slideJumpHeight = 0.75f;
	[SerializeField] private float slideJumpWindow = 0.5f;
	[SerializeField] private float jumpCooldown = 0.15f;

	[Header("Sliding Variables")]
	[SerializeField] private float slideShrinkTimeInSeconds = 0.1f;
	[SerializeField] private float slideDurationTimeInSeconds;
	[SerializeField] private float slideSpeed;
	[SerializeField] private float slideHeight = 0.5f;
	[SerializeField] private float slideCooldown = 0.5f;

	[Header("Gravity Variables")]
	[SerializeField] private float gravity = -9.31f;
	[SerializeField] private float groundedRadius = 0.5f;

	[Header("Ability Variables")]
	[SerializeField] private bool isSlideUnlocked = false;

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
	private const float slideSnapDistance = 0.1f;

	private float xInput;
	private float yInput;
	private float verticalVelocity;
	private float standingHeight;

	private bool grounded;
	private bool isSliding;

	private Vector2 previousMoveInput;
	private Vector2 playerMoveInput;
	private Vector3 moveDirection;
	private Vector3 initialCameraPosition;
	private Vector3 initialGroundCheckPosition;

	private IEnumerator currentJumpCooldown;
	private IEnumerator currentSlideJumpWindow;
	private IEnumerator currentSlideCooldown;
	private IEnumerator currentSlideAction;

	public Action OnSlideUnlocked;

	private void Start() {
		standingHeight = characterController.height;
		initialCameraPosition = cameraTransform.localPosition;
		initialGroundCheckPosition = groundCheckTransform.localPosition;
	}

	private void OnDestroy() {
		StopAllCoroutines();
	}

	private void Update() {
		Move();
		GroundCheck();
        Gravity();
	}

    public void MoveInput(InputAction.CallbackContext context){
		xInput = context.ReadValue<Vector2>().x;
		yInput = context.ReadValue<Vector2>().y;

		playerMoveInput.x = xInput;
		playerMoveInput.y = yInput;
	}

	public void SlideInput(InputAction.CallbackContext context){
		if(!isSlideUnlocked) return;

		if(currentSlideCooldown != null || context.phase != InputActionPhase.Performed || !grounded) return;
		if(currentSlideAction == null){
			StartSliding();
		}
	}

	public void JumpInput(InputAction.CallbackContext context){
		if(context.phase != InputActionPhase.Performed || currentJumpCooldown != null || !grounded || isSliding) return;

		float height = jumpHeight;

		if(currentSlideJumpWindow != null){
			height = slideJumpHeight;
			StopCoroutine(currentSlideJumpWindow);
			currentSlideJumpWindow = null;
		}

		verticalVelocity = Mathf.Sqrt(height * -2f * gravity);

		currentJumpCooldown = JumpCooldownCoroutine();
		StartCoroutine(currentJumpCooldown);
	}

	private void GroundCheck(){
		grounded = Physics.CheckSphere(groundCheckTransform.position, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

	private void Gravity(){
        if(grounded){
			verticalVelocity = 0f;
			return;
		}

		if(verticalVelocity > terminalVelocity){
			verticalVelocity += gravity * Time.deltaTime;
		}
    }

    private void StartSliding(){
		isSliding = true;
		currentSlideAction = SlideLerpAnimationCoroutine(slideHeight);
		StartCoroutine(currentSlideAction);

		AddSlidingForce();
		
		currentSlideCooldown = SlideCooldownCoroutine();
		StartCoroutine(currentSlideCooldown);
    }

    private void AddSlidingForce(){
		StartCoroutine(SlideMovementCoroutine());
    }

    private void Move(){
        moveDirection = playerOrientation.forward * yInput + playerOrientation.right * xInput;
		if(moveDirection == Vector3.zero){
			OnPlayerMovementStopped?.Invoke(this, EventArgs.Empty);
		}

		if(playerMoveInput != previousMoveInput){
			OnPlayerMovementDirectionChanged?.Invoke(this, new PlayerMovementDirectionChangedEventArgs(playerMoveInput));
		}

        characterController.Move(movementSpeed * Time.deltaTime * moveDirection.normalized + Time.deltaTime * verticalVelocity * Vector3.up);
		previousMoveInput = playerMoveInput;
    }

	public void UnlockSlideAbility(){
		if(isSlideUnlocked) return;

		isSlideUnlocked = true;
		OnSlideUnlocked?.Invoke();
	}

	public bool IsMoving(){
		return moveDirection != Vector3.zero;
	}

	private IEnumerator ValidStandingCheckCoroutine(){
		while(Physics.Raycast(transform.position, Vector3.up, standingHeight)){
			yield return null;
		}

		currentSlideAction = SlideLerpAnimationCoroutine(standingHeight);
		StartCoroutine(currentSlideAction);
	}

	private IEnumerator SlideLerpAnimationCoroutine(float desiredHeight){
		float current = 0;
		float initialYPosition = transform.position.y;
		float targetYPosition = initialYPosition - (standingHeight - desiredHeight) / 2;

		while(Mathf.Abs(characterController.height - desiredHeight) > slideSnapDistance){
			characterController.height = Mathf.Lerp(characterController.height, desiredHeight, current / slideShrinkTimeInSeconds);

			var halfHeightDifference = new Vector3(0, (standingHeight - characterController.height) / 2, 0);
			var newCameraPos = initialCameraPosition - halfHeightDifference;

			cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newCameraPos, current / slideShrinkTimeInSeconds) ;

			var newGroundCheckPos = initialGroundCheckPosition + halfHeightDifference;
			groundCheckTransform.localPosition = newGroundCheckPos;

			float newYPosition = Mathf.Lerp(initialYPosition, targetYPosition, current / slideShrinkTimeInSeconds);
    		verticalVelocity = (newYPosition - transform.position.y) / Time.deltaTime;

			current += Time.deltaTime;
			yield return null;
		}

		characterController.height = desiredHeight;
		if(desiredHeight == standingHeight){
			cameraTransform.localPosition = initialCameraPosition;
			groundCheckTransform.localPosition = initialGroundCheckPosition;
		}

		currentSlideAction = null;
		OnSlideShrinkFinished();
	}

    private void OnSlideShrinkFinished(){
		if(currentSlideAction == null && characterController.height != standingHeight){
			currentSlideAction = SlideLerpAnimationCoroutine(standingHeight);
			StartCoroutine(currentSlideAction);
		}
		else{
			isSliding = false;
			currentSlideJumpWindow = SlideJumpWindowCoroutine();
			StartCoroutine(currentSlideJumpWindow);
		}
    }

    private IEnumerator SlideMovementCoroutine(){
		float elapsedTime = 0f;
    	while (elapsedTime < slideDurationTimeInSeconds){
			Vector3 slideDirection = cameraTransform.forward;
        	characterController.Move(slideSpeed * Time.deltaTime * slideDirection);
        	elapsedTime += Time.deltaTime;
        	yield return null;
    	}
	}

	private IEnumerator SlideJumpWindowCoroutine(){
		yield return new WaitForSeconds(slideJumpWindow);
		currentSlideJumpWindow = null;
	}

	private IEnumerator JumpCooldownCoroutine(){
		yield return new WaitForSeconds(jumpCooldown);
		currentJumpCooldown = null;
	}

    private IEnumerator SlideCooldownCoroutine(){
		yield return new WaitForSeconds(slideCooldown);
		currentSlideCooldown = null;
	}

	private void OnDrawGizmosSelected() {
		if(!drawGizmos) return;

		Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
		Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

		if (grounded) Gizmos.color = transparentGreen;
		else Gizmos.color = transparentRed;

		Gizmos.DrawSphere(groundCheckTransform.position, groundedRadius);
		Gizmos.DrawRay(transform.position, Vector3.up * standingHeight);
	}
}
