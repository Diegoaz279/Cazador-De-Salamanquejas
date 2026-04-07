using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ── MOVIMIENTO ──────────────────────────────────────────
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 5f;

    [Header("Limites del mapa (ajusta segun tu escena)")]
    [SerializeField] private float limiteIzq  = -7.5f;
    [SerializeField] private float limiteDer  =  7.5f;
    [SerializeField] private float limiteAbajo = -3.5f;
    [SerializeField] private float limiteArriba =  1.5f;

    // ── LANZA ───────────────────────────────────────────────
    [Header("Lanza")]
    [SerializeField] private GameObject prefabLanza;
    [SerializeField] private float velocidadLanza  = 15f;
    [SerializeField] private float distanciaMaxima =  8f;
    [SerializeField] private float cooldown        =  0.3f;

    [Header("Lanzas limitadas")]
    [SerializeField] private int lanzasIniciales = 20;

    // ── SPRITES PERSONAJE ───────────────────────────────────
    [Header("Sprites del personaje")]
    [SerializeField] private Sprite spriteIdle;
    [SerializeField] private Sprite spriteWalk1;
    [SerializeField] private Sprite spriteWalk2;
    [SerializeField] private Sprite spriteRun1;
    [SerializeField] private Sprite spriteRun2;
    [SerializeField] private Sprite spriteAtaqueH;
    [SerializeField] private Sprite spriteAtaqueV;

    // ── ABUELA ──────────────────────────────────────────────
    [Header("Abuela (arrastra su GameObject aqui)")]
    [SerializeField] private Transform abuela;
    [SerializeField] private float offsetAbuela = -0.6f; // distancia detras del jugador
    [SerializeField] private float velocidadAbuela = 4f;

    [Header("Sprites de la abuela")]
    [SerializeField] private Sprite abuelaFrente;   // Abuela1
    [SerializeField] private Sprite abuelaEspalda;  // Abuela2
    [SerializeField] private Sprite abuelaDerecha;  // Abuela3
    [SerializeField] private Sprite abuelaIzquierda;// Abuela4

    // ── PRIVADOS ─────────────────────────────────────────────
    private Rigidbody2D  rb;
    private SpriteRenderer sr;
    private SpriteRenderer srAbuela;
    private Camera       cam;

    private Vector2 movimiento;
    private bool    puedeDisparar = true;
    private bool    atacando      = false;
    private float   timerAnim     = 0f;
    private int     frameAnim     = 0;
    private int     lanzasRestantes;

    // Ultima direccion del mouse respecto al jugador
    private Vector2 dirMouse = Vector2.right;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        sr  = GetComponent<SpriteRenderer>();
        cam = Camera.main;

        if (abuela != null)
            srAbuela = abuela.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        lanzasRestantes = lanzasIniciales;
        GameManager.Instance?.ActualizarLanzas(lanzasRestantes);
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.JuegoActivo) return;

        LeerMovimiento();
        ActualizarDireccionMouse();
        AnimarPersonaje();
        MoverAbuela();

        if (Input.GetMouseButtonDown(0) && puedeDisparar && !atacando)
            Disparar();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance == null || !GameManager.Instance.JuegoActivo) return;

        // Mover y limitar dentro del mapa
        Vector2 nuevaPos = rb.position + movimiento * velocidad * Time.fixedDeltaTime;
        nuevaPos.x = Mathf.Clamp(nuevaPos.x, limiteIzq,   limiteDer);
        nuevaPos.y = Mathf.Clamp(nuevaPos.y, limiteAbajo,  limiteArriba);
        rb.MovePosition(nuevaPos);
    }

    // ── INPUT ─────────────────────────────────────────────────
    void LeerMovimiento()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        movimiento = new Vector2(x, y).normalized;
    }

    // Calcula la direccion desde el jugador hasta donde esta el mouse en el mundo
    void ActualizarDireccionMouse()
    {
        Vector3 mouseMundo = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseMundo.z = 0f;
        Vector2 diff = (Vector2)(mouseMundo - transform.position);
        if (diff.magnitude > 0.1f)
            dirMouse = diff.normalized;
    }

    // ── ANIMACION PERSONAJE ───────────────────────────────────
    void AnimarPersonaje()
    {
        if (atacando) return;

        timerAnim += Time.deltaTime;
        if (timerAnim >= 0.15f) { timerAnim = 0f; frameAnim = 1 - frameAnim; }

        if (movimiento == Vector2.zero)
        {
            if (spriteIdle != null) sr.sprite = spriteIdle;
        }
        else if (movimiento.magnitude > 0.7f)
        {
            Sprite r = frameAnim == 0 ? spriteRun1 : spriteRun2;
            if (r != null) sr.sprite = r;
        }
        else
        {
            Sprite w = frameAnim == 0 ? spriteWalk1 : spriteWalk2;
            if (w != null) sr.sprite = w;
        }

        if (movimiento.x > 0) sr.flipX = false;
        else if (movimiento.x < 0) sr.flipX = true;
    }

    // ── ABUELA ───────────────────────────────────────────────
    void MoverAbuela()
    {
        if (abuela == null) return;

        // La abuela sigue al jugador con un poco de retraso
        Vector3 destino = transform.position + new Vector3(-movimiento.x * offsetAbuela,
                                                            -movimiento.y * offsetAbuela * 0.5f, 0f);
        // Si esta quieto, la abuela se queda separada detras y a la izquierda del jugador
        if (movimiento == Vector2.zero)
            destino = transform.position + new Vector3(-0.5f, -0.6f, 0f);

        abuela.position = Vector3.MoveTowards(abuela.position, destino,
                                               velocidadAbuela * Time.deltaTime);

        // Animar sprite de la abuela segun direccion del jugador
        if (srAbuela == null) return;

        if (movimiento == Vector2.zero)
        {
            if (abuelaFrente != null) srAbuela.sprite = abuelaFrente;
        }
        else if (Mathf.Abs(movimiento.x) > Mathf.Abs(movimiento.y))
        {
            // Movimiento horizontal
            if (movimiento.x > 0)
            {
                if (abuelaDerecha != null) { srAbuela.sprite = abuelaDerecha; srAbuela.flipX = false; }
            }
            else
            {
                if (abuelaDerecha != null) { srAbuela.sprite = abuelaDerecha; srAbuela.flipX = true; }
            }
        }
        else
        {
            // Movimiento vertical
            if (movimiento.y > 0)
            {
                if (abuelaEspalda != null) srAbuela.sprite = abuelaEspalda;
            }
            else
            {
                if (abuelaFrente != null) srAbuela.sprite = abuelaFrente;
            }
        }
    }

    // ── DISPARO HACIA EL MOUSE ────────────────────────────────
    void Disparar()
    {
        if (lanzasRestantes <= 0)
        {
            GameManager.Instance?.SinLanzas();
            return;
        }

        puedeDisparar    = false;
        atacando         = true;
        lanzasRestantes--;
        GameManager.Instance?.ActualizarLanzas(lanzasRestantes);

        // Sprite de ataque segun la direccion del mouse
        bool horizontal = Mathf.Abs(dirMouse.x) >= Mathf.Abs(dirMouse.y);
        Sprite atk = horizontal ? spriteAtaqueH : spriteAtaqueV;
        if (atk != null) sr.sprite = atk;

        if (dirMouse.x < 0) sr.flipX = true;
        else if (dirMouse.x > 0) sr.flipX = false;

        // Crear la lanza apuntando hacia el mouse
        if (prefabLanza != null)
        {
            Vector3 pos = transform.position + (Vector3)(dirMouse * 0.5f);
            GameObject obj = Instantiate(prefabLanza, pos, Quaternion.identity);
            LanzaProyectil lp = obj.GetComponent<LanzaProyectil>();
            if (lp != null) lp.Configurar(dirMouse, velocidadLanza, distanciaMaxima);
        }

        Invoke(nameof(ResetAtaque), cooldown);
    }

    void ResetAtaque()
    {
        atacando      = false;
        puedeDisparar = true;
    }

    // ── COLISION CON SALAMANQUEJA ─────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemigo"))
        {
            Salamanqueja sal = other.GetComponent<Salamanqueja>();
            if (sal != null && sal.EstaViva)
            {
                GameManager.Instance?.PerderVida();
                sal.HuirAlTocarJugador();
            }
        }
    }
}
