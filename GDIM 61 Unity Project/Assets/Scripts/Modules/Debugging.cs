using UnityEngine;
using UnityEngine.AI;

public class Debugging : MonoBehaviour {
    void Update() {
        
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if ( agent != null ) {
            print(agent.hasPath);
            print(agent.isStopped);
            print(agent.destination);
        }
        
    }
}
