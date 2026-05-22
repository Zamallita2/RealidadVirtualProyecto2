using System.Collections.Generic;
using UnityEngine;

public class Iniciar : MonoBehaviour
{
    public List<AdventurerSetup> Alies;
    public void iniciar()
    {
        AdventurerManager ad = FindFirstObjectByType<AdventurerManager>();
        ad.Initialize(Alies);
        ad.StartFight();
    }
}