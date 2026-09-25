using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Jotunn;
using UnityEngine;

namespace Underground_Ruins;

public class AttachPuzzle : MonoBehaviour
{
    private const string PuzzleInitializedKey = "BFD_puzzleInitialized";
    private const string PuzzleSolvedKey = "BFD_puzzleSolved";

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
        zNetView = gameObject.GetComponent<ZNetView>();
        if (zNetView == null || zNetView.m_zdo == null)
        {
            return;
        }

        zNetView.m_zdo.GetBool(PuzzleInitializedKey, out bool value);
        initialized = value;
    }

    public void Initialize()
    {
        GetItemStands();
        GetPickables();

        GameObject targetDoorPrefab = FindGameObjectInSector("PuzzleSecretDoor", gameObject.transform.position);
        if (targetDoorPrefab != null)
        {
            targetDoor = targetDoorPrefab.GetComponent<Door>();
        }

        GameObject puzzleSwitchPrefab = FindGameObjectInSector("PuzzleLever", gameObject.transform.position);
        if (puzzleSwitchPrefab != null)
        {
            puzzleSwitch = puzzleSwitchPrefab.GetComponentInChildren<Switch>();
        }

        if (puzzleSwitch != null)
        {
            puzzleSwitch.m_onUse = OnSwitchUse;
            puzzleSwitch.m_onHover = null;
            puzzleSwitch.m_hoverText = "[<color=yellow><b>$KEY_Use</b></color>] Submit Offering";
            if (string.IsNullOrEmpty(puzzleSwitch.m_name))
            {
                puzzleSwitch.m_name = "Hidden Door Mechanism";
            }
        }

        if (IsPuzzleSolved(targetDoor))
        {
            targetDoor.Open(Vector3.forward);
        }
        else
        {
            GenerateSolution();
            ResetDoorState();
        }

        if (!initialized && zNetView != null && zNetView.m_zdo != null)
        {
            zNetView.m_zdo.Set(PuzzleInitializedKey, true);
        }

        initialized = true;
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
        puzzleItemStandList.Clear();

        for (int i = 0; i < countPuzzleStands; i++)
        {
            string prefabName = string.Concat(i.ToString(), "_PuzzleStand");
            GameObject puzzleStandPrefab = FindGameObjectInSector(prefabName, gameObject.transform.position);

            if (puzzleStandPrefab == null)
            {
                continue;
            }

            PuzzleStand puzzleStand = new PuzzleStand
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
        puzzleSolutionPickableList.Clear();

        for (int i = 0; i < countPuzzleStands; i++)
        {
            string prefabName = string.Concat(i.ToString(), "_PuzzlePickable");
            GameObject puzzleStandPrefab = FindGameObjectInSector(prefabName, gameObject.transform.position);

            if (puzzleStandPrefab == null)
            {
                continue;
            }

            PuzzleStand puzzleStand = new PuzzleStand
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
        if (!CheckSolution())
        {
            Debug.Log("Puzzle solution is incorrect.");
            return false;
        }

        SetPuzzleSolved(targetDoor, true);
        targetDoor.Open(Vector3.forward);
        Debug.Log("Puzzle solved! Door is opening.");
        return true;
    }

    public void GenerateSolution()
    {
        puzzleSolution = Enumerable.Repeat(1, countPuzzleStands).ToList();

        if (puzzleSolutionPickableList == null || puzzleSolutionPickableList.Count == 0)
        {
            return;
        }

        foreach (PuzzleStand puzzlePickable in puzzleSolutionPickableList)
        {
            if (puzzlePickable == null || puzzlePickable.PuzzleStandGameObject == null)
            {
                continue;
            }

            Pickable pickable = puzzlePickable.PuzzleStandGameObject.GetComponent<Pickable>();
            if (pickable != null)
            {
                pickable.SetPicked(false);
            }
        }
    }

    public bool CheckSolution()
    {
        List<int> tempPuzzleSolution = Enumerable.Repeat(0, countPuzzleStands).ToList();

        if (puzzleItemStandList == null || targetDoor == null || puzzleSolution == null || puzzleSolution.Count != countPuzzleStands)
        {
            return false;
        }

        foreach (PuzzleStand puzzleStand in puzzleItemStandList)
        {
            if (puzzleStand == null || puzzleStand.PuzzleStandGameObject == null)
            {
                continue;
            }

            ItemStand itemStand = puzzleStand.PuzzleStandGameObject.GetComponent<ItemStand>();
            if (itemStand == null || itemStand.m_nview == null || itemStand.m_nview.GetZDO() == null)
            {
                continue;
            }

            string attachItemName = itemStand.m_visualName;
            int position = puzzleStand.puzzlePosition;
            if (position >= 0 && position < tempPuzzleSolution.Count)
            {
                tempPuzzleSolution[position] = attachItemName == solutionItemName ? 1 : 0;
            }
        }

        return tempPuzzleSolution.SequenceEqual(puzzleSolution);
    }

    public void ResetDoorState()
    {
        if (targetDoor == null)
        {
            return;
        }

        SetPuzzleSolved(targetDoor, false);

        ZNetView doorZNetView = targetDoor.GetComponent<ZNetView>();
        if (doorZNetView == null || doorZNetView.GetZDO() == null)
        {
            return;
        }

        doorZNetView.GetZDO().Set(ZDOVars.s_state, 0);
    }

    public static bool IsPuzzleDoor(Door door)
    {
        if (door == null || door.gameObject == null)
        {
            return false;
        }

        return door.gameObject.name.Contains("PuzzleSecretDoor");
    }

    public static bool IsPuzzleSolved(Door door)
    {
        if (door == null)
        {
            return false;
        }

        ZNetView doorZNetView = door.GetComponent<ZNetView>();
        if (doorZNetView == null || doorZNetView.GetZDO() == null)
        {
            return false;
        }

        return doorZNetView.GetZDO().GetBool(PuzzleSolvedKey, false);
    }

    public static void SetPuzzleSolved(Door door, bool solved)
    {
        if (door == null)
        {
            return;
        }

        ZNetView doorZNetView = door.GetComponent<ZNetView>();
        if (doorZNetView == null || doorZNetView.GetZDO() == null)
        {
            return;
        }

        doorZNetView.GetZDO().Set(PuzzleSolvedKey, solved);
    }

    [HarmonyPatch(typeof(Door), nameof(Door.Interact))]
    public static class PuzzleDoorInteractPatch
    {
        public static bool Prefix(Door __instance)
        {
            if (!IsPuzzleDoor(__instance))
            {
                return true;
            }

            return IsPuzzleSolved(__instance);
        }
    }

    public class PuzzleStand
    {
        public GameObject PuzzleStandGameObject = new GameObject();
        public bool puzzleStand = new bool();
        public bool puzzlePickable = new bool();
        public int puzzlePosition = new int();
    }
}
