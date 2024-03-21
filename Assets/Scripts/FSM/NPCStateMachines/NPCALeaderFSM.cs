using System.Collections.Generic;
using UnityEngine;
using MyFSM;

public class NPCALeaderFSM : MonoBehaviour
{
	public enum NPCAStates { IDLE, PATHFINDING, ATTACK, BLOCK, TAKELIFE, CATCHENEMY, DIE, WIN , DANCE}
	public NPCAStates currentState;
	private EventFSM<NPCAStates> myFSM;
	private Roulette rouletteSelection;

	public NpcController myController;
	public NPCPathFindingData pathFindingManager;
	public NPCAnimator animatorManager;
	public NPCVFX myVFX;
	public NPCSFX mySFX;
	public NPCAFSM[] myCrew;

	[HideInInspector] public bool startStateMachine;
	[HideInInspector] public bool onFightZone;
	[HideInInspector] public bool isDead;
	[HideInInspector] public bool lifeAlreadyTaked;
	[HideInInspector] public bool victory;
	
	private bool blockingState;
	private bool takingLife;
	private bool decisionsChanged;

	[HideInInspector] public float maxLife = 10.0f;
	[HideInInspector] public float blockResistance = 1.0f;
	[HideInInspector] public float currentblockResistance;
	[HideInInspector] public float currentLife;

	[Header("Promedio de vida para priorizar defenderse o recuperar")]
	[Range(0, 100)] public float limitLife;


	private float attackOrBlockTimer = 0.5f;
	private float minDistanceToTarget = 1.35f;
	private float currentAttackOrBlockTimer;
	private float percentToHealth = 30.0f;

	private int blockRepeat;
	private int maxBlocks = 3;

	[Header("Roulette Wheel Selection")]
	public List<string> posibleDecisions;
	
	[Range(0,100)] public List<int> decisionsWeights;
	[Range(0, 100)] public List<int> secondDecisionsWeights;

	[HideInInspector] public int npcsEnemiesAmount;

	[HideInInspector] public GameObject enemyTarget;

	public Transform danceZone;
	public GameObject lifeCan;
	
	void Awake()
	{
		currentLife = maxLife;
		CreateAndSetMyStates();
		rouletteSelection = new Roulette(posibleDecisions, decisionsWeights);
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
		State<NPCAStates> pathfinding = new State<NPCAStates>("PATHFINDING");
		State<NPCAStates> attack = new State<NPCAStates>("ATTACK");
		State<NPCAStates> block = new State<NPCAStates>("BLOCK");
		State<NPCAStates> takelife = new State<NPCAStates>("TAKELIFE");
		State<NPCAStates> catchEnemy = new State<NPCAStates>("CATCHENEMY");
		State<NPCAStates> die = new State<NPCAStates>("DIE");
		State<NPCAStates> win = new State<NPCAStates>("WIN");
		State<NPCAStates> dance = new State<NPCAStates>("DANCE");



		StateConfigurer.Create(idle).SetTransition(NPCAStates.PATHFINDING, pathfinding)
									.SetTransition(NPCAStates.ATTACK, attack)
									.SetTransition(NPCAStates.BLOCK, block)
									.SetTransition(NPCAStates.TAKELIFE, takelife)
									.SetTransition(NPCAStates.DIE, die)
									.SetTransition(NPCAStates.WIN, win)
									.Done();

		StateConfigurer.Create(pathfinding).SetTransition(NPCAStates.IDLE, idle)
										   .SetTransition(NPCAStates.DIE, die)
										   .Done();

		StateConfigurer.Create(attack).SetTransition(NPCAStates.IDLE, idle)
									  .SetTransition(NPCAStates.BLOCK, block)
									  .SetTransition(NPCAStates.CATCHENEMY, catchEnemy)
									  .SetTransition(NPCAStates.TAKELIFE, takelife)
									  .SetTransition(NPCAStates.WIN, win)
									  .SetTransition(NPCAStates.DIE, die)
									  .Done();

		StateConfigurer.Create(block).SetTransition(NPCAStates.IDLE, idle)
									 .SetTransition(NPCAStates.DIE, die)
									 .SetTransition(NPCAStates.TAKELIFE, takelife)
									 .SetTransition(NPCAStates.WIN, win)
									 .Done();

		StateConfigurer.Create(takelife).SetTransition(NPCAStates.IDLE, idle)
										.SetTransition(NPCAStates.BLOCK, block)
										.Done();

		StateConfigurer.Create(catchEnemy).SetTransition(NPCAStates.ATTACK, attack)
										  .SetTransition(NPCAStates.TAKELIFE, takelife)
										  .SetTransition(NPCAStates.BLOCK, block)
										  .SetTransition(NPCAStates.WIN, win)
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

		pathfinding.OnEnter += x => OnPathFindingEnter(); 
		pathfinding.OnUpdate += OnPathFinding;
		pathfinding.OnExit += x => OnPathFindingExit();

		attack.OnEnter += x => OnAttackEnter();
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
		currentState = NPCAStates.IDLE;
		animatorManager.RunState(false);
	}

