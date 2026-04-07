using UnityEngine;
using System.Collections;

public class Salamanqueja : MonoBehaviour
{
    public enum Tipo { Normal, Rapida, Resistente, Dorada }

    [Header("Tipo")]
    [SerializeField] private Tipo tipo = Tipo.Normal;

    [Header("Sprites - arrastra Lagarto1 a Lagarto4")]
    [SerializeField] private Sprite spriteNormal;
    [SerializeField] private Sprite spriteRapida;
    [SerializeField] private Sprite spriteResistente;
    [SerializeField] private Sprite spriteDorada;

    [Header("Tiempo visible por tipo")]
    [SerializeField] private float tiempoNormal     = 3.0f;
    [SerializeField] private float tiempoRapida     = 1.5f;
    [SerializeField] private float tiempoResistente = 4.5f;
    [SerializeField] private float tiempoDorada     = 1.0f;

    [Header("Puntos por tipo")]
    [SerializeField] private int puntosNormal     = 10;
    [SerializeField] private int puntosRapida     = 25;
    [SerializeField] private int puntosResistente = 15;
    [SerializeField] private int puntosDorada     = 50;

    // Estado publico
    public bool EstaViva         { get; private set; } = false;
    public bool PuedeSerGolpeada { get; private set; } = false;

    private SpriteRenderer sr;
    private Collider2D     col;
    private float          tiempoRestante;
    private bool           procesando  = false;
    private int            golpes      = 0;
    private Vector3        posHoyo;
    private Vector3        escalaBase;
    private Color          colorBase;  // Color original segun tipo

    void Awake()
    {
        sr         = GetComponent<SpriteRenderer>();
        col        = GetComponent<Collider2D>();
        escalaBase = transform.localScale;
    }

    void OnEnable()
    {
        procesando           = false;
        golpes               = 0;
        EstaViva             = false;
        PuedeSerGolpeada     = false;
        posHoyo              = transform.position;
        transform.localScale = Vector3.zero;
        transform.rotation   = Quaternion.identity;

        if (col != null) col.enabled = false;

        AplicarTipo();
        colorBase      = sr.color; // Guardar color original DESPUES de aplicar tipo
        tiempoRestante = ObtenerTiempo();
        StartCoroutine(SalirDelHoyo());
    }

    // ── TIPO → SPRITE + COLOR ────────────────────────────────
    void AplicarTipo()
    {
        sr.color = Color.white;

        switch (tipo)
        {
            case Tipo.Normal:
                if (spriteNormal != null) sr.sprite = spriteNormal;
                sr.color = Color.white;
                break;

            case Tipo.Rapida:
                if (spriteRapida != null)
                    sr.sprite = spriteRapida;
                else if (spriteNormal != null)
                    sr.sprite = spriteNormal;
                // Si tiene sprite propio el color es blanco, si no tinte azul
                if (spriteRapida == null)
                    sr.color = new Color(0.5f, 0.8f, 1f);
                break;

            case Tipo.Resistente:
                if (spriteResistente != null)
                    sr.sprite = spriteResistente;
                else if (spriteNormal != null)
                    sr.sprite = spriteNormal;
                if (spriteResistente == null)
                    sr.color = new Color(1f, 0.5f, 0.1f);
                // Resistente es mas grande
                escalaBase = transform.localScale == Vector3.zero
                    ? escalaBase * 1.3f
                    : escalaBase * 1.3f;
                break;

            case Tipo.Dorada:
                if (spriteDorada != null)
                    sr.sprite = spriteDorada;
                else if (spriteNormal != null)
                    sr.sprite = spriteNormal;
                if (spriteDorada == null)
                    sr.color = new Color(1f, 0.85f, 0f);
                break;
        }
    }

    // ── SALE DEL HOYO ────────────────────────────────────────
    IEnumerator SalirDelHoyo()
    {
        Vector3 destino    = posHoyo + Vector3.up * 0.15f;
        Vector3 escalaFinal = tipo == Tipo.Resistente ? escalaBase * 1.3f : escalaBase;
        float dur = 0.35f, t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / dur);
            transform.localScale = escalaFinal * p;
            transform.position   = Vector3.Lerp(posHoyo, destino, p);
            yield return null;
        }

        transform.localScale = escalaFinal;
        transform.position   = destino;
        EstaViva             = true;
        PuedeSerGolpeada     = true;
        if (col != null) col.enabled = true;

        AudioManager.Instance?.SonarSalamandraAparece();
        StartCoroutine(Temporizador());
    }

    // ── CUENTA REGRESIVA ─────────────────────────────────────
    IEnumerator Temporizador()
    {
        while (tiempoRestante > 0f && EstaViva)
        {
            tiempoRestante -= Time.deltaTime;
            if (tiempoRestante < 1f)
            {
                float a = Mathf.PingPong(Time.time * 8f, 1f);
                Color c = colorBase; c.a = Mathf.Max(a, 0.2f); sr.color = c;
            }
            yield return null;
        }
        if (EstaViva) HuirAlTocarJugador();
    }

    // ── RECIBIR GOLPE ────────────────────────────────────────
    public void Morir()
    {
        if (!PuedeSerGolpeada || procesando) return;

        // Flash rojo en TODOS los tipos al recibir golpe
        StartCoroutine(FlashRojo(onComplete: () =>
        {
            // Resistente necesita 2 golpes
            if (tipo == Tipo.Resistente && golpes < 1)
            {
                golpes++;
                // No muere, solo flashea
                return;
            }

            // Muere
            procesando       = true;
            EstaViva         = false;
            PuedeSerGolpeada = false;
            if (col != null) col.enabled = false;

            StopAllCoroutines();
            AudioManager.Instance?.SonarMuerteSalamandra();
            GameManager.Instance?.SumarPuntos(ObtenerPuntos());
            GameManager.Instance?.ContarEnemigo();
            StartCoroutine(AnimMuerte());
        }));
    }

    IEnumerator FlashRojo(System.Action onComplete)
    {
        PuedeSerGolpeada = false;

        // Ponerse rojo
        sr.color = Color.red;
        yield return new WaitForSeconds(0.12f);

        // Volver al color original
        sr.color = colorBase;
        yield return new WaitForSeconds(0.05f);

        PuedeSerGolpeada = true;
        onComplete?.Invoke();
    }

    // ── HUIR ─────────────────────────────────────────────────
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

    // ── HELPERS ──────────────────────────────────────────────
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
