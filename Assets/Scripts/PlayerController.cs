using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 5f;

    [Header("Ataque")]
    [SerializeField] private GameObject prefabLanza;
    [SerializeField] private float velocidadLanza = 10f;
    [SerializeField] private float distanciaMaxima = 6f;
    [SerializeField] private float cooldown = 0.4f;

    [Header("Sprites")]
    [SerializeField] private Sprite spriteIdle;
    [SerializeField] private Sprite spriteWalk1;
    [SerializeField] private Sprite spriteWalk2;
    [SerializeField] private Sprite spriteRun1;
    [SerializeField] private Sprite spriteRun2;
    [SerializeField] private Sprite spriteAtaqueH;
    [SerializeField] private Sprite spriteAtaqueV;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 movimiento;
    private Vector2 ultimaDireccion = Vector2.right;
    private bool puedeDisparar = true;
    private bool atacando = false;
    private float timerAnim = 0f;
    private int frameAnim = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.JuegoActivo) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        movimiento = new Vector2(x, y).normalized;

        if (movimiento != Vector2.zero)
            ultimaDireccion = movimiento;

        AnimarPersonaje();

        if (Input.GetMouseButtonDown(0) && puedeDisparar && !atacando)
            Atacar();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance == null || !GameManager.Instance.JuegoActivo) return;
        rb.MovePosition(rb.position + movimiento * velocidad * Time.fixedDeltaTime);
    }

    void AnimarPersonaje()
    {
        if (atacando) return;

        timerAnim += Time.deltaTime;
        if (timerAnim >= 0.15f)
        {
            timerAnim = 0f;
            frameAnim = 1 - frameAnim;
        }

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

    void Atacar()
    {
        puedeDisparar = false;
        atacando = true;

        Vector2 dir = ObtenerDireccion();

        bool horizontal = Mathf.Abs(dir.x) >= Mathf.Abs(dir.y);
        Sprite atk = horizontal ? spriteAtaqueH : spriteAtaqueV;
        if (atk != null) sr.sprite = atk;

        if (dir.x < 0) sr.flipX = true;
        else if (dir.x > 0) sr.flipX = false;

        if (prefabLanza != null)
        {
            Vector3 pos = transform.position + (Vector3)(dir * 0.5f);
            GameObject obj = Instantiate(prefabLanza, pos, Quaternion.identity);
            LanzaProyectil lp = obj.GetComponent<LanzaProyectil>();
            if (lp != null) lp.Configurar(dir, velocidadLanza, distanciaMaxima);
        }

        Invoke(nameof(ResetAtaque), cooldown);
    }

    Vector2 ObtenerDireccion()
    {
        Vector2 d = movimiento != Vector2.zero ? movimiento : ultimaDireccion;
        if (Mathf.Abs(d.x) >= Mathf.Abs(d.y))
            return d.x >= 0 ? Vector2.right : Vector2.left;
        else
            return d.y >= 0 ? Vector2.up : Vector2.down;
    }

    void ResetAtaque()
    {
        atacando = false;
        puedeDisparar = true;
    }

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
