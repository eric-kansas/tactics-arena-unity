using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRagdollSpawner : MonoBehaviour
{


    [SerializeField] private Transform ragdollPrefab;
    [SerializeField] private Transform originalRootBone;


    private UnitEnergySystem unitEnergySystem;

    private void Awake()
    {
        unitEnergySystem = GetComponent<UnitEnergySystem>();

        unitEnergySystem.OnOutOfEnergy += UnitEnergySystem_OnDead;
    }

    private void UnitEnergySystem_OnDead(object sender, EventArgs e)
    {
        Transform ragdollTransform = Instantiate(ragdollPrefab, transform.position, transform.rotation);
        UnitRagdoll unitRagdoll = ragdollTransform.GetComponent<UnitRagdoll>();
        unitRagdoll.Setup(originalRootBone);
    }


}
