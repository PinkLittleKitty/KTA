using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Definir una clase estática llamada Extensions para contener las extensiones de Vector.
public static class Extensions
{
    // Extensión para convertir un Vector4 a un Vector3 eliminando la componente w.
    public static Vector3 Xyz(this Vector4 input)
    {
        // Crear y devolver un nuevo Vector3 con las componentes x, y y z del Vector4 de entrada.
        return new Vector3(input.x, input.y, input.z);
    }

    // Extensión para convertir un Vector3 a un Vector2 eliminando la componente z.
    public static Vector2 Xy(this Vector3 input)
    {
        // Crear y devolver un nuevo Vector2 con las componentes x e y del Vector3 de entrada.
        return new Vector2(input.x, input.y);
    }

    // Extensión para convertir un Vector3 a un Vector2 eliminando la componente y.
    public static Vector2 Xz(this Vector3 input)
    {
        // Crear y devolver un nuevo Vector2 con las componentes x y z del Vector3 de entrada.
        return new Vector2(input.x, input.z);
    }
}
