using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadManager : MonoBehaviour
{
    // Lista de funciones que deben ejecutarse en el hilo principal
    private static List<KeyValuePair<Action, Action>> Functions = new List<KeyValuePair<Action, Action>>();

    // Variable que indica si la aplicación está en ejecución
    public static bool isPlaying = true;

    // Ejecuta una función en el hilo principal
    public static void ExecuteOnMainThread(Action func)
    {
        // Bloquea el acceso a la lista de funciones para evitar conflictos de concurrencia
        lock (Functions)
        {
            // Agrega la función y una función de devolución de llamada nula a la lista
            Functions.Add(new KeyValuePair<Action, Action>(func, () => NullCallBack()));
        }
    }

    // Ejecuta una función en el hilo principal y proporciona una función de devolución de llamada
    public static void ExecuteOnMainThread(Action func, Action Callback)
    {
        // Bloquea el acceso a la lista de funciones para evitar conflictos de concurrencia
        lock (Functions)
        {
            // Agrega la función y la función de devolución de llamada especificada a la lista
            Functions.Add(new KeyValuePair<Action, Action>(func, Callback));
        }
    }

    // Pausa la ejecución durante un número específico de milisegundos
    public static void Sleep(int Milliseconds)
    {
        long start = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        while (true)
        {
            long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (now - start >= Milliseconds)
                break;
        }
    }

    // Función de devolución de llamada nula
    private static void NullCallBack() { }

    void Update()
    {
        // Verifica si la aplicación está en ejecución
        isPlaying = Application.isPlaying;

        // Bloquea el acceso a la lista de funciones para evitar conflictos de concurrencia
        lock (Functions)
        {
            for (int i = Functions.Count - 1; i > -1; i--)
            {
                // Ejecuta la función principal
                Functions[i].Key();

                // Ejecuta la función de devolución de llamada
                Functions[i].Value();

                // Elimina la función de la lista
                Functions.RemoveAt(i);
            }
        }
    }
}
