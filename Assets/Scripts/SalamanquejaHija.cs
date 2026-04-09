using UnityEngine;
using System.Collections;

public class SalamanquejaHija : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite spriteHija1;
    [SerializeField] private Sprite spriteHija2;

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 8;

    [Header("Tiempo visible")]
    [SerializeField] private float tiempoVisible = 6f;

    public bool EstaViva         { get; private set; } = false;
    public bool PuedeSerGolpeada { get; private set; } = false;

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
    }

    void OnEnable()
    {
        procesando           = false;
        EstaViva             = false;
        PuedeSerGolpeada     = false;
        posHoyo              = transform.position;
        vidaActual           = vidaMaxima;
        transform.localScale = Vector3.zero;
        transform.rotation   = Quaternion.identity;
        if (col != null) col.enabled = false;

        // Alternar sprite
        Sprite s = Random.value > 0.5f ? spriteHija1 : spriteHija2;
        if (s != null) sr.sprite = s;
        sr.color = Color.white;

        StartCoroutine(SalirDelHoyo());
    }

    IEnumerator SalirDelHoyo()
    {
        Vector3 destino = posHoyo + Vector3.up * 0.2f;
        float dur = 0.4f, t = 0f;

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
        EstaViva             = true;
        PuedeSerGolpeada     = true;
        if (col != null) col.enabled = true;

        StartCoroutine(Temporizador());
    }

    IEnumerator Temporizador()
    {
        float t = tiempoVisible;
        while (t > 0f && EstaViva)
        {
            t -= Time.deltaTime;
            if (t < 1f)
            {
                float a = Mathf.PingPong(Time.time * 8f, 1f);
                Color c = sr.color; c.a = Mathf.Max(a, 0.2f); sr.color = c;
            }
            yield return null;
        }
        if (EstaViva) HuirAlHoyo();
    }

    public void Morir()
    {
        if (!PuedeSerGolpeada || procesando) return;

        vidaActual--;
        StartCoroutine(FlashGolpe());
        if (vidaActual > 0) return;

        procesando       = true;
        EstaViva         = false;
        PuedeSerGolpeada = false;
        if (col != null) col.enabled = false;

        StopAllCoroutines();
        AudioManager.Instance?.SonarMuerteSalamandra();

        // Usar GameManager normal para puntos
        GameManager.Instance?.SumarPuntos(25);

        // Avisar al SpawnerBoss
        SpawnerBoss.Instance?.NotificarHijaMuerta();

        StartCoroutine(AnimMuerte());
    }

    IEnumerator FlashGolpe()
    {
        PuedeSerGolpeada = false;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        if (!procesando) PuedeSerGolpeada = true;
    }

    public void HuirAlHoyo()
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
        float dur = 0.4f, t = 0f;
        Vector3 escIni = transform.localScale;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p = t / dur;
            transform.rotation   = Quaternion.Euler(0, 0, p * 360f);
            transform.localScale = escIni * (1f - p);
            Color c = sr.color; c.a = 1f - p; sr.color = c;
            yield return null;
        }
        transform.rotation = Quaternion.identity;
        gameObject.SetActive(false);
    }
}
