using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Assets.Generation
{
    // La clase ClosestChunk implementa IComparer<Chunk> para comparar la proximidad de los fragmentos (chunks) al jugador.
    public class ClosestChunk : IComparer<Chunk>
    {
        // La posición del jugador que se utilizará para calcular la proximidad.
        public Vector3 PlayerPos;

        // Constructor por defecto.
        public ClosestChunk()
        {
        }

        // Constructor que establece la posición del jugador.
        public ClosestChunk(Vector3 pos)
        {
            PlayerPos = pos;
        }

        // Método de comparación que determina la proximidad entre dos fragmentos (chunks).
        public int Compare(Chunk V1, Chunk V2)
        {
            try
            {
                // Si ambos fragmentos son iguales, la comparación devuelve 0.
                if (V1 == V2)
                    return 0;

                // Si V1 es nulo, se considera menos cercano.
                if (V1 == null)
                    return -1;

                // Si V2 es nulo, se considera menos cercano.
                if (V2 == null)
                    return 1;

                // Calcula la distancia cuadrada entre V1 y la posición del jugador.
                float V1f = (V1.Position - PlayerPos).sqrMagnitude;

                // Calcula la distancia cuadrada entre V2 y la posición del jugador.
                float V2f = (V2.Position - PlayerPos).sqrMagnitude;

                // Compara las distancias cuadradas para determinar qué fragmento está más cerca.
                if (V1f < V2f)
                {
                    return -1; // V1 está más cerca.
                }
                else if (V1f == V2f)
                {
                    return 0; // Ambos están a la misma distancia.
                }
                else
                {
                    return 1; // V2 está más cerca.
                }
            }
            catch (ArgumentException e)
            {
                // En caso de error, muestra un mensaje de depuración.
                Debug.Log("No se puede ordenar los fragmentos correctamente. " + e.Message);
                return 0;
            }
        }
    }
}