	private void OnIdle()
	{
		CheckVictory();
		CheckLife();
		if (!onFightZone)
			SendInputToFSM(NPCAStates.PATHFINDING);
		else
		{
			float lifeAverage = CalculateLife();
			float myLifeAverage = (currentLife / maxLife) * 100;
			if (lifeAverage < limitLife ||myLifeAverage <= percentToHealth) //Toma la decision de curarse en base a el porcentaje de vida de todo su equipo, o en base a que el tenga menos de 30%.
			{
				if (!lifeAlreadyTaked)
					SendInputToFSM(NPCAStates.TAKELIFE);
				else
				{
					ChangeDecisionsWeights();
					TakeDecision();
				}	
			}
			else
			{
				LookToEnemy();
				TakeDecision();
			}
		}
	}

	private void OnPathFindingEnter()
	{
		currentState = NPCAStates.PATHFINDING;
		animatorManager.RunState(true);

		var nodeToGo = pathFindingManager.AskAPath(myController, transform);
		myController.MoveTo(nodeToGo);
	}

	private void OnPathFinding()
	{
		PathNode[] path = myController.AstarPathGetter;
		var lastNode = path[path.Length - 1].gameObject.transform.position;
		var distanceToLastNode = Vector3.Distance(myController.transform.position, lastNode);

		myController.GoToPath();

		if (distanceToLastNode <= 1)
			SendInputToFSM(NPCAStates.IDLE);
	}

	private void OnPathFindingExit()
	{
		onFightZone = true;
	}

	private void OnAttackEnter()
	{
		currentState = NPCAStates.ATTACK;
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
		blockingState = true;
		currentState = NPCAStates.BLOCK;
		animatorManager.Block(true);
	}

	private void OnBlock()
	{
		CheckVictory();
		CheckLife();
		currentblockResistance -= Time.deltaTime;
		if(currentblockResistance <= 0)
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
		if(enemyTarget == null)
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
		currentState = NPCAStates.TAKELIFE;
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
		lifeCan.SetActive(false);
		currentLife = maxLife;
		lifeAlreadyTaked = true;
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

	private float CalculateLife()
	{
		float acumulatorCurrentLife = 0;
		float acumulatorMaxLife = 0;

		foreach (var npc in myCrew)
		{
			acumulatorMaxLife += npc.maxLife;
			acumulatorCurrentLife += npc.currentLife;
		}

		acumulatorCurrentLife = (acumulatorCurrentLife / acumulatorMaxLife) * 100;
		return acumulatorCurrentLife;
	}

	private void ChangeDecisionsWeights()
	{
		if (decisionsChanged) return;
		//Cambiamos las prioridades. Pasamos a defendernos mas.
		rouletteSelection = new Roulette(posibleDecisions, secondDecisionsWeights);
		decisionsChanged = true;
	}

	private void TakeDecision()
	{
		currentAttackOrBlockTimer += Time.deltaTime;
		if (currentAttackOrBlockTimer >= attackOrBlockTimer)
		{
			if (blockRepeat < maxBlocks)      //Utilizamos esto para que a pesar de depender del Roulette Wheel, que no sea posible que bloquee X veces seguidas.
			{
				string rouletteResult = rouletteSelection.RouletteWheelSelection();

				if (rouletteResult == posibleDecisions[0])
					SendInputToFSM(NPCAStates.ATTACK);
				else if (rouletteResult == posibleDecisions[1])
				{
					SendInputToFSM(NPCAStates.BLOCK);
					blockRepeat++;
				}
			}

			else
			{
				SendInputToFSM(NPCAStates.ATTACK);
				blockRepeat = 0;
			}

			currentAttackOrBlockTimer = 0;
		}
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

	private void CheckLife()
	{
		if (currentLife <= 0)
			SendInputToFSM(NPCAStates.DIE);
	}

	private void CheckVictory()
	{
		if (victory)
			SendInputToFSM(NPCAStates.WIN);

	}

	private void LookToEnemy()
	{
		if(enemyTarget != null)
			transform.forward = Vector3.Slerp(transform.forward, 
								(enemyTarget.transform.position - transform.position), 
								myController.rotationSpeed * Time.deltaTime);
	}


}
