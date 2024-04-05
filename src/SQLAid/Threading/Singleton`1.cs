using System.Reflection;

namespace SQLAid.Threading
{
    public class Singleton<T> where T : class
    {
        protected Singleton()
        {
        }

        public static T Instance => Nested.Singleton;

        private sealed class Nested
        {
            private static readonly T _instance = typeof(T).InvokeMember(typeof(T).Name,
                BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                null,
                null) as T;

            internal static T Singleton => _instance;
        }
    }
}