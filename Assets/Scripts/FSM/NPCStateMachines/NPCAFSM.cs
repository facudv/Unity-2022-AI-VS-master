using UnityEngine;
using MyFSM;

public class NPCAFSM : MonoBehaviour
{
    private enum NPCAStates { IDLE, FOLLOWLEADER, ATTACK, BLOCK, TAKELIFE, CATCHENEMY, DIE, WIN, DANCE}
    private EventFSM<NPCAStates> myFSM;

    public NPCALeaderFSM leaderStateMachine;
    public NPCABoid myController;
    public NPCAnimator animatorManager;
    public NPCVFX myVFX;
    public NPCSFX mySFX;

    [HideInInspector] public bool startStateMachine;
    [HideInInspector] public bool isDead;
    [HideInInspector] public bool lifeAlreadyTaked;
    [HideInInspector] public bool victory;

    private bool followLeaderDone;
    private bool blockingState;
    private bool takingLife;

    [HideInInspector] public float maxLife = 10.0f;
    [HideInInspector] public float blockResistance = 1.0f;

     private float minDistanceToTarget = 1.35f;
   
    [HideInInspector] public float currentblockResistance;
    [HideInInspector] public float currentLife;

    [HideInInspector] public int npcsEnemiesAmount;

    [HideInInspector] public GameObject enemyTarget;

    public Transform danceZone;
    public GameObject lifeCan;

    void Awake()
    {
        currentLife = maxLife;
        CreateAndSetMyStates();
        lifeCan.SetActive(false);
    }

    void Update()
    {
        if (!startStateMachine) return;
        myFSM.Update();
    }

    private void CreateAndSetMyStates()
    {
        #region CreateStates
        State<NPCAStates> idle = new State<NPCAStates>("IDLE");
        State<NPCAStates> followLeader = new State<NPCAStates>("FOLLOW");
        State<NPCAStates> attack = new State<NPCAStates>("ATTACK");
        State<NPCAStates> block = new State<NPCAStates>("BLOCK");
        State<NPCAStates> takelife = new State<NPCAStates>("TAKELIFE");
        State<NPCAStates> catchEnemy = new State<NPCAStates>("CATCHENEMY");
        State<NPCAStates> die = new State<NPCAStates>("DIE");
        State<NPCAStates> win = new State<NPCAStates>("WIN");
        State<NPCAStates> dance = new State<NPCAStates>("DANCE");

        StateConfigurer.Create(idle).SetTransition(NPCAStates.FOLLOWLEADER, followLeader)
                                    .SetTransition(NPCAStates.ATTACK, attack)
                                    .SetTransition(NPCAStates.BLOCK, block)
                                    .SetTransition(NPCAStates.TAKELIFE, takelife)
                                    .SetTransition(NPCAStates.DIE, die)
                                    .SetTransition(NPCAStates.WIN, win)
                                    .Done();

        StateConfigurer.Create(followLeader).SetTransition(NPCAStates.IDLE, idle)
                                            .SetTransition(NPCAStates.DIE, die)
                                            .Done();

        StateConfigurer.Create(attack).SetTransition(NPCAStates.IDLE, idle)
                                      .SetTransition(NPCAStates.BLOCK, block)
                                      .SetTransition(NPCAStates.CATCHENEMY, catchEnemy)
                                      .SetTransition(NPCAStates.WIN, win)
                                      .SetTransition(NPCAStates.TAKELIFE, takelife)
                                      .SetTransition(NPCAStates.DIE, die)
                                      .Done();

        StateConfigurer.Create(block).SetTransition(NPCAStates.IDLE, idle)
                                     .SetTransition(NPCAStates.WIN, win)
                                     .SetTransition(NPCAStates.DIE, die)
                                     .SetTransition(NPCAStates.TAKELIFE, takelife)
                                     .Done();

        StateConfigurer.Create(takelife).SetTransition(NPCAStates.IDLE, idle)
                                        .SetTransition(NPCAStates.BLOCK, block)
                                        .Done();

        StateConfigurer.Create(catchEnemy).SetTransition(NPCAStates.ATTACK, attack)
                                          .SetTransition(NPCAStates.BLOCK, block)
                                          .SetTransition(NPCAStates.WIN, win)
                                          .SetTransition(NPCAStates.TAKELIFE, takelife)
                                          .SetTransition(NPCAStates.DIE, die)
                                          .Done();

        StateConfigurer.Create(die).SetTransition(NPCAStates.WIN, win)
                                   .Done();

        StateConfigurer.Create(win).SetTransition(NPCAStates.DANCE, dance)
                                    .Done();

        StateConfigurer.Create(dance).Done();
        #endregion;

        #region Set States
        idle.OnEnter += x => OnIdleEnter();
        idle.OnUpdate += OnIdle;

        followLeader.OnEnter += x => OnFollowLeaderEnter();
        followLeader.OnUpdate += OnFollowLeader;
        followLeader.OnExit += x => OnFollowLeaderExit();

        attack.OnUpdate += OnAttack;

        block.OnEnter += x => OnBlockEnter();
        block.OnUpdate += OnBlock;
        block.OnExit += x => OnBlockExit();

        catchEnemy.OnEnter += x => OnCatchEnemyEnter();
        catchEnemy.OnUpdate += OnCatchEnemy;
        catchEnemy.OnExit += x => OnCatchEnemyExit();

        takelife.OnEnter += x => OnTakeLifeEnter();
        takelife.OnUpdate += OnTakeLife;
        takelife.OnExit += x => OnTakeLifeExit();

        die.OnEnter += x => OnDeadEnter();
        die.OnUpdate += OnDead;

		win.OnEnter += x => OnVictoryEnter();
        win.OnUpdate += OnVictory;
        win.OnExit += x => OnVictoryExit();

        dance.OnEnter += x => OnDanceEnter();
        #endregion;

        myFSM = new EventFSM<NPCAStates>(idle);
    }

