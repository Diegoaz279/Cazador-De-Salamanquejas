using UnityEngine;
using System.Collections;

// Pon este script en el prefab de las hijas del boss
// Es similar a Salamanqueja pero con mucha vida y sin tiempo limite
public class SalamanquejaHija : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite spriteHija1; // FinalBossHija1
    [SerializeField] private Sprite spriteHija2; // FinalBossHija2

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 8; // Necesita 8 golpes para morir

    [Header("Tiempo visible (larga)")]
    [SerializeField] private float tiempoVisible = 6f;

    // Estado publico
    public bool EstaViva         { get; private set; } = false;
    public bool PuedeSerGolpeada { get; private set; } = false;

    private SpriteRenderer sr;
    private Collider2D     col;
    private int            vidaActual;
    private bool           procesando = false;
    private Vector3        posHoyo;
    private Vector3        escalaBase;
    private int            indiceSprite = 0; // Para alternar sprites

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

        // Alternar entre los dos sprites de hija
        indiceSprite = Random.Range(0, 2);
        AsignarSprite();

        StartCoroutine(SalirDelHoyo());
    }

    void AsignarSprite()
    {
        Sprite s = indiceSprite == 0 ? spriteHija1 : spriteHija2;
        if (s != null) sr.sprite = s;
        sr.color = Color.white;
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
        float tiempo = tiempoVisible;
        while (tiempo > 0f && EstaViva)
        {
            tiempo -= Time.deltaTime;
            // Parpadeo al final
            if (tiempo < 1f)
            {
                float a = Mathf.PingPong(Time.time * 8f, 1f);
                Color c = sr.color; c.a = Mathf.Max(a, 0.2f); sr.color = c;
            }
            yield return null;
        }
        if (EstaViva) HuirAlHoyo();
    }

    // Llamado por LanzaProyectil
    public void Morir()
    {
        if (!PuedeSerGolpeada || procesando) return;

        vidaActual--;

        // Flash rojo en cada golpe
        StartCoroutine(FlashGolpe());

        if (vidaActual > 0) return; // Sigue viva

        // Murio
        procesando       = true;
        EstaViva         = false;
        PuedeSerGolpeada = false;
        if (col != null) col.enabled = false;

        StopAllCoroutines();
        AudioManager.Instance?.SonarMuerteSalamandra();

        // Avisar al GameManagerBoss
        GameManagerBoss.Instance?.HijaMuerta();

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
