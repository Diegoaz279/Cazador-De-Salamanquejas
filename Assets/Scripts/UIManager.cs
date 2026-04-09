using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Textos HUD")]
    [SerializeField] private TextMeshProUGUI textoPuntuacion;
    [SerializeField] private TextMeshProUGUI textoVidas;
    [SerializeField] private TextMeshProUGUI textoOleada;
    [SerializeField] private TextMeshProUGUI textoContador;
    [SerializeField] private TextMeshProUGUI textoLanzas;

    [Header("Tiempo limite")]
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private float tiempoLimite = 120f; // 2 minutos por ronda
    private float tiempoRestante;
    private bool  contandoTiempo = false;

    [Header("Mensaje central")]
    [SerializeField] private GameObject      panelMensaje;
    [SerializeField] private TextMeshProUGUI textoMensaje;

    [Header("Flash de dano")]
    [SerializeField] private Image imagenFlash;

    [Header("Panel de Pausa")]
    [SerializeField] private GameObject      panelPausa;
    [SerializeField] private TextMeshProUGUI txtPuntosPausa;
    [SerializeField] private Slider          sliderVolumenPausa;
    [SerializeField] private Slider          sliderEfectosPausa;
    [SerializeField] private Toggle          toggleSilenciarPausa;

    [Header("Iconos de vidas")]
    [SerializeField] private GameObject[] iconosVidas;

    void Start()
    {
        panelPausa?.SetActive(false);
        panelMensaje?.SetActive(false);
        ResetFlash();

        // Iniciar tiempo
        tiempoRestante = tiempoLimite;
        contandoTiempo = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) TogglePausa();

        // Cuenta regresiva
        if (contandoTiempo && GameManager.Instance != null && GameManager.Instance.JuegoActivo)
        {
            tiempoRestante -= Time.deltaTime;
            tiempoRestante = Mathf.Max(0f, tiempoRestante);

            if (textoTiempo != null)
            {
                int min = Mathf.FloorToInt(tiempoRestante / 60f);
                int seg = Mathf.FloorToInt(tiempoRestante % 60f);
                textoTiempo.text = $"{min:00}:{seg:00}";
                // Rojo cuando queden menos de 30 segundos
                textoTiempo.color = tiempoRestante < 30f ? Color.red : Color.white;
            }

            if (tiempoRestante <= 0f)
            {
                contandoTiempo = false;
                GameManager.Instance?.TiempoAgotado();
            }
        }
    }

    // ── HUD ───────────────────────────────────────────────────
    public void Actualizar(int puntos, int vidas, int oleada, int eliminados, int objetivo)
    {
        if (textoPuntuacion != null) textoPuntuacion.text = $"RD$ {puntos}";
        if (textoOleada     != null) textoOleada.text     = $"Oleada {oleada}";
        if (textoVidas      != null) textoVidas.text      = $"Vidas: {vidas}";
        if (textoContador   != null) textoContador.text   = $"{eliminados} / {objetivo}";

        if (iconosVidas != null)
            for (int i = 0; i < iconosVidas.Length; i++)
                if (iconosVidas[i] != null) iconosVidas[i].SetActive(i < vidas);
    }

    public void ActualizarLanzas(int restantes)
    {
        if (textoLanzas != null) textoLanzas.text = $"Lanzas: {restantes}";
    }

    // ── MENSAJES ──────────────────────────────────────────────
    public void MostrarMensaje(string msg)
    {
        if (textoMensaje != null) textoMensaje.text = msg;
        panelMensaje?.SetActive(true);
        StopCoroutine(nameof(OcultarMensaje));
        StartCoroutine(nameof(OcultarMensaje));
    }

    IEnumerator OcultarMensaje()
    {
        yield return new WaitForSeconds(2.5f);
        panelMensaje?.SetActive(false);
    }

    // ── FLASH DE DANO ─────────────────────────────────────────
    public void MostrarFlash()
    {
        if (imagenFlash != null) StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        float dur = 0.15f, t = 0f;
        while (t < dur) { t += Time.deltaTime; SetAlpha(Mathf.Lerp(0f, 0.55f, t/dur)); yield return null; }
        t = 0f;
        while (t < dur) { t += Time.deltaTime; SetAlpha(Mathf.Lerp(0.55f, 0f, t/dur)); yield return null; }
        SetAlpha(0f);
    }

    void SetAlpha(float a)
    {
        if (imagenFlash == null) return;
        Color c = imagenFlash.color; c.a = a; imagenFlash.color = c;
    }

    void ResetFlash() { SetAlpha(0f); }

    // ── PAUSA ─────────────────────────────────────────────────
    void TogglePausa()
    {
        if (panelPausa == null) return;
        bool pausado = panelPausa.activeSelf;
        if (pausado) Reanudar();
        else         Pausar();
    }

    void Pausar()
    {
        panelPausa?.SetActive(true);
        GameManager.Instance?.PausarJuego();
        contandoTiempo = false;

        // Sincronizar controles con valores actuales del AudioManager
        if (AudioManager.Instance == null) return;
        if (sliderVolumenPausa  != null) sliderVolumenPausa.value  = AudioManager.Instance.VolumenMusica;
        if (sliderEfectosPausa  != null) sliderEfectosPausa.value  = AudioManager.Instance.VolumenEfectos;
        if (toggleSilenciarPausa != null)
        {
            toggleSilenciarPausa.onValueChanged.RemoveAllListeners();
            toggleSilenciarPausa.isOn = AudioManager.Instance.EstaSilenciado();
            toggleSilenciarPausa.onValueChanged.AddListener(OnToggleSilencio);
        }
        if (sliderVolumenPausa != null)
        {
            sliderVolumenPausa.onValueChanged.RemoveAllListeners();
            sliderVolumenPausa.onValueChanged.AddListener(OnCambiarVolumen);
        }
        if (sliderEfectosPausa != null)
        {
            sliderEfectosPausa.onValueChanged.RemoveAllListeners();
            sliderEfectosPausa.onValueChanged.AddListener(OnCambiarEfectos);
        }

        // Mostrar puntos actuales
        if (txtPuntosPausa != null && GameManager.Instance != null)
            txtPuntosPausa.text = $"RD$ {GameManager.Instance.Puntuacion}";
    }

    // Boton Continuar del panel de pausa
    public void Reanudar()
    {
        panelPausa?.SetActive(false);
        GameManager.Instance?.ReanudarJuego();
        contandoTiempo = true;
    }

    // Boton Salir del panel de pausa
    public void SalirAlMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // Callbacks de los controles de pausa
    public void OnToggleSilencio(bool silenciar)  => AudioManager.Instance?.ToggleSilencio(silenciar);
    public void OnCambiarVolumen(float v)          => AudioManager.Instance?.CambiarVolumenMusica(v);
    public void OnCambiarEfectos(float v)          => AudioManager.Instance?.CambiarVolumenEfectos(v);

    // Compatibilidad
    public void ActualizarUI(int p, int v, int o, int e, int obj) => Actualizar(p, v, o, e, obj);
    public void MostrarFlashDanio()                                => MostrarFlash();
    public void MostrarMensajeOleada(string m)                    => MostrarMensaje(m);
    public void ActualizarContador(int e, int obj)
    {
        if (textoContador != null) textoContador.text = $"{e} / {obj}";
    }
}
