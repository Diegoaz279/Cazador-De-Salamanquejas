using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Musica de fondo")]
    [SerializeField] private AudioSource musicaSource;
    [SerializeField] private AudioClip   musicaMenu;        // MúsicadelMenúPrincipal
    [SerializeField] private AudioClip   musicaSala;        // MúsicadelaSala(gameplay)
    [SerializeField] private AudioClip[] musicasExtra;      // Canciones extra para opciones

    [Header("Efectos de sonido")]
    [SerializeField] private AudioSource efectosSource;
    [SerializeField] private AudioClip   sonidoLanza;       // Sonidodelanzaaldisparar
    [SerializeField] private AudioClip   sonidoMuerte;      // Sonidodesalamanquejaalmorir
    [SerializeField] private AudioClip   sonidoVida;        // Sonidodeperderunavida
    [SerializeField] private AudioClip   sonidoGameOver;    // SonidodeGameOver
    [SerializeField] private AudioClip   sonidoBoton;       // SonidodebotóndelMenú
    // Estos son opcionales - si no los tienes dejalos en None
    [SerializeField] private AudioClip   sonidoPuerta;      // Opcional
    [SerializeField] private AudioClip   sonidoAparece;     // Opcional

    private bool  silenciado    = false;
    private float volMusica     = 0.6f;
    private float volEfectos    = 1.0f;
    private int   cancionActual = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); return; }

        silenciado    = PlayerPrefs.GetInt("Silenciado", 0) == 1;
        volMusica     = PlayerPrefs.GetFloat("VolMusica", 0.4f);
        volEfectos    = PlayerPrefs.GetFloat("VolEfectos", 2.0f);
        cancionActual = PlayerPrefs.GetInt("CancionActual", 0);

        if (musicaSource  != null) musicaSource.volume  = silenciado ? 0f : volMusica;
        if (efectosSource != null) efectosSource.volume = volEfectos;
    }

    // ── MUSICA ────────────────────────────────────────────────
    public void ReproducirMusicaMenu()
    {
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

    void Reproducir(AudioClip clip)
    {
        if (musicaSource == null || clip == null) return;
        if (musicaSource.clip == clip && musicaSource.isPlaying) return;
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
        Reproducir(ObtenerCancionMenu());
    }

    public string ObtenerNombreCancion()
    {
        AudioClip c = ObtenerCancionMenu();
        return c != null ? c.name : "Sin música";
    }

    AudioClip ObtenerCancionMenu()
    {
        if (cancionActual == 0 || musicasExtra == null || musicasExtra.Length == 0)
            return musicaMenu;
        int idx = cancionActual - 1;
        return idx < musicasExtra.Length ? musicasExtra[idx] : musicaMenu;
    }

    public bool  EstaSilenciado() => silenciado;
    public float VolumenMusica    => volMusica;
    public float VolumenEfectos   => volEfectos;
    public string ObtenerNombreCancionActual() => ObtenerNombreCancion();
}
