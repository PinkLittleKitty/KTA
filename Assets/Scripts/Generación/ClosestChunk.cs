using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Assets.Generation
{
    public class ClosestChunk : IComparer<Chunk>
    {
        public Vector3 PlayerPos;

        public ClosestChunk()
        {
        }

        public ClosestChunk(Vector3 pos)
        {
            PlayerPos = pos;
        }

        public int Compare(Chunk V1, Chunk V2)
        {
            try
            {
                if (ReferenceEquals(V1, V2))
                    return 0;

                if (V1 == null)
                    return -1;

                if (V2 == null)
                    return 1;

                float V1f = (V1.Position - PlayerPos).sqrMagnitude;

                float V2f = (V2.Position - PlayerPos).sqrMagnitude;

                float difference = V1f - V2f;
                const float epsilon = 1e-6f;

                if (Mathf.Abs(difference) < epsilon)
                {
                    int hashComparison = V1.GetHashCode().CompareTo(V2.GetHashCode());
                    if (hashComparison != 0)
                        return hashComparison;
                    
                    Vector3 pos1 = V1.Position;
                    Vector3 pos2 = V2.Position;
                    
                    int xComp = pos1.x.CompareTo(pos2.x);
                    if (xComp != 0) return xComp;
                    
                    int yComp = pos1.y.CompareTo(pos2.y);
                    if (yComp != 0) return yComp;
                    
                    return pos1.z.CompareTo(pos2.z);
                }
                else if (difference < 0)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error en comparación de chunks: " + e.Message);
                
                if (V1 == null && V2 == null) return 0;
                if (V1 == null) return -1;
                if (V2 == null) return 1;
                
                return V1.GetHashCode().CompareTo(V2.GetHashCode());
            }
        }
    }
}
