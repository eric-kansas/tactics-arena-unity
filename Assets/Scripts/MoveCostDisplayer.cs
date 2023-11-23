using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MoveCostDisplayer : MonoBehaviour
{
    [SerializeField] private GameObject moveCostTextPrefab;

    private List<GameObject> moveCostTexts = new List<GameObject>();

    public void CreateMoveCost(Transform parent, Vector3 position, int cost)
    {

        GameObject textObj = Instantiate(moveCostTextPrefab, position + new Vector3(0, 1, 0), Quaternion.identity);

        // Set the parent of the new object
        textObj.transform.SetParent(parent);

        TextMeshProUGUI textComponent = textObj.GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = cost.ToString();
        moveCostTexts.Add(textObj);
    }

    public void DestroyMoveCosts()
    {
        foreach (GameObject textObj in moveCostTexts)
        {
            Destroy(textObj);
        }
        moveCostTexts.Clear();
    }
}