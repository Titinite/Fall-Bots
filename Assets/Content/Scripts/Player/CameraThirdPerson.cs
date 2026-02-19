using UnityEngine;
using UnityEngine.InputSystem;

public class CameraThirdPerson : MonoBehaviour
{
    [System.Serializable]
    private class Settings
    {
        [Header("Sensitivity")]
        [Tooltip("Camera follow smoothness (0 = instant, 1 = smooth)")]
        [Range(0, 1)]
        public float FollowSmoothness = .1f;

        [Tooltip("Camera rotation sensitivity")]
        public float LookSensitivity = 20f;

        [Header("Position")]
        [Tooltip("Camera distance from player")]
        public float Distance = 5f;

        [Tooltip("Vertical offset from player")]
        public float VerticalOffset = 2f;

        [Header("Pitch")]
        [Tooltip("Default pitch angle")]
        public float DefaultPitch = 20f;

        [Tooltip("Default pitch angle")]
        public float MinPitch = -30f;

        [Tooltip("Default pitch angle")]
        public float MaxPitch = 60f;
    }

    [System.Serializable]
    public class References
    {
        public InputActionAsset InputActions;
    }

    [SerializeField]
    private Settings _settings;

    [SerializeField]
    private References _references;

    private float _yaw;
    private float _pitch;

    private Vector3 _playerPosition;
    private InputAction _lookAction;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(GetPlayerPosition(), Vector3.one * .2f);
    }

    void OnValidate()
    {
        _settings.DefaultPitch = Mathf.Clamp(_settings.DefaultPitch, _settings.MinPitch, _settings.MaxPitch);
        _pitch = _settings.DefaultPitch;
        _playerPosition = GetPlayerPosition();
        SetPosition();
    }

    void OnEnable()
    {
        _lookAction = _references.InputActions.FindActionMap("Player").FindAction("Look");
        _lookAction.Enable();
    }

    void OnDisable()
    {
        _lookAction.Disable();
    }

    void Start()
    {
        
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        _playerPosition = Vector3.Lerp(_playerPosition, GetPlayerPosition(), (1.1f - _settings.FollowSmoothness) * 20 * deltaTime);

        SetCursor();
        SetYawAndPitch(deltaTime);
        SetPosition();
    }

    private void SetCursor()
    {
        bool lockCursor = Player.Owner && !Player.Owner.State.IsPaused;

        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
    }

    private void SetYawAndPitch(float deltaTime)
    {
        if (!Player.Owner || Player.Owner.State.IsPaused)
            return;

        Vector2 lookInput = _lookAction?.ReadValue<Vector2>() ?? Vector2.zero;

        _pitch -= lookInput.y * _settings.LookSensitivity * deltaTime;
        _pitch = Mathf.Clamp(_pitch, _settings.MinPitch, _settings.MaxPitch);

        _yaw += lookInput.x * _settings.LookSensitivity * deltaTime;
    }

    private void SetPosition()
    {
        Vector3 cameraPosition = Vector3.forward * -_settings.Distance;
        cameraPosition = Quaternion.Euler(_pitch, _yaw, 0) * cameraPosition;
        cameraPosition += _playerPosition;

        transform.position = cameraPosition;
        transform.rotation = Quaternion.LookRotation(_playerPosition - cameraPosition, Vector3.up);
    }

    private Vector3 GetPlayerPosition()
    {
        Vector3 offset = Vector3.up * _settings.VerticalOffset;

        if (!Player.Owner)
            return Vector3.zero + offset;
        return Player.Owner.transform.position + offset;
    }
}
