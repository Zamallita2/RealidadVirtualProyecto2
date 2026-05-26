using UnityEngine;
using System.Collections.Generic;

public enum TipoAldea
{
    Codiciosos,
    Temerosos,
    Cautelosos,
    Eruditos,
    Vengativos
}

public class Aldea : MonoBehaviour
{
    [Header("Datos")]
    public TipoAldea tipoAldea;


    [Header("Aventureros")]
    public List<AventureroData>
    aventureros =
    new List<AventureroData>();


    [Header("Estado")]
    public int vidas = 3;

    public int victorias = 0;


    //Actualizar aventureros
    public void ActualizarEquipo(
        List<AventureroData>
        nuevosDatos,
        bool ganaron
    )
    {
        aventureros =
        nuevosDatos;


        if (
            ganaron
        )
        {
            victorias++;
        }
    }


    //Todos murieron
    public void TodosMurieron()
    {
        aventureros.Clear();

        vidas--;

        Debug.Log(
            tipoAldea +
            " perdió equipo"
        );


        if (
            vidas <= 0
        )
        {
            Debug.Log(
                tipoAldea +
                " se quedó sin vidas"
            );
        }
    }
}