using UnityEngine;
using System.Collections;

public class Salamanqueja : MonoBehaviour
{
    public enum Tipo { Normal, Rapida, Resistente, Dorada }

    [Header("Tipo de salamanqueja")]
    [SerializeField] private Tipo tipo = Tipo.Normal;

    [Header("Sprites")]
    [SerializeField] private Sprite spriteNormal;
    [SerializeField] private Sprite spriteRapida;
    [SerializeField] private Sprite spriteResistente;
    [SerializeField] private Sprite spriteDorada;

    [Header("Tiempo visible")]
    [SerializeField] private float tiempoNormal    = 3.0f;
    [SerializeField] private float tiempoRapida    = 1.5f;
    [SerializeField] private float tiempoResistente= 4.0f;
    [SerializeField] private float tiempoDorada    = 1.0f;

    [Header("Puntos")]
    [SerializeField] private int puntosNormal     = 10;
    [SerializeField] private int puntosRapida     = 25;
    [SerializeField] private int puntosResistente = 15;
    [SerializeField] private int puntosDorada     = 50;

    // Propiedades publicas
    public bool EstaViva { get; private set; } = false;
    public bool PuedeSerGolpeada { get; private set; } = false;

    private SpriteRenderer sr;
    private Collider2D col;
    private float tiempoRestante;
    private bool procesando = false;
    private int golpes = 0;
    private Vector3 posHoyo;

    void Awake()
    {
        sr  = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        procesando          = false;
        golpes              = 0;
        EstaViva            = false;
        PuedeSerGolpeada    = false;
        posHoyo             = transform.position;
        transform.localScale = Vector3.zero;

        if (col != null) col.enabled = false;

        AsignarSprite();
        tiempoRestante = ObtenerTiempo();

        StartCoroutine(SalirDelHoyo());
    }

    void AsignarSprite()
    {
        Sprite s = tipo switch
        {
            Tipo.Rapida     => spriteRapida,
            Tipo.Resistente => spriteResistente,
            Tipo.Dorada     => spriteDorada,
            _               => spriteNormal
        };

        if (s != null) sr.sprite = s;

        // Tinte dorado si es dorada y no tiene sprite propio
        sr.color = (tipo == Tipo.Dorada && spriteDorada == null)
            ? new Color(1f, 0.85f, 0f)
            : Color.white;
    }

    // ---- SALE DEL HOYO ----
    IEnumerator SalirDelHoyo()
    {
        float dur = 0.35f, t = 0f;
        Vector3 destino = posHoyo + Vector3.up * 0.2f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / dur);
            transform.localScale = Vector3.one * p;
            transform.position   = Vector3.Lerp(posHoyo, destino, p);
            yield return null;
        }

        transform.localScale = Vector3.one;
        transform.position   = destino;
        EstaViva             = true;
        PuedeSerGolpeada     = true;
        if (col != null) col.enabled = true;

        StartCoroutine(Temporizador());
    }

    // ---- CUENTA REGRESIVA ----
    IEnumerator Temporizador()
    {
        while (tiempoRestante > 0f && EstaViva)
        {
            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante < 0.8f)
            {
                float a = Mathf.PingPong(Time.time * 8f, 1f);
                Color c = sr.color; c.a = Mathf.Max(a, 0.2f); sr.color = c;
            }

            yield return null;
        }

        if (EstaViva) HuirAlTocarJugador();
    }

    // ---- RECIBIR GOLPE (llamado por LanzaProyectil) ----
    public void Morir()
    {
        if (!PuedeSerGolpeada || procesando) return;

        if (tipo == Tipo.Resistente && golpes < 1)
        {
            golpes++;
            StartCoroutine(FlashRojo());
            return;
        }

        procesando       = true;
        EstaViva         = false;
        PuedeSerGolpeada = false;
        if (col != null) col.enabled = false;

        StopAllCoroutines();

        GameManager.Instance?.SumarPuntos(ObtenerPuntos());
        GameManager.Instance?.ContarEnemigo();

        StartCoroutine(AnimMuerte());
    }

    IEnumerator FlashRojo()
    {
        PuedeSerGolpeada = false;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        AsignarSprite();
        yield return new WaitForSeconds(0.1f);
        PuedeSerGolpeada = true;
    }

    // ---- HUIR (tiempo agotado o toco al jugador) ----
    public void HuirAlTocarJugador()
    {
        if (!EstaViva || procesando) return;

        procesando       = true;
        EstaViva         = false;
        PuedeSerGolpeada = false;
        if (col != null) col.enabled = false;

        StopAllCoroutines();
        StartCoroutine(MeterseAlHoyo());
    }

    IEnumerator MeterseAlHoyo()
    {
        float dur = 0.25f, t = 0f;
        Vector3 escIni = transform.localScale;
        Vector3 posIni = transform.position;

        while (t < dur)
        {
            t += Time.deltaTime;
            float p = t / dur;
            transform.localScale = Vector3.Lerp(escIni, Vector3.zero, p);
            transform.position   = Vector3.Lerp(posIni, posHoyo, p);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    IEnumerator AnimMuerte()
    {
        float dur = 0.3f, t = 0f;
        Vector3 escIni = transform.localScale;

        while (t < dur)
        {
            t += Time.deltaTime;
            float p = t / dur;
            transform.rotation   = Quaternion.Euler(0, 0, p * 180f);
            transform.localScale = escIni * (1f - p);
            Color c = sr.color; c.a = 1f - p; sr.color = c;
            yield return null;
        }

        transform.rotation = Quaternion.identity;
        gameObject.SetActive(false);
    }

    // ---- HELPERS ----
    float ObtenerTiempo() => tipo switch
    {
        Tipo.Rapida     => tiempoRapida,
        Tipo.Resistente => tiempoResistente,
        Tipo.Dorada     => tiempoDorada,
        _               => tiempoNormal
    };

    int ObtenerPuntos() => tipo switch
    {
        Tipo.Rapida     => puntosRapida,
        Tipo.Resistente => puntosResistente,
        Tipo.Dorada     => puntosDorada,
        _               => puntosNormal
    };

    public void SetTipo(Tipo t) { tipo = t; }
}
