using TMPro;
using UnityEngine;

public class ControladorMeta : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winScreen;

    private void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag("Player"))
        {
            Win();
        }
    }


    private void Win()
    {
        winScreen.gameObject.SetActive(true);
        Debug.Log("¡Has ganado!");

        // Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}