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
            Debug.LogError("Не задан родительский объект!");
            return;
        }

        if (customLightmapParameters == null)
        {
            Debug.LogError("Не выбраны параметры освещения (Custom Lightmap Parameters)!");
            return;
        }

        // Обхожу все LOD‑группы под родителем
        LODGroup[] lodGroups = parentObject.GetComponentsInChildren<LODGroup>(true);
        Debug.Log($"Найдено LOD‑групп: {lodGroups.Length}");

        int totalRenderers = 0;

        foreach (LODGroup lodGroup in lodGroups)
        {
            LOD[] lods = lodGroup.GetLODs();

            for (int i = 0; i < lods.Length; i++)
            {
                float scaleFactor = lod0ScaleFactor;

                // Чем ниже уровень LOD, тем меньше Scale in Lightmap
                if (i > 0)
                {
                    scaleFactor *= Mathf.Pow(lodScaleReductionFactor, i);
                }

                foreach (Renderer renderer in lods[i].renderers)
                {
                    if (renderer != null)
                    {
                        // Через SerializedObject меняю защищённые поля рендера
                        SerializedObject serializedRenderer = new SerializedObject(renderer);

                        // Scale in Lightmap
                        SerializedProperty scaleInLightmap = serializedRenderer.FindProperty("m_ScaleInLightmap");
                        scaleInLightmap.floatValue = scaleFactor;

                        // Lightmap Parameters
                        SerializedProperty lightmapParams = serializedRenderer.FindProperty("m_LightmapParameters");
                        lightmapParams.objectReferenceValue = customLightmapParameters;

                        serializedRenderer.ApplyModifiedProperties();
                        totalRenderers++;
                    }
                }
            }
        }

        Debug.Log($"Готово. Настройки применены к {totalRenderers} рендерам.");
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

        EditorGUILayout.HelpBox("1) Укажи родительский объект с LOD\n2) Выбери Lightmap Parameters (например, Default-VeryLowResolution)\n3) Настрой коэффициенты\n4) Нажми кнопку", MessageType.Info);

        if (GUILayout.Button("Оптимизировать LODs"))
        {
            optimizer.OptimizeLODs();
        }
    }
}
#endif