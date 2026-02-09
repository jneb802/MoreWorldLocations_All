using UnityEngine;

namespace More_World_Locations_AIO.Sealstone;

public class SealstonePyre : MonoBehaviour, Interactable, Hoverable
{
    public string m_name = "Sealstone Pyre";
    public string m_fuelItemName = "SurtlingCore";
    public string m_gatePrefabName = "";
    public GameObject m_fireVFX;

    private ZNetView m_nview;
    private ZDOID m_targetGateZDOID = ZDOID.None;

    private void Awake()
    {
        m_nview = GetComponent<ZNetView>();
        if (m_nview == null || m_nview.GetZDO() == null) return;

        m_targetGateZDOID = m_nview.GetZDO().GetZDOID("targetGateZDOID");

        UpdateVisuals();
    }

    private bool IsLit()
    {
        return m_nview != null && m_nview.GetZDO() != null && m_nview.GetZDO().GetBool("lit");
    }

    private void UpdateVisuals()
    {
        if (m_fireVFX != null)
            m_fireVFX.SetActive(IsLit());
    }

    public bool Interact(Humanoid character, bool hold, bool alt)
    {
        if (hold) return false;
        if (m_nview == null || m_nview.GetZDO() == null) return false;

        if (IsLit())
        {
            character.Message(MessageHud.MessageType.Center, "The pyre is already lit");
            return false;
        }

        Player player = character as Player;
        if (player == null) return false;

        Inventory inventory = player.GetInventory();
        if (!inventory.HaveItem(m_fuelItemName, false))
        {
            character.Message(MessageHud.MessageType.Center, "$msg_donthaveitem");
            return false;
        }

        inventory.RemoveItem(m_fuelItemName, 1, -1, false);

        m_nview.GetZDO().Set("lit", true);

        // Find and cache the paired gate
        if (m_targetGateZDOID == ZDOID.None && !string.IsNullOrEmpty(m_gatePrefabName))
        {
            GameObject gate = SealstoneHelper.FindGameObjectInSector(m_gatePrefabName, transform.position);
            if (gate != null)
            {
                ZNetView gateNview = gate.GetComponent<ZNetView>();
                if (gateNview != null && gateNview.GetZDO() != null)
                {
                    m_targetGateZDOID = gateNview.GetZDO().m_uid;
                    m_nview.GetZDO().Set("targetGateZDOID", m_targetGateZDOID);
                }
            }
        }

        UpdateVisuals();

        character.Message(MessageHud.MessageType.Center, "The pyre ignites");
        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        return false;
    }

    public string GetHoverText()
    {
        if (IsLit())
            return Localization.instance.Localize(m_name + "\n<color=#888888>The pyre burns</color>");

        return Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] Ignite");
    }

    public string GetHoverName()
    {
        return Localization.instance.Localize(m_name);
    }
}
