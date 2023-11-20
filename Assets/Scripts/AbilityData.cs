using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Game/Player Ability")]
public class AbilityData : ScriptableObject
{
    public string abilityName;
    public string abilityBehaviourName; // Name of the MonoBehaviour script

}