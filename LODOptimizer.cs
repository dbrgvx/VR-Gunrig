using UnityEngine;
using UnityEditor;

public class LODOptimizer : MonoBehaviour
{
    public Transform parentObject;

    public void OptimizeLODs()
    {
        if (parentObject == null)
        {
            Debug.LogError("Родительский объект не задан!");
            return;
        }

        LODGroup[] lodGroups = parentObject.GetComponentsInChildren<LODGroup>(true);

        if (lodGroups.Length == 0)
        {
            Debug.LogWarning("LOD‑группы у потомков не найдены!");
            return;
        }

        int groupsProcessed = 0;

        foreach (LODGroup lodGroup in lodGroups)
        {
            LOD[] lods = lodGroup.GetLODs();

            // Проверяю, есть ли минимум два уровня LOD
            if (lods.Length < 2)
            {
                Debug.LogWarning("Недостаточно LOD у объекта: " + lodGroup.gameObject.name);
                continue;
            }

            // Оставляю только LOD0 и LOD1
            LOD[] newLods;
            if (lods.Length > 2)
            {
                newLods = new LOD[2];
                newLods[0] = lods[0];
                newLods[1] = lods[1];
            }
            else
            {
                newLods = new LOD[2];
                newLods[0] = lods[0];
                newLods[1] = lods[1];
            }

            // Порог переключения
            newLods[0].screenRelativeTransitionHeight = 0.05f; // LOD0 — почти всегда
            newLods[1].screenRelativeTransitionHeight = 0.0f;  // LOD1 — culled

            // Для LOD1 убираю вклад в GI
            foreach (Renderer renderer in newLods[1].renderers)
            {
                if (renderer != null)
                {
                    GameObjectUtility.SetStaticEditorFlags(
                        renderer.gameObject,
                        GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) & ~StaticEditorFlags.ContributeGI
                    );
                }
            }

            // Применяю новые настройки
            lodGroup.SetLODs(newLods);
            EditorUtility.SetDirty(lodGroup);

            groupsProcessed++;
        }

        Debug.Log("Оптимизировано LOD‑групп: " + groupsProcessed);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LODOptimizer))]
public class LODOptimizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LODOptimizer optimizer = (LODOptimizer)target;

        if (GUILayout.Button("Оптимизировать LOD"))
        {
            optimizer.OptimizeLODs();
        }
    }
}
#endif