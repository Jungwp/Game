using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorControior : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            print("abc");
            SceneManager.LoadScene("JumpMapScene");
        }
    }
}