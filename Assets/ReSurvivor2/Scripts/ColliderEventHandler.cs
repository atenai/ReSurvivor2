using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColliderEventHandler : MonoBehaviour
{
    [SerializeField] Collider self = default;

    UnityEvent<ColliderEventHandler, Collider> onTriggerEnterEvent = new UnityEvent<ColliderEventHandler, Collider>();
    public UnityEvent<ColliderEventHandler, Collider> OnTriggerEnterEvent => onTriggerEnterEvent;
    UnityEvent<ColliderEventHandler, Collider> onTriggerExitEvent = new UnityEvent<ColliderEventHandler, Collider>();
    public UnityEvent<ColliderEventHandler, Collider> OnTriggerExitEvent => onTriggerExitEvent;

    void OnTriggerEnter(Collider collider)
    {
        onTriggerEnterEvent.Invoke(this, collider);
    }

    void OnTriggerExit(Collider collider)
    {
        onTriggerExitEvent.Invoke(this, collider);
    }
}