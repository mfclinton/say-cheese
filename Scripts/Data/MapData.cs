using System;
using UnityEngine;

[Serializable]
public class MapData
{
    public LevelSettings.MapType mapType;
    public GameObject mapParent;
    public AudioClip backgroundMusic;
    public BoxCollider2D walkingArea;
    public BoxCollider2D cameraArea;
}