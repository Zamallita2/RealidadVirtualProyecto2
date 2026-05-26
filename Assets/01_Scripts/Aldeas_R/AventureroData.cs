using UnityEngine;

public enum Rol
{
    Arquero,
    Luchador,
    Mago,
    Medico,
    Tanque
}

[System.Serializable]
public class AventureroData
{
    public string nombre;

    public Rol rol;

    public GameObject prefab;

    public int nivel = 1;
}