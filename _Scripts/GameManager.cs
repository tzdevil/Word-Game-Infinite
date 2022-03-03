using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

namespace tzdevil.WordGameInfinite
{
    public class GameManager : MonoBehaviour
    {
        public string _guessThisWord;

        public string _ourWord;
        public int _currentLine;

        public Sprite writtenSprite;
        public Sprite defaultSprite, wrongSprite, correctLetterButWrongSpotSprite, correctSprite;
        public Sprite defaultKeySprite, wrongKeySprite, correctLetterButWrongSpotKeySprite, correctKeySprite;

        [HideInInspector] public static bool _canPlay;

        public float _gameTime;
        [SerializeField] private TextMeshProUGUI _gameTimeText;
        [HideInInspector] public bool startedGame;

        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private TextMeshProUGUI _timeInSettingsPanel;

        private GameObject _resetButton_InGame;

        private AudioSource _audio => GetComponent<AudioSource>();
        private bool _audioPref;

        private static List<string> _words = new();

        private static List<string> _textData = new();

        // TODO: datayý daha iyi derlemek için matrix array'e çevirebilirim.
        public List<LetterInGame> _letters = new();
        [HideInInspector] public List<LetterScript> _keys = new();

        private void Awake() => Setup();

        private void SetupLetterPooling() => _letters
            .AddRange(GameObject.FindGameObjectsWithTag("GuessLetter")
                .Select(g => g.transform.parent.GetComponent<LetterInGame>())
            );

        private void SetupKeyPooling() => _keys
            .AddRange(GameObject.FindGameObjectsWithTag("Letter")
                .Select(g => g.transform.GetComponent<LetterScript>())
            );

        private void Setup()
        {
            _resetButton_InGame = GameObject.Find("ResetButton_InGame");
            _audioPref = false;
            _audio.enabled = _audioPref;
            _resetButton_InGame.SetActive(false);
            startedGame = false;
            _gameTime = 0;
            _canPlay = true;
            _currentLine = 1;
            SetupLetterPooling();
            SetupKeyPooling();
            ReadData();
            KeyRelated._keyboardHolder.ActivateInputField();
        }

        private void ReadData()
        {
            TextAsset _data = Resources.Load<TextAsset>("EnglishWords");
            string[] wordData = _data.text.Split("\n");
            IEnumerable<string> newData =
                from word in wordData
                orderby word ascending
                select word[..5];

            _textData = newData.ToList();
            _guessThisWord = _textData[Random.Range(0, _textData.Count)];
        }

        public void EnterWord()
        {
            KeyRelated._keyboardHolder.text = "";
            if (_ourWord.Length != 5) { ErrorLog("Not enough letters.", 4); return; } // tuþu deaktif edeceðim.

            if (!_textData.Contains(_ourWord)) { ErrorLog("Not in word list.", 4); return; } // kelime listede yoksa

            if (_words.Contains(_ourWord)) { ErrorLog("You already wrote this word.", 4); return; }

            if (_guessThisWord != _ourWord) { WrongGuess(); return; }

            if (_guessThisWord == _ourWord) { CorrectGuess(); return; }
        }

        private void WrongGuess()
        {
            List<LetterInGame> letterList = _letters
                                    .Where(k => k.transform.parent.name[^1] - '0' == _currentLine)
                                    .ToList();

            for (int i = 0; i < letterList.Count; i++) { 
                LetterInGame v = letterList[i];
                if (_guessThisWord.Contains(v.letter.ToString().ToLower()))
                {
                    if (v.letter == _guessThisWord[i])
                    {
                        v.WordType = WordType.CorrectLetter;
                        _letters.Remove(v);
                        _keys.FirstOrDefault(a => a.gameObject.name.ToLower() == v.letter.ToString()).letterType = LetterType.CorrectLetter;
                    }
                    else
                    {
                        int sum1 = _ourWord.Count(a => v.letter.ToString().ToLower() == a.ToString().ToLower());
                        v.WordType = WordType.CorrectLetterWrongSpot;
                        _letters.Remove(v);
                        _keys.FirstOrDefault(a => a.gameObject.name.ToLower() == v.letter.ToString()).letterType = LetterType.CorrectLetterWrongSpot;
                    }
                }
                else
                {
                    v.WordType = WordType.Wrong;
                    _letters.Remove(v);
                    _keys.FirstOrDefault(a => a.gameObject.name.ToLower() == v.letter.ToString()).letterType = LetterType.Wrong;
                }
            }

            _words.Add(_ourWord);

            if (_currentLine == 6)
            {
                YouLost();
                return;
            }

            _currentLine++;
            _ourWord = "";
        }

        private void CorrectGuess()
        {
            IEnumerable<int> correctLetters =
                from letter in _ourWord
                from _letter in _guessThisWord
                where letter == _letter && _ourWord.IndexOf(letter) == _guessThisWord.IndexOf(_letter)
                select _ourWord.IndexOf(letter);
            foreach (int v in correctLetters) GameObject.Find($"Word_{_currentLine}").transform.Find((v + 1).ToString()).GetComponent<LetterInGame>().WordType = WordType.CorrectLetter;
            _words.Add(_ourWord);
            _canPlay = false;
            for (int i = 1; i <= _ourWord.Length; i++)
            {
                GameObject.Find($"Word_{_currentLine}").transform.Find(i.ToString()).Find("Letter").GetComponent<TextMeshProUGUI>().text = _ourWord[i - 1].ToString().ToUpper();
                GameObject.Find($"Word_{_currentLine}").transform.Find(i.ToString()).GetComponent<LetterInGame>().WordType = WordType.CorrectLetter;
            }
            ErrorLog($"Correct! The word was <color=#B59F3B>{_guessThisWord}</color>.\n You guessed it in <color=#538D4E>{_gameTime:n1}</color> seconds.", 999);
            _resetButton_InGame.SetActive(true);
        }

        private void YouLost()
        {
            ErrorLog($"You lost. The word was <color=#B59F3B>{_guessThisWord}</color>.", 999);
            _canPlay = false;
            _resetButton_InGame.SetActive(true);
        }

        public static void ErrorLog(string error, float _time)
        {
            KeyRelated._errorLog.text = error;
            KeyRelated._errorTimer = _time;
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
            _ourWord = "";
            _currentLine = 1;
            _words.Clear();
            _gameTimeText.color = new Color32(185, 185, 185, 255);
            startedGame = false;
            _canPlay = true;

            foreach (var v in _letters)
            {
                v.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
                v.WordType = WordType.Default;
                v.ResetAnim();
            }

            foreach (var v in _keys)
                v.letterType = LetterType.Default;

            _guessThisWord = _textData[Random.Range(0, _textData.Count)];
            //_gameTime = 0;
            KeyRelated._errorTimer = 0.1f;
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