using System.Collections;
using UnityEngine;

public class QuestItemRepository : MonoBehaviour
{

    public bool _collectedSeed { get; private set; } = false;
    public bool _collectedSoil { get; private set; } = false;
    public bool _collectedRain { get; private set; } = false;

    [SerializeField] private Transform targetItemPosition;
    [SerializeField] private Transform targetStep1PlayerPosition;
    [SerializeField] private Transform targetStep2PlayerPosition;
    [SerializeField] private Transform targetStep3PlayerPosition; // For mid cutscenes
    [SerializeField] private Transform targetStep4PlayerPosition; // For end cutscene

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
                QuestManager.AdvanceQuest(Str.Warbler);
                QuestManager.SetBoolMemoryVar(Str.Seed);
                break;
            case QuestItem.QuestItemEnum.Soil:
                if (_collectedSoil) return;
                _collectedSoil = true;
                QuestManager.AdvanceQuest(Str.Frog);
                QuestManager.SetBoolMemoryVar(Str.Soil);
                break;
            case QuestItem.QuestItemEnum.Rain:
                if (_collectedRain) return;
                _collectedRain = true;
                QuestManager.AdvanceQuest(Str.Turtle);
                QuestManager.SetBoolMemoryVar(Str.Rain);
                break;
        }
        QuestManager.AdvanceQuest(Str.Main);

        currentQuestItem = item;

        
        
    }

    public Transform TargetItemPosition => targetItemPosition;
    public Transform TargetStep1PlayerPosition => targetStep1PlayerPosition;
    public Transform TargetStep2PlayerPosition => targetStep2PlayerPosition;
    public Transform TargetStep3PlayerPosition => targetStep3PlayerPosition;
    public Transform TargetStep4PlayerPosition => targetStep4PlayerPosition;

    public void StartSequence() => StartCoroutine(CollectItem(currentQuestItem));

    private IEnumerator CollectItem(QuestItem item)
    {
        Services.PlayerItemHolder.DetachFromTransform();
        item.holdable = false;
        item.rb.isKinematic = true;

        item.transform.SetParent(itemHolder);

        // Lerp to correct position...
        const float duration = .25f;
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
        _animator.SetTrigger(Str.MidSequence);
    }

    public void RemoveObject() => currentQuestItem.Disappear();
}
