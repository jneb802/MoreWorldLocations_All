using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn;
using Mono.Security;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

namespace Dungeon_The_Ritual;


public class AttachPuzzle : MonoBehaviour
{
    
    public List<ItemStand> puzzleItemStandList = new List<ItemStand>();
    public List<int> puzzleSolution = new List<int>();
    public int countPuzzleStands = new int();
    public Door targetDoor;
    public Switch puzzleSwitch;
    public string solutionItemName = "SurtlingCore";
    
    public void Awake()
    {
        GetItemStands(this.gameObject);
        GetDoor(this.gameObject);
        GetSwitch(this.gameObject);
        GenerateSolution();
        Debug.Log("AttachPuzzle called awake");
        
        // Assign the switch callback
        if (puzzleSwitch != null)
        {
            Debug.Log("Switch assigned");
            puzzleSwitch.m_onUse = OnSwitchUse;
        }
    }

    public void GetItemStands(GameObject gameobject)
    {
        if (gameobject != null)
        {
            puzzleItemStandList = gameobject.GetComponentsInChildren<ItemStand>(true).ToList();
            countPuzzleStands = puzzleItemStandList.Count;
            Debug.Log("Found item stands: " + countPuzzleStands);
        }
        else
        {
            Debug.Log("Failed to item stands");
        }
    }
    
    public void GetDoor(GameObject gameobject)
    {
        if (gameobject != null)
        {
            targetDoor = gameobject.GetComponentInChildren<Door>(true);
            Debug.Log("Found door: " + targetDoor);
        }
        else
        {
            Debug.Log("Failed to find door");
        }
    }
    
    public void GetSwitch(GameObject gameobject)
    {
        if (gameobject != null)
        {
            puzzleSwitch = gameobject.GetComponentInChildren<Switch>(true);
            Debug.Log("Found switch: " + puzzleSwitch);
        }
        else
        {
            Debug.Log("Failed to find switch");
        }
    }
    
    public bool OnSwitchUse(Switch caller, Humanoid user, ItemDrop.ItemData item)
    {
        if (CheckSolution())
        {
            targetDoor.Open(Vector3.forward);
            Debug.Log("Puzzle solved! Door is opening.");
            return true;
        }
        else
        {
            Debug.Log("Puzzle solution is incorrect.");
            return false;
        }
    }

    public void GenerateSolution()
    {
        if (countPuzzleStands > 0)
        {
            puzzleSolution.Clear();
            
            for (int i = 0; i < countPuzzleStands; i++)
            {
                puzzleSolution.Add(0);
            }
            
            for (int i = 0; i < countPuzzleStands; i++)
            {
                int answer = Random.Range(0, 2);
                int.TryParse(puzzleItemStandList[i].gameObject.name, out int position);
                // Ensure position is within bounds
                if (position >= 0 && position < puzzleSolution.Count)
                {
                    puzzleSolution[position] = answer;
                }
                else
                {
                    Debug.LogWarning("Position " + position + " is out of bounds for puzzle solution list.");
                }
            }
        
            Debug.Log("Solution for puzzle generated. Answer is: " + string.Join(",", puzzleSolution));
        }
    }

    public bool CheckSolution()
    { 
        List<int> tempPuzzleSolution = Enumerable.Repeat(0, countPuzzleStands).ToList();
        
        if (puzzleItemStandList == null || targetDoor == null)
        {
            return false;
        }
        
        
        foreach (ItemStand itemStand in puzzleItemStandList)
        {
            if (itemStand == null)
            {
                continue;
            }
            
            if (itemStand.m_nview == null)
            {
                continue;
            }
            
            if (itemStand.m_nview.GetZDO() == null)
            {
                continue;
            }
            
            string attachItemName = itemStand.GetAttachedItem();
            Debug.Log("Checking solution. Item attached has name " + attachItemName);
        
            if (int.TryParse(itemStand.gameObject.name, out int position))
            {
                if (position >= 0 && position < tempPuzzleSolution.Count)
                {
                    tempPuzzleSolution[position] = attachItemName == solutionItemName ? 1 : 0;
                }
                else
                {
                    Debug.LogWarning("Position " + position + " is out of bounds for temp puzzle solution.");
                }
            }
            else
            {
                Debug.LogError("Failed to parse position from GameObject name: " + itemStand.gameObject.name);
            }
        }
        
        bool isCorrectSolution = tempPuzzleSolution.SequenceEqual(puzzleSolution);
        Debug.Log("Checking solution. Provided answer is: " + string.Join(",", tempPuzzleSolution));
        return isCorrectSolution;
    }
}

