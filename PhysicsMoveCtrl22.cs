using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

[Serializable]
class SteeringBehaviours
{
    enum eDeceleration
    {
        slow = 3,
        normal = 2,
        fast = 1
    }

    public enum eSteeringBehaviour
    {
        seek = 0,
        flee,
        arrive,
        pursuit,
        wander,
        obstacleAvoidance,
        wallAvoidance,
        EndOfBehaviour
    }

    bool[] isDoingBehavior;
    public
    int behaviourCnt;
    public int BehaviourCnt
    {
        get { return behaviourCnt; }
    }

    PhysicsMoveCtrl moveCtrl;
    Rigidbody rigidbody;
    Transform transform;

    public SteeringBehaviours(PhysicsMoveCtrl moveCtrl)
    {
        this.moveCtrl = moveCtrl;
        this.rigidbody = moveCtrl.rigidbody;
        this.transform = moveCtrl.transform;

        isDoingBehavior = new bool[(int)eSteeringBehaviour.EndOfBehaviour];

        for (int i = 0; i < isDoingBehavior.Length; i++)
        {
            isDoingBehavior[i] = false;
        }
        behaviourCnt = isDoingBehavior.Length;
    }

    public Vector3 Calculate(Vector3 targetPos)
    {
        Vector3 steeringForce = Vector3.zero;
        if (isDoingBehavior[(int)eSteeringBehaviour.seek])
        {
            steeringForce += Seek(targetPos);
        }
        if (isDoingBehavior[(int)eSteeringBehaviour.flee])
        {
            steeringForce += Flee(targetPos);
        }
        if (isDoingBehavior[(int)eSteeringBehaviour.arrive])
        {
            steeringForce += Arrive(targetPos, eDeceleration.fast);
        }
        if (isDoingBehavior[(int)eSteeringBehaviour.pursuit])
        {
            if(moveCtrl.nearUnit != null)
                steeringForce += Pursuit(moveCtrl.pursuitTarget);
        }
        if (isDoingBehavior[(int)eSteeringBehaviour.wander])
        {
            steeringForce += Wander();
        }
        if (isDoingBehavior[(int)eSteeringBehaviour.obstacleAvoidance])
        {
            steeringForce += ObstacleAvoidance();
        }
        if (isDoingBehavior[(int)eSteeringBehaviour.wallAvoidance])
        {
            steeringForce += WallAvoidance();
        }

        return steeringForce;
    }

    public void SetFlagBehaviour(eSteeringBehaviour behaviour, bool isFlagOn)
    {
        isDoingBehavior[(int)behaviour] = isFlagOn;
    }

    public bool GetFlagBehaviour(eSteeringBehaviour behaviour)
    {
        return isDoingBehavior[(int)behaviour];
    }

    Vector3 Seek(Vector3 targetPos)
    {
        Vector3 desiredVelocity = Vector3.Normalize(targetPos - transform.position) * moveCtrl.maxForce;
        desiredVelocity = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
        return desiredVelocity - rigidbody.velocity;
    }

    Vector3 Flee(Vector3 targetPos)
    {
        Vector3 desiredVelocity = Vector3.Normalize(transform.position - targetPos) * moveCtrl.maxForce;
        desiredVelocity = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
        return desiredVelocity - rigidbody.velocity;
    }

    Vector3 Arrive(Vector3 targetPos, eDeceleration deceleration)
    {
        Vector3 toTarget = targetPos - transform.position;

        float dist = toTarget.magnitude;
        if (dist > 0)
        {
            //const float decelerationTweaker = 0.3f;
            float speed = dist * 4 / (float)deceleration;// * decelerationTweaker;
            speed = Mathf.Min(speed, moveCtrl.maxSpeed);

            Vector3 desiredVelocity = toTarget * speed / dist;
            desiredVelocity = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
            return desiredVelocity - rigidbody.velocity;
        }
        return Vector3.zero;
    }

    Vector3 Pursuit(PhysicsMoveCtrl evader)
    {
        Vector3 toEvader = evader.transform.position - transform.position;

        //Debug.Log(toEvader);

        float relativeHeading = Vector3.Dot(transform.forward, evader.transform.position);
        if ((Vector3.Dot(toEvader, transform.forward) > 0f) && (relativeHeading < -0.95f))
        {
            Debug.Log("just seek");
            return Seek(evader.transform.position);
        }

        //Debug.Log(relativeHeading);

        float lookAheadTime = toEvader.magnitude / (moveCtrl.maxSpeed + evader.rigidbody.velocity.magnitude);
        return Seek(evader.transform.position + evader.rigidbody.velocity * lookAheadTime);
    }

    public float wanderRadius = 4f;
    public float wanderDistance = 4f;
    public float wanderJitter = 1.5f;
    Vector3 wanderTarget;
    Vector3 Wander()
    {
        //wanderTarget = transform.position;
        wanderTarget = Vector3.zero;
        wanderTarget += new Vector3(UnityEngine.Random.Range(-1f, 1f) * wanderJitter, 0f, UnityEngine.Random.Range(-1f, 1f) * wanderJitter);
        wanderTarget.Normalize();

        //Debug.Log("Normalize wanderTarget : " + wanderTarget);

        wanderTarget *= wanderRadius;

        //Debug.Log(wanderTarget);


        //Debug.Log("wanderTarget with Radius : " + wanderTarget);


        //Vector3 targetLocal = wanderTarget + new Vector3(wanderDistance, 0f, wanderDistance);
        Vector3 targetWorld = transform.position + (transform.forward * wanderDistance) + wanderTarget;

        moveCtrl.SetWanderGizmos(targetWorld, transform.position + (transform.forward * wanderDistance), wanderRadius);

        //Debug.Log(targetWorld - transform.position);

        return (targetWorld - transform.position) * 2f;
    }

