using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuracion del nivel")]
    [SerializeField] private int    enemigosParaGanar  = 15;
    [SerializeField] private string escenaSiguiente    = "Level_Patio";
    [SerializeField] private string escenaGameOver     = "GameOver";

    [Header("Estado")]
    [SerializeField] private int  puntuacion  = 0;
    [SerializeField] private int  vidas       = 3;
    [SerializeField] private int  oleada      = 1;
    [SerializeField] private bool juegoActivo = true;

    [Header("Oleadas")]
    [SerializeField] private float spawnOleada1  = 2.5f;
    [SerializeField] private float spawnOleada2  = 1.8f;
    [SerializeField] private float spawnOleada3  = 1.0f;
    [SerializeField] private int   puntosOleada2 = 100;
    [SerializeField] private int   puntosOleada3 = 250;

    [Header("Referencias")]
    [SerializeField] private UIManager      uiManager;
    [SerializeField] private Spawner        spawner;

    [Header("Fondo de la sala")]
    [SerializeField] private SpriteRenderer fondoSala;          // El SpriteRenderer del fondo
    [SerializeField] private Sprite         spriteSalaCerrada;  // FondoSala.png
    [SerializeField] private Sprite         spriteSalaAbierta;  // FondoSalaPuertaAbierta.png

    [Header("Zona de la puerta (trigger invisible)")]
    [SerializeField] private GameObject zonaPuerta; // Collider2D trigger cerca de la puerta

    // Estado interno
    private int  enemigosEliminados = 0;
    private bool puertaAbierta      = false;
    private int  lanzasRestantes    = 20;

    // Propiedades publicas
    public int  Puntuacion         => puntuacion;
    public int  Vidas              => vidas;
    public int  Oleada             => oleada;
    public bool JuegoActivo        => juegoActivo;
    public int  EnemigosEliminados => enemigosEliminados;
    public int  EnemigosParaGanar  => enemigosParaGanar;
    public bool PuertaAbierta      => puertaAbierta;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        enemigosEliminados = 0;
        puertaAbierta      = false;

        // Fondo empieza con puerta cerrada
        if (fondoSala != null && spriteSalaCerrada != null)
            fondoSala.sprite = spriteSalaCerrada;

        // Zona de puerta desactivada al inicio
        if (zonaPuerta != null) zonaPuerta.SetActive(false);

        spawner?.ActualizarTiempoSpawn(spawnOleada1);
        AudioManager.Instance?.ReproducirMusicaSala();
        RefrescarUI();
    }

    // ── PUNTOS Y CONTEO ───────────────────────────────────────
    public void SumarPuntos(int pts)
    {
        if (!juegoActivo) return;
        puntuacion += pts;
        VerificarOleada();
        RefrescarUI();
    }

    public void ContarEnemigo()
    {
        if (!juegoActivo) return;
        enemigosEliminados++;
        RefrescarUI();
        if (enemigosEliminados >= enemigosParaGanar && !puertaAbierta)
            AbrirPuerta();
    }

    // ── LANZAS ────────────────────────────────────────────────
    public void ActualizarLanzas(int restantes)
    {
        lanzasRestantes = restantes;
        uiManager?.ActualizarLanzas(restantes);
    }

    public void SinLanzas()
    {
        if (!juegoActivo) return;
        uiManager?.MostrarMensaje("¡Se te acabaron las lanzas! ¡Coño!");
        Invoke(nameof(IniciarGameOver), 2f);
    }

    // ── VIDAS ─────────────────────────────────────────────────
    public void PerderVida()
    {
        if (!juegoActivo) return;
        vidas--;
        AudioManager.Instance?.SonarPerderVida();
        uiManager?.MostrarFlash();
        RefrescarUI();
        if (vidas <= 0) IniciarGameOver();
    }

    // ── PASAR DE NIVEL (llamado por TransicionNivel) ──────────
    public void PasarDeNivel()
    {
        if (!puertaAbierta) return;
        juegoActivo = false;
        Guardar();
        SceneManager.LoadScene(escenaSiguiente);
    }

    // ── PAUSA ─────────────────────────────────────────────────
    public void PausarJuego()   { juegoActivo = false; Time.timeScale = 0f; }
    public void ReanudarJuego() { juegoActivo = true;  Time.timeScale = 1f; }

    // ── PRIVADOS ──────────────────────────────────────────────
    void AbrirPuerta()
    {
        puertaAbierta = true;

        // Cambiar el sprite del fondo a la version con puerta abierta
        if (fondoSala != null && spriteSalaAbierta != null)
            fondoSala.sprite = spriteSalaAbierta;

        // Activar la zona trigger de la puerta
        if (zonaPuerta != null) zonaPuerta.SetActive(true);

        AudioManager.Instance?.SonarPuertaAbre();
        uiManager?.MostrarMensaje("¡Puerta abierta! ¡Corre al patio!");
    }

    void VerificarOleada()
    {
        int ant = oleada;
        if      (puntuacion >= puntosOleada3 && oleada < 3) oleada = 3;
        else if (puntuacion >= puntosOleada2 && oleada < 2) oleada = 2;

        if (oleada != ant)
        {
            float t = oleada == 3 ? spawnOleada3 : spawnOleada2;
            spawner?.ActualizarTiempoSpawn(t);
            uiManager?.MostrarMensaje($"¡OLEADA {oleada}! Se pusieron brutas...");
        }
    }

    void RefrescarUI()
    {
        uiManager?.Actualizar(puntuacion, vidas, oleada, enemigosEliminados, enemigosParaGanar);
    }

    void IniciarGameOver()
    {
        juegoActivo = false;
        AudioManager.Instance?.SonarGameOver();
        AudioManager.Instance?.DetenerMusica();
        Guardar();
        Invoke(nameof(CargarGameOver), 1.5f);
    }

    void CargarGameOver() => SceneManager.LoadScene(escenaGameOver);

    void Guardar()
    {
        PlayerPrefs.SetInt("PuntuacionFinal", puntuacion);
        PlayerPrefs.SetInt("OleadaFinal", oleada);
        PlayerPrefs.Save();
    }
}
