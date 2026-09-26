using System.Linq;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] private MapData[] maps;
    public MapData[] Maps => maps;
    [SerializeField] private int defaultMapIndex = 0;
    
    public MapData GetCurrentMap()
    {
        LevelManager levelManager = LevelManager.Instance;
        if(levelManager == null)
            return maps[defaultMapIndex];
        
        return maps.Where(m => m.mapType == levelManager.CurrentLevelSettings.mapType).FirstOrDefault();
    }
    
    public static MapManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
}
