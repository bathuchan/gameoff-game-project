using UnityEngine;

public class TentaclePhysics : MonoBehaviour
{
    public Transform rootBone;           // Root bone of the tentacle
    public float springForce = 10f;      // Strength of the spring force applied to the bones
    public float distanceConstraint = 0.5f; // Minimum distance between bones
    public float maxDistance = 2f;       // Maximum distance between bones
    public float wobbleSpeed = 1f;       // Speed at which the tentacle wobbles
    public float wobbleAmount = 0.1f;    // Amount of wobble in the tentacle's movement

    [SerializeField]private Transform[] bones;           // Array to store bones
    [SerializeField] private Rigidbody[] boneRigidbodies; // Array to store rigidbodies of bones

    void Start()
    {
        // Get all the bones in the tentacle (children of the rootBone)
        bones = rootBone.GetComponentsInChildren<Transform>();
        boneRigidbodies = new Rigidbody[bones.Length];

        // Add a Rigidbody to each bone
        for (int i = 0; i < bones.Length; i++)
        {
            if (!bones[i].GetComponent<Rigidbody>())
            {
                Rigidbody rb = bones[i].gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = false; // No gravity for tentacle bones
                boneRigidbodies[i] = rb;
            }

            // Ensure each bone has a collider
            if (!bones[i].GetComponent<Collider>())
            {
                bones[i].gameObject.AddComponent<CapsuleCollider>(); // Or any other collider
            }
        }
    }

    void Update()
    {
        ApplyTentaclePhysics();
    }

    void ApplyTentaclePhysics()
    {
        // Iterate over each bone in the tentacle
        for (int i = 0; i < bones.Length - 1; i++)
        {
            Transform bone = bones[i];
            Transform nextBone = bones[i + 1];

            // Calculate the direction from the current bone to the next
            Vector3 direction = nextBone.position - bone.position;

            // Get the current distance between the bones
            float currentDistance = direction.magnitude;

            // If the bones are too far apart, apply a force to pull them closer
            if (currentDistance > maxDistance)
            {
                Vector3 forceDirection = direction.normalized * (currentDistance - maxDistance);
                boneRigidbodies[i].AddForce(forceDirection * springForce);
            }

            // If the bones are too close, apply a force to push them apart
            if (currentDistance < distanceConstraint)
            {
                Vector3 forceDirection = direction.normalized * (distanceConstraint - currentDistance);
                boneRigidbodies[i].AddForce(-forceDirection * springForce);
            }

            // Apply some wobble to the tentacle for a more organic movement
            Vector3 wobble = new Vector3(Mathf.PerlinNoise(Time.time * wobbleSpeed, i), Mathf.PerlinNoise(Time.time * wobbleSpeed, i), 0) * wobbleAmount;
            bone.position += wobble;
        }

        // Optional: Apply wobble to the root bone too, to keep the whole tentacle moving
        rootBone.position += new Vector3(Mathf.PerlinNoise(Time.time * wobbleSpeed, 0), Mathf.PerlinNoise(Time.time * wobbleSpeed, 0), 0) * wobbleAmount;
    }

    void OnTriggerStay(Collider other)
    {
        // Ensure that the other object is a bone collider (so it interacts with other bones)
        Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
        if (otherRigidbody != null)
        {
            // Calculate direction and apply force to separate bones if needed
            Vector3 direction = (other.transform.position - transform.position).normalized;
            boneRigidbodies[0].AddForce(direction * springForce);
        }
    }
}