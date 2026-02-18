using System.Collections;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [System.Serializable]
    private class Settings
    {
        public float DeathDuration = 2f;
        public Vector3 RespawnPosition = new Vector3(0, 2, 0);
    }

    [System.Serializable]
    private class References
    {
        public Animator Animator;
        public MonoBehaviour PlayerController;
        public CharacterController CharacterController;
    }

    [SerializeField] private Settings _settings;
    [SerializeField] private References _references;

    private bool _isDead;

    public void Die()
    {
        if (_isDead)
            return;

        StartCoroutine(DeathCoroutine());
    }

    private IEnumerator DeathCoroutine()
    {
        _isDead = true;

        // Désactive le contrôle
        if (_references.PlayerController)
            _references.PlayerController.enabled = false;

        // Joue animation
        if (_references.Animator)
            _references.Animator.SetTrigger("trigger_die");

        yield return new WaitForSeconds(_settings.DeathDuration);

        // IMPORTANT avec CharacterController
        if (_references.CharacterController)
            _references.CharacterController.enabled = false;

        transform.position = _settings.RespawnPosition;

        if (_references.CharacterController)
            _references.CharacterController.enabled = true;

        if (_references.PlayerController)
            _references.PlayerController.enabled = true;

        _isDead = false;
    }
}