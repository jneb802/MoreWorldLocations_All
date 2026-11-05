using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace More_World_Locations_AIO;

/// <summary>
/// Builds TextMeshPro labels that can show Valheim's controller glyphs.
/// Valheim spells glyphs as $KEY_ButtonA style placeholders; Localization turns them into the
/// sprite tag for the connected controller type (Xbox, PlayStation, Switch), so the same
/// placeholder renders correctly on every pad. The port UI prefab uses legacy UI.Text, which
/// cannot render sprite tags, so every hint is a TextMeshProUGUI created here.
/// </summary>
public static class ControllerHints
{
    public const string ButtonA = "$KEY_ButtonA";
    public const string ButtonB = "$KEY_ButtonB";
    public const string ButtonX = "$KEY_ButtonX";
    public const string ButtonY = "$KEY_ButtonY";
    public const string LeftBumper = "$KEY_TabLeft";
    public const string RightBumper = "$KEY_TabRight";
    public const string DPadUp = "$KEY_DPadUp";
    public const string DPadDown = "$KEY_DPadDown";
    public const string DPadLeft = "$KEY_DPadLeft";
    public const string DPadRight = "$KEY_DPadRight";
    public const string RightStickUp = "$KEY_RStickUp";
    public const string RightStickDown = "$KEY_RStickDown";

    private static TMP_FontAsset? s_font;
    private static Material? s_fontMaterial;

    public static bool GamepadActive => ZInput.IsGamepadActive();

    private static void CacheFont()
    {
        if (s_font != null) return;
        TMP_Text? template = InventoryGui.instance != null ? InventoryGui.instance.m_recipeName : null;
        if (template == null) template = Object.FindAnyObjectByType<TextMeshProUGUI>();
        if (template == null) return;
        s_font = template.font;
        s_fontMaterial = template.fontSharedMaterial;
    }

    public static TextMeshProUGUI CreateText(Transform parent, string name, float fontSize, TextAlignmentOptions alignment = TextAlignmentOptions.Left, bool wrap = false)
    {
        CacheFont();
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        if (s_font != null) text.font = s_font;
        if (s_fontMaterial != null) text.fontSharedMaterial = s_fontMaterial;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        text.richText = true;
        text.color = Color.white;
        return text;
    }

    /// <summary>
    /// Several glyphs for one hint. Localization reads a $token up to the next separator and '$'
    /// is not one, so adjacent placeholders have to be separated by a space.
    /// </summary>
    public static string Glyphs(params string[] keys) => string.Join(" ", keys);

    /// <summary>
    /// One hint line: the glyph followed by the label, like the vanilla map hints. A single glyph
    /// sits in the vanilla monospace span; several glyphs are wider than that span and would
    /// overlap, so they keep their natural advance.
    /// </summary>
    public static string Format(string glyphs, string label) =>
        glyphs.Contains(" ") ? $"{glyphs} {label}" : $"<mspace=0.6em>{glyphs} </mspace>{label}";

    /// <summary>
    /// Resets a gamepad button together with every other binding on the same physical control.
    /// Jump, use and dodge share the A and B buttons depending on the input layout, so a press the
    /// UI has handled must not still read as down when the player controller runs next, or the
    /// player jumps on the way out of the menu.
    /// </summary>
    public static void Consume(string button)
    {
        ZInput input = ZInput.instance;
        if (input == null) return;
        try
        {
            string path = BindingPath(input, button);
            foreach (string name in input.m_buttons.Keys)
            {
                if (name == button || (path.Length > 0 && name.StartsWith("Joy") && BindingPath(input, name) == path))
                {
                    ZInput.ResetButtonStatus(name);
                }
            }
        }
        catch (Exception e)
        {
            // Never let a lookup problem swallow the press itself.
            ZInput.ResetButtonStatus(button);
            Debug.LogWarning($"MWL: could not resolve gamepad aliases for {button}: {e.Message}");
        }
    }

