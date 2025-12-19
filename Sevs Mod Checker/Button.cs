using UnityEngine;
using System;

namespace SevsModChecker
{
    public class Button : MonoBehaviour
    {
        public Action<bool> Click;

        public float Debounce = 0.25f;
        private static float LastPress;

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("GorillaInteractable");
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
            //gameObject.GetComponent<BoxCollider>().size = new Vector3(gameObject.GetComponent<BoxCollider>().size.x, gameObject.GetComponent<BoxCollider>().size.y*2, gameObject.GetComponent<BoxCollider>().size.z);
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && Time.time > LastPress + Debounce)
            {
                LastPress = Time.time;
                Click?.Invoke(true);
            }
        }
    }
}