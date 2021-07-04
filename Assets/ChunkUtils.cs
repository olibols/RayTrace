using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using VoxReader;

class ChunkUtils
{
    public static int CreateBlock(byte col)
    {
        return (0xFF << 24) | (0xFF << 16) | (0xFF << 8) | (col);
    }

    public static int Offset(int x, int y, int z)
    {
        if (x < 0 || x > 31 || y < 0 || y > 31 || z < 0 || z > 31) return 0;
        return x + 32 * (y + 32 * z);
    }

    private static int GetVoxelDist(int voxel)
    {
        return (voxel >> 8) & 0xFF;
    }

    private static int GetVoxelColor(int voxel)
    {
        return voxel & 0xFF;
    }

    public static void SetupChunk(ref Chunk chunk)
    {
        var data = File.ReadAllBytes("Assets/Models/grass.vox");
        var chunks = Reader.GetChunks(data);
        var voxels = chunks.FirstOrDefault(c => c.Id == nameof(ChunkType.XYZI)) as VoxelChunk;

        for (int x = 0; x < 32 / 16; x++)
        {
            for (int z = 0; z < 32 / 16; z++)
            {
                foreach(var voxel in voxels.Voxels)
                {
                    chunk.SetVoxelColour(voxel.X + x*16, voxel.Z + 3, voxel.Y + z*16, (byte)(voxel.ColorIndex - 1));
                }
            }
        }

        var tpdata = File.ReadAllBytes("Assets/Models/teapot.vox");
        var tpchunks = Reader.GetChunks(tpdata);
        var tpvoxels = tpchunks.FirstOrDefault(c => c.Id == nameof(ChunkType.XYZI)) as VoxelChunk;

        foreach (var voxel in tpvoxels.Voxels)
        {
            chunk.SetVoxelColour(voxel.X + 100, voxel.Z + 3, voxel.Y + 100, (byte)(voxel.ColorIndex-1));
        }

        for (int x = 0; x < 32; x++)
        {
            for (int z = 0; z < 32; z++)
            {
                chunk.SetVoxelColour(x, 1, z, 3);
                chunk.SetVoxelColour(x, 2, z, 79);
            }
        }

    }

    private static bool IsEmptyCube(int[] Voxels, Vector3Int StartPos, Vector3Int EndPos)
    {
        int i = 0;
        for(int x = StartPos.x; x < EndPos.x; x++)
        {
            for (int y = StartPos.y; y < EndPos.y; y++)
            {
                for (int z = StartPos.z; z < EndPos.z; z++)
                {
                    if (GetVoxelColor(Voxels[Offset(x, y, z)]) != 0) return false;
                    i++;
                    if (i > 500) return true;
                }
            }
        }
        return true;
    }

    private static int FindMaxJump(int[] Voxels, Vector3Int startpos)
    {
        int jump = 0;

        while(IsEmptyCube(Voxels, startpos - new Vector3Int(jump + 1, jump + 1, jump + 1), startpos + new Vector3Int(jump + 1, jump + 1, jump + 1)) && jump < 32)
        {
            jump++;
        }

        return jump;
    }

    public static void CalculateChunkSdf(ref Chunk chunk)
    {
        for(int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                for (int z = 0; z < 32; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);

                    int ofst = Offset(x, y, z);
                    if (GetVoxelColor(chunk.Voxels[ofst]) == 0)
                    {
                        chunk.Voxels[ofst] = ReplaceByte(1, chunk.Voxels[ofst], (byte)FindMaxJump(chunk.Voxels, pos));
                    }
                }
            }
        }
    }

    private static int ReplaceByte(int index, int value, byte replaceByte)
    {
        return (value & ~(0xFF << (index * 8))) | (replaceByte << (index * 8));
    }
}