    private static ZInput? s_pathsFor;
    private static readonly Dictionary<string, string> s_bindingPaths = new();

    // The first binding path of a button ("<Gamepad>/buttonSouth" and the like). The input system
    // types are read through reflection since the project does not reference Unity.InputSystem;
    // the result is cached per ZInput instance because the gamepad buttons are not rebindable.
    private static string BindingPath(ZInput input, string name)
    {
        if (s_pathsFor != input)
        {
            s_pathsFor = input;
            s_bindingPaths.Clear();
        }
        if (s_bindingPaths.TryGetValue(name, out string cached)) return cached;
        string path = "";
        if (input.m_buttons.TryGetValue(name, out ZInput.ButtonDef def))
        {
            object? action = Traverse.Create(def).Property("ButtonAction").GetValue();
            object? bindings = action != null ? Traverse.Create(action).Property("bindings").GetValue() : null;
            if (bindings is System.Collections.IEnumerable list)
            {
                foreach (object binding in list)
                {
                    path = Traverse.Create(binding).Property("effectivePath").GetValue<string>() ?? "";
                    break;
                }
            }
        }
        s_bindingPaths[name] = path;
        return path;
    }

    public static string Localize(string template) => Localization.instance.Localize(template);

    /// <summary>
    /// A row (or column) of hint lines. Templates are re-localized periodically so a controller swap
    /// or a layout change picks up the right glyphs without any extra plumbing.
    /// </summary>
    public class HintBar
    {
        private readonly GameObject m_root;
        private readonly RectTransform m_rect;
        private readonly List<TextMeshProUGUI> m_texts = new();
        private readonly List<string> m_templates = new();
        private readonly float m_fontSize;
        private float m_refreshTimer;
        private const float RefreshInterval = 1f;

        public RectTransform Rect => m_rect;
        public bool Visible => m_root.activeSelf;

        public HintBar(Transform parent, string name, float fontSize, bool horizontal, float spacing)
        {
            m_fontSize = fontSize;
            m_root = new GameObject(name, typeof(RectTransform));
            m_root.transform.SetParent(parent, false);
            m_rect = m_root.GetComponent<RectTransform>();
            HorizontalOrVerticalLayoutGroup layout = horizontal
                ? m_root.AddComponent<HorizontalLayoutGroup>()
                : m_root.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            ContentSizeFitter fitter = m_root.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public void Set(List<string> templates)
        {
            bool changed = templates.Count != m_templates.Count;
            for (int i = 0; !changed && i < templates.Count; ++i) changed = templates[i] != m_templates[i];
            if (!changed) return;
            m_templates.Clear();
            m_templates.AddRange(templates);
            while (m_texts.Count < m_templates.Count)
            {
                m_texts.Add(CreateText(m_root.transform, $"Hint{m_texts.Count}", m_fontSize));
            }
            for (int i = 0; i < m_texts.Count; ++i)
            {
                m_texts[i].gameObject.SetActive(i < m_templates.Count);
            }
            Relocalize();
        }

        public void SetVisible(bool visible)
        {
            if (m_root.activeSelf == visible) return;
            m_root.SetActive(visible);
            if (visible) Relocalize();
        }

        public void Update(float dt)
        {
            if (!m_root.activeInHierarchy) return;
            m_refreshTimer += dt;
            if (m_refreshTimer < RefreshInterval) return;
            m_refreshTimer = 0f;
            Relocalize();
        }

        private void Relocalize()
        {
            m_refreshTimer = 0f;
            for (int i = 0; i < m_templates.Count; ++i)
            {
                string localized = Localize(m_templates[i]);
                if (m_texts[i].text != localized) m_texts[i].text = localized;
            }
        }

        public void Destroy()
        {
            if (m_root != null) Object.Destroy(m_root);
            m_texts.Clear();
            m_templates.Clear();
        }
    }
}
