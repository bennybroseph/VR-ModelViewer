//////////////////////
//   MonoSingleton  //
//////////////////////

using UnityEngine;

namespace Library
{
    using System.Linq;

    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static bool s_IsQuitting;

        private static T s_Self;

        public static T self
        {
            get
            {
                if (s_IsQuitting)
                    return null;

                var singletons = FindObjectsOfType<T>().ToList();
                if (s_Self == null)
                    s_Self = singletons.First();
                else
                    foreach (var singleton in singletons)
                        if (singleton != s_Self)
                            Destroy(singleton.gameObject);

                return s_Self;
            }
        }

        protected MonoSingleton() { }

        protected virtual void Awake()
        {
            s_IsQuitting = false;
        }

        protected virtual void OnDestroy()
        {
            s_IsQuitting = true;
            s_Self = null;
        }
    }
}