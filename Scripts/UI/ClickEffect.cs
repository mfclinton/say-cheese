using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ClickEffect : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private SpriteRenderer clockCircle;
    [SerializeField] private float fillTransitionTime = 1f;

    private const string fillAmountStrKey = "_FillAmount";
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        // Initialize the MaterialPropertyBlock
        propertyBlock = new MaterialPropertyBlock();
    }

    public void Initialize(string text, float prevTime, float curTime, float totalGameTime)
    {
        this.text.text = text;
        
        float startFillAmount = 1f - (prevTime / totalGameTime);
        float endFillAmount = 1f - (curTime / totalGameTime);
        StartCoroutine(LerpFillAmount(startFillAmount, endFillAmount));
    }
    
    // Enumerator for coroutine
    private IEnumerator LerpFillAmount(float startFillAmount, float endFillAmount)
    {
        float timeElapsed = 0f;
        while (timeElapsed < fillTransitionTime)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / fillTransitionTime;
            SetFillAmount(Mathf.Lerp(startFillAmount, endFillAmount, t));
            yield return null;
        }
    }
    
    public void SetFillAmount(float fillAmount)
    {
        propertyBlock.SetFloat(fillAmountStrKey, fillAmount);
        clockCircle.SetPropertyBlock(propertyBlock);
    }

    public void EndEffect()
    {
        Destroy(gameObject);
    }
}