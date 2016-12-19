using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using ParadoxNotion.Design;
using NodeCanvas;

public class PhysicsMoveCtrl : MonoBehaviour
{
    public new Rigidbody rigidbody;
    public float maxSpeed = 5f;
    public float maxForce = 10f;

    float maxTurnRate;

    Vector3 steeringForce;
    public Vector3 targetPos;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 200f);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Floor"))
                targetPos = hit.point;
            else
                targetPos = Vector3.zero;
        }

        if (rigidbody.velocity != Vector3.zero)
        {
            Quaternion rot = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rigidbody.velocity), 10f * Time.deltaTime);
            rot = Quaternion.Euler(new Vector3(0f, rot.eulerAngles.y, 0f));
            transform.rotation = rot;
        }

        Vector3 acceleration = steeringForce / rigidbody.mass;
        rigidbody.velocity += acceleration * Time.deltaTime;
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, maxSpeed);
    }

    public Status Seek()
    {
        Vector3 desiredVelocity = Vector3.Normalize(targetPos - transform.position) * maxForce;
        desiredVelocity = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
        steeringForce += desiredVelocity - rigidbody.velocity;
        Debug.Log("seek");
        return Status.Running;
    }

    public Status Arrive()
    {
        Vector3 toTarget = targetPos - transform.position;

        float dist = toTarget.magnitude;
        if (dist > 0)
        {
            //const float decelerationTweaker = 0.3f;
            float speed = dist * 4 / (float)3;// * decelerationTweaker;
            speed = Mathf.Min(speed, maxSpeed);

            Vector3 desiredVelocity = toTarget * speed / dist;
            desiredVelocity = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
            steeringForce += desiredVelocity - rigidbody.velocity;
        }
        return Status.Running;
    }

    public float wanderRadius = 4f;
    public float wanderDistance = 4f;
    public float wanderJitter = 1.5f;
    Vector3 wanderTarget;
    public Status Wander()
    {
        wanderTarget = Vector3.zero;
        wanderTarget += new Vector3(UnityEngine.Random.Range(-1f, 1f) * wanderJitter, 0f, UnityEngine.Random.Range(-1f, 1f) * wanderJitter);
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;
        Vector3 targetWorld = transform.position + (transform.forward * wanderDistance) + wanderTarget;

        //moveCtrl.SetWanderGizmos(targetWorld, transform.position + (transform.forward * wanderDistance), wanderRadius);

        steeringForce += (targetWorld - transform.position) * 2f;
        return Status.Running;
    }
    

    public Status ObstacleAvoidance(float minDetectionBoxLength)
    {
        Collider[] results = new Collider[30];
        Vector3 boxLength = new Vector3(2f, 2f, minDetectionBoxLength + (rigidbody.velocity.magnitude / maxSpeed) * minDetectionBoxLength);
        Physics.OverlapBoxNonAlloc(transform.position + (transform.forward * boxLength.z * 0.5f), boxLength, results, transform.localRotation);

        float distToClosestIP = 1000f;
        Collider closestIntersectingObstacle = null;
        Vector3 localPosOfCllosestObstacle = Vector3.zero;

        for (int i = 0; i < results.Length; i++)
        {
            if (results[i] != null)
            {
                if (results[i].CompareTag("Obstacle"))
                {
                    Vector3 localPos = transform.InverseTransformPoint(results[i].transform.position);
                    if (localPos.z >= 0)
                    {
                        float expandedRadius = 5;
                        if (Mathf.Abs(localPos.x) < expandedRadius)
                        {
                            float cz = localPos.z;
                            float cx = localPos.x;

                            float sqrtPart = (float)Math.Sqrt(expandedRadius * expandedRadius - cx * cx);
                            float ip = cz - sqrtPart;

                            if (ip <= 0f)
                            {
                                ip = cx + sqrtPart;
                            }

                            if (ip < distToClosestIP)
                            {
                                distToClosestIP = ip;
                                closestIntersectingObstacle = results[i];
                                localPosOfCllosestObstacle = localPos;
                            }
                        }
                    }
                }
            }
        }

        Vector3 steeringForce = Vector3.zero;
        if (closestIntersectingObstacle != null)
        {
            float multiplier = 0.5f + (boxLength.z - localPosOfCllosestObstacle.z) / boxLength.z;
            steeringForce.x = (5 - localPosOfCllosestObstacle.x) * multiplier;

            const float breakingWeight = 1f;
            steeringForce.z = (5f - localPosOfCllosestObstacle.z) * breakingWeight;
        }

        //moveCtrl.SetObstacleAvoidanceGizmos(boxLength);

        this.steeringForce += transform.TransformVector(steeringForce);

        return Status.Running;
    }

    public Status WallAvoidance()
    {
        Ray[] feels = new Ray[3];
        RaycastHit[] hit = new RaycastHit[3];
        for (int i = 0; i < feels.Length; i++)
        {
            feels[i].origin = transform.position;
        }
        feels[0].direction = transform.TransformDirection(new Vector3(1f, 0f, 1f));
        feels[1].direction = transform.TransformDirection(new Vector3(-1f, 0f, 1f));
        feels[2].direction = transform.TransformDirection(new Vector3(0f, 0f, 1f));

        for (int i = 0; i < hit.Length; i++)
        {
            Physics.Raycast(feels[i], out hit[i], 10f);
        }

        float distToThisIP = 0f;
        float distToClosestIP = 1000f * 1000f;

        Vector3 steeringForce = Vector3.zero
            , point = Vector3.zero
            , closestPoint = Vector3.zero;

        for (int i = 0; i < feels.Length; i++)
        {
            if (hit[i].collider != null)
            {
                if (hit[i].collider.CompareTag("Wall"))
                {
                    distToThisIP = (transform.position - hit[i].point).sqrMagnitude;
                    if (distToThisIP < distToClosestIP)
                    {
                        distToClosestIP = distToThisIP;
                        closestPoint = transform.InverseTransformPoint(hit[i].point);
                    }
                }

                Vector3 overShoot = feels[i].direction - closestPoint * 6f;
                steeringForce = hit[i].transform.forward * overShoot.magnitude;
            }
        }

        //moveCtrl.SetWallAvoidanceGizmos(feels[2], feels[0], feels[1]);

        Debug.Log(steeringForce);
        this.steeringForce += steeringForce;
        return Status.Running;
    }
}
