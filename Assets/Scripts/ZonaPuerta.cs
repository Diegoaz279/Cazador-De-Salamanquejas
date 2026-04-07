using UnityEngine;

// Pon este script en el GameObject "ZonaPuerta"
// Ese objeto necesita un BoxCollider2D con Is Trigger activado
// Se activa solo cuando la puerta esta abierta (GameManager lo activa)
public class ZonaPuerta : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!GameManager.Instance.PuertaAbierta) return;

        // Iniciar fade a negro y cambio de escena
        TransicionNivel.Instance?.IniciarTransicion();
    }
}
