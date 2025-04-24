using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    [SerializeField] Sprite Button_Raw;
    [SerializeField] Sprite Button_Active;
    [SerializeField] SpriteRenderer SpRend;
    float ButtonResetTimer;


    private float TimeforDistance;
    private float travelSpeed;
    [SerializeReference] private GameObject Platform1;
    [SerializeReference] private GameObject Platform2;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            SpRend.sprite = Button_Active;
        }
    }
    private void OnTriggerStay2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            ButtonResetTimer = 0.6f;
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            ButtonResetTimer = 0.6f;
        }
    }

    private void Update() {
        ButtonResetTimer -= Time.deltaTime;
        if(ButtonResetTimer > -0.01 && ButtonResetTimer < 0.01) SpRend.sprite = Button_Raw;
    }
}
