using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Pon este script en el GameObject del FinalBoss
// El boss esta fijo en el hoyo central
public class FinalBoss : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite spriteBoss;      // FinalBoss.png
    [SerializeField] private Sprite spriteBossEnojo; // Si tienes una version enojada

    [Header("Vida del Boss")]
    [SerializeField] private int vidaMaxima = 50;    // 50 golpes para morir

    [Header("Barra de vida UI")]
    [SerializeField] private Slider barraVida;        // Slider en el Canvas
    [SerializeField] private Image  rellenoBarraVida; // La Image del fill de la barra

    [Header("Colores de la barra segun vida")]
    [SerializeField] private Color colorVidaLlena  = Color.green;
    [SerializeField] private Color colorVidaMitad  = Color.yellow;
    [SerializeField] private Color colorVidaBaja   = Color.red;

    // Estado publico
    public bool EstaVivo         { get; private set; } = false;
    public bool PuedeSerGolpeado { get; private set; } = false;

    private SpriteRenderer sr;
    private Collider2D     col;
    private int            vidaActual;
    private bool           procesando = false;
    private Vector3        posHoyo;
    private Vector3        escalaBase;

    void Awake()
    {
        sr        = GetComponent<SpriteRenderer>();
        col       = GetComponent<Collider2D>();
        escalaBase = transform.localScale;
        // El boss empieza oculto
        gameObject.SetActive(false);
    }

    // Llamado por GameManagerBoss cuando todas las hijas mueren
    public void Aparecer()
    {
        gameObject.SetActive(true);
        procesando       = false;
        EstaVivo         = false;
        PuedeSerGolpeado = false;
        posHoyo          = transform.position;
        vidaActual       = vidaMaxima;

        if (spriteBoss != null) sr.sprite = spriteBoss;
        sr.color = Color.white;
        transform.localScale = Vector3.zero;
        if (col != null) col.enabled = false;

        // Configurar barra de vida
        if (barraVida != null)
        {
            barraVida.gameObject.SetActive(true);
            barraVida.maxValue = vidaMaxima;
            barraVida.value    = vidaMaxima;
            ActualizarColorBarra();
        }

        StartCoroutine(SalirDelHoyo());
    }

    IEnumerator SalirDelHoyo()
    {
        // Animacion dramatica de salida — mas lenta que las hijas
        Vector3 destino = posHoyo + Vector3.up * 0.3f;
        float dur = 1.2f, t = 0f;

        // Temblar la camara (efecto de tension)
        while (t < dur)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / dur);
            transform.localScale = escalaBase * p;
            transform.position   = Vector3.Lerp(posHoyo, destino, p);
            yield return null;
        }

        transform.localScale = escalaBase;
        transform.position   = destino;
        EstaVivo             = true;
        PuedeSerGolpeado     = true;
        if (col != null) col.enabled = true;

        // Mensaje dramatico
        GameManagerBoss.Instance?.MostrarMensajeBoss("¡LA SALAMANQUEJA MADRE HA DESPERTADO!");
    }

    // Llamado por LanzaProyectil
    public void Morir()
    {
        if (!PuedeSerGolpeado || procesando) return;

        vidaActual--;
        vidaActual = Mathf.Max(0, vidaActual);

        // Actualizar barra
        if (barraVida != null) barraVida.value = vidaActual;
        ActualizarColorBarra();

        // Flash rojo
        StartCoroutine(FlashGolpe());

        // Si llega al 50% de vida, ponerse mas enojado
        if (vidaActual == vidaMaxima / 2)
            GameManagerBoss.Instance?.MostrarMensajeBoss("¡Está enojada! ¡CUIDADO!");

        if (vidaActual <= 0)
            StartCoroutine(SecuenciaMuerte());
    }

    IEnumerator FlashGolpe()
    {
        PuedeSerGolpeado = false;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        sr.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        if (!procesando) PuedeSerGolpeado = true;
    }

    void ActualizarColorBarra()
    {
        if (rellenoBarraVida == null) return;
        float porcentaje = (float)vidaActual / vidaMaxima;
        if      (porcentaje > 0.5f) rellenoBarraVida.color = colorVidaLlena;
        else if (porcentaje > 0.25f) rellenoBarraVida.color = colorVidaMitad;
        else                        rellenoBarraVida.color = colorVidaBaja;
    }

    IEnumerator SecuenciaMuerte()
    {
        procesando       = true;
        EstaVivo         = false;
        PuedeSerGolpeado = false;
        if (col != null) col.enabled = false;

        // Animacion de muerte epica
        float dur = 1.5f, t = 0f;
        Vector3 escIni = transform.localScale;

        // Temblar antes de morir
        while (t < dur)
        {
            t += Time.deltaTime;
            float shake = Mathf.Sin(t * 30f) * 0.05f * (1f - t/dur);
            transform.position = posHoyo + Vector3.up * 0.3f + Vector3.right * shake;

            float p = t / dur;
            Color c = sr.color;
            c.r = 1f; c.g = Mathf.Lerp(1f, 0f, p); c.b = Mathf.Lerp(1f, 0f, p);
            sr.color = c;
            yield return null;
        }

        // Escalar a 0
        t = 0f;
        dur = 0.5f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p = t / dur;
            transform.localScale = escIni * (1f - p);
            yield return null;
        }

        // Ocultar barra de vida
        if (barraVida != null) barraVida.gameObject.SetActive(false);

        AudioManager.Instance?.SonarMuerteSalamandra();
        GameManagerBoss.Instance?.BossMuerto();
        gameObject.SetActive(false);
    }
}
