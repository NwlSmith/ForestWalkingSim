using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestItemRepository : MonoBehaviour
{

    private bool _collectedSeed = false;
    private bool _collectedSoil = false;
    private bool _collectedRain = false;

    [SerializeField] private Transform targetItemPosition;

    private void OnTriggerEnter(Collider other)
    {
        QuestItem item = other.GetComponent<QuestItem>();

        if (item == null) return;

        switch (item.itemEnum)
        {
            case QuestItem.QuestItemEnum.Seed:
                if (_collectedSeed) return;
                _collectedSeed = true;
                Services.QuestManager.AdvanceQuest("Main");
                break;
            case QuestItem.QuestItemEnum.Soil:
                if (_collectedSoil) return;
                _collectedSoil = true;
                Services.QuestManager.AdvanceQuest("Main");
                break;
            case QuestItem.QuestItemEnum.Rain:
                if (_collectedRain) return;
                _collectedRain = true;
                Services.QuestManager.AdvanceQuest("Main");
                break;
        }

        StartCoroutine(CollectItem(item));
    }

    private IEnumerator CollectItem(QuestItem item)
    {
        Services.PlayerItemHolder.DropItem();
        item.holdable = false;
        item.rb.isKinematic = true;

        item.transform.SetParent(transform);

        const float duration = 1f;
        float elapsedTime = 0f;
        Vector3 initPos = item.transform.position;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            item.rb.position = Vector3.Slerp(initPos, targetItemPosition.position, elapsedTime / duration);
            yield return null;
        }

        item.transform.position = targetItemPosition.position;

        elapsedTime = 0f;
        Vector3 initScale = item.transform.localScale;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            item.transform.localScale = Vector3.Slerp(initScale, Vector3.zero, elapsedTime / duration);
            yield return null;
        }
    }

}
