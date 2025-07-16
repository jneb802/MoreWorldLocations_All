namespace Common
{
    public class DebugUtils
    {
        public static void NullCheck<T>(T obj, string name) where T : class
        {
            if (obj == null)
            {
                UnityEngine.Debug.Log(name + " is null");
            }
            else
            {
                UnityEngine.Debug.Log(name + " is not null");
            }
        }
    }
}