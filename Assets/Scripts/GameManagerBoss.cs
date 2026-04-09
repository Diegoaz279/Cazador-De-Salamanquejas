using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

// Este script reemplaza al GameManager normal en el nivel del Boss
// Tiene su propia logica de fases
public class GameManagerBoss : MonoBehaviour
{
    public static GameManagerBoss Instance { get; private set; }

    // ── FASES DEL BOSS ────────────────────────────────────────
    public enum Fase { Hijas, Boss, Victoria }
    private Fase faseActual = Fase.Hijas;

    [Header("Configuracion")]
    [SerializeField] private int   hijasParaMatar    = 6;  // Cuantas hijas hay que matar para que aparezca el boss
    [SerializeField] private string escenaVictoria   = "GameOver"; // O una escena de victoria especial
    [SerializeField] private string escenaGameOver   = "GameOver";

    [Header("Spawn de Hijas")]
    [SerializeField] private List<Transform> hojoysHijas = new List<Transform>(); // Los 4 hoyos pequeños
    [SerializeField] private GameObject      prefabHija;
    [SerializeField] private int             poolHijas        = 8;
    [SerializeField] private float           tiempoSpawnHija  = 3f;
    [SerializeField] private int             maximasHijasActivas = 3;

    [Header("Boss")]
    [SerializeField] private FinalBoss       finalBoss; // El GameObject del boss en la escena
    [SerializeField] private Transform       hoyoCentral; // El hoyo grande donde aparece el boss

    [Header("Estado del jugador")]
    [SerializeField] private int   vidas       = 3;
    [SerializeField] private int   lanzas      = 40;
    [SerializeField] private bool  juegoActivo = true;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoVidas;
    [SerializeField] private TextMeshProUGUI textoLanzas;
    [SerializeField] private TextMeshProUGUI textoFase;
    [SerializeField] private TextMeshProUGUI textoMensaje;
    [SerializeField] private GameObject      panelMensaje;
    [SerializeField] private Image           imagenFlash;
    [SerializeField] private GameObject      panelPausa;

    // Estado interno
    private List<GameObject> poolHijasObjs = new List<GameObject>();
    private int              hijasMuertas  = 0;
    private float            timerSpawn    = 0f;

    public bool JuegoActivo => juegoActivo;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // Cargar datos del nivel anterior
        vidas  = PlayerPrefs.GetInt("VidasActuales", 3);
        lanzas = 40; // El boss da 40 lanzas

        faseActual    = Fase.Hijas;
        hijasMuertas  = 0;

        CrearPoolHijas();
        ActualizarUI();
        MostrarMensajeBoss("¡FASE FINAL! ¡Mata a las hijas primero!");

        AudioManager.Instance?.DetenerMusica();
        // Puedes agregar musica de boss aqui si tienes
        // AudioManager.Instance?.ReproducirMusicaBoss();

