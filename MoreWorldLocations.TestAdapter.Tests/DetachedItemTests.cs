using MoreWorldLocations.TestAdapter;
using Xunit;

public class DetachedItemTests
{
    [Fact] public void DetachedShipmentHasPrefabIdentityWithoutMutatingPrototype()
    {
        var prefab = new UnityEngine.GameObject { name = "Wood" };
        var prototype = new ItemDrop.ItemData { m_stack = 1, m_dropPrefab = null };
        var item = DetachedItem.Create(prototype, prefab, 10);
        Assert.NotSame(prototype, item);
        Assert.Equal("Wood", item.m_dropPrefab!.name); // ShipmentItem consumes this identity.
        Assert.Equal(10, item.m_stack);
        Assert.Null(prototype.m_dropPrefab);
        Assert.Equal(1, prototype.m_stack);
    }
    [Fact] public void ExplicitPrefabWinsOverAStalePrototypeReference()
    {
        var prefab = new UnityEngine.GameObject { name = "Wood" };
        var original = new UnityEngine.GameObject { name = "Wrong" };
        var prototype = new ItemDrop.ItemData { m_dropPrefab = original };
        Assert.Same(prefab, DetachedItem.Create(prototype, prefab, 1).m_dropPrefab);
        Assert.Same(original, prototype.m_dropPrefab);
    }
}
namespace UnityEngine { public class GameObject { public string name = ""; } }
public class ItemDrop
{
    public class ItemData
    {
        public UnityEngine.GameObject? m_dropPrefab;
        public int m_stack;
        public ItemData Clone() => (ItemData)MemberwiseClone();
    }
}
