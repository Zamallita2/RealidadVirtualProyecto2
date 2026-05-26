using UnityEngine;
using System.Collections.Generic;

public class VillageManager : MonoBehaviour
{
    [Header("Lista de aldeas")]
    public List<AldeaData> aldeas =
        new List<AldeaData>();


    public List<AventureroData>
        GenerarGrupo(string nombreAldea)
    {
        List<AventureroData> grupo =
            new List<AventureroData>();


        AldeaData aldea =
            aldeas.Find(
                a => a.nombreAldea ==
                nombreAldea
            );

        if (aldea == null)
        {
            Debug.Log("No existe");

            return grupo;
        }


        List<AventureroData>
            disponibles =
            new List<AventureroData>(
                aldea.personajes
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


            grupo.Add(npc);


            disponibles.RemoveAt(
                random
            );
        }

        return grupo;
    }
}