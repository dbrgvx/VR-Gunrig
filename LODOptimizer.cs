using UnityEngine;
using UnityEditor;

public class LODOptimizer : MonoBehaviour
{
    public Transform parentObject;

    public void OptimizeLODs()
    {
        if (parentObject == null)
        {
            Debug.LogError("������������ ������ �� ������!");
            return;
        }

        LODGroup[] lodGroups = parentObject.GetComponentsInChildren<LODGroup>(true);

        if (lodGroups.Length == 0)
        {
            Debug.LogWarning("LOD ����� �� ������� � �������� ��������!");
            return;
        }

        int groupsProcessed = 0;

        foreach (LODGroup lodGroup in lodGroups)
        {
            LOD[] lods = lodGroup.GetLODs();

            // ���������, ���������� �� LOD � ������
            if (lods.Length < 2)
            {
                Debug.LogWarning("������������ LOD � ������: " + lodGroup.gameObject.name);
                continue;
            }

            // ������� ����� ������ LOD (��� LOD2, ���� �� ���)
            LOD[] newLods;
            if (lods.Length > 2) // ���� ���� LOD2 (��� ������)
            {
                newLods = new LOD[2]; // LOD0 � LOD1 
                newLods[0] = lods[0]; // ��������� LOD0 ��� ����
                newLods[1] = lods[1]; // ��������� LOD1
            }
            else // ������ LOD0 � LOD1
            {
                newLods = new LOD[2];
                newLods[0] = lods[0];
                newLods[1] = lods[1];
            }

            // ������������� ���������� ��������
            newLods[0].screenRelativeTransitionHeight = 0.05f; // LOD0 - �� ���������
            newLods[1].screenRelativeTransitionHeight = 0.0f;  // LOD1 - 0% (Culled)

            // ��������� ��������� LOD1 �� ��������� ���������
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

            // ��������� ����� ��������� LOD
            lodGroup.SetLODs(newLods);
            EditorUtility.SetDirty(lodGroup);

            groupsProcessed++;
        }

        Debug.Log("�������������� LOD �����: " + groupsProcessed);
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

        if (GUILayout.Button("�������������� LOD"))
        {
            optimizer.OptimizeLODs();
        }
    }
}
#endif