using UnityEngine;

public class Turret : MonoBehaviour
{
    private GameObject target;
    private Enemy targetEnemy;

    [Header("Turret Stats")]

    public float range = 15f;
    public float fireRate = 1f;
    private float fireCountdown = 0f;

    [Header("Projectile Stats")]
    public int damage = 20;
    public float projectileSpeed = 20f;
    public float explosionRadius = 0f;

    [Header("Use Laser")]
    public bool useLaser = false;
    public int damageOverTime = 10;

    public GameObject slowPrefab;

    [Header("Laser VFX")]
    public LineRenderer lineRenderer;
    public ParticleSystem impactEffect;
    public Light impactLight;

    [Header("Unity Setup Fields")]
    public string enemyTag = "Enemy";

    public Transform partToRotate;
    public float rotationSpeed = 10f;

    public GameObject bulletPrefab;
    public Transform firePoint;

    private void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, 0.2f);       
    }

    private void Update()
    {
        if (target == null)
        {
            if (useLaser)
            {
                if (lineRenderer.enabled)
                {
                    lineRenderer.enabled = false;
                    impactEffect.Stop();
                    impactLight.enabled = false;
                }
            }
            
            return;
        }

        LockOnTarget();

        if (useLaser)
        {
            Laser();
        } else
        {
            Projectile();
        }

        fireCountdown -= Time.deltaTime;
    }

    private void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        float distanceToTarget = Mathf.Infinity;
        float shortestPath = Mathf.Infinity;

        if (target != null)
        {
            distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            if (distanceToTarget > range)
            {
                target = null;
            }
        }

        foreach (GameObject enemy in enemies)
        {
            EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
            float pathEnemy = enemyMovement.GetDistanceToGoal();
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy <= range && pathEnemy < shortestPath)
            {
                distanceToTarget = distanceToEnemy;
                shortestPath = pathEnemy;
                target = enemy;
                targetEnemy = enemy.GetComponent<Enemy>();
            }
        }
        slowPrefab.GetComponent<SlowDebuff>().target = targetEnemy;
    }

    private void LockOnTarget()
    {
        Vector3 dir = target.transform.position - transform.position; // find Vector from Turret to enemy
        Quaternion lookRotation = Quaternion.LookRotation(dir); // find Rotation needed to allign to dir
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * rotationSpeed).eulerAngles; // get eulerAngles
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f); // set rotation of turret
    }

    private void Laser()
    {
        targetEnemy.TakeDamage(damageOverTime * Time.deltaTime);
        Instantiate(slowPrefab);

        if (!lineRenderer.enabled)
        {
            lineRenderer.enabled = true;
            impactEffect.Play();
            impactLight.enabled = true;
        }

        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, target.transform.position);

        Vector3 dir = firePoint.position - target.transform.position;
        impactEffect.transform.rotation = Quaternion.LookRotation(dir);
        impactEffect.transform.position = target.transform.position + dir.normalized;
    }

    private void Projectile()
    {
        if (fireCountdown <= 0)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }
    }

    private void Shoot()
    {
        GameObject bulletGameObject = (GameObject) Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletGameObject.GetComponent<Bullet>();

        SetBullet(bullet);

        if (bullet != null)
        {
            bullet.Seek(target.transform);
        }
    }

    private void SetBullet(Bullet _bullet)
    {
        _bullet.SetDamage(damage);
        _bullet.SetProjectileSpeed(projectileSpeed);
        _bullet.SetExplosionRadius(explosionRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
