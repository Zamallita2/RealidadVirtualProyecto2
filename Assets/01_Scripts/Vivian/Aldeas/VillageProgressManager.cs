using UnityEngine;

public class VillageProgressManager : MonoBehaviour
{
    [Header("Aldea")]
    public Aldea targetAldea;

    [Header("Pruebas")]
    public KeyCode testAddRoomKey = KeyCode.P;

    private void Update()
    {
        // TEST
        if (Input.GetKeyDown(testAddRoomKey))
        {
            AddRoomVictory();
        }
    }

    public void AddRoomVictory()
    {
        if (targetAldea == null)
            return;

        targetAldea.victorias++;

        targetAldea.ActualizarEvolucionVisual(true);

        Debug.Log(
            targetAldea.tipoAldea +
            " ganó una sala. Total: " +
            targetAldea.victorias
        );
    }
}