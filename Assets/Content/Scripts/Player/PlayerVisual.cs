using System.Linq;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [System.Serializable]
    private class Customization
    {
        [ColorUsage(false, true)] public Color[] Colors;
        public int ActiveAccessoryIndex = -1;
    }

    [System.Serializable]
    private class References
    {
        public Player Player;
        public Animator Anim;
        public Renderer[] Renderers;
        public Material[] Materials;
    }

    [System.Serializable]
    private class PlayerAnimationStateMapper
    {
        public Player.PlayerState PlayerState;
        public string AnimatorState;
        public string BlockingState;
        public string Trigger;
    }

    [SerializeField]
    private Customization _customization;

    [SerializeField]
    private References _references;

    [SerializeField]
    private GameObject[] _accessories;

    [SerializeField]
    private PlayerAnimationStateMapper[] _playerAnimationStateMapper;

    private const string PP_CUSTOM = "CustomPlayer";

    void Start()
    {
        LoadCustom();
    }

    void Update()
    {
        UpdateAnimation();
    }

    public void RandomizeCustom()
    {
        float hueShift = Random.Range(0f, 360f);

        _customization.Colors = new Color[_references.Materials.Length];

        for (int i = 0; i < _customization.Colors.Length; i++)
        {
            Material mat = _references.Materials[i];

            Color col = mat.color;
            if (mat.IsKeywordEnabled("_EMISSION"))
                col = mat.GetColor("_EmissionColor");
            col = ShiftHue(col, hueShift);

            _customization.Colors[i] = col;
        }

        if (_accessories != null && _accessories.Length > 0)
        {
            int newIndex;
            if (_accessories.Length > 1)
            {
                do { newIndex = Random.Range(0, _accessories.Length); }
                while (newIndex == _customization.ActiveAccessoryIndex);
            }
            else
            {
                newIndex = 0;
            }

            _customization.ActiveAccessoryIndex = newIndex;
        }

        SetCustom();
        SaveCustom();
    }

    private void SetCustom()
    {
        foreach (Renderer rend in _references.Renderers)
        {
            foreach (Material mat in rend.materials)
            {
                Material sourceMat = _references.Materials.FirstOrDefault(m => mat.name.StartsWith(m.name));

                if (sourceMat)
                {
                    Color col = _customization.Colors[System.Array.IndexOf(_references.Materials, sourceMat)];

                    if (!mat.IsKeywordEnabled("_EMISSION"))
                        mat.color = col;
                    else
                        mat.SetColor("_EmissionColor", col);
                }
            }
        }

        if (_accessories != null && _accessories.Length > 0)
        {
            for (int i = 0; i < _accessories.Length; i++)
            {
                if (_accessories[i] != null)
                    _accessories[i].SetActive(i == _customization.ActiveAccessoryIndex);
            }
        }
    }

    private void LoadCustom()
    {
        if (!PlayerPrefs.HasKey(PP_CUSTOM))
        {
            SetAllAccessoriesOff();
            return;
        }

        string json = PlayerPrefs.GetString(PP_CUSTOM);
        _customization = JsonUtility.FromJson<Customization>(json);

        SetCustom();
    }

    private void SaveCustom()
    {
        string json = JsonUtility.ToJson(_customization);
        PlayerPrefs.SetString(PP_CUSTOM, json);
    }

    private void SetAllAccessoriesOff()
    {
        if (_accessories == null) return;
        foreach (var acc in _accessories)
            if (acc != null) acc.SetActive(false);
    }

    private void UpdateAnimation()
    {
        if (!_references.Player)
        {
            return;
        }

        if (_references.Anim.IsInTransition(0))
            return;

        AnimatorStateInfo currentState = _references.Anim.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextState = _references.Anim.GetNextAnimatorStateInfo(0);

        PlayerAnimationStateMapper currentStateMapper = _playerAnimationStateMapper.FirstOrDefault(m => m.PlayerState == _references.Player.State.CurrentState);

        if (currentStateMapper != null
            && !currentState.IsName(currentStateMapper.AnimatorState)
            && !nextState.IsName(currentStateMapper.AnimatorState)
            && !currentState.IsName(currentStateMapper.BlockingState))
        {
            _references.Anim.SetTrigger(currentStateMapper.Trigger);
        }

        switch (_references.Player.State.CurrentState)
        {
            case Player.PlayerState.Idle:
            case Player.PlayerState.Moving:
                _references.Anim.SetFloat("move", _references.Player.State.HorizontalVelocity.magnitude);
                break;
        }
    }

    public static Color ShiftHue(Color color, float hueShift)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);

        h += hueShift / 360f;
        h = Mathf.Repeat(h, 1f);

        Color result = Color.HSVToRGB(h, s, v);
        result.a = color.a;

        return result;
    }
}