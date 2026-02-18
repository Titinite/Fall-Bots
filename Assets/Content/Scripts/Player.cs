using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
//using Unity.Netcode;
//using Unity.Netcode.Components;

/// <summary>
/// Player controller.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour //NetworkBehaviour
{
    public static Player Owner { get; private set; }

    public enum PlayerType
    {
        Local,
        //Network
    }

    public enum PlayerState
    {
        Idle,
        Moving,
        Jumping,
        Falling,
        Stunned,
        Eliminated,
        Loser,
        Winner,
    }

    [System.Serializable]
    public class Settings
    {
        public PlayerType Type;

        [Header("Movements")]

        [Tooltip("Movement speed in km/h")]
        public float Speed = 18f;

        [Tooltip("Jump force in m/s")]
        public float JumpForce = 8f;

        [Tooltip("Player rotation speed towards movement direction")]
        public float RotationSpeed = 10f;

        [Tooltip("Ground detection tolerance")]
        public float GroundTolerance = 0.2f;

        [Tooltip("Layers considered as ground")]
        public LayerMask GroundLayer = 1;

        [Header("Debug")]

        [Tooltip("GUI logs of current state")]
        public bool StateLogs;
    }

    [System.Serializable]
    public class References
    {
        public CharacterController Controller;
        public InputActionAsset InputActions;
        //public NetworkTransform NetworkTransform;
    }

    [System.Serializable]
    public class StateContainer
    {
        [Tooltip("Current player state")]
        public PlayerState CurrentState = PlayerState.Idle;

        [Tooltip("Is player paused?")]
        public bool IsPaused = false;

        [Tooltip("Is player grounded?")]
        public bool IsGrounded;

        [Tooltip("Is gravity suspended?")]
        public bool IsGravitySuspended;

        [Tooltip("Vertical velocity in m/s")]
        public float VerticalVelocity;

        [Tooltip("Horizontal velocity in m/s")]
        public Vector2 HorizontalVelocity;

        [Tooltip("Additionnal velocity in m/s")]
        public Vector3 ExtraVelocity;

        [Tooltip("Ground velocity in m/s to avoid parenting")]
        public Vector3 GroundVelocity;

        [Tooltip("Ground transform evaluated as parent")]
        public Transform GroundTransform;
    }

    [SerializeField] private Settings _settings;
    [SerializeField] private References _references;
    [SerializeField] private StateContainer _state;

    public StateContainer State => _state;

    #region Private Fields
    private bool _jumpInput;
    private Vector2 _moveInput;

    private InputAction _moveAction;
    private InputAction _jumpAction;

    private float _groundCheckRadius;
    private Vector3 _groundCheckOffset;
    private Vector3 _groundContactPosition;
    private Quaternion _groundContactRotation;
    private Collider[] _groundCheckResults = new Collider[1];

    IEnumerator _addExtraForceCoroutine;
    #endregion

    #region Constants
    private const float GRAVITY = -20;
    private const float KMH_TO_MS = 1 / 3.6f;
    private const float GROUND_STICK_FORCE = -2;
    #endregion

    #region Network
    const bool IsOwner = true; // Remove when enable Network
    /*
    // Network variables to synchronize state
    private NetworkVariable<PlayerState> _networkCurrentState = new NetworkVariable<PlayerState>(
        PlayerState.Idle,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private NetworkVariable<Vector2> _networkHorizontalVelocity = new NetworkVariable<Vector2>(
        Vector2.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _references.NetworkTransform.Interpolate = !IsOwner;

        if (IsOwner)
        {
            Init();
        }
        else
        {
            // Subscribe to NetworkVariable changes for non-owners
            _networkCurrentState.OnValueChanged += OnNetworkStateChanged;
            _networkHorizontalVelocity.OnValueChanged += OnNetworkHorizontalVelocityChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // Unsubscribe from NetworkVariable changes
        if (!IsOwner)
        {
            _networkCurrentState.OnValueChanged -= OnNetworkStateChanged;
            _networkHorizontalVelocity.OnValueChanged -= OnNetworkHorizontalVelocityChanged;
        }
    }

    // Callback when player state changes (for non-owners)
    private void OnNetworkStateChanged(PlayerState previousState, PlayerState newState)
    {
        _state.CurrentState = newState;
    }

    // Callback when horizontal velocity changes (for non-owners)
    private void OnNetworkHorizontalVelocityChanged(Vector2 previousVelocity, Vector2 newVelocity)
    {
        _state.HorizontalVelocity = newVelocity;
    }*/
    #endregion

    #region Unity Debug
    void OnGUI()
    {
        if (_settings.StateLogs)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 15;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperLeft;

            string debug = $"Horizontal Velocity: {_state.HorizontalVelocity.magnitude / KMH_TO_MS:F2} km/h\n";
            debug += $"X: {_state.HorizontalVelocity.x:F2}  Z: {_state.HorizontalVelocity.y:F2}\n";
            debug += $"Vertical Velocity: {_state.VerticalVelocity / KMH_TO_MS:F2} km/h\n";
            debug += $"Grounded: {_state.IsGrounded}\n";
            debug += $"State: {_state.CurrentState}\n";
            //debug += $"IsOwner: {IsOwner}\n";
            //debug += $"IsServer: {IsServer}\n";
            //debug += $"IsClient: {IsClient}";

            GUI.Label(new Rect(10, 10, 400, 200), debug, style);
        }
    }

    void OnDrawGizmos()
    {
        if (_settings.Type == PlayerType.Local || IsOwner)
        {
            // Draw ground check sphere
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = _state.IsGrounded ? Color.green : new Color(1, .5f, 0);
            Gizmos.DrawWireSphere(_groundCheckOffset, _groundCheckRadius);
        }
    }
    #endregion

    #region Unity Lifecycle
    void OnEnable()
    {
        if (_settings.Type == PlayerType.Local)
            Init();
    }

    void OnDisable()
    {
        _moveAction?.Disable();
        _jumpAction?.Disable();
    }

    void Update()
    {
        if (_settings.Type == PlayerType.Local || IsOwner)
        {
            float deltaTime = Time.deltaTime;

            GetInputs();
            CheckGround(deltaTime);
            SetVelocity(deltaTime);
            SetMovement(deltaTime);
            UpdateState();
        }
    }

    void LateUpdate()
    {
        // Owner sends state to network
        if (IsOwner)
        {
            //_networkCurrentState.Value = _state.CurrentState;
            //_networkHorizontalVelocity.Value = _state.HorizontalVelocity;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Add an additionnal force to the player.
    /// </summary>
    /// <param name="force">Force direction in m/s</param>
    /// <param name="suspendGravity">Suspend gravity for the duration of the force</param>
    /// <param name="duration">Duration while fore is applied (if 0: infinite duration)</param>
    /// <param name="curve">A remplir par claude</param>
    public void AddExtraForce(Vector3 force, bool suspendGravity, float duration = -1, AnimationCurve curve = null)
    {
        if (_state.CurrentState == PlayerState.Eliminated)
            return;

        if (_addExtraForceCoroutine != null)
            return;

        if (duration <= 0)
            _state.ExtraVelocity = force;
        else
            StartCoroutine(_addExtraForceCoroutine = AddExtraForceCoroutine(force, suspendGravity, duration, curve));
    }

    private IEnumerator AddExtraForceCoroutine(Vector3 force, bool suspendGravity, float duration, AnimationCurve curve)
    {
        if (suspendGravity)
            _state.IsGravitySuspended = true;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float normaliwedTime = t / duration;
            float easeTime = curve != null ? curve.Evaluate(normaliwedTime) : normaliwedTime;

            _state.ExtraVelocity = force * easeTime;

            yield return new WaitForEndOfFrame();
        }

        if (suspendGravity)
            _state.IsGravitySuspended = false;

        _state.ExtraVelocity = Vector3.zero;
        _addExtraForceCoroutine = null;
    }

    public void ResetExtraForce()
    {
        _state.ExtraVelocity = Vector3.zero;
    }

    /// <summary>
    /// Stuns the player for a specified duration (0 = infinite).
    /// </summary>
    public void Stun(float duration = 0)
    {
        if (_state.CurrentState == PlayerState.Eliminated)
            return;

        _state.CurrentState = PlayerState.Stunned;
        _state.HorizontalVelocity = Vector2.zero;
        _state.VerticalVelocity = 0f;

        if (duration > 0)
            Invoke(nameof(RecoverFromStun), duration);
    }

    /// <summary>
    /// Recover the player from stun.
    /// </summary>
    public void RecoverFromStun()
    {
        if (_state.CurrentState == PlayerState.Stunned)
            _state.CurrentState = PlayerState.Idle;
    }

    /// <summary>
    /// Eliminates the player from the game.
    /// </summary>
    public void Eliminate()
    {
        _state.CurrentState = PlayerState.Eliminated;
        _state.HorizontalVelocity = Vector2.zero;
        _state.VerticalVelocity = 0f;
        _references.Controller.enabled = false;
    }

    /// <summary>
    /// Sets the pause state of the player.
    /// </summary>
    public void Pause(bool paused)
    {
        _state.IsPaused = paused;
    }
    #endregion

    #region Player Logic
    private void Init()
    {
        if (_settings.Type == PlayerType.Local || IsOwner)
        {
            if (Owner == null)
            {
                Owner = this;
            }
            else
            {
                Debug.LogWarning($"Multiple Player instances detected. Destroy: {gameObject.name}");
                Destroy(gameObject);
            }

            _moveAction = _references.InputActions.FindActionMap("Player").FindAction("Move");
            _jumpAction = _references.InputActions.FindActionMap("Player").FindAction("Jump");

            _moveAction?.Enable();
            _jumpAction?.Enable();
        }

        _groundCheckOffset = _references.Controller.center + Vector3.up * (_references.Controller.height * -.5f + _references.Controller.radius - _references.Controller.skinWidth - _settings.GroundTolerance);
        _groundCheckRadius = _references.Controller.radius + _references.Controller.skinWidth;
    }

    private void GetInputs()
    {
        if (Keyboard.current?.escapeKey.wasPressedThisFrame ?? false)
            Pause(!_state.IsPaused);

        _moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;

        bool jumpInput = _jumpAction != null && _jumpAction.WasPressedThisFrame();
        if (jumpInput)
        {
            bool canJump = _state.IsGrounded &&
                           _state.CurrentState != PlayerState.Stunned &&
                           _state.CurrentState != PlayerState.Eliminated &&
                           _state.CurrentState != PlayerState.Jumping &&
                           _state.CurrentState != PlayerState.Falling;

            _jumpInput = canJump;
        }
        else
        {
            _jumpInput = false;
        }
    }

    private void CheckGround(float deltaTime)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position + transform.rotation * _groundCheckOffset,
            _groundCheckRadius,
            _groundCheckResults,
            _settings.GroundLayer
        );

        bool wasGrounded = _state.IsGrounded;
        _state.IsGrounded = hitCount > 0;

        if (_state.IsGrounded)
        {
            Transform newPlatform = _groundCheckResults[0].transform;

            // If platform changed, initialize last frame values
            if (newPlatform != _state.GroundTransform)
            {
                _state.GroundTransform = newPlatform;
                _state.GroundVelocity = Vector3.zero;
                _groundContactPosition = _state.GroundTransform.position;
                _groundContactRotation = _state.GroundTransform.rotation;
            }
        }
        else
        {
            // If player just left the platform
            if (wasGrounded && _state.GroundTransform != null)
            {
                Vector3 platformVelocity = (_state.GroundTransform.position - _groundContactPosition) / deltaTime;

                // Add platform velovity to player ground velocity
                _state.GroundVelocity = platformVelocity;
            }

            _state.GroundTransform = null;
        }
    }

    private void SetVelocity(float deltaTime)
    {
        // Determine movement attenuation based on current state

        float moveAtten;
        switch (_state.CurrentState)
        {
            case PlayerState.Idle:
            case PlayerState.Moving:
                moveAtten = 1;
                break;
            case PlayerState.Jumping:
                moveAtten = .8f;
                break;
            case PlayerState.Falling:
                moveAtten = .6f;
                break;
            default:
                moveAtten = 0;
                break;
        }

        // Update horizontal velocity

        Vector2 horizontalVelocity = _state.HorizontalVelocity;

        if (moveAtten > 0 && Camera.main)
        {
            Vector2 move = Vector2.zero;

            // Don't apply move to velocity if paused
            if (!_state.IsPaused)
            {
                Vector3 forward = Camera.main.transform.forward;
                Vector3 right = Camera.main.transform.right;

                forward.y = right.y = 0;
                forward.Normalize();
                right.Normalize();

                Vector3 direction3D = (forward * _moveInput.y + right * _moveInput.x);
                move = new Vector2(direction3D.x, direction3D.z).normalized;
                move *= _settings.Speed * KMH_TO_MS;
            }

            horizontalVelocity = Vector2.Lerp(horizontalVelocity, move, moveAtten * deltaTime * 5);
        }
        else
        {
            horizontalVelocity = Vector2.zero;
        }

        _state.HorizontalVelocity = horizontalVelocity.magnitude > .01f ? horizontalVelocity : Vector2.zero;

        // Update vertical velocity

        float verticalVelocity = _state.VerticalVelocity;

        if (_state.IsGrounded && _jumpInput && !_state.IsPaused)
        {
            _state.IsGrounded = _jumpInput = false;
            verticalVelocity = _settings.JumpForce;
        }
        else if (_state.IsGrounded && verticalVelocity <= 0)
        {
            verticalVelocity = _state.IsGravitySuspended ? 0 : GROUND_STICK_FORCE;
        }
        else if (!_state.IsGravitySuspended)
        {
            verticalVelocity = Mathf.Max(verticalVelocity + GRAVITY * deltaTime, GRAVITY);
        }

        _state.VerticalVelocity = verticalVelocity;
    }

    private void SetMovement(float deltaTime)
    {
        // Apply platform movement and rotation

        if (_state.GroundTransform)
        {
            // Calculate platform delta position
            Vector3 platformPositionDelta = _state.GroundTransform.position - _groundContactPosition;

            // Calculate platform delta rotation
            Quaternion platformRotationDelta = _state.GroundTransform.rotation * Quaternion.Inverse(_groundContactRotation);

            // Apply rotation around platform center
            Vector3 localPosition = transform.position - _state.GroundTransform.position;
            Vector3 rotatedPosition = platformRotationDelta * localPosition;
            platformPositionDelta += rotatedPosition - localPosition;

            // Move player with platform
            _references.Controller.Move(platformPositionDelta);

            // Rotate player with platform
            transform.Rotate(Vector3.up, platformRotationDelta.eulerAngles.y, Space.World);

            // Update last frame values
            _groundContactPosition = _state.GroundTransform.position;
            _groundContactRotation = _state.GroundTransform.rotation;
        }

        // Apply player movement

        Vector3 lookDir = new Vector3(_state.HorizontalVelocity.x, 0, _state.HorizontalVelocity.y);
        Vector3 velocity = lookDir;
        velocity.y = _state.VerticalVelocity;
        velocity += _state.GroundVelocity;
        velocity += _state.ExtraVelocity;
        velocity *= deltaTime;

        _references.Controller.Move(velocity);

        // Apply player rotation

        if (lookDir.sqrMagnitude > .01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _settings.RotationSpeed * deltaTime);
        }
    }

    private void UpdateState()
    {
        PlayerState previousState = _state.CurrentState;

        if (_state.CurrentState == PlayerState.Stunned ||
            _state.CurrentState == PlayerState.Eliminated)
        {
            return;
        }

        if (!_state.IsGrounded)
        {
            _state.CurrentState = _state.VerticalVelocity > 0
                ? PlayerState.Jumping
                : PlayerState.Falling;
        }
        else
        {
            if (_state.VerticalVelocity <= 0)
                _state.CurrentState = _state.HorizontalVelocity.sqrMagnitude > 0.1f
                    ? PlayerState.Moving
                    : PlayerState.Idle;
        }

        //if (_state.CurrentState != previousState)
        //    Debug.Log($"{previousState} → {_state.CurrentState}");
    }
    #endregion
}