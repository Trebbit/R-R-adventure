using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected Singleton() { }
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<T>();

                return instance;
            }
        }

    }
}
