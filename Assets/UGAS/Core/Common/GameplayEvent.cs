using UnityEngine;
using System.Collections.Generic;

namespace UnityGAS
{
    public struct GameplayEventData
    {
        public string eventName;
        public GameObject instigator;
        public GameObject target;
        public float magnitude;
        public Vector3 position;

        public GameplayEventData(string eventName, GameObject instigator = null, GameObject target = null, float magnitude = 0, Vector3 position = default)
        {
            this.eventName = eventName;
            this.instigator = instigator;
            this.target = target;
            this.magnitude = magnitude;
            this.position = position;
        }
    }

    public class GameplayEventSystem : MonoBehaviour
    {
        private static GameplayEventSystem instance;
        private Dictionary<string, System.Action<GameplayEventData>> eventListeners = new Dictionary<string, System.Action<GameplayEventData>>();

        public static GameplayEventSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("GameplayEventSystem");
                    instance = go.AddComponent<GameplayEventSystem>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public static void SendEvent(string eventName, GameplayEventData data)
        {
            if (instance != null && instance.eventListeners.TryGetValue(eventName, out var listeners))
            {
                listeners?.Invoke(data);
            }
        }

        public static void SendEvent(string eventName, GameObject instigator = null, GameObject target = null, float magnitude = 0)
        {
            SendEvent(eventName, new GameplayEventData(eventName, instigator, target, magnitude));
        }

        public static void Listen(string eventName, System.Action<GameplayEventData> callback)
        {
            if (!Instance.eventListeners.ContainsKey(eventName))
            {
                Instance.eventListeners[eventName] = null;
            }
            Instance.eventListeners[eventName] += callback;
        }

        public static void Unlisten(string eventName, System.Action<GameplayEventData> callback)
        {
            if (instance != null && instance.eventListeners.TryGetValue(eventName, out var listeners))
            {
                instance.eventListeners[eventName] -= callback;
            }
        }
    }
}