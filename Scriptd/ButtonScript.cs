using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    [SerializeField] Sprite Button_Raw;
    [SerializeField] Sprite Button_Active;
    [SerializeField] SpriteRenderer SpRend;
    float ButtonResetTimer;

    [SerializeField] private Transform StartPPlatform1;
    [SerializeField] private Transform StartPPlatform2;
    [SerializeField] private Transform EndPPlatform1;
    [SerializeField] private Transform EndPPlatform2;
    [SerializeField] private float SetTimeforDistance;
    private float TimeforDistance;
    private float travelSpeedPlatform1;
    private float travelSpeedPlatform2;
    [SerializeReference] private GameObject Platform1;
    [SerializeReference] private GameObject Platform2;
    private Rigidbody2D Rigidbody1;
    private Rigidbody2D Rigidbody2;

    private void Start() {
        Rigidbody1 = Platform1.GetComponent<Rigidbody2D>();
        Rigidbody1 = Platform1.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            SpRend.sprite = Button_Active;
            if(TimeforDistance < 0) CalculatingPlatformSpeed();
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

    private void CalculatingPlatformSpeed() {

        MovingPlatforms();
    }
    private void MovingPlatforms() {
    
    }
}
