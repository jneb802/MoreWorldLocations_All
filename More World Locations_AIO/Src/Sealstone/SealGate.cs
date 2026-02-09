using UnityEngine;

namespace More_World_Locations_AIO.Sealstone;

public class SealGate : MonoBehaviour, Interactable, Hoverable
{
    public string m_name = "Sealed Gate";
    public int m_keyTier = 1;
    public string m_pyrePrefabName = "";
    public GameObject m_barrier;
    public GameObject m_poweredIndicator;

    private ZNetView m_nview;
    private ZDOID m_targetPyreZDOID = ZDOID.None;

    private void Awake()
    {
        m_nview = GetComponent<ZNetView>();
        if (m_nview == null || m_nview.GetZDO() == null) return;

        m_targetPyreZDOID = m_nview.GetZDO().GetZDOID("targetPyreZDOID");

        UpdateVisuals();
    }

    private bool IsOpened()
    {
        return m_nview != null && m_nview.GetZDO() != null && m_nview.GetZDO().GetBool("opened");
    }

    private bool IsPyreLit()
    {
        // Try cached ZDOID first
        if (m_targetPyreZDOID != ZDOID.None)
        {
            ZDO pyreZDO = ZDOMan.instance.GetZDO(m_targetPyreZDOID);
            if (pyreZDO != null)
                return pyreZDO.GetBool("lit");
        }

        // Fallback: search sector
        if (string.IsNullOrEmpty(m_pyrePrefabName)) return false;

        GameObject pyre = SealstoneHelper.FindGameObjectInSector(m_pyrePrefabName, transform.position);
        if (pyre == null) return false;

        ZNetView pyreNview = pyre.GetComponent<ZNetView>();
        if (pyreNview == null || pyreNview.GetZDO() == null) return false;

        // Cache for future lookups
        m_targetPyreZDOID = pyreNview.GetZDO().m_uid;
        if (m_nview != null && m_nview.GetZDO() != null)
            m_nview.GetZDO().Set("targetPyreZDOID", m_targetPyreZDOID);

        return pyreNview.GetZDO().GetBool("lit");
    }

    private void UpdateVisuals()
    {
        bool opened = IsOpened();

        if (m_barrier != null)
            m_barrier.SetActive(!opened);

        if (m_poweredIndicator != null)
            m_poweredIndicator.SetActive(!opened && IsPyreLit());
    }

    public bool Interact(Humanoid character, bool hold, bool alt)
    {
        if (hold) return false;
        if (m_nview == null || m_nview.GetZDO() == null) return false;

        if (IsOpened())
            return false;

        if (!IsPyreLit())
        {
            character.Message(MessageHud.MessageType.Center, "The gate is sealed. A nearby pyre must be activated.");
            return false;
        }

        Player player = character as Player;
        if (player == null) return false;

        string keyPrefabName = "MWL_SealKey_Tier" + m_keyTier;
        Inventory inventory = player.GetInventory();

        if (!inventory.HaveItem(keyPrefabName, false))
        {
            character.Message(MessageHud.MessageType.Center, "Requires a Tier " + m_keyTier + " Seal Key");
            return false;
        }

        inventory.RemoveItem(keyPrefabName, 1, -1, false);

        m_nview.GetZDO().Set("opened", true);

        UpdateVisuals();

        character.Message(MessageHud.MessageType.Center, "The gate opens");
        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        return false;
    }

    public string GetHoverText()
    {
        if (IsOpened())
            return "";

        if (!IsPyreLit())
        {
            UpdateVisuals();
            return Localization.instance.Localize(m_name + "\n<color=#888888>The gate is sealed</color>");
        }

        UpdateVisuals();
        return Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] Unseal");
    }

    public string GetHoverName()
    {
        return Localization.instance.Localize(m_name);
    }
}
