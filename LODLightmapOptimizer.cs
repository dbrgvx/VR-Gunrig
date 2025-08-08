using UnityEngine;
using UnityEditor;

public class LODLightmapOptimizer : MonoBehaviour
{
    [SerializeField] private Transform parentObject;
    [SerializeField] private float lod0ScaleFactor = 0.1f;
    [SerializeField] private float lodScaleReductionFactor = 0.5f;
    [SerializeField] private LightmapParameters customLightmapParameters;

    public void OptimizeLODs()
    {
        if (parentObject == null)
        {
            Debug.LogError("������� ������������ ������!");
            return;
        }

        if (customLightmapParameters == null)
        {
            Debug.LogError("������� ��������� ��������� � ���� Custom Lightmap Parameters!");
            return;
        }

        // ����� ��� LOD ������
        LODGroup[] lodGroups = parentObject.GetComponentsInChildren<LODGroup>(true);
        Debug.Log($"������� LOD �����: {lodGroups.Length}");

        int totalRenderers = 0;

        foreach (LODGroup lodGroup in lodGroups)
        {
            LOD[] lods = lodGroup.GetLODs();

            for (int i = 0; i < lods.Length; i++)
            {
                float scaleFactor = lod0ScaleFactor;

                // ��� ����������� ������� LOD ��������� ����������� ������
                if (i > 0)
                {
                    scaleFactor *= Mathf.Pow(lodScaleReductionFactor, i);
                }

                foreach (Renderer renderer in lods[i].renderers)
                {
                    if (renderer != null)
                    {
                        // �������� SerializedObject ��� �������������� protected/private �����
                        SerializedObject serializedRenderer = new SerializedObject(renderer);

                        // ������������� Scale in Lightmap
                        SerializedProperty scaleInLightmap = serializedRenderer.FindProperty("m_ScaleInLightmap");
                        scaleInLightmap.floatValue = scaleFactor;

                        // ������������� Lightmap Parameters
                        SerializedProperty lightmapParams = serializedRenderer.FindProperty("m_LightmapParameters");
                        lightmapParams.objectReferenceValue = customLightmapParameters;

                        serializedRenderer.ApplyModifiedProperties();
                        totalRenderers++;
                    }
                }
            }
        }

        Debug.Log($"��������� ���������. ��������� ��������� � {totalRenderers} ����������.");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LODLightmapOptimizer))]
public class LODLightmapOptimizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LODLightmapOptimizer optimizer = (LODLightmapOptimizer)target;

        EditorGUILayout.HelpBox("1. ���������� ������������ ������ � LOD-��������\n2. ���������� ��������� ��������� (Default-VeryLowResolution)\n3. ��������� ���������� �������\n4. ������� ������", MessageType.Info);

        if (GUILayout.Button("�������������� LODs"))
        {
            optimizer.OptimizeLODs();
        }
    }
}
#endif