using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace tzdevil.WordGameInfinite
{
    public enum WordType { Default, Written, Wrong, CorrectLetterWrongSpot, CorrectLetter }

    public class LetterInGame : MonoBehaviour
    {
        private GameManager _gm;
        [HideInInspector] public WordType wordType;

        void Awake() => _gm = GameObject.Find("GameplayManager").GetComponent<GameManager>();

        private void Update() => CheckWordType();

        void CheckWordType() => GetComponent<Image>().sprite = wordType switch
        {
            WordType.Default => _gm.defaultSprite,
            WordType.Written => _gm.writtenSprite,
            WordType.Wrong => _gm.wrongSprite,
            WordType.CorrectLetterWrongSpot => _gm.correctLetterButWrongSpotSprite,
            WordType.CorrectLetter => _gm.correctSprite,
            _ => throw new System.NotImplementedException()
        };
    }
}