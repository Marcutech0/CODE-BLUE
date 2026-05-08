using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue 
{
    public class GroupCamera : SingletonBehaviourNonNetworked<GroupCamera>
    {
        [SerializeField] Camera _camera;
        [SerializeField] float _smoothingSpeed = 10f;
        [SerializeField] Vector2 _zoomLimits = new(5f, 30f);

        List<Transform> _camTargets = new();

        float _boundsSize;
        float _zoom;
        private void Start()
        {
            _zoom = _zoomLimits.x;
        }

        public void RefreshTargets() 
        {
            _camTargets.RemoveAll(t => t == null);
        }

        public void AddTargetToList(Transform t)
        {
            RefreshTargets();
            if (!_camTargets.Contains(t))
                _camTargets.Add(t);
        } 

        private void LateUpdate()
        {
            RefreshTargets();
            if (_camTargets.Count == 0) return;

            var center = GetBoundsCenter() + Vector3.forward * -2;
            transform.position = Vector3.Lerp(transform.position, center, _smoothingSpeed * Time.deltaTime);
           
            _zoom = Mathf.Lerp(_zoom, Mathf.Lerp(_zoomLimits.x, _zoomLimits.y, _boundsSize / _zoomLimits.y), _smoothingSpeed * Time.deltaTime);
            _camera.transform.localPosition = new Vector3(0, _zoom, -_zoom + 5);
        }

        private Vector3 GetBoundsCenter()
        {
            var bounds = new Bounds(_camTargets[0].position, Vector3.zero);
            foreach (var target in _camTargets)
                bounds.Encapsulate(target.position);

            _boundsSize = bounds.size.magnitude;
            return bounds.center;
        }
    }
}