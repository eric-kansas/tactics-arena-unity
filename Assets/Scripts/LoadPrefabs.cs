using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadPrefabs : MonoBehaviour
{
    [SerializeField] private GameObject borderVisualPrefab;
    [SerializeField] private GameObject traversalPathVisualPrefab;

    void Awake()
    {
        Zone.SetBorderVisualPrefab(borderVisualPrefab);
        MoveAction.SetTraversalPathVisualPrefab(traversalPathVisualPrefab);
    }

}
