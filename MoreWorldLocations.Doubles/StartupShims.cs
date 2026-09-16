// Hook tests call the production prefix/finalizer explicitly. These doubles do
// not emulate Harmony patch ordering, which requires the station campaign.
namespace UnityEngine
{
    public class MonoBehaviour : Component
    {
        public void StartCoroutine(System.Collections.IEnumerator routine) =>
            throw new System.NotSupportedException("Use the test scheduler explicitly.");
    }
}
namespace HarmonyLib
{
    public sealed class Traverse
    {
        public static Traverse Create(object instance) => new Traverse();
        public Traverse Method(string name) => this;
        public object GetValue() => throw new System.NotSupportedException("World loading requires a real game.");
    }
}
