// Emilian Wilczek 2003458
// Written following a Unity C# Networking tutorial by Tom Weiland

using System;
using System.Collections.Generic;
using UnityEngine;

public class ThreadManager : MonoBehaviour
{
    private static readonly List<Action> executeOnMainThread = new();
    private static readonly List<Action> ExecuteCopiedOnMainThread = new();
    private static bool _actionToExecuteOnMainThread;

    private void FixedUpdate()
    {
        UpdateMain();
    }

    public static void ExecuteOnMainThread(Action action)
    {
        if (action == null)
        {
            Console.WriteLine("No action to execute on main thread!");
            return;
        }

        lock (executeOnMainThread)
        {
            executeOnMainThread.Add(action);
            _actionToExecuteOnMainThread = true;
        }
    }

    private static void UpdateMain()
    {
        if (!_actionToExecuteOnMainThread) return;
        ExecuteCopiedOnMainThread.Clear();
        lock (executeOnMainThread)
        {
            ExecuteCopiedOnMainThread.AddRange(executeOnMainThread);
            executeOnMainThread.Clear();
            _actionToExecuteOnMainThread = false;
        }

        foreach (var t in ExecuteCopiedOnMainThread)
            t();
    }
}