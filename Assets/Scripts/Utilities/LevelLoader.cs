using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{

    [SerializeField] private int _sceneToLoad = 1;
    [SerializeField] private Canvas _canvas = null;
    [SerializeField] private Image _overlay = null;
    [SerializeField] private Image _img = null;

    private void Start() => StartCoroutine(LoadScene());

    private IEnumerator LoadScene()
    {
        AsyncOperation loading = SceneManager.LoadSceneAsync(_sceneToLoad);

        DontDestroyOnLoad(this);
        DontDestroyOnLoad(_canvas);
        _overlay.GetComponent<Animator>().SetBool(Str.Visible, true);

        while (!loading.isDone)
        {
            float prog = Mathf.Clamp01(loading.progress / .9f);
            
            yield return null;
        }

        _img.GetComponent<Animator>().SetTrigger(Str.FadeOut);
        _overlay.GetComponent<Animator>().SetBool(Str.Visible, false);
        yield return new WaitForSeconds(2f);

        _canvas.enabled = false;
        _img.enabled = false;
        Destroy(gameObject);
    }
}