    float minDetectionBoxLength = 3f;
    Vector3 ObstacleAvoidance()
    {
        Collider[] results = new Collider[30];
        Vector3 boxLength = new Vector3(2f, 2f, minDetectionBoxLength + (rigidbody.velocity.magnitude / moveCtrl.maxSpeed) * minDetectionBoxLength);
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
                        float expandedRadius = 10f;
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
            float multiplier = 1f + (boxLength.z - localPosOfCllosestObstacle.z) / boxLength.z;
            steeringForce.x = (15f - localPosOfCllosestObstacle.x) * multiplier;

            const float breakingWeight = 1f;
            steeringForce.z = (10f - localPosOfCllosestObstacle.z) * breakingWeight;
        }

        moveCtrl.SetObstacleAvoidanceGizmos(boxLength);

        return transform.TransformVector(steeringForce);
    }

    Vector3 WallAvoidance()
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
            Physics.Raycast(feels[i], out hit[i], 5f);
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

                Vector3 overShoot = feels[i].direction - closestPoint * 8f;
                steeringForce = hit[i].transform.forward * overShoot.magnitude;
            }
        }

        moveCtrl.SetWallAvoidanceGizmos(feels[2], feels[0], feels[1]);

        return steeringForce;
    }
}



public class PhysicsMoveCtrl : Unit
{

    public new Rigidbody rigidbody;
    public float maxSpeed = 10f;
    public float maxForce = 10f;

    float maxTurnRate;

    [SerializeField]
    SteeringBehaviours steering;
    Vector3 steeringForce;

    DetectionCtrl detectionCtrl;
    public Unit nearUnit;

    public Vector3 targetPos;

    public PhysicsMoveCtrl pursuitTarget;
    public bool isPursuit;

    // Use this for initialization
    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        steering = new SteeringBehaviours(this);
        detectionCtrl = GetComponent<DetectionCtrl>();
    }

    private void Start()
    {
        StartCoroutine(AIUpdate());
    }

    void FixedUpdate()
    {

        for (int i = 0; i < steering.BehaviourCnt; i++)
        {
            steering.SetFlagBehaviour((SteeringBehaviours.eSteeringBehaviour)i, false);
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 200f);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Floor"))
                targetPos = hit.point;
            else
                targetPos = Vector3.zero;

            steering.SetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.seek, true);
            steering.SetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.obstacleAvoidance, true);
            steering.SetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.wallAvoidance, true);
        }
        else
        {
            if (isPursuit)
            {
                steering.SetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.pursuit, true);
            }
            else
            {
                steering.SetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.wander, true);
                steering.SetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.obstacleAvoidance, true);
                steering.SetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.wallAvoidance, true);
            }
        }

        //steeringForce = steering.Calculate(targetPos);

        //Debug.Log("속도 : " + rigidbody.velocity);

        if(rigidbody.velocity != Vector3.zero)
        {
            Quaternion rot = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rigidbody.velocity), 10f * Time.deltaTime);
            rot = Quaternion.Euler(new Vector3(0f, rot.eulerAngles.y, 0f));
            transform.rotation = rot;
        }

        Vector3 acceleration = steeringForce / rigidbody.mass;
        rigidbody.velocity += acceleration * Time.deltaTime;
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, maxSpeed);
    }

    IEnumerator AIUpdate()
    {
        yield return new WaitForSeconds(0.08f);
        steeringForce = steering.Calculate(targetPos);
        StartCoroutine(AIUpdate());
    }

    public bool isOnDebugGizmos = false;
    void OnDrawGizmos()
    {
        if (isOnDebugGizmos)
        {
            if (rigidbody != null)
            {
                Gizmos.DrawLine(transform.position, transform.position + rigidbody.velocity);

                if (steering.GetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.wander))
                {
                    Gizmos.DrawWireSphere(debugWanderDesirePos, 0.5f);
                    Gizmos.DrawWireSphere(debugWanderSphereCenter, debugWanderSphereRadius);
                }
                if (steering.GetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.obstacleAvoidance))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(transform.position, transform.forward * debugObstacleAvoidanceBoxScale.z);
                }
                if (steering.GetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.wallAvoidance))
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, transform.position + debugWallAvoidanceFoward.direction * 5f);
                    Gizmos.DrawLine(transform.position, transform.position + debugWallAvoidanceRight.direction * 5f);
                    Gizmos.DrawLine(transform.position, transform.position + debugWallAvoidanceLeft.direction * 5f);
                }
            }
        }
    }

    Vector3 debugWanderDesirePos;
    Vector3 debugWanderSphereCenter;
    float debugWanderSphereRadius;
    public void SetWanderGizmos(Vector3 desirePos, Vector3 circleCenter, float radius)
    {
        debugWanderDesirePos = desirePos;
        debugWanderSphereCenter = circleCenter;
        debugWanderSphereRadius = radius;
    }

    Vector3 debugObstacleAvoidanceBoxScale;
    public void SetObstacleAvoidanceGizmos(Vector3 boxScale)
    {
        debugObstacleAvoidanceBoxScale = boxScale;
    }

    Ray debugWallAvoidanceFoward;
    Ray debugWallAvoidanceRight;
    Ray debugWallAvoidanceLeft;
    public void SetWallAvoidanceGizmos(Ray foward, Ray right, Ray left)
    {
        debugWallAvoidanceFoward = foward;
        debugWallAvoidanceRight = right;
        debugWallAvoidanceLeft = left;
    }

}
