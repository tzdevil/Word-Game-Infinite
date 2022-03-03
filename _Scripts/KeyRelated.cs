using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace tzdevil.WordGameInfinite
{
    public class KeyRelated : MonoBehaviour
    {
        [SerializeField] private GameObject _enterKey, _backSpaceKey;
        [SerializeField] private Sprite _enterKeyNormal, _enterKeyForbidden, _backSpaceKeyNormal, _backSpaceKeyForbidden;

        private GameManager _gameManager => GameObject.Find("GameplayManager").GetComponent<GameManager>();
        [HideInInspector] public static TextMeshProUGUI _errorLog => GameObject.Find("ErrorLog").GetComponent<TextMeshProUGUI>();
        [HideInInspector] public static float _errorTimer;

        public GameManager gameManager => GetComponent<GameManager>();

        private float _gameTime;
        private TextMeshProUGUI _gameTimeText => GameObject.Find("GameTime").GetComponent<TextMeshProUGUI>();
        [HideInInspector] public bool startedGame;

        public static TMP_InputField _keyboardHolder => GameObject.Find("KeyboardHolder_InputField").GetComponent<TMP_InputField>();

        private static float _backspaceTimer;

        private void Start() => _keyboardHolder.gameObject.SetActive(SystemInfo.deviceType != DeviceType.Handheld);

        private void Update()
        {
            TextAndColor();
            CheckKey();
        }

        private void TextAndColor()
        {
            if (startedGame && GameManager._canPlay) _gameTime += Time.deltaTime;
            if (!GameManager._canPlay) _gameTimeText.color = new Color32(83, 141, 78, 255);

            _enterKey.GetComponent<Image>().sprite = gameManager._ourWord.Length == 5 ? _enterKeyNormal : _enterKeyForbidden;
            _enterKey.GetComponent<Image>().raycastTarget = gameManager._ourWord.Length == 5;

            _backSpaceKey.GetComponent<Image>().sprite = gameManager._ourWord.Length != 0 ? _backSpaceKeyNormal : _backSpaceKeyForbidden;
            _backSpaceKey.GetComponent<Image>().raycastTarget = gameManager._ourWord.Length != 0;

            _gameTimeText.text = _gameTime.ToString("0.0s");

            if (_errorTimer > 0) _errorTimer -= Time.deltaTime;
            Color errorColor = _errorLog.color;
            errorColor.a = _errorTimer / 2;
            _errorLog.color = errorColor;
        }

        private void CheckKey()
        {
            if (Input.anyKey)
            {
                _keyboardHolder.ActivateInputField();
                if (GameManager._canPlay && !Input.GetKey(KeyCode.Backspace) && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !Input.GetKeyDown(KeyCode.Space) && gameManager._ourWord.Length <= 4)
                {
                    if (!startedGame) startedGame = true;
                    TypeWithKeyboard();
                }
                if (Input.GetKey(KeyCode.Backspace) && _backspaceTimer <= 0) { _backspaceTimer = .1f; RemoveLastLetter(); }
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) gameManager.EnterWord();
                _keyboardHolder.text = "";
            }
            if (_backspaceTimer > 0) _backspaceTimer -= Time.deltaTime;
        }

        private void TypeWithKeyboard()
        {
            if (Regex.Match(_keyboardHolder.text, @"[a-zA-Z]").Success)
            {
                AddNewLetter(_keyboardHolder.text);
            }
            else if (Regex.Match(_keyboardHolder.text, @"ý").Success || Regex.Match(_keyboardHolder.text, @"I").Success || Regex.Match(_keyboardHolder.text, @"i").Success || Regex.Match(_keyboardHolder.text, @"Ý").Success)
            {
                _keyboardHolder.text = "i";
                AddNewLetter(_keyboardHolder.text);
            }
            _keyboardHolder.text = "";
            if (gameManager._ourWord.Length > 5)
            {
                RemoveLastLetter();
            }
        }

        public void AddNewLetter(string letter)
        {
            if (!GameManager._canPlay) return;
            if (gameManager._ourWord.Length >= 5) return;
            if (letter.Length == 0) return;

            gameManager._ourWord += letter.ToLower();

            gameManager._letters.Where(a => a.transform.parent == GameObject.Find($"Word_{gameManager._currentLine}").transform).ToList().ForEach(delegate (LetterInGame l)
            {
                l.WordType = WordType.Default;
                l.WriteLetter();
            });

            for (int i = 1; i <= gameManager._ourWord.Length; i++)
            {
                string k = gameManager._ourWord[i - 1].ToString();
                if (k == "i" || k == "ý") k = "I";
                GameObject.Find($"Word_{gameManager._currentLine}").transform.Find(i.ToString()).GetComponent<LetterInGame>().letter = k.ToLower()[0];
                GameObject.Find($"Word_{gameManager._currentLine}").transform.Find(i.ToString()).GetComponent<LetterInGame>().WriteLetter();
                GameObject.Find($"Word_{gameManager._currentLine}").transform.Find(i.ToString()).GetComponent<LetterInGame>().WordType = WordType.Written;
            }
        }

        public void RemoveLastLetter()
        {
            _keyboardHolder.text = "";
            if (!GameManager._canPlay) return;
            if (gameManager._ourWord.Length == 0) return;

            IEnumerable<char> _word =
                from letter in gameManager._ourWord
                select letter;
            string newWord = "";
            for (int i = 0; i < gameManager._ourWord.Length - 1; i++) newWord += _word.ToList()[i].ToString().ToLower();
            gameManager._ourWord = newWord;

            gameManager._letters.Where(a => a.transform.parent == GameObject.Find($"Word_{gameManager._currentLine}").transform).ToList().ForEach(delegate (LetterInGame l)
            {
                print("sup");
                l.letter = ' ';
                l.WordType = WordType.Default;
                l.WriteLetter();
            });

            for (int i = 1; i <= gameManager._ourWord.Length; i++)
            {
                string k = gameManager._ourWord[i - 1].ToString();
                if (k == "i" || k == "ý") k = "I";
                GameObject.Find($"Word_{gameManager._currentLine}").transform.Find(i.ToString()).GetComponent<LetterInGame>().letter = k.ToLower()[0];
                GameObject.Find($"Word_{gameManager._currentLine}").transform.Find(i.ToString()).GetComponent<LetterInGame>().WriteLetter();
                GameObject.Find($"Word_{gameManager._currentLine}").transform.Find(i.ToString()).GetComponent<LetterInGame>().WordType = WordType.Written;
            }
        }
    }
}