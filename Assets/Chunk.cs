using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct ChunkStruct
{
    Vector3Int ChunkPos;
}

public class Chunk
{
    private Vector3Int Chunkpos;
    public int[] Voxels = new int[32 * 32 * 32];

    private int Offset(int x, int y, int z)
    {
        if (x < 0 || x > 32 || y < 0 || y > 32 || z < 0 || z > 32) return 0;
        return x + 32 * (y + 32 * z);
    }

    public int GetVoxel(int x, int y, int z)
    {
        return Voxels[Offset(x, y, z)];
    }
    public void SetVoxel(int x, int y, int z, int voxel)
    {
        Voxels[Offset(x, y, z)] = voxel;
    }

    public int GetVoxelColour(int x, int y, int z)
    {
        return Voxels[Offset(x, y, z)] & 0xFF;
    }

    public void SetVoxelColour(int x, int y, int z, byte col)
    {
        Voxels[Offset(x, y, z)] = ReplaceByte(0, Voxels[Offset(x, y, z)], col);
    }

    public int[] GetVoxels()
    {
        return Voxels;
    }

    public Chunk()
    {
        
    }


    private int ReplaceByte(int index, int value, byte replaceByte)
    {
        return (value & ~(0xFF << (index * 8))) | (replaceByte << (index * 8));
    }
}
