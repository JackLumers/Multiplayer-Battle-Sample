using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class PlayerScoreElement : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TextMeshProUGUI _text;
        
        public void SetScore(string prefix, string score)
        {
            _text.text = $"{prefix} : {score}";
        }

        public void SetBackgroundColor(Color color)
        {
            _backgroundImage.color = color;
        }
    }
}