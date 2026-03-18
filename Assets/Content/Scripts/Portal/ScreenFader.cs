using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ScreenFader — Singleton that manages full-screen fade in/out.
/// Automatically creates its own Canvas if not present in the scene.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────
    private static ScreenFader _instance;
    public static ScreenFader Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("ScreenFader");
                _instance = go.AddComponent<ScreenFader>();
                DontDestroyOnLoad(go);
                _instance.Initialize();
            }
            return _instance;
        }
    }

    // ── Private fields ─────────────────────────────────────────────────────
    private Canvas _canvas;
    private Image  _overlay;
    private bool   _initialized = false;

    // ── Initialization ─────────────────────────────────────────────────────
    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        // Canvas
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 9999;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        // Full-screen black overlay
        var overlayGo = new GameObject("Overlay");
        overlayGo.transform.SetParent(_canvas.transform, false);

        _overlay = overlayGo.AddComponent<Image>();
        _overlay.color = new Color(0, 0, 0, 0); // start transparent

        var rect = _overlay.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin  = Vector2.zero;
        rect.offsetMax  = Vector2.zero;
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Fades the screen to black over <paramref name="duration"/> seconds.</summary>
    public IEnumerator FadeOut(float duration)
    {
        yield return Fade(0f, 1f, duration);
    }

    /// <summary>Fades the screen from black to clear over <paramref name="duration"/> seconds.</summary>
    public IEnumerator FadeIn(float duration)
    {
        yield return Fade(1f, 0f, duration);
    }

    // ── Internal ───────────────────────────────────────────────────────────
    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = _overlay.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / duration);
            _overlay.color = c;
            yield return null;
        }

        c.a = to;
        _overlay.color = c;
    }
}
