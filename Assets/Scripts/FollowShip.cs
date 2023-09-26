using UnityEngine;
using System.Collections;

public class FollowShip : MonoBehaviour
{
    // El objeto que seguirá esta cámara.
    public GameObject TargetShip;

    // Desplazamiento o offset de la cámara en relación con la nave seguida.
    public Vector3 Offset;

    // Variable privada para almacenar la posición interpolada suavemente.
    private Vector3 _lerpPosition;

    // Función para realizar una interpolación lineal entre dos puntos en 3D.
    public Vector3 Lerp(Vector3 A, Vector3 B, float C)
    {
        // Devolver un nuevo Vector3 con las componentes interpoladas entre A y B.
        return new Vector3(Mathf.Lerp(A.x, B.x, C), Mathf.Lerp(A.y, B.y, C), Mathf.Lerp(A.z, B.z, C));
    }

    void Update()
    {
        // Verificar si el objeto a seguir es nulo y, si es así, salir del método.
        if (TargetShip == null)
            return;

        // Interpolar suavemente la posición actual de la cámara hacia la posición de la nave objetivo.
        _lerpPosition = Lerp(_lerpPosition, TargetShip.transform.position, Time.deltaTime * 32f);

        // Mover la posición de la cámara hacia adelante y arriba desde la nave objetivo, con un desplazamiento especificado.
        this.transform.position = Vector3.MoveTowards(this.transform.position, TargetShip.transform.position + TargetShip.transform.forward * Offset.z + TargetShip.transform.up * Offset.y, Time.deltaTime);

        // Hacer que la cámara mire hacia la posición interpolada suavemente, manteniendo arriba en el eje Y.
        this.transform.LookAt(_lerpPosition, Vector3.up);
    }
}
