// MWL's additions to the Unity doubles from Valheim.Testing.Doubles: fixture helpers the package does not have.
#nullable enable
// ReSharper disable InconsistentNaming

namespace UnityEngine
{
    public partial class GameObject
    {
        /// <summary>
        /// A new child, appended, standing at the world origin as a new object does before a fixture places it.
        /// Returns the child so fixtures read top down. Not a Unity member: a fixture helper.
        /// </summary>
        public GameObject Child(string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, true);
            return child;
        }
    }
}
