using UnityEngine;
using UnityEngine.UI;

namespace MatchForThree
{
    public class LevelSelect : MonoBehaviour
    {
        [System.Serializable]
        public struct ButtonPlayerPrefs
        {
            public Button levelButton;
            public string playerPrefKey;
        };

        [SerializeField] private ButtonPlayerPrefs[] buttons;

        private const int StarNumber = 3;

        private void Start()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                int score = PlayerPrefs.GetInt(buttons[i].playerPrefKey, 0);

                for (int starIndex = 1; starIndex <= StarNumber; starIndex++)
                {
                    Transform star = buttons[i].levelButton.transform.Find($"star{starIndex}");
                    star.gameObject.SetActive(starIndex <= score);                
                }
            }
        }

        public void OnButtonPress(string levelName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
        }
    }
}
