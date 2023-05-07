using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Scripts.Player.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI
{
    public class RoundResultElement : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private float _backgroundAlpha = 0.8f;
        [SerializeField] private TextMeshProUGUI _winnerText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private int _timerStep = 100;

        private CancellationTokenSource _timerCts;
        
        public void SetWinnerData(MetaPlayerData metaPlayerData)
        {
            _backgroundImage.color = new Color(
                metaPlayerData.TeamColor.r, 
                metaPlayerData.TeamColor.g, 
                metaPlayerData.TeamColor.b, 
                _backgroundAlpha);
            
            _winnerText.text = $"{metaPlayerData.Name} won!";
        }
        
        /// <returns>
        /// UniTaskVoid instead of void is more optimized.
        /// Also it is not awaitable so client code will not rely on this timer, using server timer instead.
        /// </returns>
        public async UniTaskVoid StartVisualTimer(int millis)
        {
            _timerCts?.Cancel();
            _timerCts = new CancellationTokenSource();
            
            await VisualTimerProcess(millis, _timerCts.Token);
        }

        private async UniTask VisualTimerProcess(int millis, CancellationToken cancellationToken)
        {
            while (millis > 0)
            {
                var seconds = (double)millis / 1000;
                var formattedTime = $"{seconds:f1}";
                
                _timerText.text = $"Next round in: {formattedTime}";

                await UniTask.Delay(_timerStep, DelayType.Realtime, PlayerLoopTiming.FixedUpdate, cancellationToken);
                
                millis -= _timerStep;
            }
        }

        private void OnDestroy()
        {
            _timerCts?.Dispose();
            _timerCts = null;
        }
    }
}