using System;
using UnityEngine;

namespace ArcCreate.Compose.Components
{
    /// <summary>
    /// Field for handling arc color input, which composes of a color at y=1 (high color), and color at y=0 (low color).
    /// </summary>
    public class ArcColorInputField : MonoBehaviour
    {
        [SerializeField] private ColorInputField highColor;
        [SerializeField] private ColorInputField lowColor;

        /// <summary>
        /// Event invoked after either high color or low color value has changed.
        /// </summary>
        public event Action<(Color high, Color low)> OnValueChange;

        public Color High => highColor.Value;

        public Color Low => lowColor.Value;

        /// <summary>
        /// Set the value without invoking <see cref="OnValueChange"/> event.
        /// </summary>
        /// <param name="high">The high color value.</param>
        /// <param name="low">The low color value.</param>
        public void SetValueWithoutNotify(Color high, Color low)
        {
            highColor.SetValueWithoutNotify(high);
            lowColor.SetValueWithoutNotify(low);
        }

        /// <summary>
        /// Set the value and invoke <see cref="OnValueChange"/> event.
        /// </summary>
        /// <param name="high">The high color value.</param>
        /// <param name="low">The low color value.</param>
        public void SetValue(Color high, Color low)
        {
            highColor.SetValueWithoutNotify(high);
            lowColor.SetValueWithoutNotify(low);
            OnValueChange?.Invoke((high, low));
        }

        private void Awake()
        {
            highColor.OnValueChange += OnChange;
            lowColor.OnValueChange += OnChange;
        }

        private void OnChange(Color obj)
        {
            OnValueChange?.Invoke((highColor.Value, lowColor.Value));
        }
    }
}