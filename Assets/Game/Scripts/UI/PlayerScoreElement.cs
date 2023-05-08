using Game.Scripts.Player.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class PlayerScoreElement : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TextMeshProUGUI _text;

        public void SetData(PlayerMetadata playerMetadata)
        {
            _text.text = $"{playerMetadata.Name} : {playerMetadata.Score}";
            _backgroundImage.color = playerMetadata.TeamColor;
        }
    }
}