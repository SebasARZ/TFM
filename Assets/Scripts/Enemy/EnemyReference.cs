using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


[DisallowMultipleComponent]
public class EnemyReference : MonoBehaviour
{

    [HideInInspector]
    public NavMeshAgent agnt;
    [HideInInspector]
    public Animator anim;
    public Transform wayPoints;
    public int currentWayPointIndex;


    [Header("Stats")]
    public float pathUpdateRate = 0.4f;
 

    private void Awake()
    {
        anim = GetComponent<Animator>();
        agnt = GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        
    }

 
    void Update()
    {
        
    }
}
