using UnityEngine;
using UnityEngine.Events;

public class Springboard : MonoBehaviour
{
    [SerializeField] private float _force = 60f;
    [SerializeField] private float _duration = .4f;
    [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private UnityEvent _onJump;

    void OnTriggerEnter(Collider col)
    {
        if (Player.Owner && col.gameObject == Player.Owner.gameObject)
        {
            Debug.Log("Springboard triggered by " + col.gameObject.name);
            Player.Owner.AddExtraForce(Vector3.up * _force, true, _duration, _curve);
            _onJump?.Invoke();
        }
    }
}
