using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class EyeVisualizer : MonoBehaviour
{
    [Header("Eye Movement")]
    [SerializeField] private CircleCollider2D eyeCollider;
    [SerializeField] private float eyeSmoothness = 1f;
    [SerializeField] private float bottomOffset = -1f;
    
    [Header("Laser Effect")]
    [SerializeField] private LineRenderer laserRenderer;
    public LineRenderer LaserRenderer => laserRenderer;
    [SerializeField] private Vector3 laserEndOffset;
    [SerializeField] private float laserSmoothness = 1f;
    [SerializeField] private float fireWidthScale = 2f;
    [SerializeField] private float fireDuration = 0.5f;
    
    // References
    private PlayerController playerController;

    // Internal Variables
    private float[] initialLaserWidths;
    private float[] scaledLaserWidths;
    private Coroutine firingCoroutine;
    
    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        initialLaserWidths = laserRenderer.widthCurve.keys.Select(key => key.value).ToArray();
    }
    
    private void OnEnable()
    {
        GameManager.Instance.OnPersonClicked += HandleFiring;
    }
    
    private void OnDisable()
    {
        GameManager.Instance.OnPersonClicked -= HandleFiring;
    }

    private void Update()
    {
        UpdateEyePosition();
        UpdateLaserPosition();
        UpdateHoverTarget();
    }

    private void UpdateHoverTarget()
    {
        Clickable target = playerController.GetClickable();
        if (target != null && target is Person)
        {
            Person person = (Person) target;
            person.OnHover();
        }
    }

    private void UpdateLaserPosition()
    {
        Vector2 pointerWorldPosition = playerController.PointerWorldPosition;
        Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint3x4(pointerWorldPosition);
        Vector3 destinationLaserPos = localPosition + laserEndOffset;
        // Vector3 actualLaserPos = Vector3.Lerp(laserRenderer.GetPosition(1), destinationLaserPos, laserSmoothness * Time.deltaTime);
        // laserRenderer.SetPosition(1, actualLaserPos);
        laserRenderer.SetPosition(1, destinationLaserPos);
    }

    private void UpdateEyePosition()
    {
        Vector2 pointerWorldPosition = playerController.PointerWorldPosition;
        Vector3 destinationPos = Utils.ProjectToCircle(eyeCollider, pointerWorldPosition, true, bottomOffset);
        transform.position = Vector3.Lerp(transform.position, destinationPos, eyeSmoothness * Time.deltaTime);
    }

    private void HandleFiring()
    {
        if (firingCoroutine != null)
            StopCoroutine(firingCoroutine);
        
        firingCoroutine = StartCoroutine(FireLaser());
    }

    private IEnumerator FireLaser()
    {
        float timer = 0f;
        while (timer < fireDuration)
        {
            timer += Time.deltaTime;
            float t = timer * 2 / fireDuration;
            if (t > 1f)
                t = 2f - t;
            
            float scale = Mathf.Lerp(1f, fireWidthScale, t);
            scaledLaserWidths = initialLaserWidths.Select(width => width * scale).ToArray();
            laserRenderer.widthCurve = new AnimationCurve(initialLaserWidths.Select((width, i) => new Keyframe(i, scaledLaserWidths[i])).ToArray());
            yield return null;
        }
        
        laserRenderer.widthCurve = new AnimationCurve(initialLaserWidths.Select((width, i) => new Keyframe(i, initialLaserWidths[i])).ToArray());
    }
}
