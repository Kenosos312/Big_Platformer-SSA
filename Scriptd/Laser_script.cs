using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    [SerializeField] private float laserMaxLength = 20f;
    [SerializeField] private float laserRadius = 0.1f; // "Dicke" des Lasers
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject ParticleObject;

    private void Update()
    {
        CastLaser();
    }

    private void CastLaser()
    {
        Vector2 laserOrigin = transform.position;
        Vector2 laserDirection = transform.right;

        // Benutze CircleCast statt Raycast
        RaycastHit2D hit = Physics2D.CircleCast(laserOrigin, laserRadius, laserDirection, laserMaxLength, collisionMask);

        Vector2 endPosition = hit.collider != null
            ? hit.point
            : laserOrigin + laserDirection * laserMaxLength;

        // LineRenderer aktualisieren
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, laserOrigin);
        lineRenderer.SetPosition(1, endPosition);
        ParticleObject.transform.position = endPosition;
    }
}
