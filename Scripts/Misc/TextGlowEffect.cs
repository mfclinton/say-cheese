using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextGlowEffect3D : MonoBehaviour
{
    [SerializeField] private float lowerBoundGlow = 0.5f;
    [SerializeField] private float upperBoundGlow = 2.0f;
    [SerializeField] private float transitionTime = 1.0f;
    [SerializeField] private float upperBoundDuration = 0.5f;
    [SerializeField] private float lowerBoundDuration = 0.5f;
    [SerializeField] private float glowOuter = 1f;
    [SerializeField] private Color glowColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private TextMeshProUGUI textMesh;
    private Material pulseMaterial;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        pulseMaterial = new Material(textMesh.fontSharedMaterial);
        textMesh.fontMaterial = pulseMaterial;
        
        pulseMaterial.EnableKeyword("GLOW_ON");
        pulseMaterial.SetFloat(ShaderUtilities.ID_GlowOuter, glowOuter); // Set the outer glow value
        pulseMaterial.SetColor(ShaderUtilities.ID_GlowColor, glowColor);
        
        StartCoroutine(GlowPulseRoutine());
    }

    IEnumerator GlowPulseRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(ChangeGlowPower(lowerBoundGlow, upperBoundGlow, transitionTime));
            yield return new WaitForSeconds(upperBoundDuration);
            yield return StartCoroutine(ChangeGlowPower(upperBoundGlow, lowerBoundGlow, transitionTime));
            yield return new WaitForSeconds(lowerBoundDuration);
        }
    }

    IEnumerator ChangeGlowPower(float start, float end, float duration)
    {
        float time = 0;

        while (time < duration)
        {
            float t = time / duration;
            float currentGlow = Mathf.Lerp(start, end, t);
            SetGlowPower(currentGlow);
            time += Time.deltaTime;
            yield return null;
        }

        SetGlowPower(end);
    }

    void SetGlowPower(float glowPower)
    {
        pulseMaterial.SetFloat(ShaderUtilities.ID_GlowPower, glowPower);
    }
}
