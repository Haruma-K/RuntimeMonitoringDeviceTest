using System;
using UnityEngine.EventSystems;

namespace RuntimeMonitoringDeviceTest.Scripts
{
    public class ClickingDetector : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool IsClicking { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            SetIsClicking(true);
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            SetIsClicking(false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SetIsClicking(false);
        }

        public event Action<bool> ClickingStateChanged;

        private void SetIsClicking(bool isClicking)
        {
            if (IsClicking == isClicking)
                return;

            IsClicking = isClicking;
            ClickingStateChanged?.Invoke(IsClicking);
        }
    }
}
