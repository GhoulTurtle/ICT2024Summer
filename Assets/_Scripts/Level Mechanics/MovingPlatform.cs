using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour{
    [Header("Required References")]
    [SerializeField] private Transform pointATransform;
    [SerializeField] private Transform pointBTransform;

	[Header("Moving Platform Variables")]
    [Tooltip("Speed that the platform will move from point A to point B.")]
    [SerializeField, Range(0.1f, 3f)] private float movementSpeed = 2f;

    [Tooltip("Delay before the platform moves from point A to B.")]
    [SerializeField, Range(0f, 60f)] private float pointADelay = 2f;
    
    [Tooltip("Delay before the platform moves from point B to A.")]
    [SerializeField, Range(0f, 60f)] private float pointBDelay = 2f;

    [SerializeField] private AnimationCurve platformAnimationCurve;

    [Header("Debugging Variables")]
    [SerializeField] private Color gizmosColor = Color.green;
    [SerializeField] private bool drawGizmos = true;

    private Transform currentPoint;

    private const string PLAYER = "Player";

    private CharacterController playerCharacterController;
    private bool isPlayerOnPlatform = false;

    Vector3 platformMovementDelta;
    Vector3 lastPlatformPosition;

    private void Awake() {
        currentPoint = pointATransform;
        transform.position = pointATransform.position;
    }

    private void Start() {
       StartCoroutine(MovementDelay());
    }

    private void OnDestroy() {
        StopAllCoroutines();
    }

    private IEnumerator MovementDelay(){
        var timer = currentPoint == pointATransform ? new WaitForSeconds(pointADelay) : new WaitForSeconds(pointBDelay);
        var nextPoint = currentPoint == pointATransform ? pointBTransform : pointATransform;
        
        yield return timer;
        StartCoroutine(MoveToPosition(nextPoint));
    }

    private IEnumerator MoveToPosition(Transform positionTransform) {
        float t = 0f;
        
        while (t < 1) {
            t += Time.deltaTime * movementSpeed;
            transform.position = Vector3.Lerp(currentPoint.position, positionTransform.position, t * platformAnimationCurve.Evaluate(t));
            
            platformMovementDelta = transform.position - lastPlatformPosition;

            if(playerCharacterController != null && isPlayerOnPlatform){
                playerCharacterController.Move(platformMovementDelta);
            }

            lastPlatformPosition = transform.position;

            yield return null;
        }

        currentPoint = positionTransform; 
        transform.position = positionTransform.position;

        StartCoroutine(MovementDelay());
    }

    private void OnTriggerEnter(Collider other) {
        if(!other.CompareTag(PLAYER)) return;

        if(playerCharacterController == null){
            other.TryGetComponent(out playerCharacterController);
        }        

        isPlayerOnPlatform = true;
    }

    private void OnTriggerExit(Collider other) {
        if(!other.CompareTag(PLAYER)) return;

        isPlayerOnPlatform = false;
    }

    private void OnDrawGizmos() {
        if(!drawGizmos) return;

        Gizmos.color = gizmosColor;

        if(pointATransform != null){
            Gizmos.DrawWireCube(pointATransform.position, transform.localScale);
        }

        if(pointBTransform != null){
            Gizmos.DrawWireCube(pointBTransform.position, transform.localScale);
        }
    }
}