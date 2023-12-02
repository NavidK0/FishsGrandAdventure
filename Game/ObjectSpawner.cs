using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace FishsGrandAdventure.Game;

public class ObjectSpawner : MonoBehaviour
{
    public static GameObject Turret;

    public static bool ShouldSpawnTurret;

    private static readonly List<GameObject> ObjectsToCleanUp = new List<GameObject>();

    internal TimeOfDay TimeOfDay;

    public void Update()
    {
        if (TimeOfDay == null)
        {
            TimeOfDay = FindFirstObjectByType<TimeOfDay>();
        }
        else
        {
            TimeOfDay.quotaVariables.baseIncrease = 275f;
        }

        if (ShouldSpawnTurret & Turret != null)
        {
            ShouldSpawnTurret = false;

            GameObject go2 = Instantiate(Turret,
                new Vector3(3.87f, 0.84f, -14.23f), Quaternion.identity);

            go2.transform.position = new Vector3(3.87f, 0.84f, -14.23f);
            go2.transform.forward = new Vector3(1f, 0f, 0f);
            go2.GetComponent<NetworkObject>().Spawn(true);

            ObjectsToCleanUp.Add(go2);
        }
    }

    public static void CleanupAllSpawns()
    {
        foreach (GameObject gameObject in ObjectsToCleanUp)
        {
            if (gameObject != null)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }

        ObjectsToCleanUp.Clear();
    }
}