        if (textoFase != null) textoFase.text = "FASE 1: Las Hijas";
        if (panelMensaje != null) panelMensaje.SetActive(false);
    }

    void Update()
    {
        if (!juegoActivo) return;

        if (Input.GetKeyDown(KeyCode.Escape)) TogglePausa();

        // Solo spawnear hijas en la fase 1
        if (faseActual == Fase.Hijas)
        {
            timerSpawn += Time.deltaTime;
            if (timerSpawn >= tiempoSpawnHija)
            {
                timerSpawn = 0f;
                SpawnHija();
            }
        }
    }

    // ── POOL DE HIJAS ─────────────────────────────────────────
    void CrearPoolHijas()
    {
        for (int i = 0; i < poolHijas; i++)
        {
            GameObject obj = Instantiate(prefabHija, transform);
            obj.SetActive(false);
            poolHijasObjs.Add(obj);
        }
    }

    void SpawnHija()
    {
        if (hojoysHijas.Count == 0) return;
        if (ContarHijasActivas() >= maximasHijasActivas) return;

        GameObject obj = ObtenerHijaLibre();
        if (obj == null) return;

        Transform hoyo = hojoysHijas[Random.Range(0, hojoysHijas.Count)];
        obj.transform.position = hoyo.position;
        obj.SetActive(true);
    }

    GameObject ObtenerHijaLibre()
    {
        foreach (GameObject obj in poolHijasObjs)
            if (!obj.activeInHierarchy) return obj;
        return null;
    }

    int ContarHijasActivas()
    {
        int n = 0;
        foreach (GameObject obj in poolHijasObjs)
            if (obj.activeInHierarchy) n++;
        return n;
    }

    // ── EVENTOS DEL JUEGO ─────────────────────────────────────

    // Llamado por SalamanquejaHija cuando muere
    public void HijaMuerta()
    {
        if (faseActual != Fase.Hijas) return;

        hijasMuertas++;
        if (textoFase != null)
            textoFase.text = $"Hijas: {hijasMuertas} / {hijasParaMatar}";

        if (hijasMuertas >= hijasParaMatar)
            StartCoroutine(TransicionAlBoss());
    }

    IEnumerator TransicionAlBoss()
    {
        faseActual = Fase.Boss;

        // Desactivar todas las hijas activas
        foreach (GameObject obj in poolHijasObjs)
            if (obj.activeInHierarchy)
            {
                SalamanquejaHija h = obj.GetComponent<SalamanquejaHija>();
                h?.HuirAlHoyo();
            }

        yield return new WaitForSeconds(1.5f);

        MostrarMensajeBoss("¡¡CUIDADO!! ¡LA SALAMANQUEJA MADRE DESPIERTA!");

        if (textoFase != null) textoFase.text = "FASE 2: ¡EL BOSS!";

        yield return new WaitForSeconds(2f);

        // Aparecer el boss en el hoyo central
        if (finalBoss != null) finalBoss.Aparecer();
    }

    // Llamado por FinalBoss cuando muere
    public void BossMuerto()
    {
        faseActual  = Fase.Victoria;
        juegoActivo = false;

        MostrarMensajeBoss("¡¡VICTORIA!! ¡La casa de la abuela está a salvo!");
        StartCoroutine(CargarVictoria());
    }

    IEnumerator CargarVictoria()
    {
        yield return new WaitForSeconds(3f);
        PlayerPrefs.SetInt("PuntuacionFinal", PlayerPrefs.GetInt("PuntuacionAcumulada", 0));
        PlayerPrefs.SetInt("OleadaFinal", 3);
        PlayerPrefs.DeleteKey("PuntuacionAcumulada");
        PlayerPrefs.DeleteKey("VidasActuales");
        PlayerPrefs.Save();
        SceneManager.LoadScene(escenaVictoria);
    }

    // ── JUGADOR ───────────────────────────────────────────────
    public void PerderVida()
    {
        if (!juegoActivo) return;
        vidas--;
        StartCoroutine(FlashDanio());
        ActualizarUI();
        if (vidas <= 0) IniciarGameOver();
    }

    public void ActualizarLanzas(int restantes)
    {
        lanzas = restantes;
        if (textoLanzas != null) textoLanzas.text = $"Lanzas: {lanzas}";
    }

    public void SinLanzas()
    {
        if (!juegoActivo) return;
        MostrarMensajeBoss("¡Se te acabaron las lanzas!");
        Invoke(nameof(IniciarGameOver), 2f);
    }

    void IniciarGameOver()
    {
        juegoActivo = false;
        AudioManager.Instance?.SonarGameOver();
        AudioManager.Instance?.DetenerMusica();
        PlayerPrefs.SetInt("PuntuacionFinal", PlayerPrefs.GetInt("PuntuacionAcumulada", 0));
        PlayerPrefs.SetInt("OleadaFinal", 3);
        PlayerPrefs.Save();
        Invoke(nameof(CargarGameOver), 1.5f);
    }

    void CargarGameOver() => SceneManager.LoadScene(escenaGameOver);

    // ── UI ────────────────────────────────────────────────────
    void ActualizarUI()
    {
        if (textoVidas  != null) textoVidas.text  = $"Vidas: {vidas}";
        if (textoLanzas != null) textoLanzas.text = $"Lanzas: {lanzas}";
    }

    public void MostrarMensajeBoss(string msg)
    {
        if (textoMensaje != null) textoMensaje.text = msg;
        panelMensaje?.SetActive(true);
        StopCoroutine(nameof(OcultarMensaje));
        StartCoroutine(nameof(OcultarMensaje));
    }

    IEnumerator OcultarMensaje()
    {
        yield return new WaitForSeconds(3f);
        panelMensaje?.SetActive(false);
    }

    IEnumerator FlashDanio()
    {
        if (imagenFlash == null) yield break;
        float dur = 0.15f, t = 0f;
        while (t < dur) { t += Time.deltaTime; SetFlash(Mathf.Lerp(0f, 0.55f, t/dur)); yield return null; }
        t = 0f;
        while (t < dur) { t += Time.deltaTime; SetFlash(Mathf.Lerp(0.55f, 0f, t/dur)); yield return null; }
        SetFlash(0f);
    }

    void SetFlash(float a)
    {
        if (imagenFlash == null) return;
        Color c = imagenFlash.color; c.a = a; imagenFlash.color = c;
    }

    void TogglePausa()
    {
        if (panelPausa == null) return;
        bool pausado = panelPausa.activeSelf;
        panelPausa.SetActive(!pausado);
        juegoActivo    = pausado;
        Time.timeScale = pausado ? 1f : 0f;
    }

    public void ReanudarJuego()
    {
        panelPausa?.SetActive(false);
        juegoActivo    = true;
        Time.timeScale = 1f;
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
