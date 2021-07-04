using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{

    private Dictionary<Vector3Int, Chunk> Chunks = new Dictionary<Vector3Int, Chunk>();

    void Awake()
    {
        Chunk chunk = new Chunk();
        ChunkUtils.SetupChunk(ref chunk);
        ChunkUtils.CalculateChunkSdf(ref chunk);
        Chunks.Add(new Vector3Int(0, 0, 0), chunk);
    }

    public Chunk GetChunk(int x, int y, int z)
    {
        return Chunks[new Vector3Int(x, y, z)];
    }
}
