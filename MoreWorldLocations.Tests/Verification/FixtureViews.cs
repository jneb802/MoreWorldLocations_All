using UnityEngine;

/// <summary>
/// What MWL's fixtures stand for: vanilla prefabs, whose networked objects are saved with the world. The game's ZNetView
/// defaults <c>m_persistent</c> to false and every such prefab sets it, so a fixture's view is persistent unless a test says
/// otherwise (Valheim.Testing.Doubles 0.1.0-preview.6 has the game's default).
/// </summary>
internal static class FixtureViews
{
    public static ZNetView AddPersistentView(this GameObject go)
    {
        ZNetView view = go.AddComponent<ZNetView>();
        view.m_persistent = true;
        return view;
    }
}
