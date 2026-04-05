using UnityEngine;

// Pon este script en el GameObject de la puerta
// La puerta necesita un Collider2D con Is Trigger activado
public class PuertaSalida : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance?.PasarDeNivel();
    }
}
