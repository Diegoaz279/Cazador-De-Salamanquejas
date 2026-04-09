using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Musica de fondo")]
    [SerializeField] private AudioSource musicaSource;
    [SerializeField] private AudioClip   musicaMenu;
    [SerializeField] private AudioClip   musicaSala;
    [SerializeField] private AudioClip[] musicasExtra;

    [Header("Efectos de sonido")]
    [SerializeField] private AudioSource efectosSource;
    [SerializeField] private AudioClip   sonidoLanza;
    [SerializeField] private AudioClip   sonidoMuerte;
    [SerializeField] private AudioClip   sonidoVida;
    [SerializeField] private AudioClip   sonidoGameOver;
    [SerializeField] private AudioClip   sonidoBoton;
    [SerializeField] private AudioClip   sonidoPuerta;
    [SerializeField] private AudioClip   sonidoAparece;

    private bool  silenciado    = false;
    private float volMusica     = 0.6f;
    private float volEfectos    = 1.0f;
    private int   cancionActual = 0;

    void Awake()
    {
        // Si ya existe una instancia, destruir este objeto completo
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        silenciado = PlayerPrefs.GetInt("Silenciado", 0) == 1;
        volMusica = PlayerPrefs.GetFloat("VolMusica", 0.6f);
        volEfectos = PlayerPrefs.GetFloat("VolEfectos", 1.0f);
        cancionActual = PlayerPrefs.GetInt("CancionActual", 0);

        if (musicaSource != null) musicaSource.volume = silenciado ? 0f : volMusica;
        if (efectosSource != null) efectosSource.volume = volEfectos;
    }

    // ── MUSICA ────────────────────────────────────────────────
    public void ReproducirMusicaMenu()
    {
        if (musicaSource == null) return;
        // Si ya esta sonando la musica del menu, no hacer nada
        if (musicaSource.isPlaying && musicaSource.clip == ObtenerCancionMenu()) return;
        Reproducir(ObtenerCancionMenu());
    }

    public void ReproducirMusicaSala()
    {
        Reproducir(musicaSala);
    }

    public void DetenerMusica()
    {
        musicaSource?.Stop();
    }

    // Siempre detiene lo que suena y pone el nuevo clip
    void Reproducir(AudioClip clip)
    {
        if (musicaSource == null || clip == null) return;
        // Ya esta sonando este clip — no reiniciar
        if (musicaSource.clip == clip && musicaSource.isPlaying) return;
        // Cambiar clip
        musicaSource.Stop();
        musicaSource.clip = clip;
        musicaSource.loop = true;
        if (!silenciado) musicaSource.Play();
    }

    // ── EFECTOS ───────────────────────────────────────────────
    public void SonarLanza()             => Efecto(sonidoLanza);
    public void SonarMuerteSalamandra()  => Efecto(sonidoMuerte);
    public void SonarSalamandraAparece() => Efecto(sonidoAparece);
    public void SonarPerderVida()        => Efecto(sonidoVida);
    public void SonarPuertaAbre()        => Efecto(sonidoPuerta);
    public void SonarGameOver()          => Efecto(sonidoGameOver);
    public void SonarBoton()             => Efecto(sonidoBoton);

    void Efecto(AudioClip clip)
    {
        if (clip == null || efectosSource == null) return;
        efectosSource.PlayOneShot(clip, volEfectos);
    }

    // ── OPCIONES ──────────────────────────────────────────────
    public void ToggleSilencio(bool valor)
    {
        silenciado = valor;
        if (musicaSource != null)
        {
            if (valor) musicaSource.Pause();
            else       musicaSource.UnPause();
        }
        PlayerPrefs.SetInt("Silenciado", valor ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void CambiarVolumenMusica(float v)
    {
        volMusica = v;
        if (musicaSource != null) musicaSource.volume = v;
        PlayerPrefs.SetFloat("VolMusica", v);
        PlayerPrefs.Save();
    }

    public void CambiarVolumenEfectos(float v)
    {
        volEfectos = v;
        PlayerPrefs.SetFloat("VolEfectos", v);
        PlayerPrefs.Save();
    }

    public void SiguienteCancion()
    {
        int total = 1 + (musicasExtra != null ? musicasExtra.Length : 0);
        cancionActual = (cancionActual + 1) % total;
        PlayerPrefs.SetInt("CancionActual", cancionActual);
        PlayerPrefs.Save();
        // Forzar cambio
        musicaSource?.Stop();
        Reproducir(ObtenerCancionMenu());
    }

    AudioClip ObtenerCancionMenu()
    {
        if (cancionActual == 0 || musicasExtra == null || musicasExtra.Length == 0)
            return musicaMenu;
        int idx = cancionActual - 1;
        return idx < musicasExtra.Length ? musicasExtra[idx] : musicaMenu;
    }

    public string ObtenerNombreCancionActual()
    {
        AudioClip c = ObtenerCancionMenu();
        return c != null ? c.name : "Sin música";
    }

    public bool  EstaSilenciado() => silenciado;
    public float VolumenMusica    => volMusica;
    public float VolumenEfectos   => volEfectos;
}
