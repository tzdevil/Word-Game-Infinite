using System;
using TMPro;
using UnityEngine;

namespace tzdevil.WordGameInfinite
{
    public enum WordType { Default, Written, Wrong, CorrectLetterWrongSpot, CorrectLetter }

    public class LetterInGame : MonoBehaviour
    {
        public WordType wordType;
        public WordType WordType
        {
            get { return wordType; }

            set
            {
                if (value != wordType)
                {
                    wordType = value;
                    CheckWordType();
                }
            }
        }

        private Animator _letterAnim => GetComponent<Animator>();

        public char letter;

        public bool _canPlayAnim = false;
        public float _canPlayAnimTimer = 0;

        private TextMeshProUGUI letterText => transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        private void Start() => WriteLetter();

        public void WriteLetter() => letterText.text = letter.ToString().ToUpper();

        private void CheckWordType()
        {
            (WordType switch
            {
                WordType.Default => (Action)(() => { _letterAnim.SetBool("isWritten", false);  }),
                WordType.Written => (Action)(() => { _letterAnim.SetBool("isWritten", true); }),
                WordType.Wrong => (Action)(() => { if (_canPlayAnim) _letterAnim.SetTrigger("Grey"); }),
                WordType.CorrectLetterWrongSpot => (Action)(() => { if (_canPlayAnim) _letterAnim.SetTrigger("Yellow"); }),
                WordType.CorrectLetter => (Action)(() => { if (_canPlayAnim) _letterAnim.SetTrigger("Green");}),
                _ => () => { }
            })();
        }

        private void Update() => CheckAnim();

        private void CheckAnim()
        {
            if (_canPlayAnimTimer > 0) _canPlayAnimTimer -= Time.deltaTime;
            if (_canPlayAnimTimer < 0)
            {
                _canPlayAnim = true;
                CheckWordType();
            }
            if (!_canPlayAnim)
            {
                if (_canPlayAnimTimer <= 0 && WordType != WordType.Default && WordType != WordType.Written)
                {
                    _canPlayAnimTimer = (float.Parse(name) - 0.9f) / (4f);
                }
            }
        }

        public void ResetAnim()
        {
            foreach(var v in _letterAnim.parameters) _letterAnim.ResetTrigger(v.name);
            _letterAnim.Play("Idle");
        }
    }
}