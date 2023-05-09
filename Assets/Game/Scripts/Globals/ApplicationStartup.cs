using UnityEngine;

namespace Game.Scripts.Globals
{
    public static class ApplicationStartup
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = ProjectConstants.TargetFrameRate;

            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}