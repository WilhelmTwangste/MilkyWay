using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;
    private Queue<System.Action> queue = new Queue<System.Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("UnityMainThreadDispatcher");
            instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
        return instance;
    }

    void Update()
    {
        lock (queue)
        {
            while (queue.Count > 0)
            {
                queue.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(System.Action action)
    {
        lock (queue)
        {
            queue.Enqueue(action);
        }
    }
}