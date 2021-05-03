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
    private QuestItem[] _questItems;
    private Transform _playerItemHolder;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _questItems = FindObjectsOfType<QuestItem>();
        _playerItemHolder = FindObjectOfType<PlayerItemHolder>().transform;
    }

    private QuestItem currentQuestItem;
    public QuestItem CurrentQuestItem()
    {
        if (currentQuestItem == null)
            currentQuestItem = FindClosestQuestItem();
        return currentQuestItem;
    }

    private QuestItem FindClosestQuestItem()
    {
        Logger.Warning("Failsafe: Someone called FindClosestQuestItem() on QuestItemRepository");
        float closestDist = 75f;
        QuestItem closeItem = null;

        foreach (QuestItem item in _questItems)
        {
            float curDist = Vector3.Distance(_playerItemHolder.position, item.transform.position);
            if (curDist < closestDist)
            {
                closestDist = curDist;
                closeItem = item;
            }
        }
        if (closeItem == null)
        {
            Logger.Warning("Failed to find a close Quest Item");
            return _questItems[0];
        }
        return closeItem;
    }

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
                QuestManager.SetBoolMemoryVar("SeedLastRetrieved");
                break;
            case QuestItem.QuestItemEnum.Soil:
                if (_collectedSoil) return;
                _collectedSoil = true;
                QuestManager.AdvanceQuest(Str.Frog);
                QuestManager.SetBoolMemoryVar(Str.Soil);
                QuestManager.SetBoolMemoryVar("SoilLastRetrieved");
                break;
            case QuestItem.QuestItemEnum.Rain:
                if (_collectedRain) return;
                _collectedRain = true;
                QuestManager.AdvanceQuest(Str.Turtle);
                QuestManager.SetBoolMemoryVar(Str.Rain);
                QuestManager.SetBoolMemoryVar("RainLastRetrieved");
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

        yield return new WaitForSeconds(4f * 60f);

        item.transform.position = Vector3.zero;
    }

    public void RemoveObject() => currentQuestItem.Disappear();
}
