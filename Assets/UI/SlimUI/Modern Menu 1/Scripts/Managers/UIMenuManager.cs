using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace SlimUI.ModernMenu{
	public class UIMenuManager : MonoBehaviour {

		public static UIMenuManager _instance;

		private Animator CameraObject;

		// campaign button sub menu
        [Header("MENUS")]
        [Tooltip("The Menu for when the MAIN menu buttons")]
        public GameObject mainMenu;		// 모든 메뉴를 갖고 있는 상위 메뉴 (Camera, Canv_Main, Canv_Options, Canv_Lobby, Canv_Room)
        [Tooltip("THe first list of buttons")]
        public GameObject firstMenu;	// 게임 실행 시 처음 보이는 메뉴(Play, Settings, Developers, Exit 버튼들 보이는 것들)
        [Tooltip("The Menu for when the PLAY button is clicked")]
        public GameObject playMenu;		// 싱글, 멀티 플레이 버튼
		[Tooltip("멀티 플레이 버튼 클릭 시 나오는 로비 메뉴")]
		public GameObject lobbyMenu;	// 로비 메뉴(Canv_Lobby)
        [Tooltip("설정 메뉴")]
        public GameObject settingsMenu; // 설정 메뉴(Canv_Options)
        [Tooltip("개발자 표시되어 있는 메뉴")]
        public GameObject developersMenu;
        [Tooltip("The Menu for when the EXIT button is clicked")]
        public GameObject exitMenu;	// 종료 버튼 눌렀을 때 나오는 버튼

        public enum Theme {custom1, custom2, custom3};
        [Header("THEME SETTINGS")]
        public Theme theme;
        private int themeIndex;
        public ThemedUIData themeController;

        [Header("PANELS")]
        [Tooltip("The UI Panel parenting all sub menus")]
        public GameObject mainCanvas;
        [Tooltip("The UI Panel that holds the CONTROLS window tab")]
        public GameObject PanelControls;
        [Tooltip("The UI Panel that holds the VIDEO window tab")]
        public GameObject PanelVideo;
        [Tooltip("The UI Panel that holds the GAME window tab")]
        public GameObject PanelGame;
        [Tooltip("The UI Panel that holds the KEY BINDINGS window tab")]
        public GameObject PanelKeyBindings;
        [Tooltip("The UI Sub-Panel under KEY BINDINGS for MOVEMENT")]
        public GameObject PanelMovement;
        [Tooltip("The UI Sub-Panel under KEY BINDINGS for COMBAT")]
        public GameObject PanelCombat;
        [Tooltip("The UI Sub-Panel under KEY BINDINGS for GENERAL")]
        public GameObject PanelGeneral;
        

        // highlights in settings screen
        [Header("SETTINGS SCREEN")]
        [Tooltip("Highlight Image for when GAME Tab is selected in Settings")]
        public GameObject lineGame;
        [Tooltip("Highlight Image for when VIDEO Tab is selected in Settings")]
        public GameObject lineVideo;
        [Tooltip("Highlight Image for when CONTROLS Tab is selected in Settings")]
        public GameObject lineControls;
        [Tooltip("Highlight Image for when KEY BINDINGS Tab is selected in Settings")]
        public GameObject lineKeyBindings;
        [Tooltip("Highlight Image for when MOVEMENT Sub-Tab is selected in KEY BINDINGS")]
        public GameObject lineMovement;
        [Tooltip("Highlight Image for when COMBAT Sub-Tab is selected in KEY BINDINGS")]
        public GameObject lineCombat;
        [Tooltip("Highlight Image for when GENERAL Sub-Tab is selected in KEY BINDINGS")]
        public GameObject lineGeneral;

        [Header("LOADING SCREEN")]
		[Tooltip("If this is true, the loaded scene won't load until receiving user input")]
		public bool waitForInput = true;
        public GameObject loadingMenu;
		[Tooltip("The loading bar Slider UI element in the Loading Screen")]
        public Slider loadingBar;
        public TMP_Text loadPromptText;
		public KeyCode userPromptKey;

		[Header("SFX")]
        [Tooltip("The GameObject holding the Audio Source component for the HOVER SOUND")]
        public AudioSource hoverSound;
        [Tooltip("The GameObject holding the Audio Source component for the AUDIO SLIDER")]
        public AudioSource sliderSound;
        [Tooltip("The GameObject holding the Audio Source component for the SWOOSH SOUND when switching to the Settings Screen")]
        public AudioSource swooshSound;

		void Start(){

			_instance = this;

            CameraObject = transform.GetComponent<Animator>();

			playMenu.SetActive(false);
			exitMenu.SetActive(false);
			if(developersMenu) developersMenu.SetActive(false);
			firstMenu.SetActive(true);
			mainMenu.SetActive(true);

			SetThemeColors();
		}

		void SetThemeColors()
		{
			switch (theme)
			{
				case Theme.custom1:
					themeController.currentColor = themeController.custom1.graphic1;
					themeController.textColor = themeController.custom1.text1;
					themeIndex = 0;
					break;
				case Theme.custom2:
					themeController.currentColor = themeController.custom2.graphic2;
					themeController.textColor = themeController.custom2.text2;
					themeIndex = 1;
					break;
				case Theme.custom3:
					themeController.currentColor = themeController.custom3.graphic3;
					themeController.textColor = themeController.custom3.text3;
					themeIndex = 2;
					break;
				default:
					Debug.Log("Invalid theme selected.");
					break;
			}
		}

        // 첫 화면에서 Play 버튼 눌렀을 때 실행
        public void PlayCampaign()
		{
			exitMenu.SetActive(false);
			if(developersMenu) developersMenu.SetActive(false);
			playMenu.SetActive(true);
		}

		public void LobbyMenu() // 로비 메뉴로 전환할 때 다른 UI 비활성화
		{
            if (settingsMenu.activeSelf)
                settingsMenu.SetActive(false);

            exitMenu.SetActive(false);
			if (developersMenu) developersMenu.SetActive(false);
            lobbyMenu.SetActive(true);
        }

        public void SettingsMenu() // 설정 메뉴로 전환
        {
            if (lobbyMenu.activeSelf)
                lobbyMenu.SetActive(false);

            exitMenu.SetActive(false);
            if (developersMenu) developersMenu.SetActive(false);
            settingsMenu.SetActive(true);
        }

        public void ReturnMenu(){
			playMenu.SetActive(false);
			if(developersMenu) developersMenu.SetActive(false);
			exitMenu.SetActive(false);
			mainMenu.SetActive(true);
		}

		public void LoadScene(string scene){
			if(scene != ""){
				StartCoroutine(LoadAsynchronously(scene));
			}
		}

		#region 메뉴 비활성화
		// 플레이 버튼들 비활성화
		public void DisablePlayCampaign()
		{
			playMenu.SetActive(false);
		}

		// 로비 스크린 비활성화
		public void DisableLobbyMenu()
		{
			lobbyMenu.SetActive(false);
		}

		//// 방 스크린 비활성화
		//public void DisableRoomMenu()
		//{
		//	lobbyMenu.SetActive(false);
		//}

		//public void DisableLobbyMenu()
		//{
		//	lobbyMenu.SetActive(false);
		//}

		//public void DisableLobbyMenu()
		//{
		//	lobbyMenu.SetActive(false);
		//}
		#endregion

		#region 카메라 애니메이션
		public void MainToLobbyCamPos(){ // 메인에서 로비로 이동
            CameraObject.SetInteger("Animate", 1);
		}

		public void LobbyToMainCamPos(){ // 로비에서 메인으로 이동
			DisablePlayCampaign();
			CameraObject.SetInteger("Animate",2);
		}

        public void LobbyToRoomCamPos() // 로비에서 방으로 이동
        {
            DisablePlayCampaign();
            CameraObject.SetInteger("Animate", 3);
        }

        public void RoomToLobbyCamPos() // 방에서 로비로 이동
        {
            DisablePlayCampaign();
            CameraObject.SetInteger("Animate", 4);
        }

        public void MainToSettingCamPos() // 메인에서 설정으로 이동
        {
            DisablePlayCampaign();
            CameraObject.SetInteger("Animate", 5);
        }
        public void SettingToMainCamPos() // 설정에서 메인으로 이동
        {
            DisablePlayCampaign();
            CameraObject.SetInteger("Animate", 6);
        }
        #endregion


        void DisablePanels(){
			PanelControls.SetActive(false);
			PanelVideo.SetActive(false);
			PanelGame.SetActive(false);
			PanelKeyBindings.SetActive(false);

			lineGame.SetActive(false);
			lineControls.SetActive(false);
			lineVideo.SetActive(false);
			lineKeyBindings.SetActive(false);

			PanelMovement.SetActive(false);
			lineMovement.SetActive(false);
			PanelCombat.SetActive(false);
			lineCombat.SetActive(false);
			PanelGeneral.SetActive(false);
			lineGeneral.SetActive(false);
		}

		public void GamePanel(){
			DisablePanels();
			PanelGame.SetActive(true);
			lineGame.SetActive(true);
		}

		public void VideoPanel(){
			DisablePanels();
			PanelVideo.SetActive(true);
			lineVideo.SetActive(true);
		}

		public void ControlsPanel(){
			DisablePanels();
			PanelControls.SetActive(true);
			lineControls.SetActive(true);
		}

		public void KeyBindingsPanel(){
			DisablePanels();
			MovementPanel();
			PanelKeyBindings.SetActive(true);
			lineKeyBindings.SetActive(true);
		}

		public void MovementPanel(){
			DisablePanels();
			PanelKeyBindings.SetActive(true);
			PanelMovement.SetActive(true);
			lineMovement.SetActive(true);
		}

		public void CombatPanel(){
			DisablePanels();
			PanelKeyBindings.SetActive(true);
			PanelCombat.SetActive(true);
			lineCombat.SetActive(true);
		}

		public void GeneralPanel(){
			DisablePanels();
			PanelKeyBindings.SetActive(true);
			PanelGeneral.SetActive(true);
			lineGeneral.SetActive(true);
		}

		public void PlayHover(){
			hoverSound.Play();
		}

		public void PlaySFXHover(){
			sliderSound.Play();
		}

		public void PlaySwoosh(){
			swooshSound.Play();
		}

		// Are You Sure - Quit Panel Pop Up
		public void AreYouSure(){
			exitMenu.SetActive(true);
			if(developersMenu) developersMenu.SetActive(false);
			DisablePlayCampaign();
		}

		public void ExtrasMenu(){
			playMenu.SetActive(false);
			if(developersMenu) developersMenu.SetActive(true);
			exitMenu.SetActive(false);
		}

		public void QuitGame(){
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		}

		// Load Bar synching animation
		IEnumerator LoadAsynchronously(string sceneName){ // scene name is just the name of the current scene being loaded
			AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
			operation.allowSceneActivation = false;
			mainCanvas.SetActive(false);
			loadingMenu.SetActive(true);

			while (!operation.isDone){
				float progress = Mathf.Clamp01(operation.progress / .95f);
				loadingBar.value = progress;

				if (operation.progress >= 0.9f && waitForInput){
					loadPromptText.text = "Press " + userPromptKey.ToString().ToUpper() + " to continue";
					loadingBar.value = 1;

					if (Input.GetKeyDown(userPromptKey)){
						operation.allowSceneActivation = true;
					}
                }else if(operation.progress >= 0.9f && !waitForInput){
					operation.allowSceneActivation = true;
				}

				yield return null;
			}
		}
	}
}