using UnityEngine;
using System.Collections;

public class Explode : MonoBehaviour
{
    public AudioSource ExplosionAudio;
    private GameObject Debris;

    void Start()
    {
        // Buscar el objeto con la etiqueta "Debris" y asignarlo a la variable Debris.
        Debris = GameObject.FindGameObjectWithTag("Debris");
        
        // Llamar a la función SplitMesh para dividir la malla.
        SplitMesh();
    }

    // Corrutina para destruir un objeto después de un tiempo aleatorio.
    IEnumerator DestroyCoroutine(GameObject GO)
    {
        yield return new WaitForSeconds(2 + Random.Range(0.0f, 5.0f));
        
        // Borrar la malla del objeto y destruirlo.
        GO.GetComponent<MeshFilter>().sharedMesh.Clear();
        Destroy(GO);
    }

    // Función para dividir la malla en triángulos separados.
    void SplitMesh()
    {
        MeshFilter MF = GetComponent<MeshFilter>();
        MeshRenderer MR = GetComponent<MeshRenderer>();
        Mesh M = MF.mesh;
        Vector3[] verts = M.vertices;
        Vector3[] normals = M.normals;
        Vector2[] uvs = M.uv;
        
        for (int submesh = 0; submesh < M.subMeshCount; submesh++)
        {
            int[] indices = M.GetTriangles(submesh);
            
            for (int i = 0; i < indices.Length; i += 3)
            {
                Vector3[] newVerts = new Vector3[3];
                Vector3[] newNormals = new Vector3[3];
                Vector2[] newUvs = new Vector2[3];
                
                for (int n = 0; n < 3; n++)
                {
                    int index = indices[i + n];
                    newVerts[n] = verts[index];
                    newUvs[n] = uvs[index];
                    newNormals[n] = normals[index];
                }
                
                Mesh mesh = new Mesh();
                mesh.vertices = newVerts;
                mesh.normals = newNormals;
                mesh.uv = newUvs;
                mesh.triangles = new int[] { 0, 1, 2, 2, 1, 0 };
                
                GameObject GO = new GameObject("Triángulo " + (i / 3));
                
                if (Debris != null)
                    GO.transform.parent = Debris.transform;
                
                GO.transform.position = transform.position;
                GO.transform.rotation = transform.rotation;
                GO.AddComponent<MeshRenderer>().material = MR.materials[submesh];
                GO.AddComponent<MeshFilter>().mesh = mesh;
                GO.AddComponent<BoxCollider>();
                GO.AddComponent<Rigidbody>().AddExplosionForce(20, transform.position, 30);
                
                // Iniciar la corrutina para destruir el objeto creado.
                StartCoroutine(DestroyCoroutine(GO));
            }
        }
        
        // Desactivar el renderizador de la malla original.
        MR.enabled = false;
        
        // Reproducir el sonido de explosión si está configurado.
        if (ExplosionAudio != null)
        {
            AudioSource au = Instantiate<AudioSource>(ExplosionAudio, this.transform.position, Quaternion.identity);
            Destroy(au.gameObject, au.time + 0.5f);
        }
        
        // Destruir este objeto.
        Destroy(this.gameObject);
    }
}
