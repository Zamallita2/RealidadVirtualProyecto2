using UnityEngine;

public class VillageManager : MonoBehaviour
{
    public Aldea[] aldeas;


    public Aldea ObtenerAldeaAleatoria()
    {
        int random =
        Random.Range(
            0,
            aldeas.Length
        );

        return aldeas[random];
    }
}