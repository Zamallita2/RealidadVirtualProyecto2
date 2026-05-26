using UnityEngine;

public class TestVillage : MonoBehaviour
{
    public VillageManager villageManager;

    void Start()
    {
        foreach (
            Aldea aldea
            in villageManager.aldeas
        )
        {
            Debug.Log(
                "=============="
            );

            Debug.Log(
                "ALDEA: " +
                aldea.tipoAldea
            );

            Debug.Log(
                "Vidas: " +
                aldea.vidas
            );

            Debug.Log(
                "Victorias: " +
                aldea.victorias
            );


            foreach (
                AventureroData npc
                in aldea.aventureros
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
}