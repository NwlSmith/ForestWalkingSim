using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingManager : MonoBehaviour
{

    [SerializeField] private VolumeProfile[] stages = new VolumeProfile[4];
    private Volume _volume;

    private int _curStage = 0;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.profile = stages[0];
    }

    public void AdvanceStage()
    {
        if (_curStage >= stages.Length - 1) return;
        _curStage++;
        _volume.profile = stages[_curStage];
    }
}