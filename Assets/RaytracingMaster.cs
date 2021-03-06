using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using VoxReader;

public class RaytracingMaster : MonoBehaviour
{
    public ComputeShader RayTracingShader;
    public Texture SkyboxTexture;

    public WorldController World;

    public Light DirectionalLight;

    private RenderTexture m_Target;
    private Camera m_Camera;

    private ComputeBuffer BlocksBuffer;
    private ComputeBuffer PaletteBuffer;

    private ComputeBuffer Debug;
    private Vector4[] element;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Render(destination);
    }

    private int Offset(int x, int y, int z)
    {
        return x + 32 * (y + 32 * z);
    }

    private void Start()
    {
        m_Camera = GetComponent<Camera>();
        element = new Vector4[1];

        // Read data from magicavox files
        var data = File.ReadAllBytes("Assets/Models/blue flower.vox");
        var chunks = Reader.GetChunks(data);
        var paletteChunk = chunks.FirstOrDefault(c => c.Id == nameof(ChunkType.RGBA)) as PaletteChunk;
        Vector3Int[] PaletteArray = new Vector3Int[256];
        for (int i = 0; i < paletteChunk.Colors.Length; i++)
        {
            var col = paletteChunk.Colors[i];
            PaletteArray[i] = new Vector3Int(col.R, col.G, col.B);
        }

        Debug = new ComputeBuffer(1, 16, ComputeBufferType.Default);
        RayTracingShader.SetBuffer(0, "debug", Debug);
        Graphics.SetRandomWriteTarget(0, Debug, false);

        PaletteBuffer = new ComputeBuffer(PaletteArray.Length, sizeof(int) * 3, ComputeBufferType.Structured);
        PaletteBuffer.SetData(PaletteArray);
        RayTracingShader.SetBuffer(0, "PaletteBuffer", PaletteBuffer);

        int[] blockarr = World.GetChunk(0, 0, 0).GetVoxels();
        BlocksBuffer = new ComputeBuffer(blockarr.Length, sizeof(int), ComputeBufferType.Structured);
        BlocksBuffer.SetData(blockarr);
        RayTracingShader.SetBuffer(0, "Blocks", BlocksBuffer);
    }

    private void SetShaderParams()
    {
        RayTracingShader.SetTexture(0, "Result", m_Target);
        RayTracingShader.SetTexture(0, "SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetMatrix("CameraToWorld", m_Camera.cameraToWorldMatrix);        
        RayTracingShader.SetMatrix("CameraInverse", m_Camera.projectionMatrix.inverse);

        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("LightDir", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));
    }

    private void Render(RenderTexture destination)
    {
        InitRenderTexture();
        SetShaderParams();

        int threadGroupsX = Mathf.CeilToInt(Screen.width / 32.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 32.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 5);
        Graphics.Blit(m_Target, destination);

        Debug.GetData(element);
        if (element[0].w == 0.0f)
        {
            print($"{element[0].x}, {element[0].y}, {element[0].z}");
            Vector4[] arr = new Vector4[1];
            arr[0] = new Vector4(0.0f, 0.0f, 0.0f, 999.0f);
            Debug.SetData(arr);
        }
    }

    private void OnDrawGizmos()
    {
        for(int x = 0; x < 6; x++)
        {
            Gizmos.DrawWireCube(new Vector3(x + 0.5f, 0.5f, 0.5f), new Vector3(1.0f, 1.0f, 1.0f));
        }
    }

    private void OnGUI()
    {
        GUI.Box(new Rect((Screen.width / 2) - 5, (Screen.height / 2) - 5, 10, 10), "");
    }

    private void InitRenderTexture()
    {
        if (m_Target == null || m_Target.width != Screen.width || m_Target.height != Screen.height)
        {
            if (m_Target != null)
                m_Target.Release();
            m_Target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            m_Target.enableRandomWrite = true;
            m_Target.Create();
        }
    }
}
