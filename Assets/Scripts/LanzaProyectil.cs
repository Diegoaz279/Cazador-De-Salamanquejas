using UnityEngine;

public class LanzaProyectil : MonoBehaviour
{
    [SerializeField] private float radio = 0.5f;

    private Vector2 dir;
    private float   vel;
    private float   distMax;
    private Vector2 origen;
    private bool    activa = true;

    public void Configurar(Vector2 direccion, float velocidad, float distanciaMax)
    {
        dir     = direccion.normalized;
        vel     = velocidad;
        distMax = distanciaMax;
        origen  = transform.position;

        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angulo);
    }

    void Update()
    {
        if (!activa) return;

        transform.position += (Vector3)dir * vel * Time.deltaTime;

        if (Vector2.Distance(origen, transform.position) >= distMax)
        {
            Destruir(); return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radio);
        foreach (Collider2D h in hits)
        {
            if (h.CompareTag("Player")) continue;

            // Salamanqueja normal
            Salamanqueja sal = h.GetComponent<Salamanqueja>();
            if (sal != null && sal.EstaViva)
            {
                sal.Morir();
                Destruir();
                return;
            }

            // Hija del boss
            SalamanquejaHija hija = h.GetComponent<SalamanquejaHija>();
            if (hija != null && hija.EstaViva)
            {
                hija.Morir();
                Destruir();
                return;
            }

            // Final Boss
            FinalBoss boss = h.GetComponent<FinalBoss>();
            if (boss != null && boss.EstaVivo)
            {
                boss.Morir();
                Destruir();
                return;
            }
        }
    }

    void Destruir()
    {
        activa = false;
        Destroy(gameObject, 0.05f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}
