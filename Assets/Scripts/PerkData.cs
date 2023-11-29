using UnityEngine;

[CreateAssetMenu(fileName = "New Perk", menuName = "Game/Player Perk")]
public class PerkData : ScriptableObject
{
    public string perkName;
    public string perkBehaviourName; // Name of the MonoBehaviour script

}