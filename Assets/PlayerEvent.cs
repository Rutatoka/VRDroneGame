using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvent : MonoBehaviour
{
    public GameObject[] cubes;
    public Transform spawner;
    public Vector3 offset;
    public DoorsScript _doors;
    private void Start()
    {
        _doors = FindAnyObjectByType<DoorsScript>().GetComponent<DoorsScript>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TriggerDoor"))
        {
            SpawnCubes();
            _doors.OnEventDoor(false); //закрыть даерь
        }
    }

    public void SpawnCubes()
    {
        for (int i = 0; i < cubes.Length; i++)
        {
            Vector3 posSpawn = new Vector3(Random.Range(spawner.position.x + offset.x, spawner.position.x - offset.x), Random.Range(spawner.position.y + offset.y, spawner.position.y - offset.y), Random.Range(spawner.position.z + offset.z, spawner.position.z - offset.z));
            Instantiate(cubes[i], posSpawn, Quaternion.identity);
        }
    }

}
