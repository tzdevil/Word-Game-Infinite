using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace tzdevil.WordGameInfinite
{
    public enum LetterType { Default, Wrong, CorrectLetterWrongSpot, CorrectLetter }

    public class LetterScript : MonoBehaviour, IPointerClickHandler
    {
        private GameManager _gm;
        [HideInInspector] public LetterType letterType;

        void Awake() => _gm = GameObject.Find("GameplayManager").GetComponent<GameManager>();

        void Start() => transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            string k = name.ToLower();
            if (k == "ý") k = "i";
            _gm.AddNewLetter(k);
            if (!_gm.startedGame) _gm.startedGame = true;
        }

        private void Update() => CheckLetterType();

        void CheckLetterType() => GetComponent<Image>().sprite = letterType switch
        {
            LetterType.Default => _gm.defaultKeySprite,
            LetterType.Wrong => _gm.wrongKeySprite,
            LetterType.CorrectLetterWrongSpot => _gm.correctLetterButWrongSpotKeySprite,
            LetterType.CorrectLetter => _gm.correctKeySprite,
            _ => throw new System.NotImplementedException()
        };
    }
}