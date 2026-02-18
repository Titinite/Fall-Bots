using System;
using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [System.Serializable]
    private class Settings
    {
        public float FallDelay = .2f;
        public float RecoverDelay = 5;
    }

    [System.Serializable]
    private class References
    {
        public Rigidbody Rigidbody;
    }

    [SerializeField]
    private Settings _settings;

    [SerializeField]
    private References _references;

    int _originLayer;
    Vector3 _originPosition;
    Quaternion _originRotation;
    private IEnumerator _fallCoroutine;

    void OnTriggerEnter(Collider col)
    {
        if (Player.Owner && Player.Owner.gameObject == col.gameObject)
        {
            Fall();
        }
    }

    void Awake()
    {
        _originLayer = _references.Rigidbody.gameObject.layer;
        _originPosition = _references.Rigidbody.transform.position;
        _originRotation = _references.Rigidbody.transform.rotation;
    }

    private void Fall()
    {
        if (_fallCoroutine != null)
            return;

        StartCoroutine(_fallCoroutine = FallCoroutine());
    }

    IEnumerator FallCoroutine()
    {
        yield return new WaitForSeconds(_settings.FallDelay);

        Vector3 random = new Vector3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1)).normalized;

        _references.Rigidbody.isKinematic = false;
        _references.Rigidbody.AddRelativeTorque(random, ForceMode.Impulse);
        _references.Rigidbody.gameObject.layer = 0;

        yield return new WaitForSeconds(_settings.RecoverDelay);

        float duration = 1f;

        _references.Rigidbody.isKinematic = true;
        _references.Rigidbody.gameObject.layer = _originLayer;

        Vector3 originPosition = _references.Rigidbody.transform.position;
        Quaternion originRotation = _references.Rigidbody.transform.rotation;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;
            float easeTime = .5f - Mathf.Cos(normalizedTime * Mathf.PI) * .5f;

            _references.Rigidbody.transform.position = Vector3.Lerp(originPosition, _originPosition, easeTime);
            _references.Rigidbody.transform.rotation = Quaternion.Slerp(originRotation, _originRotation, easeTime);

            yield return new WaitForEndOfFrame();
        }

        _references.Rigidbody.transform.position = _originPosition;
        _references.Rigidbody.transform.rotation = _originRotation;

        _fallCoroutine = null;
    }
}