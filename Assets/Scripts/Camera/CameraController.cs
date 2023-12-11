using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{

    public static CameraController Instance { get; private set; }


    private const float MIN_FOLLOW_Y_OFFSET = 2f;
    private const float MAX_FOLLOW_Y_OFFSET = 200f;


    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    private CinemachineTransposer cinemachineTransposer;
    private Vector3 targetFollowOffset;
    private const float POSITION_CLOSENESS_THRESHOLD = 0.5f; // Threshold for position closeness in units

    private float translationSpeed = 15f; // Speed of moving towards the target

    private Vector3? lookAtPoint; // Target point to look at


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one CameraController! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        targetFollowOffset = cinemachineTransposer.m_FollowOffset;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();
        HandleLookAt();
    }

    private void HandleMovement()
    {
        Vector2 inputMoveDir = InputManager.Instance.GetCameraMoveVector();

        float moveSpeed = 10f;

        Vector3 moveVector = transform.forward * inputMoveDir.y + transform.right * inputMoveDir.x;
        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }

    private void HandleRotation()
    {
        Vector3 rotationVector = new Vector3(0, 0, 0);

        rotationVector.y = InputManager.Instance.GetCameraRotateAmount();

        float rotationSpeed = 100f;
        transform.eulerAngles += rotationVector * rotationSpeed * Time.deltaTime;
    }

    private void HandleZoom()
    {
        float zoomIncreaseAmount = 1f;
        targetFollowOffset.y += InputManager.Instance.GetCameraZoomAmount() * zoomIncreaseAmount;

        targetFollowOffset.y = Mathf.Clamp(targetFollowOffset.y, MIN_FOLLOW_Y_OFFSET, MAX_FOLLOW_Y_OFFSET);

        float zoomSpeed = 10f;
        cinemachineTransposer.m_FollowOffset =
            Vector3.Lerp(cinemachineTransposer.m_FollowOffset, targetFollowOffset, Time.deltaTime * zoomSpeed);
    }

    public void SetlookAtPoint(Vector3 point)
    {
        lookAtPoint = point;
    }

    private void HandleLookAt()
    {
        if (lookAtPoint.HasValue)
        {
            // Move the camera towards the lookAtPoint
            transform.position = Vector3.MoveTowards(transform.position, lookAtPoint.Value, translationSpeed * Time.deltaTime);

            // Check if the camera is close enough to the target position and rotation
            if (Vector3.Distance(transform.position, lookAtPoint.Value) < POSITION_CLOSENESS_THRESHOLD)
            {
                lookAtPoint = null; // Reset lookAtPoint if the camera is close enough in both position and rotation
            }
        }
    }


}