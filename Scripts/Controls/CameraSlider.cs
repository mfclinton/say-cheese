using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraSlider : MonoBehaviour
{
    [Header("Configuration")] 
    [SerializeField] private float[] speeds = { 10f, 7f, 5f };
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float[] zoomLevels = { 10f, 6f, 3f };
    public float[] ZoomLevels => zoomLevels;

    [Header("Camera Shake")]
    [SerializeField] private int shakeOscillations = 3;
    [SerializeField] private float shakeMagnitude = 0.5f;
    [SerializeField] private float shakeSpeed = 1f;
    [SerializeField] private float shakeDecay = 0.5f;
    
    [Header("References")]
    [SerializeField] private BoxCollider2D restrictedArea;
    public BoxCollider2D RestrictedArea => restrictedArea;
    
    PlayerController playerController;
    private Camera cam;
    private Vector2 currentMovement;
    
    private int curZoomLevel = 0;
    private float speed => speeds[curZoomLevel];
    private float targetZoom => zoomLevels[curZoomLevel] * zoomMultiplier;
    public static CameraSlider Instance;

    private float zoomMultiplier = 1f;
    private float lastCamAspect;

    private Vector3 targetZoomPos;
    private Coroutine shakeCoroutine;
    
    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        cam = Camera.main;
        Instance = this;
        
        MapData mapData = MapManager.Instance.GetCurrentMap();
        if (mapData != null)
        {
            restrictedArea = mapData.cameraArea;
        }
    }

    private void Update()
    {
        ValidateAndUpdateZoomLevels();
        HandleMovement();
        HandleZoom();
    }

    private void OnEnable()
    {
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            pc.OnMove += OnMove;
            pc.OnZoom += OnZoom;
        }
    }

    private void OnDisable()
    {
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            pc.OnMove -= OnMove;
            pc.OnZoom -= OnZoom;
        }
    }
    
    private void OnMove(Vector2 movement)
    {
        currentMovement = movement;
        targetZoomPos = Vector3.zero;
    }

    private void OnZoom(float zoomFactor)
    {
        int oldZoomLevel = curZoomLevel;
        curZoomLevel += zoomFactor > 0 ? 1 : -1;
        curZoomLevel = Mathf.Clamp(curZoomLevel, 0, zoomLevels.Length - 1);
        
        if (oldZoomLevel != curZoomLevel)
        {
            if (zoomFactor > 0 && currentMovement.magnitude == 0)
            {
                targetZoomPos = playerController.PointerWorldPosition;
                targetZoomPos.z = transform.position.z;
            }
            else
            {
                targetZoomPos = Vector3.zero;
            }
        }
    }

    private void HandleMovement()
    {
        if (currentMovement.magnitude > 0)
        {
            Vector3 delta = new Vector3(currentMovement.x, currentMovement.y, 0) * speed * Time.deltaTime;
            Vector3 newPosition = transform.position + delta;
            UpdateCamPosInBounds(newPosition);
        }
    }

    private void UpdateCamPosInBounds(Vector3 newPosition)
    {
        float cameraHalfWidth = cam.orthographicSize * cam.aspect;
        float cameraHalfHeight = cam.orthographicSize;
        newPosition.x = Mathf.Clamp(newPosition.x, restrictedArea.bounds.min.x + cameraHalfWidth, restrictedArea.bounds.max.x - cameraHalfWidth);
        newPosition.y = Mathf.Clamp(newPosition.y, restrictedArea.bounds.min.y + cameraHalfHeight, restrictedArea.bounds.max.y - cameraHalfHeight);
        
        transform.position = newPosition;
    }
    
    private void HandleZoom()
    {
        float diff = Mathf.Abs(targetZoom - cam.orthographicSize);
        if (diff < 0.01f)
            return;
        
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomLerpSpeed * Time.deltaTime);

        Vector3 newPos = transform.position;
        if (targetZoomPos != Vector3.zero)
            newPos = Vector3.Lerp(transform.position, targetZoomPos, zoomLerpSpeed * Time.deltaTime);
        
        UpdateCamPosInBounds(newPos);
    }
    
    // Camera Tilt Shake
    private IEnumerator CameraShake(int oscillations, float magnitude, float speed, float shakeDecay)
    {
        Quaternion originalRot = transform.localRotation;

        float totalDuration = oscillations * (2 * Mathf.PI) / speed;
        float elapsed = 0.0f;
        while (elapsed < totalDuration)
        {
            float zRot = Mathf.Sin(elapsed * speed) * magnitude;
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRot * Quaternion.Euler(0f, 0f, zRot), shakeDecay);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localRotation = originalRot;
    }
    
    public void Shake()
    {
        if (shakeCoroutine != null)
        {
            transform.localRotation = Quaternion.identity;
            StopCoroutine(shakeCoroutine);
        }
        
        shakeCoroutine = StartCoroutine(CameraShake(shakeOscillations, shakeMagnitude, shakeSpeed, shakeDecay));
    }
    
    public void ValidateAndUpdateZoomLevels()
    {
        // Check if the aspect ratio has changed
        if (lastCamAspect == cam.aspect)
            return;
        lastCamAspect = cam.aspect;
        
        // Calculates if we need to adjust the zoom levels
        float maxOrthographicSizeWidth = restrictedArea.bounds.size.x / 2 / cam.aspect;
        float maxOrthographicSizeHeight = restrictedArea.bounds.size.y / 2;
        
        float maxOrthographicSize = Mathf.Min(maxOrthographicSizeWidth, maxOrthographicSizeHeight);
        
        zoomMultiplier = maxOrthographicSize / zoomLevels[0];
        cam.orthographicSize = targetZoom;
    }
}
