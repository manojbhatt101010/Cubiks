using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour {
	public GameObject loadingScreen, timer;
	public RectTransform icon, line;
	public GameObject background;
	TimerHandler th;

	void Start() {
		th = background.GetComponent<TimerHandler>();
		loadingScreen.SetActive(false);
		timer.SetActive(false);
	}

	void Update() {
		icon.Rotate(new Vector3(0, 0, 10) * Time.deltaTime * 10);
		line.Rotate(new Vector3(0, 10, 0) * Time.deltaTime * 10);

		AndroidBackButton();
	}

	void AndroidBackButton() {
		if(Input.GetKeyDown(KeyCode.Escape)) {
			if(timer.activeSelf) {
				if(th.timerRunning) {
					th.StopTimer();
				}
				else {
					th.HideTimer();
				}
			}
			else {
				Application.Quit();
			}
		}
	}

	public void Simulator() {
		MainUIHandler.menu = 0;
		loadingScreen.SetActive(true);
		SceneManager.LoadScene(1);
	}

	public void SolveMyCube() {
		MainUIHandler.menu = 1;
		loadingScreen.SetActive(true);
		SceneManager.LoadScene(1);
	}

	public void Timer() {
		timer.SetActive(true);
	}

	public void Settings() {
		MainUIHandler.menu = 2;
		loadingScreen.SetActive(true);
		SceneManager.LoadScene(1);
	}

	public void Quit() {
		Application.Quit();
	}

	public void GitHubLink() {
		Application.OpenURL("https://github.com/manojbhatt101010/cubiks");
	}

	public void Email() {
		string email = "manojbhatt101010@gmail.com";
		string subject = "Need Help With Cubik's";
		string body = "** PLEASE READ INSTRUCTIONS **\n1. IF YOU ARE GETTING INVALID COLORS MESSAGE PLEASE READ IN APP GUIDE FIRST.\n2. DON'T FORGET TO SEND THE PICTURES OF YOUR CUBE SO THAT ALL 6 SIDES ARE VISIBLE.\n3. ATLEAST 2 SIDES SHOULD BE VISIBLE IN EVERY PICTURE.\n4. DESCRIBE OTHER PROBLEMS BELOW.\n5. MAILS NOT FOLLOWING THESE INSTRUCTIONS WON'T BE CONSIDERED.\n\n";
		Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
	}

	public void GooglePlayLink() {
		Application.OpenURL("market://details?id=com.manojbhatt.cubiks");
	}
}