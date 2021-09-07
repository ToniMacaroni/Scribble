using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scribble
{
    internal class VrInputManager : InputManager
    {
        public SaberType SaberType;

        private string _inputString;

        protected override void Start()
        {
            _inputString = SaberType == SaberType.SaberA ? "TriggerLeftHand" : "TriggerRightHand";
            //Debug.LogError("VR Input Manager Initialized");
        }

        protected override void Update()
        {
            float triggerValue = Input.GetAxis(_inputString);
            if (triggerValue > 0.8f && UpTriggered)
            {
                UpTriggered = false;
                InvokeButtonPressed();
            }
            else if (triggerValue < 0.8f && !UpTriggered)
            {
                UpTriggered = true;
                InvokeButtonReleased();
            }
        }
    }

    internal class MouseInputManager : InputManager
    {
        public int ButtonIdx = 0;

        protected override void Start()
        {
            //Debug.LogError("Mouse Input Manager Initialized");
        }

        protected override void Update()
        {
            if (Input.GetMouseButton(ButtonIdx) && UpTriggered)
            {
                UpTriggered = false;
                InvokeButtonPressed();
            }
            else if (Input.GetMouseButtonUp(ButtonIdx) && !UpTriggered)
            {
                UpTriggered = true;
                InvokeButtonReleased();
            }
        }
    }

    internal class InputManager : MonoBehaviour
    {
        public event Action ButtonPressed;
        public event Action ButtonReleased;

        protected bool UpTriggered = true;

        protected virtual void Start()
        {
        }

        protected void InvokeButtonPressed()
        {
            ButtonPressed?.Invoke();
        }

        protected void InvokeButtonReleased()
        {
            ButtonReleased?.Invoke();
        }

        protected virtual void Update()
        {
        }
    }
}
