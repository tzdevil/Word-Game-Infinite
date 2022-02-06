using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace tzdevil.WordGameInfinite
{
    public class InputFieldHandler : MonoBehaviour
    {
        private TMP_InputField _inputField;

        private void Awake() => _inputField = GetComponent<TMP_InputField>();

        void OnEnable() => _inputField.onValueChanged.AddListener(InputValueChanged);

        static string CleanInput(string strIn) => Regex.Replace(strIn,
                  @"[^a-zA-Z]:"";'<>?,./]", "");

        //Called when Input changes
        void InputValueChanged(string attemptedVal) => _inputField.text = CleanInput(attemptedVal);

        void OnDisable() => _inputField.onValueChanged.RemoveAllListeners();
    }
}