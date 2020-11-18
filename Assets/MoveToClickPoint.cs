    using System.Security.Cryptography;
    using UnityEngine;
    using UnityEngine.AI;
    
    public class MoveToClickPoint : MonoBehaviour
    {
        private NavMeshAgent agent;
        private CapsuleCollider coll;

        public GameObject destinationMarkerPrefab;
        private GameObject spawnedMarker;

        void Start() {
            agent = GetComponent<NavMeshAgent>();
            coll = GetComponent<CapsuleCollider>();
        }
        
        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit hit;
                
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                {
                    if (hit.collider == coll)
                        return;
                    
                    agent.SetDestination(hit.point);

                    if (spawnedMarker != null)
                        Destroy(spawnedMarker);
                    
                    spawnedMarker = Instantiate(destinationMarkerPrefab, new Vector3(hit.point.x, hit.point.y + 1f, hit.point.z), Quaternion.identity);
                    Destroy(spawnedMarker, 2f);
                }
            }
        }
    }
