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
	[SerializeField] private float slopeRayStandingDetectDistance = 1.05f;
	[SerializeField] private float slopeRaySlidingDetectDistance = 0.5f;

	[Header("Jumping Variables")]
	[SerializeField] private float jumpHeight = 0.3f;
	[SerializeField] private float slideJumpHeight = 0.75f;
	[SerializeField] private float slideJumpWindow = 0.5f;
	[SerializeField] private float coyoteTimeWindow = 0.15f;
	[SerializeField] private float jumpBufferWindow = 0.25f;
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

	private Vector3 slideVector;

	private IEnumerator currentJumpCooldown;
	private IEnumerator currentCoyoteTimeWindow;
	private IEnumerator currentJumpBufferWindow;
	private IEnumerator currentSlideJumpWindow;
	private IEnumerator currentSlideCooldown;
	private IEnumerator currentSlideAction;

	public Action OnSlideUnlocked;

	private bool previousGroundCheck = false;

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
		if(currentSlideAction != null) return;
		
		StartSliding();
	}

	public void JumpInput(InputAction.CallbackContext context){
        if (context.phase != InputActionPhase.Performed || currentJumpCooldown != null || isSliding) return;

        if (!grounded && currentCoyoteTimeWindow == null){
            StopJumpBufferWindow();
			currentJumpBufferWindow = JumpBufferCoroutine();
			StartCoroutine(currentJumpBufferWindow);
			return;
        }

        if (!grounded && currentCoyoteTimeWindow != null){
            StopCoyoteTimeWindow();
        }

		if(currentJumpBufferWindow != null) return;

        Jump();
    }

    private void Jump(){
        float height = jumpHeight;

        if (currentSlideJumpWindow != null){
            height = slideJumpHeight;
            StopCoroutine(currentSlideJumpWindow);
            currentSlideJumpWindow = null;
        }

        verticalVelocity = Mathf.Sqrt(height * -2f * gravity);

        currentJumpCooldown = JumpCooldownCoroutine();
        StartCoroutine(currentJumpCooldown);
    }

    public void UnlockSlideAbility(){
		if(isSlideUnlocked) return;

		isSlideUnlocked = true;
		OnSlideUnlocked?.Invoke();
	}

	public bool IsMoving(){
		return moveDirection != Vector3.zero;
	}

	public bool IsGrounded(){
		return grounded;
	}

	private void GroundCheck(){
		grounded = Physics.CheckSphere(groundCheckTransform.position, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
		if(previousGroundCheck && !grounded){
			//left the ground start the coyote time window
			currentCoyoteTimeWindow = CoyoteTimeWindowCoroutine();
			StartCoroutine(CoyoteTimeWindowCoroutine());
		}

		if(!previousGroundCheck && grounded && currentCoyoteTimeWindow != null){
			//We are grounded stop the coyote time window
			StopCoyoteTimeWindow();
		}

		previousGroundCheck = grounded;
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

		Vector3 velocity = movementSpeed * moveDirection.normalized;

		velocity += slideVector;

		velocity = AdjustMovementVectorToSlope(velocity);
		velocity.y += verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
		previousMoveInput = playerMoveInput;
    }

    private void OnSlideShrinkFinished(){
		if(currentSlideAction == null && characterController.height != standingHeight){			
			currentSlideAction = ValidStandingCheckCoroutine();
			StartCoroutine(currentSlideAction);
		}
		else{
			isSliding = false;
			currentSlideJumpWindow = SlideJumpWindowCoroutine();
			StartCoroutine(currentSlideJumpWindow);
		}
    }

	private void StopJumpBufferWindow(){
		if(currentJumpBufferWindow != null){
			StopCoroutine(currentJumpBufferWindow);
			currentJumpBufferWindow = null;
		}
	}

	private void StopCoyoteTimeWindow(){
		if(currentCoyoteTimeWindow != null){
			StopCoroutine(currentCoyoteTimeWindow);
			currentCoyoteTimeWindow = null;
		}
	}

	private Vector3 AdjustMovementVectorToSlope(Vector3 velocity){
		Ray slopeDetectionRay = new Ray(transform.position, Vector3.down);

		float slopeDetectionDistance = isSliding ? slopeRaySlidingDetectDistance : slopeRayStandingDetectDistance;

		if(!Physics.Raycast(slopeDetectionRay, out RaycastHit hitInfo, slopeDetectionDistance, groundLayers)) return velocity;

		Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
		Vector3 adjustedVelocity = slopeRotation * velocity;

		if(adjustedVelocity.y < 0) return adjustedVelocity;
		return velocity;
	}

	private IEnumerator JumpBufferCoroutine(){
		float currentCheckTime = 0;

		while(currentCheckTime <= jumpBufferWindow){
			if(grounded){
				Jump();
				StopJumpBufferWindow();
				yield break;
			}
			currentCheckTime += Time.deltaTime;
			yield return null;
		}

		StopJumpBufferWindow();
	}

	private IEnumerator ValidStandingCheckCoroutine(){
		while(Physics.SphereCast(transform.position, characterController.radius,  Vector3.up, out RaycastHit hitInfo, standingHeight)){
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

    private IEnumerator SlideMovementCoroutine(){
		float elapsedTime = 0f;
    	while (elapsedTime < slideDurationTimeInSeconds){
			slideVector = cameraTransform.forward;
			slideVector *= slideSpeed;
        	elapsedTime += Time.deltaTime;
        	yield return null;
    	}

		slideVector = Vector3.zero;
	}

	private IEnumerator SlideJumpWindowCoroutine(){
		yield return new WaitForSeconds(slideJumpWindow);
		currentSlideJumpWindow = null;
	}

	private IEnumerator CoyoteTimeWindowCoroutine(){
		yield return new WaitForSeconds(coyoteTimeWindow);
		currentCoyoteTimeWindow = null;
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

		float slopeDetectionDistance = isSliding ? slopeRaySlidingDetectDistance : slopeRayStandingDetectDistance;

		Gizmos.DrawLine(transform.position, transform.position + Vector3.down * slopeDetectionDistance);

		Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
		Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

		if (grounded) Gizmos.color = transparentGreen;
		else Gizmos.color = transparentRed;

		Gizmos.DrawSphere(groundCheckTransform.position, groundedRadius);

		if(Physics.SphereCast(transform.position, characterController.radius, Vector3.up, out RaycastHit hitInfo, standingHeight)){
			Gizmos.color = transparentRed;
		}
		else{
			Gizmos.color = transparentGreen;
		}
		Gizmos.DrawWireSphere(transform.position + Vector3.up, characterController.radius);
	}
}