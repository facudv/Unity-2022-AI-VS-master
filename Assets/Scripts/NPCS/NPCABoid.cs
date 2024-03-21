using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NPCABoid : MonoBehaviour
{
	public float distanceToFollowLeader;

	public LayerMask LayerBoid;
	public LayerMask LayerObst;

	[HideInInspector] public List<Collider> friends;
	[HideInInspector] public List<Collider> obstacles;

	[Header("Speed")]
	public float speed;
	public float rotationSpeed;

	private Vector3 direction;

	private Vector3 cohesionVector;
	private Vector3 alignVector;
	private Vector3 separationVector;
	private Vector3 leaderVector;
	private Vector3 avoidanceVector;
	private Vector3 targetVector;

	[Header("Radius")]
	public float radFlock;
	public float radObst;

	[Header("Weights")]
	public float avoidWeight;
	public float leaderWeight;
	public float alineationWeight;
	public float separationWeight;
	public float cohesionWeight;

	[Header("My Homie")]
	public Transform boidLeader;

	private Collider closerObstacle;

	public void Flock()
	{
		GetFriendsAndObstacles();
		closerObstacle = GetCloserOb();

		cohesionVector = GetCohesion() * cohesionWeight;
		alignVector = GetAlign() * alineationWeight;
		separationVector = GetSeparation() * separationWeight;
		leaderVector = GetLeader() * leaderWeight;
		avoidanceVector = GetObstacleAvoidance() * avoidWeight;

		direction = avoidanceVector;
		direction += cohesionVector + alignVector + separationVector + leaderVector;

		direction = new Vector3(direction.x, 0, direction.z);

		transform.forward = Vector3.Slerp(transform.forward, direction, rotationSpeed * Time.deltaTime);
		transform.position += transform.forward * Time.deltaTime * speed;
	}

	public void GoToEnemyTarget(Transform targetEnemy)
	{
		GetFriendsAndObstacles();
		closerObstacle = GetCloserOb();

		separationVector = GetSeparation() * separationWeight;
		targetVector = (targetEnemy.position - transform.position) * leaderWeight;
		avoidanceVector = GetObstacleAvoidance() * avoidWeight;

		direction = avoidanceVector;
		direction += separationVector + targetVector;
		direction = new Vector3(direction.x, 0, direction.z);

		transform.forward = Vector3.Slerp(transform.forward, direction, rotationSpeed * Time.deltaTime);
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);	//Para que jamas ni aunque colisione haga rotaciones raras.
		transform.position += transform.forward * Time.deltaTime * speed;
	}

	public float DistanceToLeader()
	{
		float distanceToMyHomie = Vector3.Distance(transform.position, boidLeader.position);
		return distanceToMyHomie;
	}

	#region Avoidance Code
	private void GetFriendsAndObstacles()
	{
		friends.Clear();
		obstacles.Clear();

		friends.AddRange(Physics.OverlapSphere(transform.position, radFlock, LayerBoid));
		obstacles.AddRange(Physics.OverlapSphere(transform.position, radObst, LayerObst));
	}

	private Collider GetCloserOb()
	{
		if (obstacles.Count > 0)
			return obstacles.OrderBy(x => (x.transform.position - transform.position).magnitude).First();
		else
			return null;
	}

	private Vector3 GetAlign()
	{
		Vector3 align = new Vector3();
		foreach (Collider thisFriend in friends)
			align += thisFriend.transform.forward;
		return align /= friends.Count;
	}
 
	private Vector3 GetSeparation()
	{
		Vector3 separation = new Vector3();
		
		foreach (Collider thisFriend in friends)
		{
			Vector3 friendDistance = new Vector3();
			friendDistance = transform.position - thisFriend.transform.position;

			float magnitude = radFlock - friendDistance.magnitude;

			friendDistance.Normalize();
			friendDistance *= magnitude;

			separation += friendDistance;
		}
		return separation /= friends.Count;
	}
 
	private Vector3 GetCohesion()
	{
		Vector3 cohesion = new Vector3();
		foreach (Collider thisFriend in friends)
			cohesion += thisFriend.transform.position - transform.position;
		return cohesion /= friends.Count;
	}

	private Vector3 GetObstacleAvoidance()
	{
		if (closerObstacle)
			return transform.position - closerObstacle.transform.position;
		else return Vector3.zero;
	}

	private Vector3 GetLeader()
	{
		return boidLeader.transform.position - transform.position;
	}
	
	#endregion;
}
