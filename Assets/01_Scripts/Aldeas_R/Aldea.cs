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

    [Header("Personajes Base")]
    public List<AventureroData> plantillaAventureros =
        new List<AventureroData>();


    [Header("Aventureros Actuales")]
    public List<AventureroData> aventureros =
        new List<AventureroData>();


    [Header("Estado")]
    public int vidas = 3;

    public int victorias = 0;


    void Awake()
    {
        if (aventureros.Count == 0)
        {
            GenerarNuevoEquipo();
        }
    }


    //Genera equipo inicial
    public void GenerarNuevoEquipo()
    {
        aventureros.Clear();


        List<AventureroData>
        disponibles =
        new List<AventureroData>(
            plantillaAventureros
        );


        while (
            aventureros.Count < 4 &&
            disponibles.Count > 0
        )
        {
            int random =
            Random.Range(
                0,
                disponibles.Count
            );

            AventureroData original =
            disponibles[random];


            AventureroData nuevo =
            new AventureroData();

            nuevo.nombre =
            original.nombre;

            nuevo.rol =
            original.rol;

            nuevo.prefab =
            original.prefab;

            nuevo.nivel = 1;


            aventureros.Add(
                nuevo
            );

            disponibles.RemoveAt(
                random
            );
        }

        Debug.Log(
            tipoAldea +
            " generó equipo nuevo"
        );
    }


    //Actualizar personajes
    public void ActualizarEquipo(
        List<AventureroData>
        nuevosDatos,
        bool ganaron
    )
    {
        aventureros =
        nuevosDatos;


        if (ganaron)
        {
            victorias++;
        }

        Debug.Log(
            tipoAldea +
            " actualizó equipo"
        );
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
            vidas > 0
        )
        {
            GenerarNuevoEquipo();
        }
        else
        {
            Debug.Log(
                tipoAldea +
                " se quedó sin vidas"
            );
        }
    }
}