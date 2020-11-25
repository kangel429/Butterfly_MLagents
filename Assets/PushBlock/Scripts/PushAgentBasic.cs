using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class PushAgentBasic : Agent
{
    /// <summary>
    /// The ground. The bounds are used to spawn the elements.
    /// </summary>
    ///
    public GameObject ground;
    public GameObject area;

    /// <summary>
    /// The area bounds.
    /// </summary>
    [HideInInspector]
    public Bounds areaBounds;

    PushBlockSettings m_PushBlockSettings;

    /// <summary>
    /// The goal to push the block to.
    /// </summary>
    public GameObject goal;

    /// <summary>
    /// The block to be pushed to the goal.
    /// </summary>
    public GameObject block;

    /// <summary>
    /// Detects when the block touches the goal.
    /// </summary>
    [HideInInspector]
    public GoalDetect goalDetect;

    public bool useVectorObs;

    Rigidbody m_BlockRb;  //cached on initialization
    Rigidbody m_AgentRb;  //cached on initialization
    Material m_GroundMaterial; //cached on Awake()

    Vector3 vecLeftDistance; // added code

    float leftDistance;
    bool check_front;
    bool check_crush;
    float direction;

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>
    Renderer m_GroundRenderer;

    EnvironmentParameters m_ResetParams;

    public Animator butterflyAnim;

    List<GameObject> mUserList = new List<GameObject>();

    void Awake() // Untouched.
    {
        
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();
    }
    private void Start()
    {
        mUserList = area.GetComponent<NabiManager>().mUserList;
    }

    public override void Initialize() // Untouched.
    {
        goalDetect = block.GetComponent<GoalDetect>();
        goalDetect.agent = this;

        // Cache the agent rigidbody
        m_AgentRb = GetComponent<Rigidbody>();
        // Cache the block rigidbody
        m_BlockRb = block.GetComponent<Rigidbody>();
        // Get the ground's bounds
        areaBounds = ground.GetComponent<Collider>().bounds;
        // Get the ground renderer so we can change the material when a goal is scored
        m_GroundRenderer = ground.GetComponent<Renderer>();
        // Starting material
        m_GroundMaterial = m_GroundRenderer.material;

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        SetResetParameters();
    }
    public override void CollectObservations(VectorSensor sensor) // TODO : Understand this part. 
    {
        direction = AngleDir();

        check_front = AngleDir_Front(transform.right, vecLeftDistance, transform.up);

        sensor.AddObservation((transform.position - goal.transform.position).normalized);//3
        sensor.AddObservation(vecLeftDistance.normalized);//3
        sensor.AddObservation(direction / 2);//1   -
        sensor.AddObservation(check_front);//1
    }

    /// <summary>
    /// Use the ground's bounds to pick a random spawn position.
    /// </summary>
    public Vector3 GetRandomSpawnPos() // Untouched.
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            var randomPosX = Random.Range(-areaBounds.extents.x * m_PushBlockSettings.spawnAreaMarginMultiplier,
                areaBounds.extents.x * m_PushBlockSettings.spawnAreaMarginMultiplier);
            var randomPosY = Random.Range(0.01f, 5f);
            var randomPosZ = Random.Range(-areaBounds.extents.z * m_PushBlockSettings.spawnAreaMarginMultiplier,
                areaBounds.extents.z * m_PushBlockSettings.spawnAreaMarginMultiplier);
            randomSpawnPos = ground.transform.position + new Vector3(randomPosX, randomPosY, randomPosZ);
            if (Physics.CheckBox(randomSpawnPos, new Vector3(2.5f, 0.01f, 2.5f)) == false)
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }

    /// <summary>
    /// Called when the agent moves the block into the goal.
    /// </summary>
    public void ScoredAGoal() // Untouched.
    {
        // We use a reward of 5.
        AddReward(5f);

        // By marking an agent as done AgentReset() will be called automatically.
        EndEpisode();

        // Swap ground material for a bit to indicate we scored.
        StartCoroutine(GoalScoredSwapGroundMaterial(m_PushBlockSettings.goalScoredMaterial, 0.5f));
    }

    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time) // Untouched.
    {
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        m_GroundRenderer.material = m_GroundMaterial;
    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(ActionSegment<int> act) // Lots of fixes. 
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];

        leftDistance = Vector3.Distance(block.transform.position, transform.position); // added code
        vecLeftDistance = block.transform.position - transform.position; // added code

        switch (action)
        {
            case 1: // Move forward.
                dirToGo = transform.forward * 10f * Time.deltaTime; 
                break;

            case 3: // Rotate on +Y axis. 
                rotateDir = new Vector3(0, 0.01f, 0);
                break;
            case 4: // Rotate on -Y axis.
                rotateDir = new Vector3(0, -0.01f, 0);
                break;
            case 5: // Rotate on -X axis.
                rotateDir = new Vector3(-0.01f, 0, 0);
                break;
            case 6: // Rotate on +X axis. 
                rotateDir = new Vector3(0.01f, 0, 0);
                break;

        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        m_AgentRb.AddForce(dirToGo * m_PushBlockSettings.agentRunSpeed, ForceMode.VelocityChange);


        if(block.activeSelf == true)
        {
            if (leftDistance < 3)  
            {
                Debug.Log("Gain reward");
                AnimationStop();

                AddReward(0.005f);
            }
            else if (leftDistance >= 3)
            {
                AnimationStart();
            }
            else if (leftDistance > 80)
            {

                Debug.Log("Too far away, so it will be reset");
                SetReward(-1f);
                EndEpisode();


            }


        }

    }

    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public override void OnActionReceived(ActionBuffers actionBuffers) // Untouched.

    {
        // Move the agent using the action.
        MoveAgent(actionBuffers.DiscreteActions);

        // Penalty given each step to encourage agent to finish task quickly.
        AddReward(-1f / MaxStep);
    }


    private float TimeLeft = 0.1f;
    private float nextTime = 0.0f;


    public override void Heuristic(in ActionBuffers actionsOut) 
    {
        direction = AngleDir(); // added code
        check_front = AngleDir_Front(transform.right, vecLeftDistance, transform.up); // added code

        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;



        if (block.activeSelf == true && leftDistance < 40)
        {
            if (direction == -1)
            {
                //Debug.Log("right");
                discreteActionsOut[0] = 3;
            }
            else if (direction == 1)
            {
                //Debug.Log("left");
                discreteActionsOut[0] = 4;
            }
            else if (direction == 0 && (leftDistance > 1))
            {
                //Debug.Log("go");
                discreteActionsOut[0] = 1;
            }
            else if (direction == 2)
            {
                //Debug.Log("up");
                discreteActionsOut[0] = 5;
            }
            else if (direction == -2)
            {
                //Debug.Log("down");
                discreteActionsOut[0] = 6;
            }
        }
        else if (block.activeSelf == false || leftDistance >= 40)
        {

            if (Time.time > nextTime)
            {
                Debug.Log("Auto matic move");
                nextTime = Time.time + TimeLeft;
                discreteActionsOut[0] = (int)Random.Range(3, 4);
                if(block.activeSelf == false)
                {
                    AnimationStart();
                }
            }
            else
            {
                discreteActionsOut[0] = 1;
            }
        }




    }






    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("agent"))
        {
            Debug.Log("Crush other Agent");
            check_crush = true;
            resetCrush();
            AddReward(-1f);
        }
    }
    void resetCrush()
    {
        if (check_crush)
        {
            Debug.Log("resetCrush");
            check_crush = false;
        }
    }
    void OnTriggerStay(Collider col) // new part : if 
    {
        // Touched goal.
        if (col.gameObject.CompareTag("goal"))
        {
            if(leftDistance < 3.1f)
            {
                ScoredAGoal();
                Debug.Log("Goal in");
            }
            else
            {
                Debug.Log("Failed - alone Goal in   "   + +leftDistance);
                SetReward(-1f);
                EndEpisode();
            }
        }
    }

    public int AngleDir() // new part 
    {

        Vector3 perp = Vector3.Cross(transform.forward * -1, vecLeftDistance);
        float dir = Vector3.Dot(perp, transform.up);


        Vector3 perp1 = Vector3.Cross(transform.forward * -1, vecLeftDistance);
        float dir1 = Vector3.Dot(perp1, transform.right);
        float frontDegree = leftDistance * 0.25f;


        if (check_front)
        {
            if (dir > frontDegree)
                return 1;
            else if (dir < frontDegree * -1f)
                return -1;
            else if (dir < frontDegree && dir > frontDegree * -1f)
            {
                //return 0;
                if (dir1 > (frontDegree * 1f) + 1f)
                {
                    return 2;
                }
                else if (dir1 < (frontDegree * -1f) - 1f)
                {
                    return -2;
                }
                else if (dir1 < (frontDegree * 1f) + 1f && dir1 > (frontDegree * -1f) - 1f)
                {
                    return 0;

                }
            } 
        }
        else 
        {
            if (dir > 0)
                return 1;
            else 
                return -1;    
        }
        return 0;
    }

    public bool AngleDir_Front(Vector3 fwd, Vector3 targetDir, Vector3 up) // new part
    {
        Vector3 perp = Vector3.Cross(fwd * -1, targetDir);
        float dir = Vector3.Dot(perp, up);
        //Debug.Log(dir);
        if (dir > 0f)
            return true;
        else 
            return false;
    }
    /// <summary>
    /// Resets the block position and velocities.
    /// </summary>
    void ResetBlock() // 
    {
        // Get a random position for the block.
        block.transform.position = GetRandomSpawnPos();
        block.transform.localRotation = Quaternion.Euler(Vector3.zero); // added code

        // Reset block velocity back to zero.
        m_BlockRb.velocity = Vector3.zero;

        // Reset block angularVelocity back to zero.
        m_BlockRb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// In the editor, if "Reset On Done" is checked then AgentReset() will be
    /// called automatically anytime we mark done = true in an agent script.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;
        area.transform.Rotate(new Vector3(0f, rotationAngle, 0f));

        ResetBlock();
        transform.position = GetRandomSpawnPos();
        transform.localRotation = Quaternion.Euler(Vector3.zero); // added code
        m_AgentRb.velocity = Vector3.zero;
        m_AgentRb.angularVelocity = Vector3.zero;
        
        SetResetParameters();

        for(int i=0; i< mUserList.Count; i++)
        {
            GameObject nabi = mUserList[i];
            nabi.transform.position = GetRandomSpawnPos();
        }
    }

    public void SetGroundMaterialFriction() // Untouched. 
    {
        var groundCollider = ground.GetComponent<Collider>();

        groundCollider.material.dynamicFriction = m_ResetParams.GetWithDefault("dynamic_friction", 0);
        groundCollider.material.staticFriction = m_ResetParams.GetWithDefault("static_friction", 0);
    }

    public void SetBlockProperties() // Untouched. 
    {
        var scale = m_ResetParams.GetWithDefault("block_scale", 2);
        //Set the scale of the block
        m_BlockRb.transform.localScale = new Vector3(scale, 0.75f, scale);

        // Set the drag of the block
        m_BlockRb.drag = m_ResetParams.GetWithDefault("block_drag", 0.5f);
    }

    void SetResetParameters()
    {
        SetGroundMaterialFriction();
        SetBlockProperties();
    }

    void AnimationStart()
    {
        butterflyAnim.SetBool("IsDoubleFlapping", true);
        butterflyAnim.SetBool("TurnLeft", false);
        butterflyAnim.SetBool("TurnRight", false);
        butterflyAnim.SetBool("GoForward", false);
        butterflyAnim.SetBool("IsSlowFlapping", false);
        butterflyAnim.SetBool("IsTouched", false);
        butterflyAnim.SetBool("IsReturning", true);

    }
    void AnimationStop()
    {

        butterflyAnim.SetBool("IsDoubleFlapping", false);
        butterflyAnim.SetBool("TurnLeft", false);
        butterflyAnim.SetBool("TurnRight", false);
        butterflyAnim.SetBool("GoForward", false);
        butterflyAnim.SetBool("IsSlowFlapping", false);
        butterflyAnim.SetBool("IsTouched", true);
        butterflyAnim.SetBool("IsReturning", true);
    }
}