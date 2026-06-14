using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jotunn;
using UnityEngine;
using Random = UnityEngine.Random;

namespace More_World_Locations_AIO.Dungeons;

public class AttachPuzzle : MonoBehaviour
{
    private const string InitializedKey = "BFD_puzzleInitialized";
    private const string SolutionKey = "BFD_puzzleSolution";
    private const int DefaultPuzzleStandCount = 8;
    private const string PuzzleDoorPrefabName = "BFD_PuzzleSecretDoor";

    private readonly List<PuzzleTarget> puzzleItemStandList = new();
    private readonly List<PuzzleTarget> puzzleSolutionPickableList = new();
    private readonly List<int> puzzleSolution = new();

    public int countPuzzleStands = DefaultPuzzleStandCount;
    public Door? targetDoor;
    public Switch? puzzleSwitch;
    public string solutionItemName = "SurtlingCore";
    public ZNetView? zNetView;

    private void Awake()
    {
        zNetView = GetComponent<ZNetView>();
    }

    private void Start()
    {
        StartCoroutine(DelayedPuzzleInit(transform.position));
    }

    public IEnumerator DelayedPuzzleInit(Vector3 position)
    {
        while (ZNetScene.instance == null || !ZNetScene.instance.IsAreaReady(position))
        {
            yield return null;
        }

        while (zNetView == null || zNetView.GetZDO() == null)
        {
            zNetView ??= GetComponent<ZNetView>();
            yield return null;
        }

        Initialize();
    }

