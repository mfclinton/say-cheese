using System;
using UnityEngine;

public class LaneManager : MonoBehaviour
{
    [Header("Constraints")]
    [SerializeField] private BoxCollider2D laneConstrainedArea;
    
    [Header("Lane Offsets")]
    [SerializeField] private float laneVertOffset = 1f;
    [SerializeField] private float laneScaleOffset = 0.2f;
    
    [Header("End Node Offsets")]
    [SerializeField] private float endNodeOffsetMagnitude = 1f;
    
    public Vector2 spawnNodeL { get; private set; }
    public Vector2 spawnNodeR { get; private set; }
    
    public static LaneManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        
        MapData mapData = MapManager.Instance.GetCurrentMap();
        if (mapData != null)
        {
            laneConstrainedArea = mapData.walkingArea;
        }
        
        spawnNodeL = new Vector2(laneConstrainedArea.bounds.min.x, laneConstrainedArea.bounds.center.y) - Vector2.one * endNodeOffsetMagnitude;
        spawnNodeR = new Vector2(laneConstrainedArea.bounds.max.x, laneConstrainedArea.bounds.center.y) + Vector2.one * endNodeOffsetMagnitude;
    }


    public (float vertPos, float scale) GetLanePositionAndScale(int lane)
    {
        float vertOrigin = laneConstrainedArea.bounds.min.y; // TODO
        float vertOffset = laneVertOffset * lane;
        float vertPos = vertOrigin + vertOffset;
        
        float initialScale = 1f;
        float scaleOffset = laneScaleOffset * lane;
        float scale = initialScale - scaleOffset;
        
        return (vertPos, scale);
    }
}
    