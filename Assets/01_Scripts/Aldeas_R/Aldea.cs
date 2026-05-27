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
    public List<AventureroData> aventureros = new List<AventureroData>();

    [Header("Estado")]
    public int vidas = 3;

    [Tooltip("Cantidad de salas derrotadas. 9 salas = 1 piso.")]
    public int victorias = 0;

    [Header("Evolución visual")]
    public VillageEvolution villageEvolution;

    public int PisosDerrotados => victorias / 9;

    private int lastVictorias = -1;

    private void Awake()
    {
        if (villageEvolution == null)
            villageEvolution = GetComponent<VillageEvolution>();
    }

    private void Start()
    {
        ActualizarEvolucionVisual(false);
        lastVictorias = victorias;
    }

    private void Update()
    {
        if (victorias != lastVictorias)
        {
            lastVictorias = victorias;
            ActualizarEvolucionVisual(true);
        }
    }

    public void RegistrarSalaGanada()
    {
        victorias++;
        lastVictorias = victorias;
        ActualizarEvolucionVisual(true);
    }

    public void ActualizarEquipo(List<AventureroData> nuevosDatos, bool ganaron)
    {
        aventureros = nuevosDatos;

        if (ganaron)
        {
            victorias++;
            lastVictorias = victorias;
        }

        ActualizarEvolucionVisual(ganaron);
    }

    public void TodosMurieron()
    {
        aventureros.Clear();
        vidas--;

        Debug.Log(tipoAldea + " perdió equipo");

        if (vidas <= 0)
            Debug.Log(tipoAldea + " se quedó sin vidas");

        ActualizarEvolucionVisual(false);
    }

    public void ActualizarEvolucionVisual(bool playEffect)
    {
        if (villageEvolution != null)
            villageEvolution.SetProgress(victorias, playEffect);
    }
}