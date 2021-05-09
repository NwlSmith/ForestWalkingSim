using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
public class CutscenePlantsTerrainToGO : MonoBehaviour
{
    private static LayerMask layerMask = 1025;
#if UNITY_EDITOR

    [@MenuItem("Terrain/Convert cutscene terrain trees to gameObjects")]
    static void Run()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            return;
        }

        CutsceneObjectHelper cutsceneObjectHelper = FindObjectOfType<CutsceneObjectHelper>();

        Material[] flowerMats1 = cutsceneObjectHelper.flowerMats1;
        Material[] flowerMats2 = cutsceneObjectHelper.flowerMats2;

        TerrainData td = terrain.terrainData;

        CutsceneObjectsManager cutsceneObjectsManager = FindObjectOfType<CutsceneObjectsManager>();

        Transform grassParent = cutsceneObjectsManager.transform.GetChild(0);
        Transform flowerParent = cutsceneObjectsManager.transform.GetChild(1);
        Transform treeParent = cutsceneObjectsManager.transform.GetChild(2);
        foreach (TreeInstance tree in td.treeInstances)
        {
            TreePrototype treeProt = td.treePrototypes[tree.prototypeIndex];
            GameObject prefab = treeProt.prefab;

            if (prefab.name.Contains("Grass") && prefab.name.Contains("Cutscene"))
            {
                Vector3 pos = Vector3.Scale(tree.position, td.size) + terrain.transform.position;

                GameObject obj = GameObject.Instantiate(prefab, pos, Quaternion.AngleAxis(tree.rotation * Mathf.Rad2Deg, Vector3.up)) as GameObject;
                MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer renderer in renderers)
                {
                    renderer.receiveShadows = false;
                    //renderer.shadowCastingMode = ShadowCastingMode.TwoSided;
                    GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.ContributeGI);
                }

                Transform t = obj.transform;
                t.localScale = new Vector3(tree.widthScale, tree.heightScale, tree.widthScale);
                t.parent = grassParent.transform;

                if (Physics.Raycast(obj.transform.position + 30f * Vector3.up, Vector3.down, out RaycastHit hit, 50f, layerMask.value, QueryTriggerInteraction.Ignore))
                {
                    obj.transform.position = hit.point;
                    obj.transform.up = hit.normal;
                }
            }
            else if (prefab.name.Contains("flower"))
            {
                Vector3 pos = Vector3.Scale(tree.position, td.size) + terrain.transform.position;

                GameObject obj = GameObject.Instantiate(prefab, pos, Quaternion.AngleAxis(tree.rotation * Mathf.Rad2Deg, Vector3.up)) as GameObject;
                MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer renderer in renderers)
                {
                    renderer.receiveShadows = false;
                    renderer.shadowCastingMode = ShadowCastingMode.TwoSided;
                    GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.ContributeGI);
                    if (renderer.gameObject.name.Equals("Circle"))
                    {
                        renderer.material = flowerMats1[Random.Range(0, flowerMats1.Length)];
                    }
                    else if (renderer.gameObject.name.Equals("Circle.001"))
                    {
                        renderer.sharedMaterials[1] = flowerMats2[Random.Range(0, flowerMats2.Length)];
                    }
                }

                Transform t = obj.transform;
                t.localScale = new Vector3(tree.widthScale, tree.heightScale, tree.widthScale);
                t.parent = flowerParent.transform;

                if (Physics.Raycast(t.position + 30f * Vector3.up, Vector3.down, out RaycastHit hit, 50f, layerMask.value, QueryTriggerInteraction.Ignore))
                {
                    t.position = hit.point;
                    t.up = hit.normal;
                }
            }

        }
    }
#endif

}
