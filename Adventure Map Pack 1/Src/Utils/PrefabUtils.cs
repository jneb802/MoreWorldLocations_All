using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Adventure_Map_Pack_1.Utils;

public class PrefabUtils
{
    public static CustomPrefab vfx_ImmuneShieldPrefab;
    public static EffectList immuneShieldBreak;
    public static EffectList immuneShieldHit;
    public static EffectList immuneShieldBubble;
    
    public static void CreateCustomPrefabs()
    {
        GameObject vfx_GoblinShield = PrefabManager.Instance.GetPrefab("vfx_GoblinShield");
        if (vfx_GoblinShield == null)
        {
            Debug.Log("vfx_GoblinShield is null");
        }
        
        GameObject vfx_GoblinShield_Clone = PrefabManager.Instance.CreateClonedPrefab("vfx_ImmuneShieldPrefab",vfx_GoblinShield);
        CustomPrefab vfx_ImmuneShieldPrefab = new CustomPrefab(vfx_GoblinShield_Clone, false);
        
        GameObject Sphere = vfx_ImmuneShieldPrefab.Prefab.FindDeepChild("Sphere").gameObject;
        if (Sphere == null)
        {
            Debug.Log("sphere is null");
        }
        
        MeshRenderer meshRenderer = Sphere.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.Log("meshRenderer is null");
        }
        
        Material totemMaterial  = Adventure_Map_Pack_1Plugin.assetBundleTotem.LoadAsset<Material>("totem_immune_shield");
        if (totemMaterial == null)
        {
            Debug.Log("totemMaterial is null");
        }

        meshRenderer.material = totemMaterial;
        
        PrefabManager.Instance.AddPrefab(vfx_ImmuneShieldPrefab);
        
        PrefabManager.OnVanillaPrefabsAvailable -= CreateCustomPrefabs;

        CreateEffects();
    }
    
    public static void CreateEffects()
    {
        Debug.Log("create effects 1");
        
        EffectList breakEffectList = new EffectList();
        EffectList.EffectData[] breakEffect = new EffectList.EffectData[1];
        breakEffect[0] = new EffectList.EffectData();
        breakEffect[0].m_enabled = true;
        breakEffect[0].m_variant = -1;
        breakEffect[0].m_prefab = PrefabManager.Instance.GetPrefab("fx_GoblinShieldBreak");
        breakEffectList.m_effectPrefabs = breakEffect;
        immuneShieldBreak = breakEffectList;
        
        Debug.Log("create effects 2");
        EffectList hitEffectList = new EffectList();
        EffectList.EffectData[] hitEffect = new EffectList.EffectData[1];
        hitEffect[0] = new EffectList.EffectData();
        hitEffect[0].m_enabled = true;
        hitEffect[0].m_variant = -1;
        hitEffect[0].m_prefab = PrefabManager.Instance.GetPrefab("fx_GoblinShieldHit");
        hitEffectList.m_effectPrefabs = hitEffect;
        immuneShieldHit = hitEffectList;
        
        Debug.Log("create effects 3");
        EffectList startEffectList = new EffectList();
        EffectList.EffectData[] startEffect = new EffectList.EffectData[1];
        startEffect[0] = new EffectList.EffectData();
        startEffect[0].m_enabled = true;
        startEffect[0].m_variant = -1;
        startEffect[0].m_attach = true;
        startEffect[0].m_scale = true;
        Debug.Log("create effects 4");
        startEffect[0].m_prefab = PrefabManager.Instance.GetPrefab("vfx_ImmuneShieldPrefab").gameObject;
        startEffectList.m_effectPrefabs = startEffect;
        Debug.Log("create effects 5");
        immuneShieldBubble = startEffectList;
    }
}