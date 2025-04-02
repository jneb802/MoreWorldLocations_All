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
            Debug.Log("Altar door called awake");
            m_nview = GetComponent<ZNetView>();
        
            ZDOID targetDoorID = m_nview.GetZDO().GetZDOID("targetDoorZDOID");
            Debug.Log("Target door ID is " + targetDoorID.ToString());
            
            if (targetDoorID.ToString() != "0:0")
            {
                Debug.Log("target doorID is not null, grabbing from zdo");
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
            Debug.Log("Starting search for prefab with name: " + prefabName);
            Debug.Log("Starting search for prefab with hash: " + prefabHash);
        
            Debug.Log("Searching for sector that matches position: " + position);
            Vector2i sector = ZoneSystem.GetZone(position);
            Debug.Log("Determined sector coordinates: " + sector);
        
            int sectorIndex = ZDOMan.instance.SectorToIndex(sector);
            Debug.Log("Calculated sector index: " + sectorIndex);
        
            if (sectorIndex < 0 || sectorIndex >= ZDOMan.instance.m_objectsBySector.Length)
            {
                Debug.Log("Sector index out of range: " + sectorIndex);
                return null;
            }
        
            List<ZDO> sectorList = ZDOMan.instance.m_objectsBySector[sectorIndex];
            Debug.Log("Retrieved sector list, item count: " + (sectorList != null ? sectorList.Count : 0));
        
            if (sectorList == null)
            {
                Debug.Log("Sector list is null, no ZDOs to check");
                Debug.Log("No matching GameObject found in sector");
                return null;
            }
        
            foreach (ZDO zdo in sectorList)
            {
                int zdoPrefabHash = zdo.GetPrefab();
                Debug.Log("Checking ZDO with prefab hash: " + zdoPrefabHash);
        
                if (zdoPrefabHash != prefabHash)
                {
                    continue;
                }
        
                Debug.Log("Match found for prefab hash: " + prefabHash);
        
                GameObject instance = ZNetScene.instance.FindInstance(zdo)?.gameObject;
                if (instance != null)
                {
                    Debug.Log("Found instance of GameObject with name " + prefabName + " in sector");
                    return instance;
                }
                else
                {
                    Debug.Log("No active GameObject instance found for matching ZDO");
                }
            }
        
            Debug.Log("No matching GameObject found in sector");
            return null;
        }

    }
}

