using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    /// <summary>
    /// Component for number input fields.
    /// Provides an increase and decrease button.
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class NumberInputField : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private TMP_InputField inputField;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private float increment;

        public void OnPointerEnter(PointerEventData eventData)
        {
            increaseButton.gameObject.SetActive(true);
            decreaseButton.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            increaseButton.gameObject.SetActive(false);
            decreaseButton.gameObject.SetActive(false);
        }

        private void Increment()
        {
            if (Evaluator.TryFloat(inputField.text, out float val))
            {
                val += increment;
                string str = val.ToString();
                inputField.text = str;
                inputField.onEndEdit.Invoke(str);
            }
        }

        private void Decrement()
        {
            if (Evaluator.TryFloat(inputField.text, out float val))
            {
                val -= increment;
                string str = val.ToString();
                inputField.text = str;
                inputField.onEndEdit.Invoke(str);
            }
        }

        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
            increaseButton.onClick.AddListener(Increment);
            decreaseButton.onClick.AddListener(Decrement);
            increaseButton.gameObject.SetActive(false);
            decreaseButton.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            increaseButton.onClick.RemoveAllListeners();
            decreaseButton.onClick.RemoveAllListeners();
        }
    }
}