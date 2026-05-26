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
    public TipoAldea tipoAldea;

    public List<AventureroData>
    aventureros =
    new List<AventureroData>();
}