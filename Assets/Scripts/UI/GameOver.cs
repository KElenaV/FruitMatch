using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MatchForThree
{
    public class GameOver : MonoBehaviour
    {
        [SerializeField] private GameObject _screen;
        [SerializeField] private GameObject _scoreParent;
        [SerializeField] private Text _loseText;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Image[] _stars;

        private readonly int GameOverHash = Animator.StringToHash("GameOver");
        private readonly string LevelSelect = "LevelSelect";
        private WaitForSeconds _waitForSeconds = new WaitForSeconds(0.5f);

        private void Start ()
        {
            _screen.SetActive(false);

            for (int i = 0; i < _stars.Length; i++)
                _stars[i].enabled = false;
        }

        public void ShowLose()
        {
            _screen.SetActive(true);
            _scoreParent.SetActive(false);

            TryAnimateGameOver();
        }

        public void ShowWin(int score, int starCount)
        {
            _screen.SetActive(true);
            _loseText.enabled = false;

            _scoreText.text = score.ToString();
            _scoreText.enabled = false;

            TryAnimateGameOver();

            StartCoroutine(ShowWinCoroutine(starCount));
        }

        private IEnumerator ShowWinCoroutine(int starCount)
        {
            yield return _waitForSeconds;

            if(starCount < _stars.Length)
            {
                for (int i = 0; i <= starCount; i++)
                {
                    _stars[i].enabled = true;
                    _stars[Mathf.Max(0, i - 1)].enabled = false;
                    yield return _waitForSeconds;
                }
            }

            _scoreText.enabled = true;
        }

        private void TryAnimateGameOver()
        {
            if (TryGetComponent(out Animator animator))
                animator.Play(GameOverHash);
        }

        public void OnReplayClicked() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        public void OnDoneClicked() => SceneManager.LoadScene(LevelSelect);
    }
}
