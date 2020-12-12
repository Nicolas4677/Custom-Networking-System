//Copyright (C) 2020, Nicolas Morales Escobar. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.AI;

namespace Character
{
    public class NetCharacterController : MonoBehaviour
    {
        [SerializeField] private LayerMask floorMask;

        private NavMeshAgent navMeshAgent;

        public event Action<Vector3> onPositionSelected; 

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        //I created this method so that the character only read input when the clint calls it.
        public void ReadInput()
        {
            if (Input.GetMouseButtonUp(0))
            {
                RaycastHit hit;

                Camera cameraScreen = Camera.main;

                if (cameraScreen != null)
                {
                    Ray ray = cameraScreen.ScreenPointToRay(Input.mousePosition);
                    
                    if (Physics.Raycast(ray, out hit, floorMask))
                    {
                        MoveTo(hit.point);
                        onPositionSelected?.Invoke(hit.point);
                    }
                }
            }
        }

        //Encapsulated the 'SetDestination' method so that when the server send message to clients,
        //they can move independently each netCharacter
        public void MoveTo(Vector3 targetPos)
        {
            navMeshAgent.SetDestination(targetPos);
        }
    }
}