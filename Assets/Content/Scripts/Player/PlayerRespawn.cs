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

        Player player = _references.PlayerController as Player;

        if (player != null)
            player.Eliminate();

        if (_references.Animator)
            _references.Animator.SetTrigger("trigger_die");

        yield return new WaitForSeconds(_settings.DeathDuration);

        if (player != null)
            player.Respawn(_settings.RespawnPosition);

        _isDead = false;
    }
}