    public void Initialize()
    {
        if (zNetView == null || zNetView.GetZDO() == null)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                $"AttachPuzzle: missing ZNetView/ZDO on {name}");
            return;
        }

        RebuildSceneReferences();

        bool hasNoPuzzleObjects = puzzleItemStandList.Count == 0
                                  && puzzleSolutionPickableList.Count == 0
                                  && targetDoor == null
                                  && puzzleSwitch == null;
        if (hasNoPuzzleObjects)
        {
            return;
        }

        if (puzzleItemStandList.Count != countPuzzleStands
            || puzzleSolutionPickableList.Count != countPuzzleStands
            || targetDoor == null
            || puzzleSwitch == null)
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                $"AttachPuzzle: incomplete puzzle setup in dungeon at {transform.position}. " +
                $"stands={puzzleItemStandList.Count}/{countPuzzleStands}, " +
                $"pickables={puzzleSolutionPickableList.Count}/{countPuzzleStands}, " +
                $"door={(targetDoor != null)}, lever={(puzzleSwitch != null)}");
            return;
        }

        puzzleSwitch.m_onUse = OnSwitchUse;

        if (!EnsureSolutionLoaded())
        {
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogWarning(
                $"AttachPuzzle: no solution available yet for dungeon at {transform.position}");
            return;
        }

        if (zNetView.IsOwner())
        {
            ApplyPickableStateToMatchSolution();
            zNetView.GetZDO().Set(InitializedKey, true);
        }
    }

    private void RebuildSceneReferences()
    {
        puzzleItemStandList.Clear();
        puzzleSolutionPickableList.Clear();

        CollectPuzzleTargets("_PuzzleStand", puzzleItemStandList);
        CollectPuzzleTargets("_PuzzlePickable", puzzleSolutionPickableList);

        targetDoor = FindGameObjectInSector(PuzzleDoorPrefabName, transform.position)?.GetComponent<Door>();
        puzzleSwitch = GetComponentInChildren<Switch>();
    }

    private void CollectPuzzleTargets(string suffix, List<PuzzleTarget> targets)
    {
        for (int i = 0; i < countPuzzleStands; i++)
        {
            GameObject? puzzleObject = FindGameObjectInSector($"{i}{suffix}", transform.position);
            if (puzzleObject == null)
            {
                continue;
            }

            targets.Add(new PuzzleTarget
            {
                GameObject = puzzleObject,
                Position = i
            });
        }

        targets.Sort((left, right) => left.Position.CompareTo(right.Position));
    }

    private bool EnsureSolutionLoaded()
    {
        if (zNetView == null || zNetView.GetZDO() == null)
        {
            return false;
        }

        if (TryLoadSolutionFromZdo())
        {
            return true;
        }

        ZDO zdo = zNetView.GetZDO();
        bool wasInitialized = zdo.GetBool(InitializedKey, false);
        if (wasInitialized && TryBuildSolutionFromPickables())
        {
            if (zNetView.IsOwner())
            {
                PersistSolution();
            }

            return true;
        }

        if (!zNetView.IsOwner())
        {
            return false;
        }

        GenerateSolution();
        PersistSolution();
        return true;
    }

    private bool TryLoadSolutionFromZdo()
    {
        if (zNetView == null || zNetView.GetZDO() == null)
        {
            return false;
        }

        string serialized = zNetView.GetZDO().GetString(SolutionKey, "");
        if (string.IsNullOrEmpty(serialized) || serialized.Length != countPuzzleStands)
        {
            return false;
        }

        puzzleSolution.Clear();

        foreach (char value in serialized)
        {
            if (value != '0' && value != '1')
            {
                puzzleSolution.Clear();
                return false;
            }

            puzzleSolution.Add(value - '0');
        }

        return true;
    }

    private bool TryBuildSolutionFromPickables()
    {
        if (puzzleSolutionPickableList.Count != countPuzzleStands)
        {
            return false;
        }

        puzzleSolution.Clear();
        for (int i = 0; i < countPuzzleStands; i++)
        {
            puzzleSolution.Add(0);
        }

        bool foundPickable = false;

        foreach (PuzzleTarget puzzleTarget in puzzleSolutionPickableList)
        {
            Pickable pickable = puzzleTarget.GameObject.GetComponent<Pickable>();
            if (pickable == null)
            {
                continue;
            }

            foundPickable = true;
            puzzleSolution[puzzleTarget.Position] = pickable.GetPicked() ? 0 : 1;
        }

        return foundPickable;
    }

    private void GenerateSolution()
    {
        puzzleSolution.Clear();
        for (int i = 0; i < countPuzzleStands; i++)
        {
            puzzleSolution.Add(0);
        }

        foreach (PuzzleTarget puzzleTarget in puzzleSolutionPickableList)
        {
            puzzleSolution[puzzleTarget.Position] = Random.Range(0, 2);
        }

        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug(
            $"AttachPuzzle: generated solution {SerializeSolution()} for dungeon at {transform.position}");
    }

    private void PersistSolution()
    {
        if (zNetView == null || zNetView.GetZDO() == null)
        {
            return;
        }

        if (!zNetView.IsOwner() || puzzleSolution.Count != countPuzzleStands)
        {
            return;
        }

        zNetView.GetZDO().Set(SolutionKey, SerializeSolution());
        zNetView.GetZDO().Set(InitializedKey, true);
    }

    private void ApplyPickableStateToMatchSolution()
    {
        if (zNetView == null)
        {
            return;
        }

        if (!zNetView.IsOwner() || puzzleSolution.Count != countPuzzleStands)
        {
            return;
        }

        foreach (PuzzleTarget puzzleTarget in puzzleSolutionPickableList)
        {
            Pickable pickable = puzzleTarget.GameObject.GetComponent<Pickable>();
            if (pickable == null)
            {
                continue;
            }

            bool shouldBePicked = puzzleSolution[puzzleTarget.Position] == 0;
            if (pickable.GetPicked() != shouldBePicked)
            {
                pickable.SetPicked(shouldBePicked);
            }
        }
    }

    public bool OnSwitchUse(Switch caller, Humanoid user, ItemDrop.ItemData item)
    {
        if (!EnsureSolutionLoaded())
        {
            return false;
        }

        if (CheckSolution())
        {
            targetDoor?.Open(Vector3.forward);
            More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug(
                $"AttachPuzzle: solved puzzle in dungeon at {transform.position}");
            return true;
        }

        More_World_Locations_AIOPlugin.More_World_Locations_AIOLogger.LogDebug(
            $"AttachPuzzle: incorrect puzzle solution in dungeon at {transform.position}");
        return false;
    }

    public bool CheckSolution()
    {
        if (puzzleSolution.Count != countPuzzleStands || targetDoor == null)
        {
            return false;
        }

        List<int> providedSolution = Enumerable.Repeat(0, countPuzzleStands).ToList();

        foreach (PuzzleTarget puzzleTarget in puzzleItemStandList)
        {
            ItemStand itemStand = puzzleTarget.GameObject.GetComponent<ItemStand>();
            if (itemStand == null)
            {
                continue;
            }

            providedSolution[puzzleTarget.Position] =
                itemStand.GetAttachedItem() == solutionItemName ? 1 : 0;
        }

        return providedSolution.SequenceEqual(puzzleSolution);
    }

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

            GameObject? instance = ZNetScene.instance?.FindInstance(zdo)?.gameObject;
            if (instance != null)
            {
                return instance;
            }
        }

        return null;
    }

    private string SerializeSolution()
    {
        return new string(puzzleSolution.Select(value => value == 0 ? '0' : '1').ToArray());
    }

    private sealed class PuzzleTarget
    {
        public GameObject GameObject = null!;
        public int Position;
    }
}
