using UnityEngine;
using UnityEditor;

public class LODOptimizer : MonoBehaviour
{
    public Transform parentObject;

    public void OptimizeLODs()
    {
        if (parentObject == null)
        {
            Debug.LogError("Родительский объект не указан!");
            return;
        }

        LODGroup[] lodGroups = parentObject.GetComponentsInChildren<LODGroup>(true);

        if (lodGroups.Length == 0)
        {
            Debug.LogWarning("LOD групп не найдено в дочерних объектах!");
            return;
        }

        int groupsProcessed = 0;

        foreach (LODGroup lodGroup in lodGroups)
        {
            LOD[] lods = lodGroup.GetLODs();

            // Проверяем, достаточно ли LOD в группе
            if (lods.Length < 2)
            {
                Debug.LogWarning("Недостаточно LOD в группе: " + lodGroup.gameObject.name);
                continue;
            }

            // Создаем новый массив LOD (без LOD2, если он был)
            LOD[] newLods;
            if (lods.Length > 2) // Если есть LOD2 (или больше)
            {
                newLods = new LOD[2]; // LOD0 и LOD1 
                newLods[0] = lods[0]; // Сохраняем LOD0 как есть
                newLods[1] = lods[1]; // Сохраняем LOD1
            }
            else // Только LOD0 и LOD1
            {
                newLods = new LOD[2];
                newLods[0] = lods[0];
                newLods[1] = lods[1];
            }

            // Устанавливаем процентные значения
            newLods[0].screenRelativeTransitionHeight = 0.05f; // LOD0 - по умолчанию
            newLods[1].screenRelativeTransitionHeight = 0.0f;  // LOD1 - 0% (Culled)

            // Отключаем рендереры LOD1 от запекания освещения
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

            // Применяем новые настройки LOD
            lodGroup.SetLODs(newLods);
            EditorUtility.SetDirty(lodGroup);

            groupsProcessed++;
        }

        Debug.Log("Оптимизировано LOD групп: " + groupsProcessed);
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