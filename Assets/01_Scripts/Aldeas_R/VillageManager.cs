using UnityEngine;
using System.Collections.Generic;

public class VillageManager : MonoBehaviour
{
    public List<Aldea>
    aldeas =
    new List<Aldea>();


    public List<AventureroData>
    GenerarGrupo()
    {
        List<AventureroData>
        grupo =
        new List<AventureroData>();


        if (aldeas.Count == 0)
        {
            return grupo;
        }


        int randomAldea =
        Random.Range(
            0,
            aldeas.Count
        );


        Aldea aldeaSeleccionada =
        aldeas[randomAldea];


        Debug.Log(
            "Aldea seleccionada: "
            + aldeaSeleccionada.tipoAldea
        );


        List<AventureroData>
        disponibles =
        new List<AventureroData>(
            aldeaSeleccionada
            .aventureros
        );


        while (
            grupo.Count < 4 &&
            disponibles.Count > 0
        )
        {
            int random =
            Random.Range(
                0,
                disponibles.Count
            );

            AventureroData npc =
            disponibles[random];

            npc.nivel = 1;

            grupo.Add(
                npc
            );

            disponibles.RemoveAt(
                random
            );
        }


        return grupo;
    }
}