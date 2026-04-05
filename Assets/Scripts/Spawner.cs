using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [Header("Puntos de spawn (los hoyos del mapa)")]
    [SerializeField] private List<Transform> puntosSpawn = new List<Transform>();

    [Header("Prefab de la salamanqueja")]
    [SerializeField] private GameObject prefabSalamanqueja;
    [SerializeField] private int        tamanoPool    = 20;
    [SerializeField] private int        maximasActivas = 5;

    [Header("Tiempo entre spawns")]
    [SerializeField] private float tiempoSpawn = 3f;

    private List<GameObject> pool  = new List<GameObject>();
    private float            timer = 0f;

    void Start()
    {
        CrearPool();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.JuegoActivo) return;

        timer += Time.deltaTime;
        if (timer >= tiempoSpawn)
        {
            timer = 0f;
            Spawn();
        }
    }

    void CrearPool()
    {
        for (int i = 0; i < tamanoPool; i++)
        {
            GameObject obj = Instantiate(prefabSalamanqueja, transform);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    void Spawn()
    {
        if (puntosSpawn.Count == 0 || ContarActivas() >= maximasActivas) return;

        GameObject obj = ObtenerLibre();
        if (obj == null) return;

        Transform punto = puntosSpawn[Random.Range(0, puntosSpawn.Count)];
        obj.transform.position = punto.position;

        Salamanqueja sal = obj.GetComponent<Salamanqueja>();
        if (sal != null) sal.SetTipo(ElegirTipo());

        obj.SetActive(true);
    }

    Salamanqueja.Tipo ElegirTipo()
    {
        int oleada = GameManager.Instance != null ? GameManager.Instance.Oleada : 1;
        float r = Random.value;

        if (oleada == 1) return Salamanqueja.Tipo.Normal;
        if (oleada == 2)
        {
            if (r < 0.6f) return Salamanqueja.Tipo.Normal;
            if (r < 0.9f) return Salamanqueja.Tipo.Rapida;
            return Salamanqueja.Tipo.Resistente;
        }
        // oleada 3
        if (r < 0.3f) return Salamanqueja.Tipo.Normal;
        if (r < 0.6f) return Salamanqueja.Tipo.Rapida;
        if (r < 0.9f) return Salamanqueja.Tipo.Resistente;
        return Salamanqueja.Tipo.Dorada;
    }

    GameObject ObtenerLibre()
    {
        foreach (GameObject obj in pool)
            if (!obj.activeInHierarchy) return obj;
        return null;
    }

    int ContarActivas()
    {
        int n = 0;
        foreach (GameObject obj in pool)
            if (obj.activeInHierarchy) n++;
        return n;
    }

    public void ActualizarTiempoSpawn(float t) => tiempoSpawn = t;
}
