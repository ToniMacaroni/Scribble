using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scribble
{
    internal class Eraser : MonoBehaviour
    {
        private Vector3 _lastErasePosition;
        private BrushBehaviour _brushBehaviour;

        public float EraserSize = 0.1f;

        void Awake()
        {
            enabled = false;
        }

        public void Init(BrushBehaviour brushBehaviour)
        {
            _brushBehaviour = brushBehaviour;
        }

        public void StartErasing()
        {
            _brushBehaviour.BrushMesh.transform.localScale = new Vector3(EraserSize, EraserSize, EraserSize);
            _brushBehaviour.BrushMesh.GetComponent<MeshRenderer>().material.color = new Color(0.98f, 0.15f, 0.01f);
            _brushBehaviour.BrushMesh.SetActive(true);
            enabled = true;
        }

        public void StopErasing()
        {
            _brushBehaviour.BrushMesh.SetActive(false);
            enabled = false;
        }

        void Update()
        {
            if (Vector3.Distance(_lastErasePosition, transform.position) > 0.1f)
            {
                _lastErasePosition = transform.position;
                ScribbleContainer.Instance.Erase(_lastErasePosition, EraserSize);
            }
        }
    }
}
