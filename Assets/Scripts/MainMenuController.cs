using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string      escenaJuego   = "Level_Sala";
    [SerializeField] private GameObject  panelOpciones;

    public void Jugar()          => SceneManager.LoadScene(escenaJuego);
    public void AbrirOpciones()  => panelOpciones?.SetActive(true);
    public void CerrarOpciones() => panelOpciones?.SetActive(false);
    public void Salir()          => Application.Quit();
}
