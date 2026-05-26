using System.Collections.Generic;

[System.Serializable]
public class AldeaData
{
    public string nombreAldea;

    public List<AventureroData> personajes =
        new List<AventureroData>();
}