using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class SpawnerBoss : MonoBehaviour
{
    public static SpawnerBoss Instance { get; private set; }

    public enum FaseBoss { Hijas, Boss, Victoria }
    public FaseBoss Fase { get; private set; } = FaseBoss.Hijas;

    [Header("Hoyos de las hijas")]
    [SerializeField] private List<Transform> hoyosHijas = new List<Transform>();

    [Header("Prefab de la hija")]
    [SerializeField] private GameObject prefabHija;
    [SerializeField] private int        poolSize        = 8;
    [SerializeField] private float      tiempoSpawn     = 3f;
    [SerializeField] private int        maxHijasActivas = 3;
    [SerializeField] private int        hijasParaMatar  = 6;

    [Header("Boss")]
    [SerializeField] private FinalBoss finalBoss;

    [Header("UI del Boss")]
    [SerializeField] private TextMeshProUGUI textoFase;
    [SerializeField] private Slider          barraVida;
    [SerializeField] private Image           fillBarra;

    [Header("Panel de Victoria")]
    [SerializeField] private GameObject      panelVictoria;
    [SerializeField] private TextMeshProUGUI textoPuntuacionVictoria;

    private List<GameObject> pool         = new List<GameObject>();
    private int              hijasMuertas = 0;
    private float            timer        = 0f;

    void Awake() { Instance = this; }

    void Start()
    {
        CrearPool();
        if (textoFase    != null) textoFase.text = "Fase 1: Derrota a las hijas";
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (barraVida    != null) barraVida.gameObject.SetActive(false);

        if (finalBoss != null)
        {
            finalBoss.ConfigurarBarra(barraVida, fillBarra);
            finalBoss.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.JuegoActivo) return;
        if (Fase != FaseBoss.Hijas) return;

        timer += Time.deltaTime;
        if (timer >= tiempoSpawn) { timer = 0f; SpawnHija(); }
    }

    void CrearPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefabHija, transform);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    void SpawnHija()
    {
        if (hoyosHijas.Count == 0) return;
        if (ContarActivas() >= maxHijasActivas) return;

        GameObject obj = ObtenerLibre();
        if (obj == null) return;

        // Solo spawnear en hoyos libres
        List<Transform> hoyosLibres = new List<Transform>();
        foreach (Transform hoyo in hoyosHijas)
        {
            bool ocupado = false;
            foreach (GameObject activo in pool)
            {
                if (activo.activeInHierarchy &&
                    Vector2.Distance(activo.transform.position, hoyo.position) < 0.5f)
                {
                    ocupado = true;
                    break;
                }
            }
            if (!ocupado) hoyosLibres.Add(hoyo);
        }

        if (hoyosLibres.Count == 0) return;

        Transform hoyoElegido = hoyosLibres[Random.Range(0, hoyosLibres.Count)];
        obj.transform.position = hoyoElegido.position;
        obj.SetActive(true);
    }

    // Llamado por SalamanquejaHija
    public void NotificarHijaMuerta()
    {
        if (Fase != FaseBoss.Hijas) return;
        hijasMuertas++;
        GameManager.Instance?.SumarPuntos(50);

        if (textoFase != null)
            textoFase.text = $"Hijas: {hijasMuertas} / {hijasParaMatar}";

        if (hijasMuertas >= hijasParaMatar)
            StartCoroutine(ActivarBoss());
    }

    IEnumerator ActivarBoss()
    {
        Fase = FaseBoss.Boss;

        foreach (GameObject obj in pool)
            if (obj.activeInHierarchy)
            {
                SalamanquejaHija h = obj.GetComponent<SalamanquejaHija>();
                h?.HuirAlHoyo();
            }

        GameManager.Instance?.UIManager_MostrarMensaje("LA SALAMANQUEJA MADRE DESPIERTA!");
        if (textoFase != null) textoFase.text = "FASE 2: DERROTA AL BOSS!";

        yield return new WaitForSeconds(2f);
        if (finalBoss != null) finalBoss.Aparecer();
    }

    // Llamado por FinalBoss cuando muere
    public void NotificarBossMuerto()
    {
        Fase = FaseBoss.Victoria;

        // Guardar puntuacion final
        int puntos = GameManager.Instance != null ? GameManager.Instance.Puntuacion : 0;
        int oleada = GameManager.Instance != null ? GameManager.Instance.Oleada : 3;
        int record = PlayerPrefs.GetInt("Record", 0);
        if (puntos > record) { PlayerPrefs.SetInt("Record", puntos); }
        PlayerPrefs.SetInt("PuntuacionFinal", puntos);
        PlayerPrefs.SetInt("OleadaFinal", oleada);
        PlayerPrefs.DeleteKey("PuntuacionAcumulada");
        PlayerPrefs.DeleteKey("VidasActuales");
        PlayerPrefs.Save();

        StartCoroutine(MostrarVictoria());
    }

    IEnumerator MostrarVictoria()
    {
        yield return new WaitForSeconds(1.5f);

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
            if (textoPuntuacionVictoria != null)
            {
                int puntos = GameManager.Instance != null ? GameManager.Instance.Puntuacion : 0;
                textoPuntuacionVictoria.text = $"RD$ {puntos}";
            }
        }

        Time.timeScale = 0f; // Pausar el juego
    }

    // Boton Menu Principal del panel de victoria
    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
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
        foreach (GameObject obj in pool) if (obj.activeInHierarchy) n++;
        return n;
    }
}
