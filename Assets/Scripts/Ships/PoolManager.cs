using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PoolManager : MonoBehaviour
{

    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();

    private Dictionary<int, Queue<GameObject>> poolWeapons = new Dictionary<int, Queue<GameObject>>();

    private static PoolManager instance;

    public static PoolManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PoolManager>();

            return instance;
        }
    }


    #region Создаём пул корабликов

    public void CreatePool(GameObject prefab, int key, int count)       // Добавляем кораблики в пул
    {
        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary.Add(key, new Queue<GameObject>());

            for (int i = 0; i < count; i++)
            {
                GameObject go = Instantiate(prefab) as GameObject;
                go.GetComponent<Transform>().parent = transform;
                go.transform.localScale = new Vector3(0.98f, 0.98f, 0.98f);
                go.SetActive(false);
                poolDictionary[key].Enqueue(go);
            }
        }
    }

    public GameObject GetShip(int key)                  // Извлекаем из пула объект
    {
        return poolDictionary[key].Count > 0 ? poolDictionary[key].Dequeue() : null;
    }

    public int CountOfShips(int key)
    {
        return poolDictionary[key].Count;
    }

    public void ReturnShip(int key, GameObject ship)    // Возврат в пул
    {
        ship.SetActive(false);
        poolDictionary[key].Enqueue(ship);
    }

    #endregion

    public void CreateWeapons(GameObject gun, int key)
    {
        if (!poolWeapons.ContainsKey(key))
        {
            poolWeapons.Add(key, new Queue<GameObject>());

            GameObject go = Instantiate(gun) as GameObject;
            go.GetComponent<Transform>().parent = transform;
            go.SetActive(false);
            poolWeapons[key].Enqueue(go);
        }
    }

    public GameObject GetGun(int key)
    {
        return poolWeapons[key].Dequeue();
    }

    public void ReturnGun(int key, GameObject gun)    // Возврат в пул
    {
        gun.SetActive(false);
        poolWeapons[key].Enqueue(gun);
    }
}
