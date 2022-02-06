using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace tzdevil.WordGameInfinite
{
    public class GameManager : MonoBehaviour
    {
        private string _guessThisWord;

        public string _ourWord;
        private int _currentLine;

        [SerializeField] private GameObject _enterKey, _backSpaceKey;
        [SerializeField] private Sprite _enterKeyNormal, _enterKeyForbidden, _backSpaceKeyNormal, _backSpaceKeyForbidden;

        public Sprite writtenSprite;
        public Sprite defaultSprite, wrongSprite, correctLetterButWrongSpotSprite, correctSprite;
        public Sprite defaultKeySprite, wrongKeySprite, correctLetterButWrongSpotKeySprite, correctKeySprite;

        [SerializeField] private TextMeshProUGUI _errorLog;
        private float _errorTimer;

        private bool _canPlay;

        private float _gameTime;
        [SerializeField] private TextMeshProUGUI _gameTimeText;
        [HideInInspector] public bool startedGame;

        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private TextMeshProUGUI _timeInSettingsPanel;

        [SerializeField] private GameObject _resetButton_InGame;

        private AudioSource _audio;
        private bool _audioPref; // false = kapalý.

        private List<string> _words = new();

        [SerializeField] private TMP_InputField _keyboardHolder;

        private float _backspaceTimer;

        private List<string> _textData = new();

        private void Awake() => Setup();

        void Setup()
        {
            _keyboardHolder.ActivateInputField();
            _audio = GetComponent<AudioSource>();
            _audioPref = false;
            _audio.enabled = _audioPref;
            _resetButton_InGame.SetActive(false);
            startedGame = false;
            _gameTime = 0;
            _canPlay = true;
            ReadData();
            // þu anki lineý belirle.
            _currentLine = 1;
            // yukarýdaki bütün harfleri "" yap.
            GameObject.FindGameObjectsWithTag("GuessLetter").ToList().ForEach(delegate (GameObject g) { g.GetComponent<TextMeshProUGUI>().text = ""; });
        }

        private void Update()
        {
            TextAndColor();
            CheckKey();
        }

        void TextAndColor()
        {
            if (startedGame && _canPlay) _gameTime += Time.deltaTime;
            if (!_canPlay) _gameTimeText.color = new Color32(83, 141, 78, 255);

            _enterKey.GetComponent<Image>().sprite = _ourWord.Length == 5 ? _enterKeyNormal : _enterKeyForbidden;
            _enterKey.GetComponent<Image>().raycastTarget = _ourWord.Length == 5 ? true : false;

            _backSpaceKey.GetComponent<Image>().sprite = _ourWord.Length != 0 ? _backSpaceKeyNormal : _backSpaceKeyForbidden;
            _backSpaceKey.GetComponent<Image>().raycastTarget = _ourWord.Length != 0 ? true : false;

            _gameTimeText.text = _gameTime.ToString("0.0s");

            if (_errorTimer > 0) _errorTimer -= Time.deltaTime;
            Color errorColor = _errorLog.color;
            errorColor.a = _errorTimer / 2;
            _errorLog.color = errorColor;
        }

        void CheckKey()
        {
            if (Input.anyKey)
            {
                _keyboardHolder.ActivateInputField();
                if (_canPlay && !Input.GetKey(KeyCode.Backspace) && !(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !Input.GetKeyDown(KeyCode.Space) && _ourWord.Length <= 4)
                {
                    if (!startedGame) startedGame = true;
                    TypeWithKeyboard();
                }
                if (Input.GetKey(KeyCode.Backspace) && _backspaceTimer < 0) { _backspaceTimer = .1f; RemoveLastLetter(); }
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) EnterWord();
                _keyboardHolder.text = "";
            }
        }

        void TypeWithKeyboard()
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
            if (_ourWord.Length > 5)
            {
                RemoveLastLetter();
            }
        }

        void ReadData()
        {
            TextAsset _data = Resources.Load<TextAsset>("EnglishWords");
            string[] wordData = _data.text.Split("\n");
            IEnumerable<string> newData =
                from word in wordData
                orderby word ascending
                select word.Substring(0, 5);

            _textData = newData.ToList();
            _guessThisWord = _textData[Random.Range(0, _textData.Count)];
        }

        public void AddNewLetter(string letter)
        {
            if (!_canPlay) return;
            if (_ourWord.Length >= 5) return;
            if (letter.Length == 0) return;

            _ourWord += letter.ToLower();
            GameObject.FindGameObjectsWithTag("GuessLetter").Where(a => a.transform.parent == GameObject.Find($"Word_{_currentLine}").transform).ToList().ForEach(delegate (GameObject g) { g.GetComponent<TextMeshProUGUI>().text = ""; });
            GameObject.FindGameObjectsWithTag("GuessLetter").Where(a => a.transform.parent == GameObject.Find($"Word_{_currentLine}").transform).ToList().ForEach(delegate (GameObject g) { g.GetComponent<LetterInGame>().wordType = WordType.Default; });

            for (int i = 1; i <= _ourWord.Length; i++)
            {
                string k = _ourWord[i - 1].ToString();
                if (k == "i" || k == "ý") k = "I";
                GameObject.Find($"Word_{_currentLine}").transform.Find(i.ToString()).Find("Letter").GetComponent<TextMeshProUGUI>().text = k.ToUpper();
                GameObject.Find($"Word_{_currentLine}").transform.Find(i.ToString()).GetComponent<LetterInGame>().wordType = WordType.Written;
            }
        }

        public void RemoveLastLetter()
        {
            _keyboardHolder.text = "";
            if (!_canPlay) return;
            if (_ourWord.Length == 0) return;

            IEnumerable<char> _word =
                from letter in _ourWord
                select letter;
            string newWord = "";
            for (int i = 0; i < _ourWord.Length - 1; i++) newWord += _word.ToList()[i].ToString().ToLower();
            _ourWord = newWord;
            GameObject.FindGameObjectsWithTag("GuessLetter").Where(a => a.transform.parent.parent == GameObject.Find($"Word_{_currentLine}").transform).ToList().ForEach(delegate (GameObject g) { g.GetComponent<TextMeshProUGUI>().text = ""; });
            GameObject.FindGameObjectsWithTag("GuessLetter").Where(a => a.transform.parent == GameObject.Find($"Word_{_currentLine}").transform).ToList().ForEach(delegate (GameObject g) { g.GetComponent<LetterInGame>().wordType = WordType.Default; });

            for (int i = 1; i <= _ourWord.Length; i++)
            {
                string k = _ourWord[i - 1].ToString();
                if (k == "i" || k == "ý") k = "I";
                GameObject.Find($"Word_{_currentLine}").transform.Find(i.ToString()).Find("Letter").GetComponent<TextMeshProUGUI>().text = k.ToUpper();
                GameObject.Find($"Word_{_currentLine}").transform.Find(i.ToString()).GetComponent<LetterInGame>().wordType = WordType.Written;
            }
        }

        public void EnterWord()
        {
            _keyboardHolder.text = "";
            if (_ourWord.Length != 5) { ErrorLog("Not enough letters.", 4); return; } // tuþu deaktif edeceðim.

            if (!_textData.Contains(_ourWord)) { ErrorLog("Not in word list.", 4); return; } // kelime listede yoksa

            if (_words.Contains(_ourWord)) { ErrorLog("You already wrote this word.", 4); return; }

            if (_guessThisWord != _ourWord) { WrongGuess(); return; }

            if (_guessThisWord == _ourWord) { CorrectGuess(); return; }
        }

        public void ErrorLog(string error, float _time)
        {
            _errorLog.text = error;
            _errorTimer = _time;
        }

        void WrongGuess()
        {
            for (int i = 0; i < 5; i++)
            {
                if (_ourWord[i] == _guessThisWord[i])
                {
                    GameObject.Find($"Word_{_currentLine}").transform.Find((i + 1).ToString()).GetComponent<LetterInGame>().wordType = WordType.CorrectLetter;
                    GameObject.FindGameObjectsWithTag("Letter").FirstOrDefault(a => a.name.ToLower() == _ourWord[i].ToString().ToLower()).GetComponent<LetterScript>().letterType = LetterType.CorrectLetter;
                }
                else
                {
                    if (_guessThisWord.ToCharArray().Contains(_ourWord[i]))
                    {
                        if (_ourWord.Where(a => a == _ourWord[i]).ToList().Count > _guessThisWord.Where(a => a == _ourWord[i]).ToList().Count)
                        {
                            int _sum = 0;
                            foreach (char x in _ourWord.Substring(0, i + 1)) // 
                            {
                                _sum += (GameObject.Find($"Word_{_currentLine}").transform.Find((_ourWord.IndexOf(x) + 1).ToString()).GetComponent<LetterInGame>().wordType == WordType.CorrectLetter || GameObject.Find($"Word_{_currentLine}").transform.Find((_ourWord.IndexOf(x) + 1).ToString()).GetComponent<LetterInGame>().wordType == WordType.CorrectLetterWrongSpot) ? 1 : 0;
                            }
                            if (_sum < _ourWord.Where(a => a == _ourWord[i]).ToList().Count)
                            {
                                GameObject.Find($"Word_{_currentLine}").transform.Find((i + 1).ToString()).GetComponent<LetterInGame>().wordType = WordType.CorrectLetterWrongSpot;
                                GameObject.FindGameObjectsWithTag("Letter").FirstOrDefault(a => a.name.ToLower() == _ourWord[i].ToString().ToLower()).GetComponent<LetterScript>().letterType = LetterType.CorrectLetterWrongSpot;
                            }
                            else // >= mý > mý?
                            {
                                GameObject.Find($"Word_{_currentLine}").transform.Find((i + 1).ToString()).GetComponent<LetterInGame>().wordType = WordType.Wrong;
                                GameObject.FindGameObjectsWithTag("Letter").FirstOrDefault(a => a.name.ToLower() == _ourWord[i].ToString().ToLower()).GetComponent<LetterScript>().letterType = LetterType.Wrong;
                            }
                            int newSum = 0;
                            for (int x = i; x < 5; x++)
                            {
                                newSum += (GameObject.Find($"Word_{_currentLine}").transform.Find((x + 1).ToString()).GetComponent<LetterInGame>().wordType == WordType.CorrectLetter || GameObject.Find($"Word_{_currentLine}").transform.Find((x + 1).ToString()).GetComponent<LetterInGame>().wordType == WordType.CorrectLetterWrongSpot) ? 1 : 0;
                            }
                            if (newSum == _guessThisWord.Where(a => a == _ourWord[i]).ToList().Count)
                            {
                                GameObject.Find($"Word_{_currentLine}").transform.Find((i + 1).ToString()).GetComponent<LetterInGame>().wordType = WordType.Wrong;
                                GameObject.FindGameObjectsWithTag("Letter").FirstOrDefault(a => a.name.ToLower() == _ourWord[i].ToString().ToLower()).GetComponent<LetterScript>().letterType = LetterType.Wrong;
                            }
                        }
                        else
                        {
                            GameObject.Find($"Word_{_currentLine}").transform.Find((i + 1).ToString()).GetComponent<LetterInGame>().wordType = WordType.CorrectLetterWrongSpot;
                            GameObject.FindGameObjectsWithTag("Letter").FirstOrDefault(a => a.name.ToLower() == _ourWord[i].ToString().ToLower()).GetComponent<LetterScript>().letterType = LetterType.CorrectLetterWrongSpot;
                        }
                    }
                    else
                    {
                        GameObject.Find($"Word_{_currentLine}").transform.Find((i + 1).ToString()).GetComponent<LetterInGame>().wordType = WordType.Wrong;
                        GameObject.FindGameObjectsWithTag("Letter").FirstOrDefault(a => a.name.ToLower() == _ourWord[i].ToString().ToLower()).GetComponent<LetterScript>().letterType = LetterType.Wrong;
                    }
                }
            }

            _words.Add(_ourWord);

            if (_words.Count == 2 && _words[0] == "guile" && _words[1] == "solar")
            {
                ErrorLog("<color=#feda55>BROOOOOOOOOO</color>", 5);
            }

            if (_currentLine == 6)
            {
                YouLost();
                return;
            }

            _currentLine++;
            _ourWord = "";
        }

        void CorrectGuess()
        {
            IEnumerable<int> correctLetters =
                from letter in _ourWord
                from _letter in _guessThisWord
                where letter == _letter && _ourWord.IndexOf(letter) == _guessThisWord.IndexOf(_letter)
                select _ourWord.IndexOf(letter);
            foreach (int v in correctLetters) GameObject.Find($"Word_{_currentLine}").transform.Find((v + 1).ToString()).GetComponent<LetterInGame>().wordType = WordType.CorrectLetter;
            _words.Add(_ourWord);
            _canPlay = false;
            for (int i = 1; i <= _ourWord.Length; i++)
            {
                GameObject.Find($"Word_{_currentLine}").transform.Find(i.ToString()).Find("Letter").GetComponent<TextMeshProUGUI>().text = _ourWord[i - 1].ToString().ToUpper();
                GameObject.Find($"Word_{_currentLine}").transform.Find(i.ToString()).GetComponent<LetterInGame>().wordType = WordType.CorrectLetter;
            }
            ErrorLog($"Correct! The word was <color=#B59F3B>{_guessThisWord}</color>.\n You guessed it in <color=#538D4E>{_gameTime:0.0}</color> seconds.", 999);
            _resetButton_InGame.SetActive(true);
        }

        void YouLost()
        {
            ErrorLog($"You lost. The word was <color=#B59F3B>{_guessThisWord}</color>.", 999);
            _canPlay = false;
            _resetButton_InGame.SetActive(true);
        }

        public void OpenSettings()
        {
            _timeInSettingsPanel.text = $"Time: {_gameTime:0.0}s";
            _canPlay = false;
            _settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            _canPlay = true;
            _settingsPanel.SetActive(false);
            _gameTimeText.color = new Color32(185, 185, 185, 255);
        }

        public void ResetGame()
        {
            // deðerleri sýfýrlayacaðým.
            _ourWord = "";
            _currentLine = 1;
            _words.Clear();
            _gameTimeText.color = new Color32(185, 185, 185, 255);
            // bool'larý deaktif edeceðim.
            startedGame = false;
            _canPlay = true;
            // her þeyi sileceðim.
            GameObject.FindGameObjectsWithTag("GuessLetter").ToList().ForEach(delegate (GameObject g) { g.GetComponent<TextMeshProUGUI>().text = ""; });
            GameObject.FindGameObjectsWithTag("GuessLetter").ToList().ForEach(delegate (GameObject g) { g.transform.parent.GetComponent<LetterInGame>().wordType = WordType.Default; });
            GameObject.FindGameObjectsWithTag("Letter").ToList().ForEach(delegate (GameObject g) { g.GetComponent<LetterScript>().letterType = LetterType.Default; });
            // yeni kelime bulacaðým.
            _guessThisWord = _textData[Random.Range(0, _textData.Count)];
            // timeý sýfýrlayacaðým.
            _gameTime = 0;
            _errorTimer = 0.1f;
            // buton ve panelleri kapatacaðým.
            _settingsPanel.SetActive(false);
            _resetButton_InGame.SetActive(false);
        }

        public void OnOffMusic()
        {
            _audioPref = !_audioPref;
            _audio.enabled = _audioPref;
        }
    }
}