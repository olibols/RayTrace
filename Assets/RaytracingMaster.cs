using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaytracingMaster : MonoBehaviour
{
    public ComputeShader RayTracingShader;
    public Texture SkyboxTexture;

    public Texture CrosshairTexture;

    public Light DirectionalLight;

    private RenderTexture m_Target;
    private Camera m_Camera;

    private ComputeBuffer BlocksBuffer;

    private ComputeBuffer Debug;
    private Vector4[] element;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Render(destination);
    }

    private int offset(int x, int y, int z)
    {
        return x + 32 * (y + 32 * z);
    }

    private void Awake()
    {
        m_Camera = GetComponent<Camera>();

        int[] blockarr = new int[256 * 256 * 256];

        element = new Vector4[1];

        for(int x = 0; x < 32; x++)
        {
            for (int z = 0; z < 32; z++)
            {
                blockarr[offset(x, 1, z)] = 2;
            }
        }

        blockarr[offset(5, 4, 5)] = 2;
        blockarr[offset(5, 3, 5)] = 2;
        blockarr[offset(5, 2, 5)] = 2;

        Debug = new ComputeBuffer(1, 16, ComputeBufferType.Default);
        RayTracingShader.SetBuffer(0, "debug", Debug);
        Graphics.SetRandomWriteTarget(0, Debug, false);

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
        //Gizmos.DrawCube(new Vector3(element[0].x - 0.5f, element[0].y - 0.5f, element[0].z - 0.5f), new Vector3(1.0f, 1.0f, 1.0f));

        for(int x = 0; x < 6; x++)
        {
            Gizmos.DrawWireCube(new Vector3(x - 0.5f, -0.5f, -0.5f), new Vector3(1.0f, 1.0f, 1.0f));
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
