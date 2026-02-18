using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [System.Serializable]
    private class Settings
    {
        public float FallDelay = 0.5f;
        public float ResetDelay = 3f;
    }

    [System.Serializable]
    private class References
    {
        public Rigidbody Rigidbody;
    }

    [SerializeField]
    private Settings settings;

    [SerializeField]
    private References reference;

    private bool _isPlatformFalling;
    private bool _playerCollide;
    private float _playerCollideTime;

    void OnTriggerEnter(Collider col)
    {
        if (Player.Owner && col.gameObject == Player.Owner.gameObject)
        {
            _playerCollide = true;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (Player.Owner && col.gameObject == Player.Owner.gameObject)
        {
            _playerCollide = false;
        }
    }

    private void Update()
    {
        if (!_isPlatformFalling && _playerCollide)
        {
            _playerCollideTime += Time.deltaTime;

            if (_playerCollideTime >= settings.FallDelay)
            {
                Fall();
            }
        }
    }

    private void Fall()
    {
        _isPlatformFalling = true;
        _references = 
    }
}
