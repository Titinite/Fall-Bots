using UnityEngine;

/// <summary>
/// Procedural platform animation supporting translation, rotation, and scaling.
/// Supports multiple animation types (Constant, PingPong, Loop, Single) with easing.
/// </summary>
public class ProceduralAnimator : MonoBehaviour
{
    public enum AnimationSpace
    {
        Local,
        World
    }

    public enum AnimationType
    {
        Constant, // Continuous movement without return
        PingPong, // Back and forth (A → B → A)
        Loop,     // Loop with teleport (A → B, teleport to A)
        Single    // Play once then stop
    }

    public enum AnimationBalance
    {
        Centered,
        Forward,
        Backward
    }

    [System.Serializable]
    public class Settings
    {
        [Tooltip("Animation space")]
        public AnimationSpace Space = AnimationSpace.Local;

        [Tooltip("Type of animation loop")]
        public AnimationType Type = AnimationType.PingPong;

        [Tooltip("TCenter of animation")]
        public AnimationBalance Balance = AnimationBalance.Centered;

        [Tooltip("Custom animation curve")]
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Tooltip("Animation speed multiplier")]
        public float Duration = 4;

        [Tooltip("Time offset to desynchronize multiple animations")]
        public float TimeOffset = 0;

        [Tooltip("Translation amount in local space (meters)")]
        public Vector3 Translation = Vector3.zero;

        [Tooltip("Rotation amount in degrees")]
        public Vector3 Rotation = Vector3.zero;

        [Tooltip("Scale amount relative to initial scale")]
        public Vector3 Scale = Vector3.zero;
    }

    [System.Serializable]
    public class State
    {
        [Tooltip("Current animation time")]
        public float CurrentTime;

        [Tooltip("Is animation finished? (Single only)")]
        public bool IsFinished;

        [HideInInspector] public Vector3 InitialPosition;
        [HideInInspector] public Quaternion InitialRotation;
        [HideInInspector] public Vector3 InitialScale;
    }

    [SerializeField] private Settings _settings;
    [SerializeField] private State _state;

    void OnDrawGizmosSelected()
    {

    }

    void Start()
    {
        _state.InitialPosition = transform.localPosition;
        _state.InitialRotation = transform.localRotation;
        _state.InitialScale = transform.localScale;
        _state.CurrentTime = _settings.TimeOffset;
        _state.IsFinished = false;
    }

    void LateUpdate()
    {
        if (_state.IsFinished)
            return;

        _state.CurrentTime = Time.timeSinceLevelLoad + _settings.TimeOffset * _settings.Duration;

        float normalizedTime = GetNormalizedTime(_state.CurrentTime);
        float easedTime = ApplyEasing(normalizedTime);
        float balanceTime = ApplyBalance(easedTime);

        ApplyTranslation(balanceTime);
        ApplyRotation(balanceTime);
        ApplyScale(balanceTime);
    }

    /// <summary>
    /// Resets animation to initial state.
    /// </summary>
    public void ResetAnimation()
    {
        _state.CurrentTime = _settings.TimeOffset;
        _state.IsFinished = false;
        transform.localPosition = _state.InitialPosition;
        transform.localRotation = _state.InitialRotation;
        transform.localScale = _state.InitialScale;
    }

    /// <summary>
    /// Pauses or resumes animation.
    /// </summary>
    public void SetPaused(bool paused)
    {
        enabled = !paused;
    }

    /// <summary>
    /// Gets normalized time [0,1] based on animation type.
    /// </summary>
    private float GetNormalizedTime(float time)
    {
        if (_settings.Duration == 0)
            return 0;

        switch (_settings.Type)
        {
            case AnimationType.Constant:
                return time / _settings.Duration;

            case AnimationType.PingPong:
                return Mathf.PingPong(time / _settings.Duration, 1);

            case AnimationType.Loop:
                return Mathf.Repeat(time / _settings.Duration, 1f);

            case AnimationType.Single:
                if (time >= _settings.Duration)
                {
                    _state.IsFinished = true;
                    return 1;
                }
                return time / _settings.Duration;

            default:
                return 0f;
        }
    }

    private float ApplyBalance(float time)
    {
        switch (_settings.Balance)
        {
            case AnimationBalance.Centered:
                return time - .5f;

            case AnimationBalance.Backward:
                return -time;

            default:
                return time;
        }
    }

    /// <summary>
    /// Applies easing function to normalized time.
    /// </summary>
    private float ApplyEasing(float time)
    {
        switch (_settings.Type)
        {
            default:
                return _settings.Curve.Evaluate(time); ;

            case AnimationType.Constant:
                return time;
        }
    }

    /// <summary>
    /// Applies translation based on eased time.
    /// </summary>
    private void ApplyTranslation(float time)
    {
        if (_settings.Translation == Vector3.zero)
            return;

        Vector3 offset = _settings.Translation * time;
        if (_settings.Space == AnimationSpace.Local)
            offset = _state.InitialRotation * offset;

        transform.localPosition = _state.InitialPosition + offset;
    }

    /// <summary>
    /// Applies rotation based on eased time.
    /// </summary>
    private void ApplyRotation(float time)
    {
        if (_settings.Rotation == Vector3.zero)
            return;

        Vector3 rotation = _settings.Rotation * time;
        Quaternion rotationOffset = Quaternion.Euler(rotation);
        if (_settings.Space == AnimationSpace.Local)
            rotationOffset = _state.InitialRotation * rotationOffset;
        else
            rotationOffset = rotationOffset * _state.InitialRotation;

        transform.localRotation = rotationOffset;
    }

    /// <summary>
    /// Applies scale based on eased time.
    /// </summary>
    private void ApplyScale(float t)
    {
        if (_settings.Scale == Vector3.zero)
            return;

        Vector3 scale = Vector3.one + _settings.Scale * t;
        transform.localScale = Vector3.Scale(_state.InitialScale, scale);
    }
}