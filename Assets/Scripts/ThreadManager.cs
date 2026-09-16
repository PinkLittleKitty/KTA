using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

public class ThreadManager : MonoBehaviour
{
    private static readonly Queue<Action> _mainThreadQueue = new Queue<Action>();
    private readonly Stopwatch _frameStopwatch = new Stopwatch();

    public static bool isPlaying = true;

    public static void ExecuteOnMainThread(Action func)
    {
        if (func == null) return;
        lock (_mainThreadQueue)
        {
            _mainThreadQueue.Enqueue(func);
        }
    }

    public static void ExecuteOnMainThread(Action func, Action callback)
    {
        if (func == null) return;
        lock (_mainThreadQueue)
        {
            _mainThreadQueue.Enqueue(() =>
            {
                func();
                callback?.Invoke();
            });
        }
    }

    public static void Sleep(int milliseconds)
    {
        System.Threading.Thread.Sleep(milliseconds);
    }

    void Update()
    {
        isPlaying = Application.isPlaying;

        _frameStopwatch.Restart();

        while (_frameStopwatch.Elapsed.TotalMilliseconds < 8.0)
        {
            Action action = null;
            lock (_mainThreadQueue)
            {
                if (_mainThreadQueue.Count > 0)
                {
                    action = _mainThreadQueue.Dequeue();
                }
                else
                {
                    break;
                }
            }

            action?.Invoke();
        }
    }
}