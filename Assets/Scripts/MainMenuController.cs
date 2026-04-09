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

    [Header("Personajes - sprites disponibles")]
    [SerializeField] private Sprite[]        spritesPersonaje;  // Arrastra Personaje1, etc
    [SerializeField] private Image           imagenPersonaje;   // Image que muestra el personaje
    [SerializeField] private TextMeshProUGUI textoNombrePersonaje;
    [SerializeField] private string[]        nombresPersonaje = { "Diego", "Jugador 2" };

    private int personajeSeleccionado = 0;

    // ─────────────────────────────────────────────────────────
    void Start()
    {
        MostrarPanel(panelMenuPrincipal);
        AudioManager.Instance?.ReproducirMusicaMenu();
        SincronizarOpciones();
        ActualizarPersonaje();
    }

    // ── NAVEGACION ENTRE PANELES ──────────────────────────────
    void MostrarPanel(GameObject panel)
    {
        panelMenuPrincipal?.SetActive(panel == panelMenuPrincipal);
        panelOpciones?.SetActive(panel == panelOpciones);
        panelPersonajes?.SetActive(panel == panelPersonajes);
    }

    // ── BOTONES MENU PRINCIPAL ────────────────────────────────
    public void Jugar()
    {
        AudioManager.Instance?.SonarBoton();
        // Limpiar puntuacion acumulada para empezar desde 0
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
        Debug.Log("Saliendo...");
    }

    // ── BOTONES OPCIONES ──────────────────────────────────────
    public void CerrarOpciones()
    {
        AudioManager.Instance?.SonarBoton();
        MostrarPanel(panelMenuPrincipal);
    }

    // Este es el que conectas al Toggle → On Value Changed
    public void OnToggleSilencio(bool silenciar)
    {
        AudioManager.Instance?.ToggleSilencio(silenciar);
        if (sliderMusica  != null) sliderMusica.interactable  = !silenciar;
        if (sliderEfectos != null) sliderEfectos.interactable = !silenciar;
    }

    // Este conectas al Slider Musica → On Value Changed
    public void OnCambiarVolumenMusica(float valor)
    {
        AudioManager.Instance?.CambiarVolumenMusica(valor);
    }

    // Este conectas al Slider Efectos → On Value Changed
    public void OnCambiarVolumenEfectos(float valor)
    {
        AudioManager.Instance?.CambiarVolumenEfectos(valor);
    }

    // Este conectas al boton Cambiar Cancion
    public void OnSiguienteCancion()
    {
        AudioManager.Instance?.SonarBoton();
        AudioManager.Instance?.SiguienteCancion();
        if (textoCancion != null)
            textoCancion.text = "♪ " + AudioManager.Instance?.ObtenerNombreCancionActual();
    }

    // ── BOTONES PERSONAJES ────────────────────────────────────
    public void CerrarPersonajes()
    {
        AudioManager.Instance?.SonarBoton();
        MostrarPanel(panelMenuPrincipal);
    }

    // Boton flecha izquierda
    public void PersonajeAnterior()
    {
        AudioManager.Instance?.SonarBoton();
        if (spritesPersonaje == null || spritesPersonaje.Length == 0) return;
        personajeSeleccionado--;
        if (personajeSeleccionado < 0)
            personajeSeleccionado = spritesPersonaje.Length - 1;
        ActualizarPersonaje();
    }

    // Boton flecha derecha
    public void PersonajeSiguiente()
    {
        AudioManager.Instance?.SonarBoton();
        if (spritesPersonaje == null || spritesPersonaje.Length == 0) return;
        personajeSeleccionado++;
        if (personajeSeleccionado >= spritesPersonaje.Length)
            personajeSeleccionado = 0;
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
            // Desuscribir para no triggear el evento al setear
            toggleSilencio.onValueChanged.RemoveAllListeners();
            toggleSilencio.isOn = sil;
            toggleSilencio.onValueChanged.AddListener(OnToggleSilencio);
        }

        if (sliderMusica != null)
        {
            sliderMusica.onValueChanged.RemoveAllListeners();
            sliderMusica.minValue    = 0f;
            sliderMusica.maxValue    = 1f;
            sliderMusica.value       = AudioManager.Instance.VolumenMusica;
            sliderMusica.interactable = !sil;
            sliderMusica.onValueChanged.AddListener(OnCambiarVolumenMusica);
        }

        if (sliderEfectos != null)
        {
            sliderEfectos.onValueChanged.RemoveAllListeners();
            sliderEfectos.minValue    = 0f;
            sliderEfectos.maxValue    = 1f;
            sliderEfectos.value       = AudioManager.Instance.VolumenEfectos;
            sliderEfectos.interactable = !sil;
            sliderEfectos.onValueChanged.AddListener(OnCambiarVolumenEfectos);
        }

        if (textoCancion != null)
            textoCancion.text = "♪ " + AudioManager.Instance.ObtenerNombreCancionActual();
    }

    [Header("Tamaño de cada personaje en el panel (width, height)")]
    [SerializeField] private Vector2[] tamanosPersonaje = {
        new Vector2(200, 300),  // Personaje 1
        new Vector2(200, 300)   // Personaje 2
    };

    void ActualizarPersonaje()
    {
        if (spritesPersonaje == null || spritesPersonaje.Length == 0) return;

        if (imagenPersonaje != null)
        {
            imagenPersonaje.sprite = spritesPersonaje[personajeSeleccionado];

            // Aplicar tamaño configurado para este personaje
            if (tamanosPersonaje != null && personajeSeleccionado < tamanosPersonaje.Length)
            {
                RectTransform rt = imagenPersonaje.GetComponent<RectTransform>();
                if (rt != null)
                    rt.sizeDelta = tamanosPersonaje[personajeSeleccionado];
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
