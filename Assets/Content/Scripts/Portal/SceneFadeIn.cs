using System.Collections;
using UnityEngine;

/// <summary>
/// SceneFadeIn — place this on any persistent GameObject (e.g. Camera).
/// Automatically fades in from black whenever a new scene loads.
/// </summary>
public class SceneFadeIn : MonoBehaviour
{
    [Range(0.1f, 3f)]
    public float fadeInDuration = 0.8f;

    private IEnumerator Start()
    {
        // Give ScreenFader one frame to initialize
        yield return null;
        yield return ScreenFader.Instance.FadeIn(fadeInDuration);
    }
}
