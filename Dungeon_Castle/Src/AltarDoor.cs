using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon_Castle
{
    public class AltarDoor : MonoBehaviour, Interactable, Hoverable
    {
        public List<ItemStand> itemStands = new List<ItemStand>();
    
        public GameObject offeringItem;
    
        public Door targetDoor;

        public string m_name = "Sacrificial Altar";

        public bool Interact(Humanoid character, bool hold, bool alt)
        {
            Debug.Log("Player has called interact on altar");
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
            Debug.Log("checking offering for altar");
            List<ItemStand> itemStands = this.itemStands;
            foreach (ItemStand itemStand in itemStands)
            {
                if (itemStand.m_visualName != offeringItem.name)
                {
                    return false;
                }
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
                Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] " + "Sacrifice");
            return hoverText;
        }
        
        public string GetHoverName() => Localization.instance.Localize(this.m_name);
    }
}

