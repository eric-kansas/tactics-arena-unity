using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class SpawnSystem : MonoBehaviour
{

    public static SpawnSystem Instance { get; private set; }

    public UnityEngine.Color[] TerritoryColorList = new UnityEngine.Color[]
    {
        UnityEngine.Color.blue,
        UnityEngine.Color.red,
    };

    private Dictionary<int, Rect> spawnZones;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one SpawnSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        spawnZones = new Dictionary<int, Rect>{
            { 0, new(2,0,8,2) },
            { 1, new(2,15,8,2) }
        };
    }

    private void Start()
    {

    }
}
