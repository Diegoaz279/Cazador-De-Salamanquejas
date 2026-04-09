using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Escena del juego")]
    [SerializeField] private string escenaJuego = "Level_Sala";

    [Header("Paneles")]
    [SerializeField] private GameObject panelMenuPrincipal;
    [SerializeField] private GameObject panelOpciones;
    [SerializeField] private GameObject panelPersonajes;

    [Header("Opciones - referencias UI")]
    [SerializeField] private Toggle          toggleSilencio;
    [SerializeField] private Slider          sliderMusica;
    [SerializeField] private Slider          sliderEfectos;
    [SerializeField] private TextMeshProUGUI textoCancion;

    [Header("Personajes")]
    [SerializeField] private Sprite[]        spritesPersonaje;
    [SerializeField] private Image           imagenPersonaje;
    [SerializeField] private TextMeshProUGUI textoNombrePersonaje;
    [SerializeField] private string[]        nombresPersonaje = { "Diego", "Railyn" };

    [Header("Tamano de cada personaje en el panel")]
    [SerializeField] private Vector2[] tamanosPersonaje = {
        new Vector2(200, 300),
        new Vector2(200, 300)
    };

    private int personajeSeleccionado = 0;

    void Start()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelOpciones      != null) panelOpciones.SetActive(false);
        if (panelPersonajes    != null) panelPersonajes.SetActive(false);

        AudioManager.Instance?.ReproducirMusicaMenu();
        SincronizarOpciones();
        ActualizarPersonaje();
    }

    void MostrarPanel(GameObject panel)
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(panel == panelMenuPrincipal);
        if (panelOpciones      != null) panelOpciones.SetActive(panel == panelOpciones);
        if (panelPersonajes    != null) panelPersonajes.SetActive(panel == panelPersonajes);
    }

    // ── MENU PRINCIPAL ────────────────────────────────────────
    public void Jugar()
    {
        AudioManager.Instance?.SonarBoton();
        PlayerPrefs.DeleteKey("PuntuacionAcumulada");
        PlayerPrefs.DeleteKey("VidasActuales");
        PlayerPrefs.SetInt("PersonajeSeleccionado", personajeSeleccionado);
        PlayerPrefs.Save();
        SceneManager.LoadScene(escenaJuego);
    }

    public void AbrirOpciones()
    {
        AudioManager.Instance?.SonarBoton();
        SincronizarOpciones();
        MostrarPanel(panelOpciones);
    }

    public void AbrirPersonajes()
    {
        AudioManager.Instance?.SonarBoton();
        MostrarPanel(panelPersonajes);
    }

    public void Salir()
    {
        AudioManager.Instance?.SonarBoton();
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ── OPCIONES ──────────────────────────────────────────────
    public void CerrarOpciones()
    {
        AudioManager.Instance?.SonarBoton();
        MostrarPanel(panelMenuPrincipal);
    }

    public void OnToggleSilencio(bool silenciar)
    {
        AudioManager.Instance?.ToggleSilencio(silenciar);
        if (sliderMusica  != null) sliderMusica.interactable  = !silenciar;
        if (sliderEfectos != null) sliderEfectos.interactable = !silenciar;
    }

    public void OnCambiarVolumenMusica(float valor)  => AudioManager.Instance?.CambiarVolumenMusica(valor);
    public void OnCambiarVolumenEfectos(float valor) => AudioManager.Instance?.CambiarVolumenEfectos(valor);

    public void OnSiguienteCancion()
    {
        AudioManager.Instance?.SonarBoton();
        AudioManager.Instance?.SiguienteCancion();
        if (textoCancion != null)
            textoCancion.text = "♪ " + AudioManager.Instance?.ObtenerNombreCancionActual();
    }

    // ── PERSONAJES ────────────────────────────────────────────
    public void CerrarPersonajes()
    {
        AudioManager.Instance?.SonarBoton();
        MostrarPanel(panelMenuPrincipal);
    }

    public void PersonajeAnterior()
    {
        AudioManager.Instance?.SonarBoton();
        if (spritesPersonaje == null || spritesPersonaje.Length == 0) return;
        personajeSeleccionado--;
        if (personajeSeleccionado < 0) personajeSeleccionado = spritesPersonaje.Length - 1;
        ActualizarPersonaje();
    }

    public void PersonajeSiguiente()
    {
        AudioManager.Instance?.SonarBoton();
        if (spritesPersonaje == null || spritesPersonaje.Length == 0) return;
        personajeSeleccionado++;
        if (personajeSeleccionado >= spritesPersonaje.Length) personajeSeleccionado = 0;
        ActualizarPersonaje();
    }

    public void SeleccionarPersonaje()
    {
        AudioManager.Instance?.SonarBoton();
        PlayerPrefs.SetInt("PersonajeSeleccionado", personajeSeleccionado);
        PlayerPrefs.Save();
        MostrarPanel(panelMenuPrincipal);
    }

    // ── PRIVADOS ──────────────────────────────────────────────
    void SincronizarOpciones()
    {
        if (AudioManager.Instance == null) return;

        bool sil = AudioManager.Instance.EstaSilenciado();

        if (toggleSilencio != null)
        {
            toggleSilencio.onValueChanged.RemoveAllListeners();
            toggleSilencio.isOn = sil;
            toggleSilencio.onValueChanged.AddListener(OnToggleSilencio);
        }

        if (sliderMusica != null)
        {
            sliderMusica.onValueChanged.RemoveAllListeners();
            sliderMusica.minValue     = 0f;
            sliderMusica.maxValue     = 1f;
            sliderMusica.value        = AudioManager.Instance.VolumenMusica;
            sliderMusica.interactable = !sil;
            sliderMusica.onValueChanged.AddListener(OnCambiarVolumenMusica);
        }

        if (sliderEfectos != null)
        {
            sliderEfectos.onValueChanged.RemoveAllListeners();
            sliderEfectos.minValue     = 0f;
            sliderEfectos.maxValue     = 1f;
            sliderEfectos.value        = AudioManager.Instance.VolumenEfectos;
            sliderEfectos.interactable = !sil;
            sliderEfectos.onValueChanged.AddListener(OnCambiarVolumenEfectos);
        }

        if (textoCancion != null)
            textoCancion.text = "♪ " + AudioManager.Instance.ObtenerNombreCancionActual();
    }

    void ActualizarPersonaje()
    {
        if (spritesPersonaje == null || spritesPersonaje.Length == 0) return;

        if (imagenPersonaje != null)
        {
            imagenPersonaje.sprite = spritesPersonaje[personajeSeleccionado];
            if (tamanosPersonaje != null && personajeSeleccionado < tamanosPersonaje.Length)
            {
                RectTransform rt = imagenPersonaje.GetComponent<RectTransform>();
                if (rt != null) rt.sizeDelta = tamanosPersonaje[personajeSeleccionado];
            }
        }

        if (textoNombrePersonaje != null)
        {
            string nombre = (nombresPersonaje != null && personajeSeleccionado < nombresPersonaje.Length)
                ? nombresPersonaje[personajeSeleccionado]
                : $"Personaje {personajeSeleccionado + 1}";
            textoNombrePersonaje.text = nombre;
        }
    }
}
