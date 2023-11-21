using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneSystem : MonoBehaviour
{
    [SerializeField] private GameObject borderVisualPrefab;

    void Awake()
    {
        Zone.SetBorderVisualPrefab(borderVisualPrefab);
    }

}
