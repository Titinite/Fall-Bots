using UnityEngine;

public class LevelPortal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the portal. Transitioning to next level...");
        }
    }
}