    private void SendInputToFSM(NPCAStates input)
    {
        myFSM.SendInput(input);
    }

	#region FSM Voids
	private void OnIdleEnter()
    {
        currentblockResistance = blockResistance;
        animatorManager.RunState(false);
    }

    private void OnIdle()
    {
		CheckVictory();
        CheckLife();
        CheckLeaderFSM();
        LookToEnemy();
    }    

    private void OnFollowLeaderEnter()
    {
        animatorManager.RunState(true);
    }

    private void OnFollowLeader()
    {
        //Si el lider NO llego lo seguimos
        if (!leaderStateMachine.onFightZone)
            myController.Flock();

        //Si el lider llego, seguimos ejecutando el Flock hasta estar cerquita de el. Una vez cerca
        //salimos de este estado, al salir avisamos que ya seguimos al lider entonces volvemos a copiar sus acciones. 
        //Se hace de esta manera ya que si el lider llega mucho antes que nosotros, no estariamos cerca de el.
        else
        {
            if (myController.DistanceToLeader() > myController.distanceToFollowLeader)
                myController.Flock();
            else
                SendInputToFSM(NPCAStates.IDLE);
        }
    }

    private void OnFollowLeaderExit()
    {
        followLeaderDone = true;
    }

    private void OnAttack()
    {
        CheckVictory();
        CheckLife();
        if (enemyTarget == null)
        {
            SendInputToFSM(NPCAStates.IDLE);
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, enemyTarget.transform.position);
        if (distanceToTarget <= minDistanceToTarget)
        {
			transform.LookAt(enemyTarget.transform.position);
            float randomNumber = Random.value;
			if (randomNumber <= 0.5f)
                animatorManager.AttackState(false);
            else
                animatorManager.AttackState(true);

            SendInputToFSM(NPCAStates.IDLE);
        }
        else
            SendInputToFSM(NPCAStates.CATCHENEMY);
    }

    private void OnBlockEnter()
    {
        animatorManager.Block(true);
        blockingState = true;
    }

    private void OnBlock()
    {
        CheckVictory();
        CheckLife();
        currentblockResistance -= Time.deltaTime;
        if (currentblockResistance <= 0)
            SendInputToFSM(NPCAStates.IDLE);
    }

    private void OnBlockExit()
    {
        animatorManager.Block(false);
        blockingState = false;
    }

