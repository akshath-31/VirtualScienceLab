using UnityEngine;

namespace VirtualScienceLab
{
    [DisallowMultipleComponent]
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private GameObject _welcomeScreen;
        [SerializeField] private GameObject _instructionsScreen;

        [Header("Buttons")]
        [SerializeField] private UnityEngine.UI.Button _startButton;
        [SerializeField] private UnityEngine.UI.Button _howToPlayButton;
        [SerializeField] private UnityEngine.UI.Button _exitButton;
        [SerializeField] private UnityEngine.UI.Button _backButton;

        [Header("Simulation Scene")]
        [SerializeField] private string _laboratorySceneName = "SampleScene";

        private bool _loading;

        private void Awake()
        {
            if (_welcomeScreen == null || _instructionsScreen == null || _startButton == null ||
                _howToPlayButton == null || _exitButton == null || _backButton == null)
            {
                Debug.LogError("MainMenuController: assign all screen and button references.", this);
                enabled = false;
                return;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _welcomeScreen.SetActive(true);
            _instructionsScreen.SetActive(false);
            _startButton.onClick.AddListener(StartSimulation);
            _howToPlayButton.onClick.AddListener(ShowInstructions);
            _exitButton.onClick.AddListener(ExitApplication);
            _backButton.onClick.AddListener(ShowWelcome);
        }

        private void StartSimulation()
        {
            if (_loading) return;
            if (!Application.CanStreamedLevelBeLoaded(_laboratorySceneName))
            {
                Debug.LogError("The laboratory scene is missing from the enabled build scene list.", this);
                return;
            }
            _loading = true;
            _startButton.interactable = false;
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_laboratorySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        private void ShowInstructions()
        {
            _welcomeScreen.SetActive(false);
            _instructionsScreen.SetActive(true);
        }

        private void ShowWelcome()
        {
            _instructionsScreen.SetActive(false);
            _welcomeScreen.SetActive(true);
        }

        private void ExitApplication()
        {
            Application.Quit();
            if (Application.isEditor)
                Debug.Log("Exit requested. Application.Quit closes the built application; Play Mode remains open in the Editor.");
        }

        private void OnDestroy()
        {
            if (_startButton != null) _startButton.onClick.RemoveListener(StartSimulation);
            if (_howToPlayButton != null) _howToPlayButton.onClick.RemoveListener(ShowInstructions);
            if (_exitButton != null) _exitButton.onClick.RemoveListener(ExitApplication);
            if (_backButton != null) _backButton.onClick.RemoveListener(ShowWelcome);
        }
    }
}
