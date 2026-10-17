using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.iOS;

public class PlayerController : MonoBehaviour
{

    [SerializeField] Transform cameraPivot;
    enum PlayerState
    {
        Walking,
        AligningToCenter,
        WaitingForTurnInput,
        Turning,
        Stumbling,
        Avoiding,
        Stuck
    }
    #region Movement Ref

    [Header("Statistics")]

    [SerializeField] public float stepSize = 1f;


    [Header("Transform References")]


    [Header("Walking Bob")]
    [SerializeField] float stepHeight = 0.08f;          // Bobbing height

    [SerializeField] float stepDuration = 0.25f;        // Step Speed

    [Header("Step Drift")]
    [SerializeField] float lateralStepOffset = 0.5f;    // Drifting from center length


    private float targetLateralOffset = 0f;

    private Vector3 baseWorldPos;                       // Player Position
    private float stepTimer;                            // For lerping movement per step
    private Vector3 stepStartPos;
    private Vector3 stepEndPos;

    private Vector3 moveDirection;                      // Check Player's facing direction

    enum StepFoot                                       // Player Stepping State
    {
        None,
        Left,
        Right
    }

    private StepFoot lastStep = StepFoot.None;          // Track Player's Last Step
    private bool isStepping = false;

    #endregion

    #region Avoid Ref
    [Header("Avoid Stats")]
    [SerializeField] private float avoidDuration = 0.25f;
    [SerializeField] private float _avoidCooldownDuration = 1f;
    [SerializeField] private float avoidStepDistance = 1f;
    [SerializeField] private float avoidCameraLift = 0.5f;

    private float avoidTimer;
    private bool obstacleInAvoidTrigger;
    private bool isAvoiding = false;

    private Vector3 _avoidStartPos;
    private Vector3 _cameraStartLocalPos;

    private bool _avoidAvailable = true;             // Check if Avoid is available to be used again
    public bool avoidActivate = false;               // Activate Avoid
    public float avoidDistance = 0;                  // The distance that avoid will be active

    #endregion

    #region Turning Ref

    #region Stuck Ref

    private int stuckPressesRemaining;
    private bool isStuck = false;

    #endregion


    private CenterAnchor currentAnchor;

    private PlayerState state;
    [Header("Turning Stats")]
    [SerializeField] float snapSpeed = 6f;
    [SerializeField] UIController UIController;
    [SerializeField] float turnDuration = 0.25f;
    private bool isTurning = false;

    #endregion
    bool isStumbling;



    void Awake()
    {
        baseWorldPos = transform.position;
        moveDirection = transform.forward;
        state = PlayerState.Walking;

        StartCoroutine(WarmUp());

    }
        private PlayerState _lastState;
    void Update()
    {

        if (state != _lastState)
        {
            Debug.Log($"PlayerState changed: {_lastState} → {state}");
            _lastState = state;
        }

        switch (state)
        {
            case PlayerState.Walking:
                HandleStepInput();
                MoveStep();
                break;

            case PlayerState.AligningToCenter:
                AlignToCenter();
                break;

            case PlayerState.WaitingForTurnInput:
                HandleTurnInput();
                break;

            case PlayerState.Turning:
                break;

            case PlayerState.Stumbling:
                break;

            case PlayerState.Avoiding:
                AvoidSuccess();
                break;

            case PlayerState.Stuck:
                HandleStuckInput();
                break;
        }

        UseAvoid();


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "CenterAnchor")
        {
            CenterAnchor anchor = other.GetComponent<CenterAnchor>();
            if (anchor == null) return;

            currentAnchor = anchor;
            anchor.OnPlayerEnter(this);
        }


