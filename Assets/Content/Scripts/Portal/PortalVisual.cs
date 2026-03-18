using UnityEngine;

/// <summary>
/// PortalVisual — drives the shader parameters and transforms for the portal effect.
/// Attach on the portal mesh GameObject (child of Portal root).
/// </summary>
[RequireComponent(typeof(Renderer))]
public class PortalVisual : MonoBehaviour
{
    [Header("Rotation")]
    public float innerRotationSpeed = 45f;   // degrees per second
    public float outerRotationSpeed = -20f;

    [Header("Pulse")]
    public float pulseSpeed = 1.8f;
    public float pulseAmplitude = 0.06f;

    [Header("Color Shift")]
    [ColorUsage(true, true)]
    public Color colorA = new Color(0.1f, 0.4f, 1.0f, 1f);   // blue-violet
    [ColorUsage(true, true)]
    public Color colorB = new Color(0.6f, 0.1f, 1.0f, 1f);   // violet-magenta

    [Header("References")]
    [Tooltip("Inner disc transform (child)")]
    public Transform innerDisc;
    [Tooltip("Outer ring transform (child)")]
    public Transform outerRing;

    // Shader property IDs — match names in PortalUnlit.shader
    private static readonly int PropColor1 = Shader.PropertyToID("_Color1");
    private static readonly int PropColor2 = Shader.PropertyToID("_Color2");
    private static readonly int PropTwirl = Shader.PropertyToID("_TwirlSpeed");
    private static readonly int PropEdgeGlow = Shader.PropertyToID("_EdgeGlow");

    private Material _mat;
    private Vector3 _baseScale;

    private void Start()
    {
        _mat = GetComponent<Renderer>().material; // instance copy
        _baseScale = transform.localScale; // préserve le scale non-uniforme (ex: 2, 0.05, 2)
    }

    private void Update()
    {
        float t = Time.time;

        // ── Rotation ──
        if (innerDisc != null)
            innerDisc.Rotate(Vector3.up, innerRotationSpeed * Time.deltaTime, Space.Self);
        if (outerRing != null)
            outerRing.Rotate(Vector3.up, outerRotationSpeed * Time.deltaTime, Space.Self);

        // ── Pulse scale ──
        float pulse = 1f + Mathf.Sin(t * pulseSpeed * Mathf.PI * 2f) * pulseAmplitude;
        transform.localScale = _baseScale * pulse;

        // ── Color shift ──
        if (_mat != null)
        {
            float lerp = (Mathf.Sin(t * 0.5f) + 1f) * 0.5f;
            Color current = Color.Lerp(colorA, colorB, lerp);
            _mat.SetColor(PropColor1, current);
            _mat.SetColor(PropColor2, Color.Lerp(colorB, colorA, lerp));
        }
    }
}