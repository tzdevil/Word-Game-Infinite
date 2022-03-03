using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace tzdevil.WordGameInfinite
{
    public class InputFieldHandler : MonoBehaviour
    {
        private TMP_InputField _inputField => GetComponent<TMP_InputField>();

        private void OnEnable() => _inputField.onValueChanged.AddListener(InputValueChanged);

        private static string CleanInput(string strIn) => Regex.Replace(strIn,
                  @"[^a-zA-Z]:"";'<>?,./]", "");

        //Called when Input changes
        private void InputValueChanged(string attemptedVal) => _inputField.text = CleanInput(attemptedVal);

        private void OnDisable() => _inputField.onValueChanged.RemoveAllListeners();
    }
}