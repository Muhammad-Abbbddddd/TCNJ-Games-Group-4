using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAi : MonoBehaviour
{
  
    public Transform target;

    //speed of enemy
    public float speed = 200f;
    //individual gridpoints which the enemy moves to to get to the player
    public float nextWaypointDistance = 3f;

   
    public Transform enemyGFX; 

    Path path;
    int currentWayPoint;
    bool reachedEndOfPath;

    Seeker seeker;
    Rigidbody2D rb;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        // Set target to player dynamically
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure the player has the 'Player' tag.");
        }

        InvokeRepeating("UpdatePath", 0f, 0.5f); 
        
    }

    void UpdatePath()
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }

    }

    void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            path = p;
            currentWayPoint = 0; 
        }
    }

    void FixedUpdate()
    {
        if (path == null)
            return; 

        if(currentWayPoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return; 
        } else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWayPoint] - rb.position).normalized; 
        Vector2 force = direction * speed * Time.deltaTime;

        rb.AddForce(force);

        float distane = Vector2.Distance(rb.position, path.vectorPath[currentWayPoint]);

        
        if (distane < nextWaypointDistance) 
        {
            currentWayPoint++;
        }

       
        if (force.x >= 0.01f)
        {
            enemyGFX.localScale = new Vector3(-0.6f, 0.6f, 0.6f);
        }
        else if (force.x <= -0.01f)
        {
            enemyGFX.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        }
    }
}