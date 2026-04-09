using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 5f;

    [Header("Limites del mapa")]
    [SerializeField] private float limiteIzq   = -7.5f;
    [SerializeField] private float limiteDer   =  7.5f;
    [SerializeField] private float limiteAbajo = -3.5f;
    [SerializeField] private float limiteArriba=  1.5f;

    [Header("Lanza")]
    [SerializeField] private GameObject prefabLanza;
    [SerializeField] private float velocidadLanza   = 15f;
    [SerializeField] private float distanciaMaxima  =  8f;
    [SerializeField] private float cooldown         =  0.3f;

    [Header("Lanzas limitadas")]
    [SerializeField] private int lanzasIniciales = 20;

    [Header("Sprites Personaje 1 (chancleta)")]
    [SerializeField] private Sprite p1Idle;
    [SerializeField] private Sprite p1Walk1;
    [SerializeField] private Sprite p1Walk2;
    [SerializeField] private Sprite p1Run1;
    [SerializeField] private Sprite p1Run2;
    [SerializeField] private Sprite p1AtaqueH;
    [SerializeField] private Sprite p1AtaqueV;

    [Header("Sprites Personaje 2 (camisa negra)")]
    [SerializeField] private Sprite p2Idle;
    [SerializeField] private Sprite p2Walk1;
    [SerializeField] private Sprite p2Walk2;
    [SerializeField] private Sprite p2Run1;
    [SerializeField] private Sprite p2Run2;
    [SerializeField] private Sprite p2AtaqueH;
    [SerializeField] private Sprite p2AtaqueV;

    [Header("Abuela")]
    [SerializeField] private Transform       abuela;
    [SerializeField] private float           offsetAbuela    = 0.6f;
    [SerializeField] private float           velocidadAbuela = 4f;
    [SerializeField] private Sprite          abuelaFrente;
    [SerializeField] private Sprite          abuelaEspalda;
    [SerializeField] private Sprite          abuelaDerecha;
    [SerializeField] private Sprite          abuelaIzquierda;

    // Privados
    private Rigidbody2D    rb;
    private SpriteRenderer sr;
    private SpriteRenderer srAbuela;
    private Camera         cam;

    private Vector2 movimiento;
    private Vector2 ultimaDireccion = Vector2.right;
    private Vector2 dirMouse        = Vector2.right;
    private bool    puedeDisparar   = true;
    private bool    atacando        = false;
    private float   timerAnim       = 0f;
    private int     frameAnim       = 0;
    private int     lanzasRestantes;
    private int     personajeActual = 0; // 0 = p1, 1 = p2

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        sr  = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        if (abuela != null) srAbuela = abuela.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Leer personaje seleccionado en el menu
        personajeActual = PlayerPrefs.GetInt("PersonajeSeleccionado", 0);
        lanzasRestantes = lanzasIniciales;
        GameManager.Instance?.ActualizarLanzas(lanzasRestantes);
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.JuegoActivo) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        movimiento = new Vector2(x, y).normalized;
        if (movimiento != Vector2.zero) ultimaDireccion = movimiento;

        ActualizarDireccionMouse();
        AnimarPersonaje();
        MoverAbuela();

        if (Input.GetMouseButtonDown(0) && puedeDisparar && !atacando)
            Disparar();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance == null || !GameManager.Instance.JuegoActivo) return;
        Vector2 nuevaPos = rb.position + movimiento * velocidad * Time.fixedDeltaTime;
        nuevaPos.x = Mathf.Clamp(nuevaPos.x, limiteIzq,    limiteDer);
        nuevaPos.y = Mathf.Clamp(nuevaPos.y, limiteAbajo,   limiteArriba);
        rb.MovePosition(nuevaPos);
    }

    void ActualizarDireccionMouse()
    {
        Vector3 mw = cam.ScreenToWorldPoint(Input.mousePosition);
        mw.z = 0f;
        Vector2 diff = (Vector2)(mw - transform.position);
        if (diff.magnitude > 0.1f) dirMouse = diff.normalized;
    }

    // Devuelve el sprite correcto segun personaje activo
    Sprite Sprite(Sprite s1, Sprite s2) => personajeActual == 0 ? s1 : s2;

    void AnimarPersonaje()
    {
        if (atacando) return;
        timerAnim += Time.deltaTime;
        if (timerAnim >= 0.15f) { timerAnim = 0f; frameAnim = 1 - frameAnim; }

        if (movimiento == Vector2.zero)
        {
            Sprite s = Sprite(p1Idle, p2Idle);
            if (s != null) sr.sprite = s;
        }
        else if (movimiento.magnitude > 0.7f)
        {
            Sprite s = frameAnim == 0 ? Sprite(p1Run1, p2Run1) : Sprite(p1Run2, p2Run2);
            if (s != null) sr.sprite = s;
        }
        else
        {
            Sprite s = frameAnim == 0 ? Sprite(p1Walk1, p2Walk1) : Sprite(p1Walk2, p2Walk2);
            if (s != null) sr.sprite = s;
        }

        if (movimiento.x > 0) sr.flipX = false;
        else if (movimiento.x < 0) sr.flipX = true;
    }

    void MoverAbuela()
    {
        if (abuela == null) return;
        Vector3 destino = movimiento == Vector2.zero
            ? transform.position + new Vector3(-0.5f, -0.6f, 0f)
            : transform.position + new Vector3(-movimiento.x * offsetAbuela,
                                               -movimiento.y * offsetAbuela * 0.5f, 0f);

        abuela.position = Vector3.MoveTowards(abuela.position, destino,
                                               velocidadAbuela * Time.deltaTime);
        if (srAbuela == null) return;

        if (movimiento == Vector2.zero)
        { if (abuelaFrente != null) srAbuela.sprite = abuelaFrente; }
        else if (Mathf.Abs(movimiento.x) > Mathf.Abs(movimiento.y))
        {
            if (abuelaDerecha != null) { srAbuela.sprite = abuelaDerecha; srAbuela.flipX = movimiento.x < 0; }
        }
        else
        {
            if (movimiento.y > 0) { if (abuelaEspalda  != null) srAbuela.sprite = abuelaEspalda; }
            else                  { if (abuelaFrente   != null) srAbuela.sprite = abuelaFrente;  }
        }
    }

    void Disparar()
    {
        if (lanzasRestantes <= 0) { GameManager.Instance?.SinLanzas(); return; }

        puedeDisparar   = false;
        atacando        = true;
        lanzasRestantes--;
        AudioManager.Instance?.SonarLanza();
        GameManager.Instance?.ActualizarLanzas(lanzasRestantes);

        bool horizontal = Mathf.Abs(dirMouse.x) >= Mathf.Abs(dirMouse.y);
        Sprite atk = horizontal ? Sprite(p1AtaqueH, p2AtaqueH) : Sprite(p1AtaqueV, p2AtaqueV);
        if (atk != null) sr.sprite = atk;
        if (dirMouse.x < 0) sr.flipX = true;
        else if (dirMouse.x > 0) sr.flipX = false;

        if (prefabLanza != null)
        {
            Vector3 pos = transform.position + (Vector3)(dirMouse * 0.5f);
            GameObject obj = Instantiate(prefabLanza, pos, Quaternion.identity);
            LanzaProyectil lp = obj.GetComponent<LanzaProyectil>();
            if (lp != null) lp.Configurar(dirMouse, velocidadLanza, distanciaMaxima);
        }

        Invoke(nameof(ResetAtaque), cooldown);
    }

    void ResetAtaque() { atacando = false; puedeDisparar = true; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemigo"))
        {
            Salamanqueja sal = other.GetComponent<Salamanqueja>();
            if (sal != null && sal.EstaViva)
            {
                GameManager.Instance?.PerderVida();
                AudioManager.Instance?.SonarPerderVida();
                sal.HuirAlTocarJugador();
            }
        }
    }
}
