using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndCreditsHandler : MonoBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private RectTransform textHolder;

    private Vector3 movement;

    private void Awake() => movement = new Vector3(0, speed, 0);

    private void Start() => StartCoroutine(EndCredits());

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        else if (Input.GetKeyDown(KeyCode.Space)) NextScene();
    }

    private IEnumerator EndCredits()
    {
        FModMusicManager.Init();
        FModMusicManager.StartFoxTheme();
        yield return new WaitForSeconds(2f);
        float elapsed = 0;
        float duration = 75f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            textHolder.transform.position = textHolder.transform.position + movement * Time.deltaTime;
            yield return null;
        }

        NextScene();
    }

    private void NextScene()
    {
        FModMusicManager.EndFoxTheme();
        FModMusicManager.OnDestroy();
        SceneManager.LoadScene(0);
    }
}
