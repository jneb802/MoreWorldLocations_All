using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;

namespace More_World_Locations_AIO.Managers;

[PublicAPI]
public class LocalizeKey
{
	public static readonly List<LocalizeKey> keys = new();

	public readonly string Key;
	public readonly Dictionary<string, string> Localizations = new();

	public LocalizeKey(string key)
	{
		Key = key.Replace("$", "");
		keys.Add(this);
	}

	public void Alias(string alias)
	{
		Localizations.Clear();
		if (!alias.Contains("$"))
		{
			alias = $"${alias}";
		}
		Localizations["alias"] = alias;
		Localization.instance.AddWord(Key, Localization.instance.Localize(alias));
	}

	public LocalizeKey English(string key) => addForLang("English", key);
	public LocalizeKey Swedish(string key) => addForLang("Swedish", key);
	public LocalizeKey French(string key) => addForLang("French", key);
	public LocalizeKey Italian(string key) => addForLang("Italian", key);
	public LocalizeKey German(string key) => addForLang("German", key);
	public LocalizeKey Spanish(string key) => addForLang("Spanish", key);
	public LocalizeKey Russian(string key) => addForLang("Russian", key);
	public LocalizeKey Romanian(string key) => addForLang("Romanian", key);
	public LocalizeKey Bulgarian(string key) => addForLang("Bulgarian", key);
	public LocalizeKey Macedonian(string key) => addForLang("Macedonian", key);
	public LocalizeKey Finnish(string key) => addForLang("Finnish", key);
	public LocalizeKey Danish(string key) => addForLang("Danish", key);
	public LocalizeKey Norwegian(string key) => addForLang("Norwegian", key);
	public LocalizeKey Icelandic(string key) => addForLang("Icelandic", key);
	public LocalizeKey Turkish(string key) => addForLang("Turkish", key);
	public LocalizeKey Lithuanian(string key) => addForLang("Lithuanian", key);
	public LocalizeKey Czech(string key) => addForLang("Czech", key);
	public LocalizeKey Hungarian(string key) => addForLang("Hungarian", key);
	public LocalizeKey Slovak(string key) => addForLang("Slovak", key);
	public LocalizeKey Polish(string key) => addForLang("Polish", key);
	public LocalizeKey Dutch(string key) => addForLang("Dutch", key);
	public LocalizeKey Portuguese_European(string key) => addForLang("Portuguese_European", key);
	public LocalizeKey Portuguese_Brazilian(string key) => addForLang("Portuguese_Brazilian", key);
	public LocalizeKey Chinese(string key) => addForLang("Chinese", key);
	public LocalizeKey Japanese(string key) => addForLang("Japanese", key);
	public LocalizeKey Korean(string key) => addForLang("Korean", key);
	public LocalizeKey Hindi(string key) => addForLang("Hindi", key);
	public LocalizeKey Thai(string key) => addForLang("Thai", key);
	public LocalizeKey Abenaki(string key) => addForLang("Abenaki", key);
	public LocalizeKey Croatian(string key) => addForLang("Croatian", key);
	public LocalizeKey Georgian(string key) => addForLang("Georgian", key);
	public LocalizeKey Greek(string key) => addForLang("Greek", key);
	public LocalizeKey Serbian(string key) => addForLang("Serbian", key);
	public LocalizeKey Ukrainian(string key) => addForLang("Ukrainian", key);

	private LocalizeKey addForLang(string lang, string value)
	{
		Localizations[lang] = value;
		if (Localization.instance.GetSelectedLanguage() == lang)
		{
			Localization.instance.AddWord(Key, value);
		}
		else if (lang == "English" && !Localization.instance.m_translations.ContainsKey(Key))
		{
			Localization.instance.AddWord(Key, value);
		}
		return this;
	}

	[HarmonyPriority(Priority.LowerThanNormal)]
	internal static void AddLocalizedKeys(Localization __instance, string language)
	{
		foreach (LocalizeKey key in keys)
		{
			if (key.Localizations.TryGetValue(language, out string Translation) || key.Localizations.TryGetValue("English", out Translation))
			{
				__instance.AddWord(key.Key, Translation);
			}
			else if (key.Localizations.TryGetValue("alias", out string alias))
			{
				__instance.AddWord(key.Key, Localization.instance.Localize(alias));
			}
		}
	}

	public static Dictionary<string, string> GetKeys(string language)
	{
		Dictionary<string, string> output = new Dictionary<string, string>();
		foreach (var key in keys)
		{
			if (!key.Localizations.TryGetValue(language, out string translation)) continue;
			output.Add(key.Key, translation);
		}

		return output;
	}
}