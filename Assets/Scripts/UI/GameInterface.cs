using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MatchForThree
{
    public class GameInterface : MonoBehaviour
    {
        [SerializeField] private Text _count;
        [SerializeField] private Text _countText;
        [SerializeField] private Text _target;
        [SerializeField] private Text _targetText;
        [SerializeField] private Text _score;
        [SerializeField] private Level level;
        [SerializeField] private GameOver gameOver;
        [SerializeField] private Image[] stars;

        private int _starIndex = 0;

        private void Start()
        {
            SetStarsVisibility();
        }

        public void SetScore(int score)
        {
            _score.text = score.ToString();
            int[] starThresholds = { level.score1Star, level.score2Star, level.score3Star };
            int visibleStar = Mathf.Clamp(Array.FindIndex(starThresholds, threshold => score >= threshold) + 1, 0, stars.Length);
            SetStarsVisibility(visibleStar);
        }

        public void SetTarget(int target) => _target.text = target.ToString();
        public void SetRemaining(int remaining) => _count.text = remaining.ToString();
        public void SetRemaining(string remaining) => _count.text = remaining;

        public void SetLevelType(LevelType type)
        {
            _countText.text = $"{type.ToString().ToLower()} remaining";
            _targetText.text = type == LevelType.Obstacle ? "bubbles remaining" : "target score";
        }

        public void OnGameWin(int score)
        {
            gameOver.ShowWin(score, _starIndex);

            if (_starIndex > PlayerPrefs.GetInt(SceneManager.GetActiveScene().name, _starIndex))
                PlayerPrefs.SetInt(SceneManager.GetActiveScene().name, 0);
        }

        public void OnGameLose() => gameOver.ShowLose();

        private void SetStarsVisibility(int visibleStar = 0)
        {
            for (int i = 0; i < stars.Length; i++)
                stars[i].enabled = (i == visibleStar);

            _starIndex = visibleStar;
        }
    }
}
