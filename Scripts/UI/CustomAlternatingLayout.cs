using System;
using UnityEngine;

public class CustomAlternatingLayout : MonoBehaviour
{
    [SerializeField] private float horizontalSpacing = 5f;
    [SerializeField] private float verticalOffset = 10f;

    private void Start()
    {
        RearrangeChildren();
    }
    
    private void OnTransformChildrenChanged()
    {
        RearrangeChildren();
    }

    private void RearrangeChildren()
    {
        float currentXPosition = 0f;
        bool applyPositiveOffset = true;

        for (int i = 0; i < transform.childCount; i++)
        {
            // int reverseIndex = transform.childCount - 1 - i;
            Transform child = transform.GetChild(i);

            // Only rearrange active children
            if (!child.gameObject.activeSelf) continue;

            Vector2 newPosition = new Vector2(currentXPosition, applyPositiveOffset ? verticalOffset : -verticalOffset);
            child.localPosition = newPosition;

            // Prepare for the next child
            currentXPosition += horizontalSpacing - child.GetComponent<RectTransform>().rect.width;
            applyPositiveOffset = !applyPositiveOffset;
        }
    }
}