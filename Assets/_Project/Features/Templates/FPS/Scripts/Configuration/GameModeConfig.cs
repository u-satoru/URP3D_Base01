using UnityEngine;
using asterivo.Unity60.Features.Templates.FPS.Data;

namespace asterivo.Unity60.Features.Templates.FPS.Configuration
{
    /// <summary>
    /// FPS Template用ゲームモード設定
    /// ゲームルール、勝利条件、制限時間等を管理
    /// </summary>
    [CreateAssetMenu(menuName = "FPS/Configuration/Game Mode Config")]
    public class GameModeConfig : ScriptableObject
    {
        [Header("Game Mode Settings")]
        [SerializeField] private GameModeType _gameModeType = GameModeType.Deathmatch;
        [SerializeField] private string _gameModeName = "Deathmatch";
        [SerializeField] private string _description = "Eliminate all enemies to win";

        [Header("Match Settings")]
        [SerializeField] private float _matchDurationMinutes = 10f;
        [SerializeField] private int _scoreLimit = 50;
        [SerializeField] private int _killLimit = 25;
        [SerializeField] private bool _enableFriendlyFire = false;

        [Header("Player Settings")]
        [SerializeField] private int _maxPlayers = 8;
        [SerializeField] private float _respawnTime = 5f;
        [SerializeField] private bool _enableRespawn = true;

        [Header("Weapon Settings")]
        [SerializeField] private bool _unlimitedAmmo = false;
        [SerializeField] private float _weaponSpawnRate = 30f;

        // Properties
        public GameModeType GameModeType => _gameModeType;
        public string GameModeName => _gameModeName;
        public string Description => _description;
        public float MatchDurationMinutes => _matchDurationMinutes;
        public int ScoreLimit => _scoreLimit;
        public int KillLimit => _killLimit;
        public bool EnableFriendlyFire => _enableFriendlyFire;
        public int MaxPlayers => _maxPlayers;
        public float RespawnTime => _respawnTime;
        public bool EnableRespawn => _enableRespawn;
        public bool UnlimitedAmmo => _unlimitedAmmo;
        public float WeaponSpawnRate => _weaponSpawnRate;

        public float MatchDurationSeconds => _matchDurationMinutes * 60f;

        /// <summary>
        /// ゲームモードを適用する
        /// </summary>
        /// <param name="gameMode">適用するゲームモード</param>
        public void ApplyGameMode(FPSGameMode gameMode)
        {
            // ゲームモードに応じた設定を適用
            switch (gameMode)
            {
                case FPSGameMode.TeamDeathmatch:
                    _gameModeType = GameModeType.TeamDeathmatch;
                    _gameModeName = "Team Deathmatch";
                    _description = "Team vs Team elimination";
                    _enableFriendlyFire = false;
                    break;
                case FPSGameMode.FreeForAll:
                    _gameModeType = GameModeType.Deathmatch;
                    _gameModeName = "Free For All";
                    _description = "Eliminate all enemies to win";
                    _enableFriendlyFire = false;
                    break;
                case FPSGameMode.Elimination:
                    _gameModeType = GameModeType.SearchAndDestroy;
                    _gameModeName = "Elimination";
                    _description = "Last team standing wins";
                    _enableRespawn = false;
                    break;
                case FPSGameMode.Training:
                    _gameModeType = GameModeType.Deathmatch;
                    _gameModeName = "Training";
                    _description = "Practice mode with unlimited ammo";
                    _unlimitedAmmo = true;
                    _enableRespawn = false;
                    break;
            }
        }
    }

    public enum GameModeType
    {
        Deathmatch,
        TeamDeathmatch,
        CaptureTheFlag,
        Domination,
        SearchAndDestroy
    }
}