    private void OnCatchEnemyEnter()
    {
        animatorManager.RunState(true);
    }

    private void OnCatchEnemy()
    {
        CheckVictory();
        CheckLife();
        if (enemyTarget == null)
        {
            SendInputToFSM(NPCAStates.ATTACK);
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, enemyTarget.transform.position);

        if (distanceToTarget > minDistanceToTarget)
            myController.GoToEnemyTarget(enemyTarget.transform);
        else
            SendInputToFSM(NPCAStates.ATTACK);
    }

    private void OnCatchEnemyExit()
    {
        animatorManager.RunState(false);
    }

    private void OnTakeLifeEnter()
    {
        lifeCan.SetActive(true);
        takingLife = true;
        animatorManager.RunState(false);
        animatorManager.Block(false);
        animatorManager.TakeLife();
    }

    private void OnTakeLife()
    {
        if (!takingLife)
            SendInputToFSM(NPCAStates.IDLE);
    }

    private void OnTakeLifeExit()
    {
        animatorManager.LifeTaked();
        lifeAlreadyTaked = true;
        lifeCan.SetActive(false);
        currentLife = maxLife;
        takingLife = false;
    }

    private void OnDeadEnter()
    {
        animatorManager.DeadState();
        gameObject.layer = 0;
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<CapsuleCollider>().enabled = false;
        isDead = true;
    }

    private void OnDead()
    {
        CheckVictory();
    }

    private void OnVictoryEnter()
    {
        GetComponent<CapsuleCollider>().enabled = true;
    }

    private void OnVictory()
    {
        float distanceToTarget = Vector3.Distance(transform.position, danceZone.position);
        if (distanceToTarget >= 0.8f)
        {
            animatorManager.RunState(true);
            myController.GoToEnemyTarget(danceZone);
        }
        else
            SendInputToFSM(NPCAStates.DANCE);
    }

    private void OnVictoryExit()
    {
        transform.rotation = Quaternion.Euler(0, -90, 0);
        animatorManager.RunState(false);
    }

    private void OnDanceEnter()
    {
        transform.rotation = Quaternion.Euler(0, -90, 0);
        animatorManager.VictoryState();
    }

    #endregion;

    private void CheckLife()
    {
        if (currentLife <= 0)
            SendInputToFSM(NPCAStates.DIE);
    }

    private void CheckLeaderFSM()
    {
        if (!leaderStateMachine.isDead)
        {
            if (leaderStateMachine.currentState == NPCALeaderFSM.NPCAStates.IDLE && followLeaderDone)
                SendInputToFSM(NPCAStates.IDLE);

            else if (leaderStateMachine.currentState == NPCALeaderFSM.NPCAStates.PATHFINDING)
                SendInputToFSM(NPCAStates.FOLLOWLEADER);

            else if (leaderStateMachine.currentState == NPCALeaderFSM.NPCAStates.ATTACK)
                SendInputToFSM(NPCAStates.ATTACK);

            else if (leaderStateMachine.currentState == NPCALeaderFSM.NPCAStates.BLOCK)
                SendInputToFSM(NPCAStates.BLOCK);

            else if (leaderStateMachine.currentState == NPCALeaderFSM.NPCAStates.TAKELIFE)
                SendInputToFSM(NPCAStates.TAKELIFE);
        }

        else
            SendInputToFSM(NPCAStates.ATTACK);
    }

    public void TakeDamage(float damage)
    {
        if (takingLife || isDead) return;

        mySFX.DamageSound();
        if (!blockingState)
        {
            animatorManager.TakeDamageState();
            currentLife -= damage;
            myVFX.OnDamageVFX();
        }
    }

    public void OnTakeLifeEnd()
    {
        takingLife = false;
    }

    private void CheckVictory()
    {
        if (victory)
            SendInputToFSM(NPCAStates.WIN);
    }

    private void LookToEnemy()
    {
        if (enemyTarget != null && followLeaderDone)
            transform.forward = Vector3.Slerp(transform.forward,
                                (enemyTarget.transform.position - transform.position),
                                myController.rotationSpeed * Time.deltaTime);
    }
}
