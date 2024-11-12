using UnityEngine;
using System.Collections;

public class TentacleMovement : MonoBehaviour
{
    public float baseSpeed = 0.1f;
    public float baseXAmplitude = 0.5f;
    public float baseYAmplitude = 0.5f;
    public float baseZAmplitude = 0.5f;
    public float lerpSpeed = 2.0f;

    public Transform player;
    public float attractDistance = 5f;
    public float stopDistance = 1f;
    
    public float idleWobbleAmount = 0.2f;
    public float idleWobbleSpeed = 0.5f;
    public float maxIdleDriftRadius = 0.3f;

    private Transform tipPosition;
    private Transform rootPosition;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float angle;

    private float speed;
    private float xAmplitude;
    private float yAmplitude;
    private float zAmplitude;

    private float maxDistance;
    private float idleNoiseOffset;

    private Coroutine currentCoroutine;

    void Start()
    {
        rootPosition = transform.GetChild(0).GetChild(0);
        tipPosition = transform.GetChild(0).GetChild(1);

        startPosition = tipPosition.position;

        speed = Random.Range(baseSpeed * 0.5f, baseSpeed * 1.5f);
        xAmplitude = Random.Range(baseXAmplitude * 0.2f, baseXAmplitude * 1.2f);
        yAmplitude = Random.Range(baseYAmplitude * 0.2f, baseYAmplitude * 1.2f);
        zAmplitude = Random.Range(baseZAmplitude * 0.2f, baseZAmplitude * 1.2f);

        angle = Random.Range(0f, Mathf.PI * 2);
        maxDistance = Vector3.Distance(startPosition, rootPosition.position);

        idleNoiseOffset = Random.Range(0f, 100f);
        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentCoroutine = StartCoroutine(IdleMotion());
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(tipPosition.position, player.position);

        // Switch to player attraction if player is close
        if (distanceToPlayer < attractDistance)
        {
            if (currentCoroutine != null)
                StopAllCoroutines();

            currentCoroutine = StartCoroutine(AttractToPlayer());
        }
        else if (currentCoroutine == null || currentCoroutine != StartCoroutine(IdleMotion()))
        {
            // If the player is far, ensure only one IdleMotion coroutine runs
            if (currentCoroutine != null)
                StopAllCoroutines();

            currentCoroutine = StartCoroutine(IdleMotion());
        }
    }

    private IEnumerator IdleMotion()
    {
        float randomIdleSpeedOffset = Random.Range(1f, 1.2f);      // Random speed variation for idle wobble
        float randomIdleAmplitude = Random.Range(1f, 5f);        // Random amplitude for idle wobble
        float idleNoiseOffsetX = Random.Range(0f, 100f);             // Unique noise offset for each axis
        float idleNoiseOffsetY = Random.Range(100f, 200f);
        float idleNoiseOffsetZ = Random.Range(200f, 300f);

        while (true)
        {
            angle += speed * Time.deltaTime;

            // Apply Perlin noise-based idle wobbling with unique offsets
            float driftX = (Mathf.PerlinNoise(idleNoiseOffsetX, Time.time * randomIdleSpeedOffset) * 2f - 1f) * randomIdleAmplitude;
            float driftY = (Mathf.PerlinNoise(idleNoiseOffsetY, Time.time * randomIdleSpeedOffset) * 2f - 1f) * randomIdleAmplitude;
            float driftZ = (Mathf.PerlinNoise(idleNoiseOffsetZ, Time.time * randomIdleSpeedOffset) * 2f - 1f) * randomIdleAmplitude;

            Vector3 idleDrift = new Vector3(driftX, driftY, driftZ);

            // Regular idle wobble with random drift
            float x = Mathf.Cos(angle) * xAmplitude * idleWobbleAmount + idleDrift.x;
            float y = Mathf.Sin(angle) * yAmplitude * idleWobbleAmount + idleDrift.y;
            float z = Mathf.Sin(angle * 0.5f) * zAmplitude * idleWobbleAmount + idleDrift.z;

            targetPosition = startPosition + new Vector3(x, y, z);

            // Max distance constraint with smoothing to avoid snapping
            Vector3 directionToTarget = targetPosition - rootPosition.position;
            if (directionToTarget.magnitude > maxDistance)
            {
                directionToTarget = directionToTarget.normalized * Random.Range(maxDistance * 0.3f, maxDistance);
                targetPosition = Vector3.Lerp(tipPosition.position, rootPosition.position + directionToTarget, Time.deltaTime * lerpSpeed);
            }

            // Smoothly move the tip position towards the target
            tipPosition.position = Vector3.Lerp(tipPosition.position, targetPosition, Time.deltaTime * lerpSpeed);
            tipPosition.rotation = Quaternion.Slerp(tipPosition.rotation, Quaternion.identity, Time.deltaTime * lerpSpeed);

            yield return new WaitForFixedUpdate();
        }
    }


    private IEnumerator AttractToPlayer()
    {
        float randomWobbleSpeedOffset = Random.Range(0.8f, 1.2f);  // Random speed variation for wobble
        float randomWobbleAmplitude = Random.Range(0.2f, 0.4f);    // Random amplitude for wobble
        float noiseOffsetX = Random.Range(0f, 100f);               // Unique noise offset for each axis
        float noiseOffsetY = Random.Range(100f, 200f);
        float noiseOffsetZ = Random.Range(200f, 300f);

        while (Vector3.Distance(tipPosition.position, player.position) < attractDistance)
        {
            Vector3 directionToPlayer = (player.position - tipPosition.position).normalized;

            // Apply a randomized wobbling effect around the player using Perlin noise
            float wobbleX = (Mathf.PerlinNoise(noiseOffsetX, Time.time * randomWobbleSpeedOffset) * 2f - 1f) * randomWobbleAmplitude;
            float wobbleY = (Mathf.PerlinNoise(noiseOffsetY, Time.time * randomWobbleSpeedOffset) * 2f - 1f) * randomWobbleAmplitude;
            float wobbleZ = (Mathf.PerlinNoise(noiseOffsetZ, Time.time * randomWobbleSpeedOffset) * 2f - 1f) * randomWobbleAmplitude;

            Vector3 wobbleOffset = new Vector3(wobbleX, wobbleY, wobbleZ);
            targetPosition = player.position - directionToPlayer * stopDistance + wobbleOffset;

            // Max distance constraint with smoothing to avoid snapping
            Vector3 directionToTarget = targetPosition - rootPosition.position;
            if (directionToTarget.magnitude > maxDistance)
            {
                directionToTarget = directionToTarget.normalized * Random.Range(maxDistance * 0.3f, maxDistance);
                targetPosition = Vector3.Lerp(tipPosition.position, rootPosition.position + directionToTarget, Time.deltaTime * lerpSpeed);
            }

            // Smoothly move the tip position towards the target
            tipPosition.position = Vector3.Lerp(tipPosition.position, targetPosition, Time.deltaTime * lerpSpeed);

            // Smoothly rotate towards the player
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            tipPosition.rotation = Quaternion.Slerp(tipPosition.rotation, targetRotation, Time.deltaTime * lerpSpeed);

            yield return null;
        }

        currentCoroutine = StartCoroutine(IdleMotion()); // Return to idle motion once player is far
    }

}
