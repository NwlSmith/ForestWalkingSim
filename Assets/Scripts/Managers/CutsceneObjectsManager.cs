using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneObjectsManager : MonoBehaviour
{

    [SerializeField] private GameObject[] gameObjects;

    private void Start()
    {
        foreach (GameObject gameObject in gameObjects)
        {
            gameObject.transform.localScale = Vector3.zero;
        }
    }

    public IEnumerator Transition(int phase)
    {
        float elapsedTime = 0f;
        float duration = 5f;

        if (phase == 3)
            duration = 4f;

        float target;
        switch (phase)
        {
            case 1:
                target = .25f;
                break;
            case 2:
                target = .5f;
                break;
            case 3:
                target = 1.25f;
                break;
            default:
                break;
        }
        yield return null;

    }
}
