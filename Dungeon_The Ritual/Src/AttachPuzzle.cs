using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jotunn;
using Mono.Security;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

namespace Underground_Ruins;

public class AttachPuzzle : MonoBehaviour
{
    public List<PuzzleStand> puzzleItemStandList = new List<PuzzleStand>();
    public List<PuzzleStand> puzzleSolutionPickableList = new List<PuzzleStand>();
    public List<int> puzzleSolution = new List<int>();
    public int countPuzzleStands = 8;
    public Door targetDoor;
    public Switch puzzleSwitch;
    public string solutionItemName = "SurtlingCore";
    public ZNetView zNetView;
    public bool initialized;

    public void Awake()
    {
        zNetView = this.gameObject.GetComponent<ZNetView>();
        zNetView.m_zdo.GetBool("BFD_puzzleInitialized", out bool value);
        initialized = value;
    }

    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        GetItemStands();
        GetPickables();
            
        GameObject targetDoorPrefab = FindGameObjectInSector("PuzzleSecretDoor",this.gameObject.transform.position);
        if (targetDoorPrefab != null)
        {
            targetDoor = targetDoorPrefab.GetComponent<Door>();
        }
        
        GameObject puzzleSwitchPrefab = FindGameObjectInSector("PuzzleLever",this.gameObject.transform.position);
        if (puzzleSwitchPrefab != null)
        {
            puzzleSwitch = puzzleSwitchPrefab.GetComponentInChildren<Switch>();
        } 
        
        if (puzzleSwitch != null)
        {
            puzzleSwitch.m_onUse = OnSwitchUse;
        }
        
        GenerateSolution();
        
        zNetView.m_zdo.Set("BFD_puzzleInitialized", true);
    }
    
    public IEnumerator DelayedPuzzleInit(Vector3 pos)
    {
        while (!ZNetScene.instance.IsAreaReady(pos))
        {
            yield return null;
        }
        
        Initialize();
    }

    
    public GameObject FindGameObjectInSector(string prefabName, Vector3 position)
    {
        int prefabHash = prefabName.GetStableHashCode();
        
        Vector2i sector = ZoneSystem.GetZone(position);
    
        int sectorIndex = ZDOMan.instance.SectorToIndex(sector);
    
        if (sectorIndex >= 0 && sectorIndex < ZDOMan.instance.m_objectsBySector.Length)
        {
            List<ZDO> sectorList = ZDOMan.instance.m_objectsBySector[sectorIndex];
        
            if (sectorList != null)
            {
                foreach (ZDO zdo in sectorList)
                {
                    int zdoPrefabHash = zdo.GetPrefab();
                
                    if (zdoPrefabHash == prefabHash)
                    {
                    
                        GameObject instance = ZNetScene.instance.FindInstance(zdo)?.gameObject;
                        if (instance != null)
                        {
                            return instance;
                        }
                    }
                }
            }
        }
        return null;
    }
    
    public void GetItemStands()
    {
        for (int i = 0; i < countPuzzleStands; i++)
        {
            string prefabName = string.Concat(i.ToString(),"_PuzzleStand");
            
            GameObject puzzleStandPrefab = FindGameObjectInSector(prefabName, this.gameObject.transform.position);

            if (puzzleStandPrefab == null)
            {
                // Debug.Log("Can not find puzzle stand with name " + prefabName + " in scene");
                continue;
            }
            
            PuzzleStand puzzleStand = new PuzzleStand()
            {
                PuzzleStandGameObject = puzzleStandPrefab,
                puzzleStand = true,
                puzzlePickable = false,
                puzzlePosition = i
            };
            
            puzzleItemStandList.Add(puzzleStand);
        }
    }
    
    public void GetPickables()
    {
        for (int i = 0; i < countPuzzleStands; i++)
        {
            string prefabName = string.Concat(i.ToString(),"_PuzzlePickable");
            GameObject puzzleStandPrefab = FindGameObjectInSector(prefabName, this.gameObject.transform.position);

            if (puzzleStandPrefab == null)
            {
                // Debug.Log("Can not find puzzle pickable with name " + prefabName + " in scene");
                continue;
            }
            
            PuzzleStand puzzleStand = new PuzzleStand()
            {
                PuzzleStandGameObject = puzzleStandPrefab,
                puzzleStand = false,
                puzzlePickable = true,
                puzzlePosition = i
            };
            
            puzzleSolutionPickableList.Add(puzzleStand);
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
        Debug.Log("Puzzle solution is incorrect.");
        return false;
        
    }

    public void GenerateSolution()
    {
        Debug.Log("Generating solution");
        if (puzzleSolutionPickableList.Count > 0 && puzzleSolutionPickableList != null)
        {
            puzzleSolution.Clear();
            
            for (int i = 0; i < countPuzzleStands; i++)
            {
                puzzleSolution.Add(0);
            }
            
            for (int i = 0; i < countPuzzleStands; i++)
            {
                int answer = Random.Range(0, 2);
                int.TryParse(puzzleSolutionPickableList[i].PuzzleStandGameObject.name[0].ToString(), out int position);
                if (position >= 0 && position < puzzleSolution.Count)
                {
                    puzzleSolution[position] = answer;
                    if (answer == 0)
                    {
                        puzzleSolutionPickableList[i].PuzzleStandGameObject.GetComponent<Pickable>().SetPicked(true);
                    }
                }
            }
        }
    }

    public bool CheckSolution()
    { 
        List<int> tempPuzzleSolution = Enumerable.Repeat(0, countPuzzleStands).ToList();
        
        if (puzzleItemStandList == null || targetDoor == null)
        {
            return false;
        }
        
        
        foreach (PuzzleStand puzzleStand in puzzleItemStandList)
        {
            if (puzzleStand == null)
            {
                continue;
            }

            ItemStand itemStand = puzzleStand.PuzzleStandGameObject.GetComponent<ItemStand>();

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

            string attachItemName = itemStand.m_visualName;
            Debug.Log("Checking solution. Item attached has name " + attachItemName);
            
            if (int.TryParse(puzzleStand.PuzzleStandGameObject.name[0].ToString(), out int position))
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

    public class PuzzleStand
    {
        public GameObject PuzzleStandGameObject = new GameObject();
        public bool puzzleStand = new bool();
        public bool puzzlePickable = new bool();
        public int puzzlePosition = new int();
    }
}

