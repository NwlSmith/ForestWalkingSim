using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    #region Const Strings.
    private readonly int _fadeOut = Animator.StringToHash("FadeOut");
    private readonly int _visible = Animator.StringToHash("Visible");
    #endregion

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
        _overlay.GetComponent<Animator>().SetBool(_visible, true);

        while (!loading.isDone)
        {
            float prog = Mathf.Clamp01(loading.progress / .9f);
            Logger.Debug($"Progress: {prog}, scene activation allowed?");
            
            yield return null;
        }

        _img.GetComponent<Animator>().SetTrigger(_fadeOut);
        _overlay.GetComponent<Animator>().SetBool(_visible, false);
        yield return new WaitForSeconds(2f);

        _canvas.enabled = false;
        _img.enabled = false;
        Destroy(gameObject);
    }
}
