using UnityEngine;
using System.Collections.Generic;

public class TestVillage : MonoBehaviour
{
    public VillageManager villageManager;

    void Start()
    {
        List<AventureroData>
        grupo =
        villageManager.GenerarGrupo(
            "Codiciosos"
        );

        foreach (
            AventureroData npc
            in grupo
        )
        {
            Debug.Log(
                npc.nombre +
                " Nivel:" +
                npc.nivel
            );
        }
    }
}