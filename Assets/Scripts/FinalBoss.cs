using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FinalBoss : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite spriteBoss;

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 50;

    [Header("Colores barra de vida")]
    [SerializeField] private Color colorLlena  = Color.green;
    [SerializeField] private Color colorMitad  = Color.yellow;
    [SerializeField] private Color colorBaja   = Color.red;

    public bool EstaVivo         { get; private set; } = false;
    public bool PuedeSerGolpeado { get; private set; } = false;

    private SpriteRenderer sr;
    private Collider2D     col;
    private int            vidaActual;
    private bool           procesando = false;
    private Vector3        posHoyo;
    private Vector3        escalaBase;
    private Slider         barraVida;
    private Image          fillBarra;

    void Awake()
    {
        sr        = GetComponent<SpriteRenderer>();
        col       = GetComponent<Collider2D>();
        escalaBase = transform.localScale;
    }

    // Llamado por SpawnerBoss para pasar la barra de vida
    public void ConfigurarBarra(Slider barra, Image fill)
    {
        barraVida = barra;
        fillBarra = fill;
    }

    public void Aparecer()
    {
        gameObject.SetActive(true);
        procesando       = false;
        EstaVivo         = false;
        PuedeSerGolpeado = false;
        posHoyo          = transform.position;
        vidaActual       = vidaMaxima;

        if (spriteBoss != null) sr.sprite = spriteBoss;
        sr.color             = Color.white;
        transform.localScale = Vector3.zero;
        if (col != null) col.enabled = false;

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
        Vector3 destino = posHoyo + Vector3.up * 0.3f;
        float dur = 1.5f, t = 0f;

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
    }

    public void Morir()
    {
        if (!PuedeSerGolpeado || procesando) return;

        vidaActual--;
        vidaActual = Mathf.Max(0, vidaActual);

        if (barraVida != null) barraVida.value = vidaActual;
        ActualizarColorBarra();

        StartCoroutine(FlashGolpe());

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
        if (fillBarra == null) return;
        float p = (float)vidaActual / vidaMaxima;
        fillBarra.color = p > 0.5f ? colorLlena : p > 0.25f ? colorMitad : colorBaja;
    }

    IEnumerator SecuenciaMuerte()
    {
        procesando       = true;
        EstaVivo         = false;
        PuedeSerGolpeado = false;
        if (col != null) col.enabled = false;

        float dur = 1.5f, t = 0f;
        Vector3 posBase = transform.position;

        while (t < dur)
        {
            t += Time.deltaTime;
            float shake = Mathf.Sin(t * 30f) * 0.05f * (1f - t / dur);
            transform.position = posBase + Vector3.right * shake;
            Color c = sr.color;
            c.r = 1f; c.g = Mathf.Lerp(1f, 0f, t / dur); c.b = Mathf.Lerp(1f, 0f, t / dur);
            sr.color = c;
            yield return null;
        }

        t = 0f; dur = 0.5f;
        Vector3 escIni = transform.localScale;
        while (t < dur)
        {
            t += Time.deltaTime;
            transform.localScale = escIni * (1f - t / dur);
            yield return null;
        }

        if (barraVida != null) barraVida.gameObject.SetActive(false);
        AudioManager.Instance?.SonarMuerteSalamandra();

        // Avisar al SpawnerBoss
        SpawnerBoss.Instance?.NotificarBossMuerto();
        gameObject.SetActive(false);
    }
}
