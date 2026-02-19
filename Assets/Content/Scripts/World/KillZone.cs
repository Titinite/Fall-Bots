using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (Player.Owner && other.gameObject == Player.Owner.gameObject)
        {
            PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();
            respawn?.Die();
        }
    }
}