        //spawnManager.SpawnTriggerEntered();
    }

    IEnumerator WarmUp()
    {
        yield return null;
        UIController.TurnUI(true, true);
        UIController.StuckUI(true);
        UIController.Hide();
    }

    #region Movement Manager

    void HandleStepInput()
    {
        if (isStepping) return;

        if (Input.GetKeyDown(KeyCode.LeftShift) && lastStep != StepFoot.Left)
        {
            StartStep(StepFoot.Left);
        }

        else if (Input.GetKeyDown(KeyCode.RightShift) && lastStep != StepFoot.Right)
        {
            StartStep(StepFoot.Right);
        }


    }
    void StartStep(StepFoot foot)                                       // This gets the correct params before the step happens
    {

        lastStep = foot;
        isStepping = true;
        stepTimer = 0f;


        stepStartPos = transform.position;


        targetLateralOffset = (foot == StepFoot.Left)                   // Left foot lands left, right foot lands right
            ? -lateralStepOffset
            : lateralStepOffset;

        Vector3 step = moveDirection * stepSize;                        // Forward displacement
        Vector3 sideOffset = transform.right * targetLateralOffset;     // Side movement of character

        stepEndPos = stepStartPos + step + sideOffset;
        //stepEndPos.x = targetLateralOffset;
    }


    void MoveStep()                                                     // Starts the animation
    {
        if (!isStepping) return;                                        // Ensure only move when isStepping

        stepTimer += Time.deltaTime;
        float t = Mathf.Clamp01(stepTimer / stepDuration);

        float smoothT = t * t * (3f - 2f * t);
        float curve = Mathf.Sin(t * Mathf.PI);

        Vector3 flatPos = Vector3.Lerp(stepStartPos, stepEndPos, smoothT);
        Vector3 bobOffset = Vector3.up * (curve * stepHeight);

        transform.position = flatPos + bobOffset;

        if (t >= 1f)                                                    // Only stop when t = 1, aka animation completed
        {
            isStepping = false;
            baseWorldPos = stepEndPos;
            transform.position = baseWorldPos;
        }
    }



    #endregion

    #region Avoid Manager
    private void UseAvoid()
    {
        if (state != PlayerState.Walking) return;
        if (isStepping) return;
        if (_avoidAvailable == false) return;

        if (Input.GetKeyDown(KeyCode.Space) && _avoidAvailable)
        {

            if (obstacleInAvoidTrigger)
            {
                // SUCCESS
                avoidActivate = true;
                state = PlayerState.Avoiding;
                AvoidActivate();
            }
            else
            {
                // FAIL
                state = PlayerState.Stumbling;
                Stumble(1.5f, 2f);
                StartCoroutine(StartCooldown());
            }


        }

    }

    public IEnumerator StartCooldown()
    {
        _avoidAvailable = false;
        yield return new WaitForSeconds(_avoidCooldownDuration);
        _avoidAvailable = true;
    }

    private void AvoidActivate()
    {
        if (!_avoidAvailable) return;

        avoidTimer = 0f;
        avoidDistance = 0f;
        isAvoiding = true;

        _avoidStartPos = transform.position;
        _cameraStartLocalPos = cameraPivot.localPosition;
    }


    public void SetObstacleInFront(bool value)
    {
        obstacleInAvoidTrigger = value;
    }

    private void AvoidSuccess()
    {
        avoidTimer += Time.deltaTime;
        float t = Mathf.Clamp01(avoidTimer / avoidDuration);

        // Smooth curve
        float smoothT = t * t * (3f - 2f * t);

        // --- PLAYER MOVEMENT ---
        Vector3 targetPos =
            _avoidStartPos + transform.forward * avoidStepDistance;

        transform.position =
            Vector3.Lerp(_avoidStartPos, targetPos, smoothT);

        // --- CAMERA SLIDE OVER ---
        float arc = Mathf.Sin(t * Mathf.PI); // up then down
        cameraPivot.localPosition =
            _cameraStartLocalPos + Vector3.up * (arc * avoidCameraLift);

        // --- FINISH ---
        if (t >= 1f)
        {
            transform.position = targetPos;
            cameraPivot.localPosition = _cameraStartLocalPos;
            isAvoiding = false;

            state = PlayerState.Walking;
        }
    }


    #endregion

    #region Turning Manager

    public void BeginCenterAlignment(CenterAnchor anchor)
    {
        currentAnchor = anchor;
        state = PlayerState.AligningToCenter;
    }

    void EnterTurnState()
    {
        state = PlayerState.WaitingForTurnInput;

        UIController.TurnUI(
            currentAnchor.CanTurnLeft(),
            currentAnchor.CanTurnRight()
        );
    }

    void HandleTurnInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.LeftShift) && currentAnchor.CanTurnLeft())
                TurnLeft();
            else if (Input.GetKey(KeyCode.RightShift) && currentAnchor.CanTurnRight())
                TurnRight();
            else
                return; // invalid input

            UIController.TurnUI(false, false);

        }
    }


    void AlignToCenter()
    {
        Vector3 target = transform.position;
        target.x = currentAnchor.Center.position.x;
        target.z = currentAnchor.Center.position.z;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            snapSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            transform.position = target;
            state = PlayerState.WaitingForTurnInput;
            EnterTurnState();
        }
    }


    void TurnLeft()
    {
        if (isTurning) return;
        StartCoroutine(SmoothTurn(-90f, -transform.right));

    }

    void TurnRight()
    {
        if (isTurning) return;
        StartCoroutine(SmoothTurn(90f, transform.right));
    }


    IEnumerator SmoothTurn(float angle, Vector3 sideDir)

    {
        isTurning = true;
        isStepping = false;

        state = PlayerState.Turning;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, angle, 0);

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + sideDir.normalized * 1f;


        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / turnDuration;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        transform.rotation = targetRot;
        transform.position = targetPos;

        moveDirection = transform.forward;

        isTurning = false;
        state = PlayerState.Walking;
    }

    #endregion

    #region Stumbling Manager
    public void Stumble(float duration, float stumbleDistance)
    {
        if (isAvoiding || isStumbling) return;
        isStumbling = true;
        isStepping = false;
        StartCoroutine(StumbleRoutine(duration, stumbleDistance));
    }

    IEnumerator StumbleRoutine(float duration, float stumbleDistance)
    {

        state = PlayerState.Stumbling;

        Vector3 camStartPos = cameraPivot.localPosition;
        Quaternion camStartRot = cameraPivot.localRotation;

        Vector3 stepStartPos = transform.position;
        Vector3 stepEndPos = stepStartPos + moveDirection * stumbleDistance;

        Quaternion stumbleRot = Quaternion.Euler(50f, 0f, 0f);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float curve = Mathf.Sin(t * Mathf.PI);

            // Camera stumble


            cameraPivot.localRotation =
                Quaternion.Slerp(camStartRot, camStartRot * stumbleRot, curve);

            // Player stumble movement
            float smoothT = t * t * (3f - 2f * t);
            transform.position =
                Vector3.Lerp(stepStartPos, stepEndPos, smoothT);

            yield return null;
        }

        cameraPivot.localPosition = camStartPos;
        cameraPivot.localRotation = camStartRot;
        baseWorldPos = stepEndPos;
        transform.position = baseWorldPos;


        isStumbling = false;
        state = PlayerState.Walking;
    }
    #endregion
    void HandleStuckInput()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            stuckPressesRemaining--;

            Debug.Log($"Struggling... {stuckPressesRemaining} left");

            if (stuckPressesRemaining <= 0)
            {
                ExitStuck();
            }
        }
    }

    public void Stuck(int minStuckPresses, int maxStuckPresses)
    {
        if (isStuck) return;
        if (state != PlayerState.Walking) return;

        isStuck = true;
 
        state = PlayerState.Stuck;

        UIController.StuckUI(true);

        stuckPressesRemaining =
        Random.Range(minStuckPresses, maxStuckPresses + 1);


    }
    
    void ExitStuck()
    {
        isStuck = false;
        stuckPressesRemaining = 0;

        state = PlayerState.Walking;
        UIController.StuckUI(false);

    }

}
