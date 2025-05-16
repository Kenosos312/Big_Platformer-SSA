using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    [SerializeField] public float laserMaxLength;
    [SerializeField] private float laserRadius = 0.3f; // "Dicke" des Lasers
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private LayerMask PlayerLayer;

    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject ParticleObject;
    [SerializeField] private Transform LineStart1;
    [SerializeField] private Transform LineEnd2;
    [SerializeField] private float SetTimeforDistance;
    
    private Vector2 laserOrigin;
    private Vector2 laserDirection;
    private float TimeforDistance;
    private float travelSpeed;
    [SerializeReference] private Rigidbody2D rb;
    private RaycastHit2D hit;
    
    
    private void Start()
    {
        
        transform.localPosition = new Vector3(LineStart1.localPosition.x,transform.localPosition.y,transform.localPosition.z);   
        travelSpeed = (LineStart1.localPosition.x * -1 + LineEnd2.localPosition.x) / SetTimeforDistance;
        TimeforDistance = SetTimeforDistance;
    }


    private void Update()
    {
        CastLaser();
        MoveLaserMaker();
    }

    private void CastLaser()
    {
        laserOrigin = transform.position;
        laserOrigin.y -= 0.65f;
        laserDirection = transform.right;

        // Benutze CircleCast
        RaycastHit2D hit = Physics2D.CircleCast(laserOrigin, laserRadius, laserDirection, laserMaxLength, collisionMask);

        Vector2 endPosition = hit.collider != null
            ? hit.point
            : laserOrigin + laserDirection * laserMaxLength;

        // LineRenderer aktualisieren
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, laserOrigin);
        lineRenderer.SetPosition(1, endPosition);
        ParticleObject.transform.position = endPosition;
        DetectPlayer();
    }

    private void DetectPlayer(){
        RaycastHit2D hit = Physics2D.CircleCast(laserOrigin, laserRadius, laserDirection, laserMaxLength, PlayerLayer);
        if (hit.collider != null && hit.collider.CompareTag("Player")){
            PlayerMovement PlayerScript = hit.collider.gameObject.GetComponent<PlayerMovement>();
            if(PlayerScript.Dying == false) {
                PlayerScript.Dying = true;
                PlayerScript.Died = true;
            }
        }


    }

    private void MoveLaserMaker(){
        TimeforDistance -= Time.deltaTime;
        
        rb.linearVelocityX = travelSpeed;
        if(TimeforDistance <= 0){
            TimeforDistance = SetTimeforDistance;
            travelSpeed *= -1;
            
        }
    }
}
