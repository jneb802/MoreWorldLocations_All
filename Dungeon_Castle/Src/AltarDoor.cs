using System;
using System.Collections.Generic;
using UnityEngine;

namespace Forbidden_Catacombs
{
    public class AltarDoor : MonoBehaviour, Interactable, Hoverable
    {
        public List<ItemStand> itemStands = new List<ItemStand>();

        public GameObject offeringItem;

        public Door targetDoor;
       
        public string m_name = "Sacrificial Altar";

        private ZNetView m_nview;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
        
            ZDOID targetDoorID = m_nview.GetZDO().GetZDOID("targetDoorZDOID");
            
            if (targetDoorID.ToString() != "0:0")
            {
                targetDoor = ZNetScene.instance.FindInstance(targetDoorID).gameObject.GetComponent<Door>();
            }
        }
        
        public bool Interact(Humanoid character, bool hold, bool alt)
        {
            if (targetDoor == null)
            {
                GameObject door = FindGameObjectInSector("CD_kit_secretdoor", this.gameObject.transform.position);
        
                if (door != null)
                {
                    targetDoor = door.GetComponent<Door>();
                    
                    ZDOID targetDoorID = targetDoor.GetComponent<ZNetView>().GetZDO().m_uid;
                    m_nview.GetZDO().Set("targetDoorZDOID",targetDoorID);
                }
            }
            
            if (this.offeringItem == null)
                return false;
        
            if (CheckOffering())
            {
                Vector3 normalized = (character.transform.position - this.transform.position).normalized;
                targetDoor.Open(normalized);
                return true;
            }
        
            return false;
        }
        
        public bool CheckOffering()
        {
            List<ItemStand> itemStands = this.itemStands;
            foreach (ItemStand itemStand in itemStands)
            {
                if (itemStand.m_visualName != offeringItem.name)
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
            string hoverText =
                Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] " +
                                               "Sacrifice");
            return hoverText;
        }
        
        public string GetHoverName() => Localization.instance.Localize(this.m_name);
        
        public GameObject FindGameObjectInSector(string prefabName, Vector3 position)
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
                int zdoPrefabHash = zdo.GetPrefab();
        
                if (zdoPrefabHash != prefabHash)
                {
                    continue;
                }
        
                GameObject instance = ZNetScene.instance.FindInstance(zdo)?.gameObject;
                if (instance != null)
                {
                    return instance;
                }
            }
            
            return null;
        }

    }
}

