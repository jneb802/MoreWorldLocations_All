using Jotunn.Managers;
using SoftReferenceableAssets;
using UnityEngine;
using More_World_Locations_AIO.Shrines;

namespace More_World_Locations_AIO;

internal static class ShrineEventLocation
{
    internal static void RegisterPrefabPatch(string locationName)
    {
        if (locationName != "MWL_ShrineEvent1") return;

        SoftReference<GameObject> prefab = AssetManager.Instance.GetSoftReference<GameObject>(locationName);
        AssetManager.Instance.ResolveMocksOnLoad(prefab.m_assetID, null, resolved =>
        {
            if (resolved is not GameObject locationRoot) return;

            foreach (Transform child in locationRoot.GetComponentsInChildren<Transform>(true))
            {
                if (child.name != "VF_wild_shrine_yellow1" && child.name != "VF_wild_shrine_yellow1(Clone)") continue;
                // Keep the resolved model, colliders and effects. Only this embedded
                // shrine uses our API interaction; outdoor Fortress shrines are untouched.
                foreach (MonoBehaviour behaviour in child.GetComponents<MonoBehaviour>())
                {
                    string type = behaviour.GetType().FullName;
                    if (type == "ValheimFortress.Challenge.WildShrine" || type == "ValheimFortress.Challenge.Spawner")
                        Object.DestroyImmediate(behaviour);
                }
                FortressTributeShrine tribute = child.GetComponent<FortressTributeShrine>();
                if (tribute == null) tribute = child.gameObject.AddComponent<FortressTributeShrine>();
                tribute.SpawnRoot = locationRoot.transform.Find("Interior/ShrineEventSpawnPoints");
            }
        });
    }
}
