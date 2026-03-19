using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTeleporter : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Exact name of the scene to load (must be added to Build Settings)")]
    public string targetSceneName = "Menu";

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
        foreach (var ps in burstParticles)
        {
            if (ps != null) ps.Play();
        }

        yield return new WaitForSeconds(delayBeforeFade);

        yield return ScreenFader.Instance.FadeOut(fadeDuration);

        AsyncOperation operation = SceneManager.LoadSceneAsync(targetSceneName);

        while (!operation.isDone)
        {
            yield return null;
        }
    }
}
