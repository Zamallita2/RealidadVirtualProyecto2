using System.Collections.Generic;
using UnityEngine;

public class BuildingRoomVisual : MonoBehaviour
{
    [Header("Número local dentro del piso")]
    public int localRoomNumber = 1;

    [Header("Puntos de spawn")]
    public Transform[] spawnPoints = new Transform[5];

    private readonly List<GameObject> spawnedObjects =
        new List<GameObject>();


    public void ClearVisual()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(
                    spawnedObjects[i]
                );
            }
        }

        spawnedObjects.Clear();
    }


    public void RenderRoom(
        RoomConfigData config
    )
    {
        ClearVisual();

        if (config == null)
            return;

        if (GachaInventoryManager.Instance == null)
            return;


        for (
            int i = 0;
            i < spawnPoints.Length &&
            i < config.enemyIds.Count;
            i++
        )
        {
            string enemyId =
                config.enemyIds[i];

            if (
                string.IsNullOrEmpty(
                    enemyId
                )
            )
            {
                continue;
            }

            GameObject prefab =
            GachaInventoryManager.Instance
            .GetEnemyPrefab(enemyId);

            if (prefab == null)
                continue;

            Transform spawn =
                spawnPoints[i];

            if (spawn == null)
                continue;


            GameObject obj =
            Instantiate(
                prefab,
                spawn.position,
                spawn.rotation,
                spawn
            );

            obj.transform.localPosition =
                Vector3.zero;

            obj.transform.localRotation =
                Quaternion.identity;


            // Obtener datos del enemigo
            EnemyGachaData enemyData =
            GachaInventoryManager.Instance
            .GetEnemyData(enemyId);


            if (enemyData != null)
            {
                obj.transform.localScale =
                    enemyData.roomScale;
            }
            else
            {
                // tamaño por defecto
                obj.transform.localScale =
                new Vector3(
                    0.1f,
                    0.1f,
                    0.1f
                );
            }


            spawnedObjects.Add(obj);
        }
    }
}