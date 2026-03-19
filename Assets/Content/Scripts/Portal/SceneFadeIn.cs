using System.Collections;
using UnityEngine;

public class SceneFadeIn : MonoBehaviour
{
    [Range(0.1f, 3f)]
    public float fadeInDuration = 0.8f;

    private IEnumerator Start()
    {
        yield return null;
        yield return ScreenFader.Instance.FadeIn(fadeInDuration);
    }
}
