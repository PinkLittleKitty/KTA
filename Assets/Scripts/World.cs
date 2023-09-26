using System.Collections;
using System.Collections.Generic;
using Assets.Generation;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour {

    public GameObject Player; // Referencia al objeto jugador en el mundo.
    public Material WorldMaterial; // Material utilizado para el mundo.
    public Vector3 PlayerPosition, PlayerOrientation; // Posición y orientación del jugador.
    public int GenQueue, MeshQueue; // Contadores de cola para generación y mallas.
    public readonly Dictionary<Vector3, Chunk> Chunks = new Dictionary<Vector3, Chunk>(); // Diccionario para almacenar fragmentos de terreno.
    private MeshQueue _meshQueue; // Cola para mallas.
    private GenerationQueue _generationQueue; // Cola para generación.
    public int ChunkLoaderRadius = 8; // Radio de carga de fragmentos.
    public bool Loaded { get; set; } // Indica si el mundo ha sido cargado.
    public Slider sliderUI; // Referencia al control deslizante en la interfaz de usuario.
    public Text chunksNum; // Referencia al texto para mostrar el número de fragmentos.

    void Awake(){
        Application.targetFrameRate = 60; // Establecer la velocidad de fotogramas objetivo a 60 FPS.
        _meshQueue = new MeshQueue (this); // Inicializar la cola de mallas.
        _generationQueue = new GenerationQueue (this); // Inicializar la cola de generación.
        Loaded = true; // Marcar el mundo como cargado.
        sliderUI.value = PlayerPrefs.GetFloat("Chunks"); // Configurar el valor del control deslizante desde las preferencias del jugador.
    }

    void Update(){

        PlayerPosition = Player.transform.position; // Actualizar la posición del jugador.
        PlayerOrientation = Player.transform.forward; // Actualizar la orientación del jugador.

        int _genCount = 0, _meshCount = 0;
        foreach (KeyValuePair<Vector3, Chunk> Pair in Chunks) {
            if (!Pair.Value.IsGenerated)
                _genCount++; // Incrementar el contador de generación si el fragmento no está generado.

            if (Pair.Value.ShouldBuild)
                _meshCount++; // Incrementar el contador de mallas si se debe construir una malla para el fragmento.
        }

        GenQueue = _genCount; // Actualizar la cola de generación.
        MeshQueue = _meshCount; // Actualizar la cola de mallas.

        ChunkLoaderRadius = (int)sliderUI.value; // Actualizar el radio de carga de fragmentos desde el control deslizante.
    }

    void OnApplicationQuit(){
        _meshQueue.Stop = true; // Detener la cola de mallas al salir de la aplicación.
        _generationQueue.Stop = true; // Detener la cola de generación al salir de la aplicación.
    }

    public void SortGenerationQueue(){
        _generationQueue.Sort (); // Ordenar la cola de generación.
    }

    public void SortMeshQueue(){
        _meshQueue.Sort (); // Ordenar la cola de mallas.
    }

    public void AddToQueue(Chunk Chunk, bool DoMesh)
    {
        if (DoMesh) {
            _meshQueue.Add (Chunk); // Agregar el fragmento a la cola de mallas si es necesario.
        } else {
            _generationQueue.Add (Chunk); // Agregar el fragmento a la cola de generación si no es necesario construir una malla.
        }
    }

    public void AddChunk(Vector3 Offset, Chunk Chunk)
    {
        lock (this.Chunks) {
            if (!this.Chunks.ContainsKey (Offset)) {
                this.Chunks.Add (Offset, Chunk); // Agregar el fragmento al diccionario si no existe previamente.
                this._generationQueue.Add (Chunk); // Agregar el fragmento a la cola de generación.
            }
        }
    }

    public void RemoveChunk(Chunk Chunk) { 
        lock(Chunks){
            if (Chunks.ContainsKey (Chunk.Position))
                Chunks.Remove (Chunk.Position); // Eliminar el fragmento del diccionario si existe.

            _meshQueue.Remove (Chunk); // Eliminar el fragmento de la cola de mallas.
            _generationQueue.Remove (Chunk); // Eliminar el fragmento de la cola de generación.
        }
        Chunk.Dispose (); // Liberar recursos del fragmento.
    }

    public bool ContainsMeshQueue(Chunk chunk){
        return _meshQueue.Contains(chunk); // Verificar si la cola de mallas contiene el fragmento.
    }

    public Vector3 ToBlockSpace(Vector3 Vec3){
        
        int ChunkX = (int) Vec3.x >> Chunk.Bitshift;
        int ChunkY = (int) Vec3.y >> Chunk.Bitshift;
        int ChunkZ = (int) Vec3.z >> Chunk.Bitshift;
            
        ChunkX *= Chunk.ChunkSize;
        ChunkY *= Chunk.ChunkSize;
        ChunkZ *= Chunk.ChunkSize;
            
        int X = (int) Mathf.Floor( (Vec3.x - ChunkX) / (float) Chunk.ChunkSize );
        int Y = (int) Mathf.Floor( (Vec3.y - ChunkY) / (float) Chunk.ChunkSize );
        int Z = (int) Mathf.Floor( (Vec3.z - ChunkZ) / (float) Chunk.ChunkSize );

        return new Vector3(X, Y ,Z); // Convertir las coordenadas a espacio de bloques.
    }
        
    public Chunk GetChunkAt(Vector3 Vec3){
        int ChunkX = (int) Vec3.x >> Chunk.Bitshift;
        int ChunkY = (int) Vec3.y >> Chunk.Bitshift;
        int ChunkZ = (int) Vec3.z >> Chunk.Bitshift;
            
        ChunkX *= Chunk.ChunkSize;
        ChunkY *= Chunk.ChunkSize;
        ChunkZ *= Chunk.ChunkSize;
            
        return this.GetChunkByOffset(ChunkX, ChunkY, ChunkZ); // Obtener el fragmento en las coordenadas especificadas.
    }
        
    public Vector3 ToChunkSpace(Vector3 Vec3){
        int ChunkX = (int) Vec3.x >> Chunk.Bitshift;
        int ChunkY = (int) Vec3.y >> Chunk.Bitshift;
        int ChunkZ = (int) Vec3.z >> Chunk.Bitshift;

        ChunkX *= Chunk.ChunkSize;
        ChunkY *= Chunk.ChunkSize;
        ChunkZ *= Chunk.ChunkSize;    
        
        return new Vector3(ChunkX, ChunkY, ChunkZ); // Convertir las coordenadas a espacio de fragmentos.
    }
    
    public Chunk GetChunkByOffset(float X, float Y, float Z) {
        return this.GetChunkByOffset (new Vector3(X,Y,Z)); // Obtener el fragmento en las coordenadas especificadas.
    }

    public Chunk GetChunkByOffset(Vector3 Offset) {
        lock(Chunks){
            if (Chunks.ContainsKey (Offset))
                return Chunks [Offset]; // Obtener el fragmento del diccionario si existe.
        }
        return null; // Devolver nulo si el fragmento no se encuentra en el diccionario.
    }

    public void ActualizarChunks()
    {
        PlayerPrefs.SetFloat("Chunks", sliderUI.value); // Actualizar las preferencias del jugador con el valor del control deslizante.
        chunksNum.text = PlayerPrefs.GetFloat("Chunks").ToString(); // Actualizar el texto con el número de fragmentos.
    }
}
