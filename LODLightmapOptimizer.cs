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
            Debug.LogError("Укажите родительский объект!");
            return;
        }

        if (customLightmapParameters == null)
        {
            Debug.LogError("Укажите параметры лайтмапов в поле Custom Lightmap Parameters!");
            return;
        }

        // Найти все LOD группы
        LODGroup[] lodGroups = parentObject.GetComponentsInChildren<LODGroup>(true);
        Debug.Log($"Найдено LOD групп: {lodGroups.Length}");

        int totalRenderers = 0;

        foreach (LODGroup lodGroup in lodGroups)
        {
            LOD[] lods = lodGroup.GetLODs();

            for (int i = 0; i < lods.Length; i++)
            {
                float scaleFactor = lod0ScaleFactor;

                // Для последующих уровней LOD применяем уменьшающий фактор
                if (i > 0)
                {
                    scaleFactor *= Mathf.Pow(lodScaleReductionFactor, i);
                }

                foreach (Renderer renderer in lods[i].renderers)
                {
                    if (renderer != null)
                    {
                        // Получаем SerializedObject для редактирования protected/private полей
                        SerializedObject serializedRenderer = new SerializedObject(renderer);

                        // Устанавливаем Scale in Lightmap
                        SerializedProperty scaleInLightmap = serializedRenderer.FindProperty("m_ScaleInLightmap");
                        scaleInLightmap.floatValue = scaleFactor;

                        // Устанавливаем Lightmap Parameters
                        SerializedProperty lightmapParams = serializedRenderer.FindProperty("m_LightmapParameters");
                        lightmapParams.objectReferenceValue = customLightmapParameters;

                        serializedRenderer.ApplyModifiedProperties();
                        totalRenderers++;
                    }
                }
            }
        }

        Debug.Log($"Обработка завершена. Настройки применены к {totalRenderers} рендерерам.");
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

        EditorGUILayout.HelpBox("1. Перетащите родительский объект с LOD-группами\n2. Перетащите параметры лайтмапов (Default-VeryLowResolution)\n3. Настройте масштабные факторы\n4. Нажмите кнопку", MessageType.Info);

        if (GUILayout.Button("Оптимизировать LODs"))
        {
            optimizer.OptimizeLODs();
        }
    }
}
#endif