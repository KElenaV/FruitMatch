using UnityEngine;

namespace MatchForThree
{
    public class LevelTimer : Level
    {
        [SerializeField] private int _timeInSeconds;
        [SerializeField] private int _targetScore;

        private float _timer;

        private void Start ()
        {
            type = LevelType.Timer;

            hud.SetLevelType(type);
            hud.SetScore(currentScore);
            hud.SetTarget(_targetScore);
            hud.SetRemaining($"{_timeInSeconds / 60}:{_timeInSeconds % 60:00}");
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            hud.SetRemaining($"{(int) Mathf.Max((_timeInSeconds - _timer) / 60, 0)}:{(int) Mathf.Max((_timeInSeconds - _timer) % 60, 0):00}");

            if (_timeInSeconds - _timer <= 0)
            {
                if (currentScore >= _targetScore)
                    GameWin();
                else
                    GameLose();
            }
        }
	
    }
}
