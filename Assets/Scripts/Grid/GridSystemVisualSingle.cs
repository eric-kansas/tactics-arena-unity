using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystemVisualSingle : MonoBehaviour
{

    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Card card;


    public void Show(Material material)
    {
        if (material != null) {
            meshRenderer.material = material;
        }
        gameObject.SetActive(true);

    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

}