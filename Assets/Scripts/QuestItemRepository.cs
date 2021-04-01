using System.Collections;
using UnityEngine;

public class QuestItemRepository : MonoBehaviour
{
    #region String Cache.
    private readonly string _midSequence = "MidSequence";
    private readonly string _endSequence = "EndSequence";
    #endregion

    private bool _collectedSeed = false;
    private bool _collectedSoil = false;
    private bool _collectedRain = false;

    [SerializeField] private Transform targetItemPosition;
    [SerializeField] private Transform targetStep1PlayerPosition;
    [SerializeField] private Transform targetStep2PlayerPosition;
    [SerializeField] private Transform targetStep3PlayerPosition;

    [SerializeField] private Transform itemHolder;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public QuestItem currentQuestItem { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Item")) return;

        QuestItem item = other.GetComponent<QuestItem>();

        if (item == null) return;

        switch (item.itemEnum)
        {
            case QuestItem.QuestItemEnum.Seed:
                if (_collectedSeed) return;
                _collectedSeed = true;
                Services.QuestManager.AdvanceQuest("Warbler");
                break;
            case QuestItem.QuestItemEnum.Soil:
                if (_collectedSoil) return;
                _collectedSoil = true;
                Services.QuestManager.AdvanceQuest("Frog");
                break;
            case QuestItem.QuestItemEnum.Rain:
                if (_collectedRain) return;
                _collectedRain = true;
                Services.QuestManager.AdvanceQuest("Turtle");
                break;
        }
        Services.QuestManager.AdvanceQuest("Main");

        currentQuestItem = item;

        Services.GameManager.MidrollCutscene();
        
    }

    public Transform TargetItemPosition => targetItemPosition;
    public Transform TargetStep1PlayerPosition => targetStep1PlayerPosition;
    public Transform TargetStep2PlayerPosition => targetStep2PlayerPosition;
    public Transform TargetStep3PlayerPosition => targetStep3PlayerPosition;

    public void StartSequence() => StartCoroutine(CollectItem(currentQuestItem));

    private IEnumerator CollectItem(QuestItem item)
    {
        Services.PlayerItemHolder.DetachFromTransform();
        item.holdable = false;
        item.rb.isKinematic = true;

        item.transform.SetParent(itemHolder);

        // Lerp to correct position...
        const float duration = .5f;
        float elapsedTime = 0f;
        Vector3 initPos = item.transform.position;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            item.transform.position = Vector3.Lerp(initPos, itemHolder.position, elapsedTime / duration);
            yield return null;
        }

        item.transform.position = itemHolder.position;
        // And animator handles the rest!
        _animator.SetTrigger(_midSequence);
    }

    public void RemoveObject() => currentQuestItem.Disappear();
}
