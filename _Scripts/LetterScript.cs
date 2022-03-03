using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace tzdevil.WordGameInfinite
{
    public enum LetterType { Default, Wrong, CorrectLetterWrongSpot, CorrectLetter }

    public class LetterScript : MonoBehaviour
    {
        private GameManager _gameManager => GameObject.Find("GameplayManager").GetComponent<GameManager>();
        private KeyRelated _keyRelated => GameObject.Find("GameplayManager").GetComponent<KeyRelated>();
        [HideInInspector] public LetterType letterType;

        private void Start()
        {
            transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;

            EventTrigger trigger = GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { OnPointerClick((PointerEventData)data); });
            trigger.triggers.Add(entry);
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            string k = pointerEventData.pointerDrag.name.ToLower();
            if (k == "ý") k = "i"; // TODO.
            _keyRelated.AddNewLetter(k);
            if (!_gameManager.startedGame) _gameManager.startedGame = true;
        }

        private void Update() => CheckLetterType();

        private void CheckLetterType() => GetComponent<Image>().sprite = letterType switch
        {
            LetterType.Default => _gameManager.defaultKeySprite,
            LetterType.Wrong => _gameManager.wrongKeySprite,
            LetterType.CorrectLetterWrongSpot => _gameManager.correctLetterButWrongSpotKeySprite,
            LetterType.CorrectLetter => _gameManager.correctKeySprite,
            _ => throw new System.NotImplementedException()
        };
    }
}