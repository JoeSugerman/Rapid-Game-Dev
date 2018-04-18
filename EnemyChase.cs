using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChase : MonoBehaviour
{

    public Transform Player;
    public List<Transform> path = new List<Transform>();

    public GameObject player;
    public GameObject enemyPosition;

    private float MoveSpeed = 2.0f;
    private int MaxDist = 10;
    private int MinDist = 3;
    private int currentPoint = 0;
    private float speed = 5.0f;
    public float reachDistance = 1.0f;
    private float rotationSpeed = 10.0f;
    private int damage = 10;
    public float timer;
    public bool chasing = false;

    public LayerMask mask;
    private Health health;
    public Light eyes;
    public SpriteRenderer vision;
    public GameObject tempPos;
    private RaycastHit visionR;
    private RaycastHit visionRP;
    public float rayLength = 20.0f;
    public float ray2Length = 50.0f;
    private float direction;


    void Start()
    {
        timer = 0.0f;
    }

    void Update()
    {
        Debug.DrawRay(vision.transform.position, enemyPosition.transform.forward * rayLength, Color.red, 0.1f);
        if (Physics.Raycast(vision.transform.position, enemyPosition.transform.forward, out visionRP, ray2Length))
        {
            if (visionRP.collider.tag == "Player")
            {
                timer = 15.0f;
            }
        }
        if (timer <= 0)
        {
            eyes.color = Color.green;
            vision.color = Color.green;
            MoveSpeed = 5.0f;
            if (currentPoint < path.Count)
            {

                //Rotate player to keep player looking foward - using next point in path
                Quaternion targetRotation = Quaternion.LookRotation(path[currentPoint].transform.position - transform.position);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                transform.LookAt(path[currentPoint]);

                //move player along path - using points created on path
                //points should be created on 
                if (transform.rotation == targetRotation)
                {

                    float dist = Vector3.Distance(path[currentPoint].position, transform.position);

                    transform.position = Vector3.MoveTowards(transform.position, path[currentPoint].position, Time.deltaTime * speed);

                    if (dist <= reachDistance)
                    {
                        if (currentPoint >= path.Count - 1)
                        {
                            currentPoint = 0;
                        }
                        else
                        {
                            currentPoint++;
                        }
                    }

                }
            }


        }
        else if (timer <= 15.0f && timer > 0.0f)
        {
            transform.LookAt(new Vector3(player.transform.position.x, enemyPosition.transform.position.y, 0.0f));
            if (Vector3.Distance(transform.position, Player.position) > MinDist)
            {
                MoveSpeed = 11.0f;
                transform.position += transform.forward * MoveSpeed * Time.deltaTime;
            }
            if(player.transform.position.x < transform.position.x)
            {
                direction = 1;
            }
            else
            {
                direction = 2;
            }

            if(direction == 1)
            {
                if(transform.position.x <= player.transform.position.x + 1)
                {
                    timer = 0;
                }
            }
            else if (direction == 2)
            {
                if (transform.position.x >= player.transform.position.x -1)
                {
                    timer = 0;
                }
            }
            if(player.GetComponent<Health>().currentHealth == 0)
            {
                timer = 0;
            }
            eyes.color = Color.red;
            vision.color = Color.red;
            timer -= Time.deltaTime;
        }

    }
    public void Attack(GameObject victim, int dmg)
    {
        health = victim.GetComponent<Health>();
        //deal dmg
        if (health && !health.flashing)
            health.currentHealth -= dmg;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            timer = 0;
            Attack(player, damage);
        }

    }
}

