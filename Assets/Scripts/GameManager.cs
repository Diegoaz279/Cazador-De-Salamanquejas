using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuracion del nivel")]
    [SerializeField] private int enemigosParaGanar    = 15;
    [SerializeField] private string escenaSiguiente   = "Level_Sala";
    [SerializeField] private string escenaGameOver    = "GameOver";

    [Header("Estado")]
    [SerializeField] private int puntuacion  = 0;
    [SerializeField] private int vidas       = 3;
    [SerializeField] private int oleada      = 1;
    [SerializeField] private bool juegoActivo = true;

    [Header("Oleadas - velocidad de spawn")]
    [SerializeField] private float spawnOleada1 = 3f;
    [SerializeField] private float spawnOleada2 = 2f;
    [SerializeField] private float spawnOleada3 = 1.2f;
    [SerializeField] private int   puntosOleada2 = 100;
    [SerializeField] private int   puntosOleada3 = 250;

    [Header("Referencias")]
    [SerializeField] private UIManager  uiManager;
    [SerializeField] private Spawner    spawner;
    [SerializeField] private GameObject puerta;

    private int  enemigosEliminados = 0;
    private bool puertaAbierta      = false;

    public int  Puntuacion          => puntuacion;
    public int  Vidas               => vidas;
    public int  Oleada              => oleada;
    public bool JuegoActivo         => juegoActivo;
    public int  EnemigosEliminados  => enemigosEliminados;
    public int  EnemigosParaGanar   => enemigosParaGanar;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        enemigosEliminados = 0;
        puertaAbierta      = false;
        if (puerta != null) puerta.SetActive(false);

        spawner?.ActualizarTiempoSpawn(spawnOleada1);
        uiManager?.Actualizar(puntuacion, vidas, oleada, enemigosEliminados, enemigosParaGanar);
    }

    public void SumarPuntos(int pts)
    {
        if (!juegoActivo) return;
        puntuacion += pts;
        VerificarOleada();
        uiManager?.Actualizar(puntuacion, vidas, oleada, enemigosEliminados, enemigosParaGanar);
    }

    public void ContarEnemigo()
    {
        if (!juegoActivo) return;
        enemigosEliminados++;
        uiManager?.Actualizar(puntuacion, vidas, oleada, enemigosEliminados, enemigosParaGanar);

        if (enemigosEliminados >= enemigosParaGanar && !puertaAbierta)
            AbrirPuerta();
    }

    public void PerderVida()
    {
        if (!juegoActivo) return;
        vidas--;
        uiManager?.Actualizar(puntuacion, vidas, oleada, enemigosEliminados, enemigosParaGanar);
        uiManager?.MostrarFlash();

        if (vidas <= 0) IniciarGameOver();
    }

    public void PasarDeNivel()
    {
        if (!puertaAbierta) return;
        PlayerPrefs.SetInt("PuntuacionFinal", puntuacion);
        PlayerPrefs.SetInt("OleadaFinal", oleada);
        PlayerPrefs.Save();
        SceneManager.LoadScene(escenaSiguiente);
    }

    public void PausarJuego()
    {
        juegoActivo    = false;
        Time.timeScale = 0f;
    }

    public void ReanudarJuego()
    {
        juegoActivo    = true;
        Time.timeScale = 1f;
    }

    void AbrirPuerta()
    {
        puertaAbierta = true;
        if (puerta != null) puerta.SetActive(true);
        uiManager?.MostrarMensaje("¡Puerta abierta! ¡Corre!");
    }

    void VerificarOleada()
    {
        int anterior = oleada;
        if      (puntuacion >= puntosOleada3 && oleada < 3) oleada = 3;
        else if (puntuacion >= puntosOleada2 && oleada < 2) oleada = 2;

        if (oleada != anterior)
        {
            float t = oleada == 3 ? spawnOleada3 : spawnOleada2;
            spawner?.ActualizarTiempoSpawn(t);
            uiManager?.MostrarMensaje($"¡OLEADA {oleada}! Se pusieron brutas...");
        }
    }

    void IniciarGameOver()
    {
        juegoActivo = false;
        PlayerPrefs.SetInt("PuntuacionFinal", puntuacion);
        PlayerPrefs.SetInt("OleadaFinal", oleada);
        PlayerPrefs.Save();
        Invoke(nameof(CargarGameOver), 1.5f);
    }

    void CargarGameOver() => SceneManager.LoadScene(escenaGameOver);
}
