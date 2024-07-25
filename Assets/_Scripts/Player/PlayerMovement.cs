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
	[SerializeField] private float coyoteTimeWindow = 0.15f;
	[SerializeField] private float jumpBufferWindow = 0.25f;
	[SerializeField] private float jumpCooldown = 0.15f;

	[Header("Wall Riding Variables")]
	[SerializeField] private float wallRideGravity = -2f;
	[SerializeField] private float wallRideDetectionRayDistance = 1.25f;
	[SerializeField] private float wallJumpDistance = 1.5f;
	[SerializeField] private float wallJumpHeight = 0.5f;
	[SerializeField] private float wallJumpConsumeRate = 4.5f;
	[SerializeField] private float wallJumpForceTime = 0.5f;
	[SerializeField] private float wallRideVerticalVelocityDamp = 1.25f;

	[Header("Sliding Variables")]
	[SerializeField] private float slideShrinkTimeInSeconds = 0.1f;
	[SerializeField] private float slideDurationTimeInSeconds;
	[SerializeField] private float slideVelocityCarryOverTimeInSeconds = 0.5f;
	[SerializeField] private float slideVelocityConsumeRate = 3.8f;
	[SerializeField] private float slideSpeed;
	[SerializeField, Range(0.8f, 2f)] private float slideHeight = 0.5f;
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
	private bool isSliding = false;
	private bool isWallRiding = false;
	private bool hasJumped = false;

	private Vector2 previousMoveInput;
	private Vector2 playerMoveInput;
	private Vector3 moveDirection;
	private Vector3 initialCameraPosition;
	private Vector3 initialGroundCheckPosition;

	private Vector3 slideVector;
	private Vector3 slideVectorCarry;
	private Vector3 wallJumpVector;

	private IEnumerator currentJumpCooldown;
	private IEnumerator currentCoyoteTimeWindow;
	private IEnumerator currentJumpBufferWindow;

	private IEnumerator currentSlideCooldown;
	private IEnumerator currentSlideSizeCoroutine;
	private IEnumerator currentSlideCarry;

	private IEnumerator currentWallJumpForce;

	private Collider currentWallRideCollider;
	private RaycastHit currentWallRideHitInfo;

	public Action OnSlideUnlocked;
	public Action<bool> OnStartWallRide;
	public Action OnStopWallRide;

	private bool previousGroundCheck = false;
	private bool isWallRidingLeft;

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
		WallRideCheck();
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

		if(currentSlideCooldown != null || context.phase != InputActionPhase.Performed || !grounded || isSliding) return;
		
		StartSliding();
	}

	public void JumpInput(InputAction.CallbackContext context){
        if (context.phase != InputActionPhase.Performed || currentJumpCooldown != null || !isSliding && characterController.height != standingHeight) return;

		if(isWallRiding && !hasJumped){
			PerformWallJump();
			return;
		}

        if (!grounded && currentCoyoteTimeWindow == null){
            StopJumpBufferWindow();
			currentJumpBufferWindow = JumpBufferCoroutine();
			StartCoroutine(currentJumpBufferWindow);
			return;
        }

        if (!grounded && currentCoyoteTimeWindow != null){
            StopCoyoteTimeWindow();
        }

		if(currentJumpBufferWindow != null){
			Debug.Log(jumpBufferWindow + " isn't null?");
			return;
		} 

        Jump();
    }

    private void Jump(){
		hasJumped = true;

        float height = jumpHeight;

        if (isSliding){
            height = slideJumpHeight;
        }

        verticalVelocity = Mathf.Sqrt(height * -2f * gravity);

        currentJumpCooldown = JumpCooldownCoroutine();
        StartCoroutine(currentJumpCooldown);

		if(isSliding){
			OnSlideFinished();
		}
    }

	private void PerformWallJump(){
		hasJumped = true;
		Vector3 wallJumpNormal = currentWallRideHitInfo.normal;
		Vector3 wallJumpForce = wallJumpNormal * wallJumpDistance;
		wallJumpVector = wallJumpForce;
		verticalVelocity = Mathf.Sqrt(wallJumpHeight * -2f * gravity);;

		StopWallJumpForce();
		StartWallJumpForce();
	}

	private void StopWallJumpForce(){
		if(currentWallJumpForce != null){
			StopCoroutine(currentWallJumpForce);
			currentWallJumpForce = null;
		} 
	}

	private void StartWallJumpForce(){
		currentWallJumpForce = WallJumpVectorCoroutine();
		StartCoroutine(currentWallJumpForce);
	}

    public void UnlockSlideAbility(){
		if(isSlideUnlocked) return;

		isSlideUnlocked = true;
		OnSlideUnlocked?.Invoke();
	}

	public Vector3 GetMoveDirection(){
		return moveDirection;
	}

	public bool IsMoving(){
		return moveDirection != Vector3.zero;
	}

	public bool IsGrounded(){
		return grounded;
	}

	public bool IsSliding(){
		return isSliding;
	}

	public bool IsWallRiding(){
		return isWallRiding;
	}

	private void WallRideCheck(){
		if(grounded){
			StopWallRiding();
			return;
		}

		RaycastHit hitInfo;
		//See if we are still on the wall if current wall ride collider isn't null
		if(currentWallRideCollider != null){
			if(Physics.Raycast(cameraTransform.position, -cameraTransform.right, out hitInfo, wallRideDetectionRayDistance, groundLayers) && hitInfo.collider == currentWallRideCollider || 
			   Physics.Raycast(cameraTransform.position, cameraTransform.right, out hitInfo, wallRideDetectionRayDistance, groundLayers) && hitInfo.collider == currentWallRideCollider) 
			   return;

			StopWallRiding();
		}

		if(isWallRiding) return;
        
		//See if there is a wall to our left
        if (Physics.Raycast(cameraTransform.position, -cameraTransform.right, out hitInfo, wallRideDetectionRayDistance, groundLayers)){
			currentWallRideHitInfo = hitInfo;
			StartWallRiding(currentWallRideHitInfo.collider, true);
        }

        //See if there is a wall to our right
        if (Physics.Raycast(cameraTransform.position, cameraTransform.right, out hitInfo, wallRideDetectionRayDistance, groundLayers)){
			currentWallRideHitInfo = hitInfo;
			StartWallRiding(currentWallRideHitInfo.collider, false);
		}
    }

    private void StopWallRiding(){
		if(currentWallRideCollider != null){
			isWallRiding = false;
        	currentWallRideCollider = null;
			OnStopWallRide?.Invoke();
		}
    }

    private void StartWallRiding(Collider wallCollider, bool isLeftWall){
		StopWallRiding();
		verticalVelocity /= wallRideVerticalVelocityDamp; //damp our vertical velocity when starting to ride a wall
		isWallRiding = true;

		if(hasJumped) hasJumped = false;
		
		currentWallRideCollider = wallCollider;
		isWallRidingLeft = isLeftWall;
		OnStartWallRide?.Invoke(isLeftWall);
    }

    private void GroundCheck(){
		grounded = Physics.CheckSphere(groundCheckTransform.position, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
		if(previousGroundCheck && !grounded && !hasJumped){
			//left the ground start the coyote time window
			currentCoyoteTimeWindow = CoyoteTimeWindowCoroutine();
			StartCoroutine(CoyoteTimeWindowCoroutine());
		}

		if(!previousGroundCheck && grounded && currentCoyoteTimeWindow != null){
			//We are grounded stop the coyote time window
			StopCoyoteTimeWindow();
		}

		if(grounded && hasJumped) hasJumped = false;

		previousGroundCheck = grounded;
    }

	private void Gravity(){
        if(grounded){
			verticalVelocity = 0f;
			return;
		}

		if(verticalVelocity > terminalVelocity){
			float currentGravity = isWallRiding ? wallRideGravity : gravity;
			verticalVelocity += currentGravity * Time.deltaTime;
		}
    }

    private void StartSliding(){
		isSliding = true;
		currentSlideSizeCoroutine = SlideLerpAnimationCoroutine(slideHeight);
		StartCoroutine(currentSlideSizeCoroutine);

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

		Vector3 velocity = isSliding ? slideVector : movementSpeed * moveDirection.normalized;

		velocity += slideVectorCarry;
		velocity += wallJumpVector;

		velocity = AdjustMovementVectorToSlope(velocity);
		velocity.y += verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
		previousMoveInput = playerMoveInput;
    }

    private void OnSlideFinished(){
		isSliding = false;
		StopCurrentSlideSizeCoroutine();

		if(characterController.height != standingHeight){
			StartStandingCheck();
		}

		StartSlideMomentum();
	}

    private void StartStandingCheck(){
        currentSlideSizeCoroutine = ValidStandingCheckCoroutine();
        StartCoroutine(currentSlideSizeCoroutine);
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

	private void StartSlideMomentum(){
        StopSlideMomentumCoroutine();

        currentSlideCarry = SlideVectorCarryCoroutine();
        StartCoroutine(currentSlideCarry);
    }

    private void StopSlideMomentumCoroutine(){
        if (currentSlideCarry != null){
            StopCoroutine(currentSlideCarry);
            currentSlideCarry = null;
        }
    }

	private void StopCurrentSlideSizeCoroutine(){
		if(currentSlideSizeCoroutine != null){
			StopCoroutine(currentSlideSizeCoroutine);
			currentSlideSizeCoroutine = null;
		}
	}

	private Vector3 AdjustMovementVectorToSlope(Vector3 velocity){
		Ray slopeDetectionRay = new Ray(transform.position, Vector3.down);

		float slopeDetectionDistance = isSliding ? slopeRaySlidingDetectDistance : slopeRayStandingDetectDistance;

		if(!Physics.Raycast(slopeDetectionRay, out RaycastHit hitInfo, slopeDetectionDistance, groundLayers)) return velocity;

		Vector3 adjustedVelocity = Vector3.ProjectOnPlane(velocity, hitInfo.normal);

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

		currentSlideSizeCoroutine = SlideLerpAnimationCoroutine(standingHeight);
		StartCoroutine(currentSlideSizeCoroutine);
	}

	private IEnumerator SlideLerpAnimationCoroutine(float desiredHeight){
		float current = 0;

		while(Mathf.Abs(characterController.height - desiredHeight) > slideSnapDistance){
			characterController.height = Mathf.Lerp(characterController.height, desiredHeight, current / slideShrinkTimeInSeconds);

			var halfHeightDifference = new Vector3(0, (standingHeight - characterController.height) / 2, 0);
			var newCameraPos = initialCameraPosition - halfHeightDifference;

			cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newCameraPos, current / slideShrinkTimeInSeconds) ;

			var newGroundCheckPos = initialGroundCheckPosition + halfHeightDifference;
			groundCheckTransform.localPosition = newGroundCheckPos;

			current += Time.deltaTime;
			yield return null;
		}

		characterController.height = desiredHeight;
		if(desiredHeight == standingHeight){
			cameraTransform.localPosition = initialCameraPosition;
			groundCheckTransform.localPosition = initialGroundCheckPosition;
		}

		currentSlideSizeCoroutine = null;
	}

    private IEnumerator SlideMovementCoroutine(){
        slideVectorCarry = Vector3.zero;

        float elapsedTime = 0f;
        while (elapsedTime < slideDurationTimeInSeconds){
            slideVector = cameraTransform.forward;
            slideVector *= slideSpeed;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

		OnSlideFinished();
    }

    private IEnumerator SlideVectorCarryCoroutine(){
		float elapsedTime = 0f;
		slideVectorCarry += slideVector.normalized * slideSpeed;

		while(elapsedTime < slideVelocityCarryOverTimeInSeconds){
			slideVectorCarry = Vector3.Lerp(slideVectorCarry, Vector3.zero, slideVelocityConsumeRate * Time.deltaTime);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		
		slideVectorCarry = Vector3.zero;
		slideVector = Vector3.zero;
	}

	private IEnumerator WallJumpVectorCoroutine(){
		float elapsedTime = 0f;
		while(elapsedTime < wallJumpForceTime){
			wallJumpVector = Vector3.Lerp(wallJumpVector, Vector3.zero, wallJumpConsumeRate * Time.deltaTime);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		
		wallJumpVector = Vector3.zero;
		StopWallJumpForce();
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

		//Wall ride rays
		Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + -cameraTransform.right * wallRideDetectionRayDistance);
		Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + cameraTransform.right * wallRideDetectionRayDistance);

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