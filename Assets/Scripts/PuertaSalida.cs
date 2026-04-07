using UnityEngine;

public class PuertaSalida : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance?.PasarDeNivel();
    }
}
