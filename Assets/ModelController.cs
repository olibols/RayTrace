using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VoxReader;

public class ModelController : MonoBehaviour
{
    private Dictionary<string, VoxelChunk> Models;

    // Start is called before the first frame update
    void Awake()
    {
        /*DirectoryInfo d = new DirectoryInfo("Assets/Models");
        foreach (FileInfo files in d.GetFiles())
        {
            var data = File.ReadAllBytes($"Assets/Models/{files.FullName}");
            var chunks = Reader.GetChunks(data);
            var voxels = chunks.FirstOrDefault(c => c.Id == nameof(ChunkType.XYZI)) as VoxelChunk;
            Models.Add(files.Name, voxels);
        }*/
    }

    public void BuildModelOnChunk(int x, int y, int z, string model, ref Chunk chunk)
    {
        foreach (var voxel in Models[model].Voxels)
        {
            chunk.SetVoxelColour(voxel.X + x, voxel.Z + y, voxel.Y + z, (byte)(voxel.ColorIndex - 1));
        }
    }
}
