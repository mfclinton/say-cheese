using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class Person : MonoBehaviour, Clickable
{
    [Header("Movement Stats")]
    [SerializeField, Range(15f, 90f)] private float timeToReachDestination = 40f;
    [SerializeField, Range(1f, 10f)] private float timeToChangeOneLane = 2f;

    [Header("Randomness")] 
    [SerializeField, Range(0f, 1f)] private float probOfSwitchingLanes = 0.05f;
    [SerializeField, Range(0f, 1f)] private float probOfChangingSpeed = 0.05f;
    [SerializeField, Range(0.5f, 2f)] private float eventTimeRangeMin = 1f;
    [SerializeField, Range(1f, 4f)] private float eventTimeRangeMax = 2f;
    [SerializeField, Range(2f, 7f)] private float maxChangeSpeedMag = 5f;
    
    [Header("Death")]
    [SerializeField] private float deathDuration = 0.2f;
    [SerializeField] private Material personMaterialRef;
    
    [Header("Hover")]
    [SerializeField] private float hoverIntensity = 8f;
    [SerializeField] private float hoverSpeed = 8f;
    [SerializeField] private float hoverChangeStateTime = 0.05f;
    
    // Internals
    public bool IsTarget { get; set; }
    float nextEventTime;
    
    // Progress
    private float destinationT;
    private float laneChangeT;
    
    // Lane State
    [Header("Position Stuff")]
    [SerializeField]
    private int prevLane;
    [SerializeField]
    private int targetLane;
    [SerializeField] private bool triggerLaneChange = false;
    
    // Position State
    [SerializeField] private float xPosOrigin;
    [SerializeField] private float xPosDestination;
    
    
    public bool DestinationReached => destinationT >= 1f;
    public bool ChangingLane => laneChangeT < 1f;
    
    // Internal References
    private SortingGroup sortingGroup;
    private Animator animator;
    private Material personMaterial;
    
    private SpriteRenderer[] spriteRenderers;
    private MaterialPropertyBlock[] propertyBlocks;
    
    // Hover Stuff
    private float lastHoverTime;
    private Coroutine hoverCoroutine;
    
    private const string DEADPARAMNAME = "_tDead";
    private const string HOVERPARAMNAME = "_isScanning";

    private void Awake()
    {
        sortingGroup = GetComponent<SortingGroup>();
        animator = GetComponent<Animator>();
        
        personMaterial = personMaterialRef;
        
        // Set up material
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in spriteRenderers)
            spriteRenderer.material = personMaterial;
        
        // Set up property blocks
        propertyBlocks = new MaterialPropertyBlock[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            propertyBlocks[i] = new MaterialPropertyBlock();
            spriteRenderers[i].GetPropertyBlock(propertyBlocks[i]);
        }
    }

    private void Update()
    {
        UpdatePerson(Time.deltaTime);
    }

    #region Clickable Implementation

    public bool IsClickable { get; private set; }
    
    public void Initialize(float xPosOrigin, float xPosDestination, int targetLane, float timeToReachDestination)
    {
        this.xPosOrigin = xPosOrigin;
        this.xPosDestination = xPosDestination;
        bool facingLeft = xPosDestination < xPosOrigin;
        transform.localScale = new Vector3(facingLeft ? -1f : 1f, 1f, 1f);
        
        this.destinationT = 0f;
        this.laneChangeT = 0f;
        this.prevLane = 0;
        this.targetLane = targetLane;
        
        this.timeToReachDestination = timeToReachDestination;
        
        // TODO: Set Position
        
        IsClickable = true;
        IsTarget = false;
    }

    void PerformEvent()
    {
        bool switchLanes = (Random.value < probOfSwitchingLanes) && !ChangingLane;
        if (switchLanes)
        {
            prevLane = targetLane;
            targetLane = Random.Range(0, GameManager.Instance.LaneCount);
            this.laneChangeT = 0f;
        }
        
        bool changeSpeed = Random.value < probOfChangingSpeed;
        if (changeSpeed)
        {
            timeToReachDestination = Mathf.Clamp(timeToReachDestination + Random.Range(-maxChangeSpeedMag, maxChangeSpeedMag),
                PersonManager.Instance.MinTimeToReachDestination, PersonManager.Instance.MaxTimeToReachDestination);
        }
        
        nextEventTime = Time.time + Random.Range(eventTimeRangeMin, eventTimeRangeMax);
    }
    
    public void OnClick()
    {
        if (!IsClickable || GameManager.Instance.IsGameOver) return;
        IsClickable = false;

        if (IsTarget)
        {
            GameManager.Instance.OnCorrectTargetClicked();
        }
        else
        {
            GameManager.Instance.OnWrongTargetClicked();
        }
        
        StopCoroutine(HandleHoverChange());

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].GetPropertyBlock(propertyBlocks[i]);
            propertyBlocks[i].SetInt(HOVERPARAMNAME, 0);
            spriteRenderers[i].SetPropertyBlock(propertyBlocks[i]);
        }
        
        UIManager.Instance.ProcessPersonClicked(GetComponent<PersonVisualizer>());
        
        StartCoroutine(HandleDeath());
    }

    public void OnHover()
    {
        lastHoverTime = Time.time;
        StartCoroutine(HandleHoverChange());
    }

    #endregion

    private void UpdatePosition(float timeDelta)
    {
        (float destVertPos, float destScale) = LaneManager.Instance.GetLanePositionAndScale(targetLane);
        (float prevVertPos, float prevScale) = LaneManager.Instance.GetLanePositionAndScale(prevLane);
        
        int numLanesChanged = Mathf.Abs(targetLane - prevLane);
        laneChangeT += timeDelta / (timeToChangeOneLane * numLanesChanged);
        float vertPos = Mathf.Lerp(prevVertPos, destVertPos, laneChangeT);
        float scale = Mathf.Lerp(prevScale, destScale, laneChangeT);
        
        float interLane = Mathf.Lerp(prevLane, targetLane, laneChangeT);
        
        transform.position = new Vector3(transform.position.x, vertPos, interLane * 10);
        transform.localScale = new Vector3(scale * Mathf.Sign(transform.localScale.x), scale, transform.localScale.z);
    }
    
    private void UpdatePerson(float timeDelta)
    {
        // Move towards destination
        if (!DestinationReached)
        {
            destinationT += timeDelta / timeToReachDestination;
            float xPos = Mathf.Lerp(xPosOrigin, xPosDestination, destinationT);
            
            transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
        }

        if (triggerLaneChange)
        {
            triggerLaneChange = false;
            laneChangeT = 0f;
        }
        
        // Change Lane
        if (ChangingLane)
        {
            UpdatePosition(timeDelta);
        }
        
        // Conditionals
        if(DestinationReached)
            OnDestinationReached();
        else if (Time.time > nextEventTime)
            PerformEvent();
        
        animator.SetFloat("speedMultiplier", (30f / timeToReachDestination));
    }

    void OnDestinationReached()
    {
        bool wasTarget = IsTarget;
        Initialize(xPosDestination, xPosOrigin, targetLane, timeToReachDestination);
        IsTarget = wasTarget;
    }
    
    IEnumerator HandleDeath()
    {
        float timer = 0f;
        while (timer < deathDuration)
        {
            timer += Time.deltaTime;
            float t = timer / deathDuration;
            
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].GetPropertyBlock(propertyBlocks[i]);
                propertyBlocks[i].SetFloat(DEADPARAMNAME, t);
                spriteRenderers[i].SetPropertyBlock(propertyBlocks[i]);
            }
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    IEnumerator HandleHoverChange()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].GetPropertyBlock(propertyBlocks[i]);
            propertyBlocks[i].SetInt(HOVERPARAMNAME, 1);
            spriteRenderers[i].SetPropertyBlock(propertyBlocks[i]);
        }
        
        while ((Time.time - lastHoverTime) < hoverChangeStateTime)
        {
            yield return null;
        }
        
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].GetPropertyBlock(propertyBlocks[i]);
            propertyBlocks[i].SetInt(HOVERPARAMNAME, 0);
            spriteRenderers[i].SetPropertyBlock(propertyBlocks[i]);
        }
    }
}
