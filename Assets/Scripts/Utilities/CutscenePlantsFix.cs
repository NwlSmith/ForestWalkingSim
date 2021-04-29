using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class CutscenePlantsFix
{

    private static LayerMask layerMask = 1025;

    [@MenuItem("Terrain/Cutscene Plants Fix")]
    static void Run()
    {
        Transform parent = Object.FindObjectOfType<CutsceneObjectsManager>().transform;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (Physics.Raycast(child.position + 30f * Vector3.up, Vector3.down, out RaycastHit hit, 50f, layerMask.value, QueryTriggerInteraction.Ignore))
            {
                child.position = hit.point;
                if (child.GetComponent<LODGroup>() == null)
                    child.up = hit.normal;
                else
                    child.up = Vector3.up;
                child.RotateAround(hit.point, child.up, Random.Range(0, 360f));
            }
        }
    }
}
