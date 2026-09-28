// Stand-ins for the Unity and Valheim types the template walk reads.
//
// The extractor is the one part of the verification that cannot be reasoned
// about without an object hierarchy, which is why it needed doubles at all.
// These model exactly what it reads -- parentage, activeness, components,
// transforms -- and nothing else: no Unity lifecycle, no physics, no
// networking, no ghost spawning. A test here establishes what the code decides,
// never what the game does.

using System.Collections.Generic;
using System.Linq;

namespace UnityEngine
{
    /// <summary>Anything attached to a GameObject.</summary>
    public class Component
    {
        public GameObject gameObject = null!;
        public global::Transform transform => gameObject.transform;
    }

    /// <summary>A component that can be switched off.</summary>
    public class Behaviour : Component
    {
        public bool enabled = true;
    }

    /// <summary>
    /// A renderer, with the parts the subtree comparison reads: whether it is
    /// on, and which materials it shows.
    /// </summary>
    public class Renderer : Component
    {
        public bool enabled = true;
        public Material[] sharedMaterials = new Material[0];
    }

    public class Material : Object { }

    public class Mesh : Object { }

    public class MeshFilter : Component
    {
        public Mesh? sharedMesh;
    }

    /// <summary>A named engine object: what the comparison records a reference by.</summary>
    public class Object
    {
        public string name = "";
    }

    /// <summary>A collider, and the geometry a player actually meets.</summary>
    public class Collider : Component
    {
        public bool enabled = true;
        public bool isTrigger;
    }

    public class BoxCollider : Collider
    {
        public Vector3 size = new(1f, 1f, 1f);
        public Vector3 center;
    }

    public class SphereCollider : Collider
    {
        public float radius = 0.5f;
        public Vector3 center;
    }

    public class CapsuleCollider : Collider
    {
        public float radius = 0.5f;
        public float height = 2f;
        public int direction = 1;
        public Vector3 center;
    }

    public class MeshCollider : Collider
    {
        public bool convex;
        public Mesh? sharedMesh;
    }

    /// <summary>Engine and presentation state: present, and not compared. See TemplateFacts.UncomparedComponents.</summary>
    public class Light : Component { }

    public class Animator : Component { }

    /// <summary>
    /// An object in a template. Children are ordered, because a template's own
    /// order is vanilla's tie-break for terrain modifiers.
    /// </summary>
    public class GameObject
    {
        private readonly List<Component> _components = new();

        public GameObject(string name)
        {
            this.name = name;
            transform = new global::Transform { gameObject = this };
            _components.Add(transform);
        }

        public string name;
        public bool activeSelf = true;
        public global::Transform transform;

        public T AddComponent<T>() where T : Component, new()
        {
            var component = new T { gameObject = this };
            _components.Add(component);
            return component;
        }

        public T? GetComponent<T>() where T : Component => _components.OfType<T>().FirstOrDefault();

        public Component[] GetComponents<T>() where T : Component => _components.ToArray();

        /// <summary>A new child, appended. Returns the child so fixtures read top down.</summary>
        public GameObject Child(string name)
        {
            var child = new GameObject(name);
            child.transform.parent = transform;
            transform.children.Add(child.transform);
            return child;
        }
    }
}

/// <summary>
/// Extra members on the road code's Transform shim: the hierarchy and the local
/// transform the template walk reads. The position field it already had is
/// unchanged.
/// </summary>
public partial class Transform : UnityEngine.Component
{
    public Transform? parent;
    public readonly System.Collections.Generic.List<Transform> children = new();
    public UnityEngine.Vector3 localScale = new(1f, 1f, 1f);
    public UnityEngine.Vector3 eulerAngles;
    /// <summary>
    /// Where this object sits relative to its parent.
    ///
    /// Derived from <see cref="position"/> rather than kept beside it, because
    /// in Unity the two are one piece of state and a double that let them drift
    /// apart would answer a question the engine never asks: a test that moved an
    /// object by its world position would leave its local position unchanged,
    /// and a check reading the local one would see nothing move.
    /// </summary>
    public UnityEngine.Vector3 localPosition
    {
        get => parent == null ? position : position - parent.position;
        set => position = parent == null ? value : parent.position + value;
    }

    public UnityEngine.Quaternion localRotation = UnityEngine.Quaternion.identity;

    /// <summary>
    /// The world rotation, the way Unity derives it: the parent's and then this
    /// one's. Yaw only, like the rest of this double.
    /// </summary>
    public UnityEngine.Quaternion rotation =>
        parent == null ? localRotation : parent.rotation * localRotation;

    public new string name => gameObject.name;
    public int childCount => children.Count;
    public Transform GetChild(int index) => children[index];

    /// <summary>
    /// The template root is at the origin in these fixtures, so a world position
    /// minus the root's is the position in the template's own space.
    /// </summary>
    public UnityEngine.Vector3 InverseTransformPoint(UnityEngine.Vector3 point) => point - position;
}

/// <summary>The loot a container rolls on the server and saves into its ZDO.</summary>
public class DropTable
{
    public System.Collections.Generic.List<DropData> m_drops = new();

    public class DropData
    {
        public UnityEngine.GameObject? m_item;
    }
}

/// <summary>Vanilla's chance-gated branch. Only the shape the trace reads.</summary>
public class RandomSpawn : UnityEngine.Component
{
    public UnityEngine.GameObject? m_OffObject;
    public float m_chanceToSpawn = 50f;
}

/// <summary>Vanilla's weighted pick among alternatives. Only the shape the trace reads.</summary>
public class RandomObject : UnityEngine.Component
{
    public class ObjectEntry
    {
        public UnityEngine.GameObject? m_object;
        public float m_weight = 1f;
    }
    public System.Collections.Generic.List<ObjectEntry> m_objects = new();
}

public class Container : UnityEngine.Component
{
    public DropTable m_defaultItems = new();
}

public class CreatureSpawner : UnityEngine.Component
{
    public UnityEngine.GameObject? m_creaturePrefab;
}

public class DropOnDestroyed : UnityEngine.Component
{
    public DropTable m_dropWhenDestroyed = new();
}

public class SpawnArea : UnityEngine.Component
{
    public System.Collections.Generic.List<SpawnData> m_prefabs = new();

    public class SpawnData
    {
        public UnityEngine.GameObject? m_prefab;
    }
}

public class PickableItem : UnityEngine.Component
{
    public RandomItem[] m_randomItemPrefabs = new RandomItem[0];

    /// <summary>A struct in the game, so there is no null entry to skip.</summary>
    public struct RandomItem
    {
        public UnityEngine.GameObject? m_itemPrefab;
    }
}

public class Pickable : UnityEngine.Component
{
    public UnityEngine.GameObject? m_itemPrefab;
}
