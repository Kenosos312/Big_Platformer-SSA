using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    [SerializeField] Sprite Button_Raw;
    [SerializeField] Sprite Button_Active;
    [SerializeField] SpriteRenderer SpRend;
    float ButtonResetTimer;

    [SerializeField] private int ButtonNumber;
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
    [SerializeReference] private GameObject Laser;
    private Rigidbody2D RigidbodyP1;
    private Rigidbody2D RigidbodyP2;

    [SerializeField] private Transform Layer1;
    [SerializeField] private Transform Layer2;
    [SerializeField] private Transform Layer3;
    [SerializeField] private Transform Layer4;
    [SerializeField] private Transform Layer5;
    [SerializeField] private Transform Layer6;

    private void Start() {
        RigidbodyP1 = Platform1.GetComponent<Rigidbody2D>();
        RigidbodyP2 = Platform2.GetComponent<Rigidbody2D>();
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
        StopPlatforms();
        TimeforDistance -= Time.deltaTime;
        ButtonResetTimer -= Time.deltaTime;
        if(ButtonResetTimer > -0.01 && ButtonResetTimer < 0.01) SpRend.sprite = Button_Raw;
    }

    private void CalculatingPlatformSpeed() {
        if(ButtonNumber == 6) ShutDownLaser();
        TimeforDistance = SetTimeforDistance;
        float distance1 = Vector3.Distance(StartPPlatform1.position, EndPPlatform1.position);
        travelSpeedPlatform1 = distance1 / SetTimeforDistance;

        if(StartPPlatform1.position.y > EndPPlatform1.position.y) 
            travelSpeedPlatform1 *= -1;

        float distance2 = Vector3.Distance(StartPPlatform2.position, EndPPlatform2.position);
        travelSpeedPlatform2 = distance2 / SetTimeforDistance;

        if(StartPPlatform2.position.y > EndPPlatform2.position.y)
            travelSpeedPlatform2 *= -1;

        MovingPlatforms();
    }
    //(30-36)/2
    private void MovingPlatforms() {
        RigidbodyP1.linearVelocityY = travelSpeedPlatform1;
        RigidbodyP2.linearVelocityY = travelSpeedPlatform2;
    }
    private void StopPlatforms() {
        SetPlatformYtoZero();

        if(TimeforDistance > -0.1 && TimeforDistance <= 0) {

            RigidbodyP1.linearVelocityY = 0;
            RigidbodyP2.linearVelocityY = 0;
        }
    }
    private void SetPlatformYtoZero() {
        Layer1.localPosition = Vector3.zero;
        Layer2.localPosition = Vector3.zero;
        Layer3.localPosition = Vector3.zero;
        Layer4.localPosition = Vector3.zero;
        Layer5.localPosition = Vector3.zero;
        Layer6.localPosition = Vector3.zero;

    }
    private void ShutDownLaser() {
        LaserBeam Laser_Script = Laser.GetComponent<LaserBeam>();
        if(ButtonNumber == 6)
            Laser_Script.laserMaxLength = 0;
        else
            Laser_Script.laserMaxLength = 50;
    }

}
