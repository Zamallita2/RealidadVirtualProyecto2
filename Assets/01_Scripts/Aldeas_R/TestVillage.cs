using UnityEngine;
using System.Collections.Generic;

public class TestVillage : MonoBehaviour
{
    public VillageManager villageManager;

    void Start()
    {
        Debug.Log("====== TEST DE TODAS LAS ALDEAS ======");

        foreach (Aldea aldea in villageManager.aldeas)
        {
            Debug.Log(" ");
            Debug.Log("ALDEA: " + aldea.tipoAldea);

            List<AventureroData> grupo =
                GenerarGrupoDeAldea(aldea);

            foreach (AventureroData npc in grupo)
            {
                Debug.Log(
                    "Nombre: " + npc.nombre +
                    " | Rol: " + npc.rol +
                    " | Nivel: " + npc.nivel
                );
            }
        }
    }

    List<AventureroData> GenerarGrupoDeAldea(Aldea aldea)
    {
        List<AventureroData> grupo =
            new List<AventureroData>();

        List<AventureroData> disponibles =
            new List<AventureroData>(
                aldea.aventureros
            );

        while (grupo.Count < 4 &&
               disponibles.Count > 0)
        {
            int random =
                Random.Range(
                    0,
                    disponibles.Count
                );

            AventureroData npc =
                disponibles[random];

            npc.nivel = 1;

            grupo.Add(npc);

            disponibles.RemoveAt(random);
        }

        return grupo;
    }
}