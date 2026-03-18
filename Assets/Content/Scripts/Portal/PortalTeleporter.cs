using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PortalTeleporter — attach on the Portal prefab root.
/// Configure targetSceneName in the Inspector.
/// </summary>
public class PortalTeleporter : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Exact name of the scene to load (must be added to Build Settings)")]
    public string targetSceneName = "Level2";

    [Header("Portal FX")]
    [Tooltip("Particle system children to activate on player enter")]
    public ParticleSystem[] burstParticles;

    [Header("Timing")]
    [Range(0.1f, 3f)]
    public float fadeDuration = 0.8f;
    [Range(0f, 3f)]
    public float delayBeforeFade = 0.3f;

    [Header("Tag")]
    [Tooltip("Tag used to identify the player GameObject")]
    public string playerTag = "Player";

    private bool _isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_isTeleporting) return;
        if (!other.CompareTag(playerTag)) return;

        _isTeleporting = true;
        StartCoroutine(TeleportSequence());
    }

    private IEnumerator TeleportSequence()
    {
        // Trigger burst particles
        foreach (var ps in burstParticles)
        {
            if (ps != null) ps.Play();
        }

        yield return new WaitForSeconds(delayBeforeFade);

        // Fade out
        yield return ScreenFader.Instance.FadeOut(fadeDuration);

        // Load next scene
        SceneManager.LoadScene(targetSceneName);
    }
}
