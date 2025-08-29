using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace More_World_Locations_AIO.Managers;

[PublicAPI]
public static class FontManager
{
    public enum FontOptions
    {
        Norse, 
        NorseBold, 
        AveriaSerifLibre,
        AveriaSerifLibreBold,
        AveriaSerifLibreLight,
        LegacyRuntime
    }
    
    private static readonly Dictionary<FontOptions, Font?> m_fonts = new();
    private static readonly List<TextFont> m_allTexts = new();
    
    private static string GetFontName(FontOptions option) => option switch
    {
        FontOptions.Norse => "Norse",
        FontOptions.AveriaSerifLibre => "AveriaSerifLibre-Regular",
        FontOptions.AveriaSerifLibreBold => "AveriaSerifLibre-Bold",
        FontOptions.AveriaSerifLibreLight => "AveriaSerifLibre-Light",
        FontOptions.NorseBold => "Norsebold",
        FontOptions.LegacyRuntime => "LegacyRuntime",
        _ => "AveriaSerifLibre-Regular"
    };

    public static Font? GetFont(FontOptions option)
    {
        if (m_fonts.TryGetValue(option, out Font? font)) return font;
        Font[]? fonts = Resources.FindObjectsOfTypeAll<Font>();
        Font? match = fonts.FirstOrDefault(x => x.name == GetFontName(option));
        m_fonts[option] = match;
        return match;
    }

    public static void OnFontChange(object sender, EventArgs args)
    {
        // change the input to your config.Value
        // if you want to allow users to choose a font
        Font? font = GetFont(FontOptions.AveriaSerifLibre);
        // since we register all the texts to font manager
        // we can loop over them and update
        foreach (var text in m_allTexts) text.Update(font);
    }

    public static void SetFont(Text[] array)
    {
        foreach (Text text in array)
        {
            new TextFont(text, GetFont(FontOptions.AveriaSerifLibre)); // averia serif is my default font, since that is what valheim mostly uses
        }
    }

    private class TextFont
    {
        private readonly Text m_text;
        public TextFont(Text text, Font? font)
        {
            m_text = text;
            m_text.SetFont(font);
            m_allTexts.Add(this);
        }
        public void Update(Font? font) => m_text.SetFont(font);
    }
    private static void SetFont(this Text text, Font? font) => text.font = font;
}