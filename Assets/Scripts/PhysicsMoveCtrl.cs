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
        wander,
        obstacleAvoidance,
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
        if (isDoingBehavior[(int)eSteeringBehaviour.wander])
        {
            steeringForce += Wander();
        }
        if (isDoingBehavior[(int)eSteeringBehaviour.obstacleAvoidance])
        {
            steeringForce += ObstacleAvoidance();
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
        Vector3 desiredVelocity = Vector3.Normalize(targetPos - transform.position) * moveCtrl.maxSpeed;
        desiredVelocity = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
        return desiredVelocity - rigidbody.velocity;
    }

    Vector3 Flee(Vector3 targetPos)
    {
        Vector3 desiredVelocity = Vector3.Normalize(transform.position - targetPos) * moveCtrl.maxSpeed;
        desiredVelocity = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
        return desiredVelocity - rigidbody.velocity;
    }

    Vector3 Arrive(Vector3 targetPos, eDeceleration deceleration)
    {
        Vector3 toTarget = targetPos - transform.position;

        float dist = toTarget.magnitude;
        if(dist > 0)
        {
            //const float decelerationTweaker = 0.3f;
            float speed = dist*4 / (float)deceleration;// * decelerationTweaker;
            speed = Mathf.Min(speed, moveCtrl.maxSpeed);

            Vector3 desiredVelocity = toTarget * speed / dist;
            desiredVelocity = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
            return desiredVelocity - rigidbody.velocity;
        }
        return Vector3.zero;
    }


    public float wanderRadius = 3f;
    public float wanderDistance = 10f;
    public float wanderJitter = 1f;
    Vector3 wanderTarget;
    Vector3 Wander()
    {
        wanderTarget = transform.position;
        wanderTarget += new Vector3(UnityEngine.Random.Range(-1f, 1f) * wanderJitter, 0f, UnityEngine.Random.Range(-1f, 1f) * wanderJitter);
        wanderTarget.Normalize();

        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0f, 0f, wanderDistanced);
        Vector3 targetWorld = transform.InverseTransformVector(targetLocal);

        moveCtrl.SetWanderGizmos(targetWorld, wanderRadius);

        return targetWorld - transform.position;
    }

    float minDetectionBoxLength = 3f;
    Vector3 ObstacleAvoidance()
    {
        Collider[] results = null;
        Vector3 boxLength = new Vector3(0.5f, 0.5f, minDetectionBoxLength + rigidbody.velocity.magnitude / moveCtrl.maxSpeed);
        Physics.OverlapBoxNonAlloc(transform.position + transform.forward * 2f, boxLength, results, transform.localRotation, LayerMask.NameToLayer("Obstacle"));

        //임시
        return Vector3.zero;
    }

}

public class PhysicsMoveCtrl : MonoBehaviour {

    public new Rigidbody rigidbody;
    public float maxSpeed = 10f;
    public float maxForce = 10f;

    float maxTurnRate;

    [SerializeField]
    SteeringBehaviours steering;
    Vector3 steeringForce;

    public Vector3 targetPos;

	// Use this for initialization
	void Start () {
        rigidbody = GetComponent<Rigidbody>();
        steering = new SteeringBehaviours(this);
        StartCoroutine(AIUpdate());
	}
	
	void FixedUpdate () {

        for (int i = 0; i < steering.BehaviourCnt; i++)
        {
            steering.SetFlagBehaviour((SteeringBehaviours.eSteeringBehaviour)i, false);
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 100f);
        if(hit.collider != null)
        {
            if (hit.collider.CompareTag("Floor"))
                targetPos = hit.point;
            else
                targetPos = Vector3.zero;

            steering.SetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.arrive, true);
        }
        else
        {
            steering.SetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.wander, true);
        }

        //steeringForce = steering.Calculate(targetPos);

        Quaternion rot = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rigidbody.velocity), 10f * Time.deltaTime);
        rot = Quaternion.Euler(new Vector3(0f, rot.eulerAngles.y, 0f));
        transform.rotation = rot;

        Vector3 acceleration = steeringForce / rigidbody.mass;
        rigidbody.velocity += acceleration * Time.deltaTime;
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, maxSpeed);
	}

    IEnumerator AIUpdate()
    {
        yield return new WaitForSeconds(0.05f);
        steeringForce = steering.Calculate(targetPos);
        StartCoroutine(AIUpdate());
    }

    Vector3 debugWanderSphereCenter;
    float debugWanderSphereRadius;

    void OnDrawGizmos()
    {
        if(rigidbody != null)
        {
            Gizmos.DrawLine(transform.position, transform.position + rigidbody.velocity);

            if(steering.GetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.wander))
            {
                Gizmos.DrawWireSphere(debugWanderSphereCenter, debugWanderSphereRadius);
            }
            if(steering.GetFlagBehaviour(SteeringBehaviours.eSteeringBehaviour.obstacleAvoidance))
            {

            }
        }

    }

    public void SetWanderGizmos(Vector3 center, float radius)
    {
        debugWanderSphereCenter = center;
        debugWanderSphereRadius = radius;
    }
    


}

