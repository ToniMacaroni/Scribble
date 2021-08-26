using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scribble
{
    internal class InputManager : MonoBehaviour
    {
        public SaberType SaberType;

        public event Action ButtonPressed;
        public event Action ButtonReleased;

        private string _inputString;
        private bool _upTriggered = true;

        void Start()
        {
            _inputString = SaberType==SaberType.SaberA ? "TriggerLeftHand" : "TriggerRightHand";
            Plugin.Log.Debug("Input Manager Initialized");
        }

        void Update()
        {
            float triggerValue = Input.GetAxis(_inputString);
            if (triggerValue > 0.8f && _upTriggered)
            {
                _upTriggered = false;
                ButtonPressed?.Invoke();
            }else if (triggerValue < 0.8f && !_upTriggered)
            {
                _upTriggered = true;
                ButtonReleased?.Invoke();
            }
        }
    }
}
