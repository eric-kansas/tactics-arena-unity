using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerritoryControlIcon : MonoBehaviour
{

    [SerializeField] private Image foregroundImage;
    [SerializeField] private Button button;

    private void Start()
    {

    }

    public void SetTeamControl(Team team)
    {
        if (team == null)
        {
            foregroundImage.color = Color.grey;
            return;
        }

        foregroundImage.color = Match.Instance.GetTeamColor(team);
    }

    public Button GetButton()
    {
        return button;
    }
}
