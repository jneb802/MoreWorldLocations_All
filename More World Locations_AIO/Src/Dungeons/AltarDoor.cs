#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Forbidden_Catacombs
{
    // Keep the namespace and type name stable so Unity-authored prefabs can be rebound
    // in the editor while the runtime implementation now lives in More_World_Locations_AIO.
    public class AltarDoor : MonoBehaviour, Interactable, Hoverable
    {
        public List<ItemStand> itemStands = new();

        public GameObject? offeringItem;

        public Door? targetDoor;

        public string m_name = "Sacrificial Altar";

        private ZNetView? m_nview;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            if (m_nview == null || m_nview.GetZDO() == null)
            {
                return;
            }

            ZDOID targetDoorID = m_nview.GetZDO().GetZDOID("targetDoorZDOID");
            if (targetDoorID.ToString() != "0:0")
            {
                GameObject? targetDoorObject = ZNetScene.instance.FindInstance(targetDoorID);
                if (targetDoorObject != null)
                {
                    targetDoor = targetDoorObject.GetComponent<Door>();
                }
            }
        }

        public bool Interact(Humanoid character, bool hold, bool alt)
        {
            if (m_nview == null || m_nview.GetZDO() == null)
            {
                return false;
            }

            if (targetDoor == null)
            {
                GameObject door = FindGameObjectInSector("CD_kit_secretdoor", gameObject.transform.position);
                if (door != null)
                {
                    targetDoor = door.GetComponent<Door>();
                    ZNetView? targetDoorView = targetDoor?.GetComponent<ZNetView>();
                    if (targetDoorView?.GetZDO() != null)
                    {
                        ZDOID targetDoorID = targetDoorView.GetZDO().m_uid;
                        m_nview.GetZDO().Set("targetDoorZDOID", targetDoorID);
                    }
                }
            }

            if (offeringItem == null || targetDoor == null)
            {
                return false;
            }

            if (CheckOffering())
            {
                Vector3 normalized = (character.transform.position - transform.position).normalized;
                targetDoor.Open(normalized);
                return true;
            }

            return false;
        }

        public bool CheckOffering()
        {
            foreach (ItemStand itemStand in itemStands)
            {
                if (itemStand.m_visualName != offeringItem!.name)
                {
                    return false;
                }

                itemStand.DestroyAttachment();
            }

            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return true;
        }

        public string GetHoverText()
        {
            return Localization.instance.Localize(
                m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] Sacrifice");
        }

        public string GetHoverName() => Localization.instance.Localize(m_name);

        public GameObject? FindGameObjectInSector(string prefabName, Vector3 position)
        {
            int prefabHash = prefabName.GetStableHashCode();

            Vector2i sector = ZoneSystem.GetZone(position);
            int sectorIndex = ZDOMan.instance.SectorToIndex(sector);

            if (sectorIndex < 0 || sectorIndex >= ZDOMan.instance.m_objectsBySector.Length)
            {
                return null;
            }

            List<ZDO> sectorList = ZDOMan.instance.m_objectsBySector[sectorIndex];
            if (sectorList == null)
            {
                return null;
            }

            foreach (ZDO zdo in sectorList)
            {
                if (zdo.GetPrefab() != prefabHash)
                {
                    continue;
                }

                GameObject? instance = ZNetScene.instance.FindInstance(zdo)?.gameObject;
                if (instance != null)
                {
                    return instance;
                }
            }

            return null;
        }
    }
}
