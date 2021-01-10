using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// using UnityEngine.Networking;
using Kociemba;
using System;

public class MainUIHandler : MonoBehaviour {
	RotateLayers rl;
	public GameObject cube;
	public static int menu = 0;
	GameObject[] stickers = new GameObject[54];
	public GameObject[] cfopSol;

	void Start() {
		rl = cube.GetComponent<RotateLayers>();

		int k = 0;
		for(int i = 0; i < 6; i++) {
			for(int j = 0; j < 9; j++) {
				stickers[k++] = GameObject.Find("" + i + j);
			}
		}

		string info = "";
		Search.solution("UUUUUUUUURRRRRRRRRFFFFFFFFFDDDDDDDDDLLLLLLLLLBBBBBBBBB", out info);

		if(menu == 0) {
			InitializeSimulatorUI();

			foreach(GameObject sticker in stickers) {
				sticker.GetComponent<BoxCollider>().enabled = false;
			}
		}

		else if(menu == 1) {
			InitializeFillColorsUI();

			for(int i = 0; i < 54; i++) {
				if(i != 4 && i != 13 && i != 22 && i != 31 && i != 40 && i != 49) {
					stickers[i].GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f);
				}
			}
		}

		else if(menu == 2) {
			simulatorCanvas.SetActive(false);
			fillColorsCanvas.SetActive(false);
			guideCanvas.SetActive(false);
			settingsCanvas.SetActive(true);
		}

		InitializeSettings();
	}

	public GameObject simulatorCanvas, timer, fillColorsCanvas, guideCanvas, settingsCanvas, alreadySolved, simulatorSettings, pauseTimerIcon;
	public Button pauseTimer;
	void InitializeSimulatorUI() {
		simulatorCanvas.SetActive(true);
		timer.SetActive(PlayerPrefs.GetInt("showTimer") == 0 ? false : true);
		fillColorsCanvas.SetActive(false);
		guideCanvas.SetActive(false);
		settingsCanvas.SetActive(false);
		alreadySolved.SetActive(false);
		simulatorSettings.SetActive(false);
		pauseTimer.gameObject.SetActive(false);
		pauseTimerIcon.SetActive(false);
	}

	public GameObject warning, fillColorsSettings, fillColorsGuide;
	void InitializeFillColorsUI() {
		simulatorCanvas.SetActive(false);
		fillColorsCanvas.SetActive(true);
		guideCanvas.SetActive(false);
		settingsCanvas.SetActive(false);
		warning.SetActive(false);
		fillColorsSettings.SetActive(false);
		fillColorsGuide.SetActive(false);
		RotateLayersWithMouseDrag.canRotate = false;
	}

	public Slider crs, crs1, crs2, lrs, lrs1, sols, sols1, scrs, scrs1, fov, fov1, fov2, fov3;
	public Toggle tt, tt1, wc, wc1;
	bool flag = true;
	void InitializeSettings() {
		crs.value = crs1.value = crs2.value = PlayerPrefs.GetFloat("cubeRotationSensitivity", 4);
		lrs.value = lrs1.value = (int)(PlayerPrefs.GetFloat("layersRotationSensitivity", 0.4f) * 10);
		sols.value = sols1.value = (int)(PlayerPrefs.GetFloat("solutionSpeed", 0.3f) * 3);
		scrs.value = scrs1.value = PlayerPrefs.GetFloat("scrambleSpeed", 10);
		Camera.main.fieldOfView = PlayerPrefs.GetFloat("fieldOfView", 35);
		fov.value = fov1.value = fov2.value = fov3.value = 75 - Camera.main.fieldOfView;
		flag = false;
		tt.isOn = tt1.isOn = PlayerPrefs.GetInt("showTimer", 1) == 0 ? false : true;
		if(tt.isOn) {
			timer.SetActive(true);
		} else {
			timer.SetActive(false);
		}
		wc.isOn = wc1.isOn = PlayerPrefs.GetInt("whiteCross", 0) == 0 ? false : true;
		flag = true;
	}

	public Button solveButton, scrambleButton, resetButton, resumeTimer, resetTimer, nextButton;
	public GameObject currentMoves;
	public Text startText, solverHeaderText;
	bool holdUnInteractable = false, startRotating = false, solvedUsingGuide = false, moreMoves = true;
	Color lerpFrom = Color.white, lerpTo = new Color32(80, 80, 80, 255);
	void Update() {
		if(!rl.isRotating && rl.queue.Count == 0) {
			solveButton.interactable = scrambleButton.interactable = resetButton.interactable = pauseTimer.interactable = resumeTimer.interactable = resetTimer.interactable = true;
			if(!holdUnInteractable) {
				nextButton.interactable = true;
			}
			if(solvedUsingGuide) {
				startRotating = true;
				nextButton.interactable = false;
				holdUnInteractable = true;
				RotateCube.canRotate = true;
				currentMoves.GetComponent<Text>().fontSize = 150;
				solverHeaderText.text = "Solved!";
				moreMoves = false;
				AskForRating();
			}
		}
		if(rl.isRotating && rl.queue.Count > 0) {
			solveButton.interactable = scrambleButton.interactable = resetButton.interactable = pauseTimer.interactable = resumeTimer.interactable = resetTimer.interactable =nextButton.interactable = false;
		}

		if(startRotating) {
			cube.transform.Rotate(new Vector3(0f, 1.5f, 0f) * Time.deltaTime * 30);
		}

		startText.color = Color.Lerp(lerpFrom, lerpTo, Mathf.PingPong(Time.time, 1));
		AndroidBackButton();
		Timer();
	}

	void AndroidBackButton() {
		if(Input.GetKeyDown(KeyCode.Escape)) {
			if(solutionsPanel.activeInHierarchy) {
				HideSolutions();
			} else if(note.activeInHierarchy) {
				HideNote();
			} else if(simulatorSettings.activeInHierarchy) {
				SimulatorSettings();
			} else if(fillColorsGuide.activeInHierarchy) {
				HideFillColorsGuide();
			} else if(fillColorsSettings.activeInHierarchy) {
				FillColorsSettings();
			} else if(guideSettings.activeInHierarchy) {
				GuideSettings();
			} else {
				BackToMainMenu();
			}
		}
	}

	float elapsedTime = 0f;
	public Text time;
	bool runTimer = false, stopOnSolve = false, startOnRotating = false;
	void Timer() {
		if(startOnRotating && RotateLayersWithMouseDrag.isRotating) {
			startOnRotating = false;
			ResumeTimer();
		}

		if(stopOnSolve && rl.cfopSolver.IsSolved()) {
			stopOnSolve = false;
			PauseTimer();
		}

		if(runTimer) {
			elapsedTime += Time.deltaTime;
			time.text = TimeSpan.FromSeconds(elapsedTime).ToString("mm' : 'ss' : 'ff");
		}
	}
 
 	public GameObject resumeIcon;
	public void ResumeTimer() {
		runTimer = true;
		resumeTimer.gameObject.SetActive(false);
		resumeIcon.SetActive(false);
		pauseTimer.gameObject.SetActive(true);
		pauseIcon.SetActive(true);
	}

	public GameObject pauseIcon;
	public void PauseTimer() {
		runTimer = false;
		startOnRotating = false;
		stopOnSolve = false;
		resumeTimer.gameObject.SetActive(true);
		resumeIcon.SetActive(true);
		pauseTimer.gameObject.SetActive(false);
		pauseIcon.SetActive(false);
	}

	public void ResetTimer() {
		PauseTimer();
		time.text = "00 : 00 : 00";
		elapsedTime = 0f;
	}

	public void BackToMainMenu() {
		SceneManager.LoadScene(0);
	}

	public void SimulatorSettings() {
		simulatorSettings.SetActive(!simulatorSettings.activeInHierarchy);
	}

	public void Scramble() {
		PlayerPrefs.SetFloat("layersRSK", PlayerPrefs.GetFloat("scrambleSpeed", 10f));
		string scramble = Tools.randomCube().Substring(0, 25);

		List<string> scrambleAsList = new List<string>();
		for(int i = 0; i < scramble.Length; i++) {
			scrambleAsList.Add(scramble[i].ToString());
		}

		rl.cfopSolver.Clean(ref scrambleAsList);
		for(int i = 0; i < scrambleAsList.Count; i++) {
			rl.queue.Enqueue(scrambleAsList[i]);
		}

		runTimer = false;
		ResetTimer();
		startOnRotating = true;
		stopOnSolve = true;
	}

	public void Reset() {
		SceneManager.LoadScene(1);
	}

	string advancedSolution;
	List<List<string>> cfopSolution;
	public GameObject solutionsPanel, advSolutionText, guideHeader, guide, guideSettings, note;
	public void ShowSolutions() {
		ResetTimer();
		if(rl.cfopSolver.IsSolved()) {
			alreadySolved.SetActive(true);
			StartCoroutine(ShowAlreadySolved());
			return;
		}

		guideCanvas.SetActive(true);
		solutionsPanel.SetActive(true);
		guideHeader.SetActive(false);
		note.SetActive(false);
		guide.SetActive(false);
		guideSettings.SetActive(false);
		ColorBlock cb = cfopButton.colors;
		cb.disabledColor = new Color32(25, 25, 25, 255);
		cfopButton.colors = advancedButton.colors = cb;

		if(PlayerPrefs.GetInt("whiteCross", 0) == 1) {
			int count = 0;
			float temp = PlayerPrefs.GetFloat("layersRSK", 5f);
			PlayerPrefs.SetFloat("layersRSK", 1000f);

			while(rl.cube[22].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color != Color.white && count++ < 3) {
				rl.R(0); rl.M(1); rl.L(1);
				rl.cfopSolver.x(0);
			}
			count = 0;
			while(rl.cube[22].transform.GetChild(0).transform.GetComponent<Renderer>().material.color != Color.white && count++ < 3) {
				rl.F(0); rl.S(0); rl.B(1);
				rl.cfopSolver.z(0);
			}

			PlayerPrefs.SetFloat("layersRSK", temp);
		}

		advancedSolution = GetAdvancedSolution();
		cfopSolution = rl.cfopSolver.Solve();


		advSolutionText.GetComponent<Text>().text = advancedSolution;
		
		for(int i = 0; i < cfopSolution.Count; i++) {
			if(cfopSolution[i].Count == 1 && cfopSolution[i][0] == "y") {
				cfopSol[i].GetComponent<Text>().text = cfopSolution[i][0] + " (Already Solved)";
			}
			else if(cfopSolution[i].Count == 0) {
				cfopSol[i].GetComponent<Text>().text = "Already Solved";
			}
			else {
				string a = "";
				foreach(string s in cfopSolution[i]) {
					a += s + " ";
				}
				cfopSol[i].GetComponent<Text>().text = a;
			}
		}

		for(int i = cfopSolution.Count; i < 7; i++) {
			cfopSol[i].GetComponent<Text>().text = "Already Solved";
		}
	}

	void InitializeGuideUI() {
		InitializeSimulatorUI();
		guideCanvas.SetActive(true);
		solutionsPanel.SetActive(true);
		guideHeader.SetActive(false);
		guide.SetActive(false);
		note.SetActive(false);
	}

	IEnumerator ShowAlreadySolved() {
		yield return new WaitForSeconds(3);
		alreadySolved.SetActive(false);
	}
	
	string GetAdvancedSolution() {
		string s = "";
		for(int i = 0; i < 6; i++) {
			for(int j = 0; j < 9; j++) {
				if(rl.cfopSolver.c[i][j] == rl.cfopSolver.c[0][4]) {
					s += "U";
				} else if(rl.cfopSolver.c[i][j] == rl.cfopSolver.c[1][4]) {
					s += "D";
				} else if(rl.cfopSolver.c[i][j] == rl.cfopSolver.c[2][4]) {
					s += "F";
				} else if(rl.cfopSolver.c[i][j] == rl.cfopSolver.c[3][4]) {
					s += "B";
				} else if(rl.cfopSolver.c[i][j] == rl.cfopSolver.c[4][4]) {
					s += "L";
				} else if(rl.cfopSolver.c[i][j] == rl.cfopSolver.c[5][4]) {
					s += "R";
				} 
			}
		}

		s = s.Substring(0, 9) + s.Substring(45, 9) + s.Substring(18, 9) + s.Substring(9, 9) + s.Substring(36, 9) + s.Substring(27, 9);
		string info = "";
		// string solution = SearchRunTime.solution(s, out info, buildTables: true);
		return Search.solution(s, out info);
	}

	public void FillColorsSettings() {
		fillColorsSettings.SetActive(!fillColorsSettings.activeInHierarchy);
	}

	Color gray = new Color(0.4f, 0.4f, 0.4f);
	public void Done() {
		int[] count = {0, 0, 0, 0, 0, 0};
		for(int i = 0, k = 0; i < 6; i++) {
			for(int j = 0; j < 9; j++) {
				Color color = stickers[k++].GetComponent<Renderer>().material.color;
				if(color == gray) {
					warning.SetActive(true);
					StartCoroutine(ShowError());
					return;
				}
				if(!rl.cfopSolver.colorsMap.ContainsKey(color)) {
					print(color);
					return;
				}
				char c = rl.cfopSolver.colorsMap[color];
				int index = (int)(c - 97);
				if(count[index] == 9) {
					warning.SetActive(true);
					StartCoroutine(ShowError());
					return;
				} else {
					rl.cfopSolver.c[i][j] = c;
					count[index]++;
				}
			}
		}

		if(rl.cfopSolver.Solve() != null) {
			InitializeSimulatorUI();
			RotateLayersWithMouseDrag.canRotate = true;

			foreach(GameObject sticker in stickers) {
				sticker.GetComponent<BoxCollider>().enabled = false;
			}
		}
		else {
			warning.SetActive(true);
			StartCoroutine(ShowError());
		}
	}

	IEnumerator ShowError() {
		yield return new WaitForSeconds(3);
		warning.SetActive(false);
	}

	public void Email() {
		string email = "manojbhatt101010@gmail.com";
		string subject = "Need Help With Cubik's";
		string body = "** PLEASE READ INSTRUCTIONS **\n1. DON'T FORGET TO SEND THE PICTURES OF YOUR CUBE SO THAT ALL 6 SIDES ARE VISIBLE.\n2. ATLEAST 2 SIDES SHOULD BE VISIBLE IN EVERY PICTURE.\n3. DESCRIBE OTHER PROBLEMS BELOW.\n4. MAILS NOT FOLLOWING THESE INSTRUCTIONS WON'T BE CONSIDERED.\n\n";
		Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
	}

	public void HideSolutions() {
		solutionsPanel.SetActive(false);
	}

	public void ShowFillColorsGuide() {
		fillColorsGuide.SetActive(true);
		warning.SetActive(false);
	}

	public void HideFillColorsGuide() {
		fillColorsGuide.SetActive(false);
	}

	public GameObject stepName;
	List<string> currentSolution;
	bool cfopMethod;
	int index;
	public Button cfopButton;
	public void SolveCFOP() {
		ColorBlock cb = cfopButton.colors;
		cb.disabledColor = new Color32(42, 75, 38, 255);
		cfopButton.colors = cb;
		cfopMethod = true;
		currentSolution = new List<string>();
		foreach(List<string> temp in cfopSolution) {
			string a = "";
			foreach(string s in temp) {
				a += s + " ";
			}
			currentSolution.Add(a);
		}
		currentMoves.GetComponent<Text>().fontSize = 75;
		stepName.GetComponent<Text>().text = "CFOP Method!";
		CommonSteps();
	}

	public Button advancedButton;
	public void SolveAdvanced() {
		ColorBlock cb = advancedButton.colors;
		cb.disabledColor = new Color32(42, 75, 38, 255);
		advancedButton.colors = cb;
		cfopMethod = false;
		currentSolution = new List<string>(advancedSolution.Split(null));
		currentSolution.RemoveAt(currentSolution.Count - 1);
		currentMoves.GetComponent<Text>().fontSize = 150;
		stepName.GetComponent<Text>().text = "Advanced Method!";
		CommonSteps();
	}

	public GameObject solutions, successText;
	void CommonSteps() {
		index = 0;
		simulatorCanvas.SetActive(false);
		solutionsPanel.SetActive(false);
		note.SetActive(true);
		guide.SetActive(true);
		guideHeader.SetActive(true);
		guideSettings.SetActive(false);
		PlayerPrefs.SetFloat("layersRSK", PlayerPrefs.GetFloat("solutionSpeed", 0.3f));
		
		successText.gameObject.SetActive(false);
		solutions.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 300, 0);
		Camera.main.transform.position = new Vector3(-13.6f, 7f, -8.45f);
		Camera.main.transform.rotation = Quaternion.Euler(25f, 58f, 0f);
		RotateLayersWithMouseDrag.canRotate = RotateCube.canRotate = cfopButton.interactable = advancedButton.interactable = false;
	}

	public void GuideSettings() {
		guideSettings.SetActive(!guideSettings.activeInHierarchy);
	}

	public void ShowSolutionsReadOnly() {
		solutionsPanel.SetActive(true);
	}

	public void HideNote() {
		note.SetActive(false);
	}

	string[] cfopSteps = {"Cross", "1st F2L Pair", "2nd F2L Pair", "3rd F2L Pair", "4th F2L Pair", "OLL", "PLL"};
	public GameObject start;
	public void NextMove() {
		if(!moreMoves) {
			return;
		}

		start.SetActive(false);
		nextButton.interactable = false;

		if(cfopMethod) {
			stepName.GetComponent<Text>().text = "CFOP Method!\n" + cfopSteps[index];
			currentMoves.GetComponent<Text>().text = cfopSol[index].GetComponent<Text>().text;
		} else {
			currentMoves.GetComponent<Text>().text = "" + currentSolution[index];
		}
		
		foreach(string s in currentSolution[index].Split(null)) {
			rl.queue.Enqueue(s);
		}

		if(index == currentSolution.Count - 1) {
			solvedUsingGuide = true;
			return;
		}

		index++;
	}

	void AskForRating() {
		start.SetActive(true);
		currentMoves.SetActive(false);
		stepName.GetComponent<Text>().text = "Like the app? Please take a moment to Rate it, if you haven't already!";
		startText.GetComponent<Text>().text = "Rate!";
		lerpFrom = new Color32(60, 200, 40, 255);
		lerpTo = new Color32(60, 200, 40, 100);
	}

	public void GooglePlayLink() {
		if(!moreMoves) {
			GooglePlayLinkHelper();
		}
	}

	public void GooglePlayLinkHelper() {
		Application.OpenURL("market://details?id=com.manojbhatt.cubiks");
	}

	public void UpdateCRS() {
		PlayerPrefs.SetFloat("cubeRotationSensitivity", (int)crs.value);
		crs1.value = crs2.value = crs.value;
	}

	public void UpdateCRS1() {
		PlayerPrefs.SetFloat("cubeRotationSensitivity", (int)crs1.value);
		crs.value = crs2.value = crs1.value;
	}

	public void UpdateCRS2() {
		PlayerPrefs.SetFloat("cubeRotationSensitivity", (int)crs2.value);
		crs.value = crs1.value = crs2.value;
	}

	public void UpdateLRS() {
		PlayerPrefs.SetFloat("layersRotationSensitivity", lrs.value / 10f);
		lrs1.value = lrs.value;
	}

	public void UpdateLRS1() {
		PlayerPrefs.SetFloat("layersRotationSensitivity", lrs1.value / 10f);
		lrs.value = lrs1.value;
	}

	public void UpdateSOLS() {
		PlayerPrefs.SetFloat("solutionSpeed", sols.value / 3f);
		PlayerPrefs.SetFloat("layersRSK", PlayerPrefs.GetFloat("solutionSpeed", 1f));
		sols1.value = sols.value;
	}

	public void UpdateSOLS1() {
		PlayerPrefs.SetFloat("solutionSpeed", sols1.value / 3f);
		PlayerPrefs.SetFloat("layersRSK", PlayerPrefs.GetFloat("solutionSpeed", 1f));
		sols.value = sols1.value;
	}

	public void UpdateSCRS() {
		PlayerPrefs.SetFloat("scrambleSpeed", scrs.value);
		PlayerPrefs.SetFloat("layersRSK", PlayerPrefs.GetFloat("scrambleSpeed", 10f));
		scrs1.value = scrs.value;
	}

	public void UpdateSCRS1() {
		PlayerPrefs.SetFloat("scrambleSpeed", scrs1.value);
		PlayerPrefs.SetFloat("layersRSK", PlayerPrefs.GetFloat("scrambleSpeed", 10f));
		scrs.value = scrs1.value;
	}

	public void UpdateFOV() {
		PlayerPrefs.SetFloat("fieldOfView", 75 - fov.value);
		Camera.main.fieldOfView = 75 - fov.value;
		fov1.value = fov2.value = fov3.value = fov.value;
	}

	public void UpdateFOV1() {
		PlayerPrefs.SetFloat("fieldOfView", 75 - fov1.value);
		Camera.main.fieldOfView = 75 - fov1.value;
		fov.value = fov2.value = fov3.value = fov1.value;
	}

	public void UpdateFOV2() {
		PlayerPrefs.SetFloat("fieldOfView", 75 - fov2.value);
		Camera.main.fieldOfView = 75 - fov2.value;
		fov.value = fov1.value = fov3.value = fov2.value;
	}

	public void UpdateFOV3() {
		PlayerPrefs.SetFloat("fieldOfView", 75 - fov3.value);
		Camera.main.fieldOfView = 75 - fov3.value;
		fov.value = fov2.value = fov1.value = fov3.value;
	}

	public void UpdateTT() {
		if(flag) {
			flag = false;
			tt1.isOn = tt.isOn;
			flag = true;
			ToggleTimer();
		}
	}

	public void UpdateTT1() {
		if(flag) {
			flag = false;
			tt.isOn = tt1.isOn;
			flag = true;
			ToggleTimer();
		}
	}

	void ToggleTimer() {
		timer.SetActive(!timer.activeInHierarchy);
		PlayerPrefs.SetInt("showTimer", tt.isOn ? 1 : 0);
		runTimer = stopOnSolve = startOnRotating = false;
		ResetTimer();
	}

	public void UpdateWC() {
		if(flag) {
			flag = false;
			wc1.isOn = wc.isOn;
			PlayerPrefs.SetInt("whiteCross", wc.isOn ? 1 : 0);
			flag = true;
		}
	}

	public void UpdateWC1() {
		if(flag) {
			flag = false;
			wc.isOn = wc1.isOn;
			PlayerPrefs.SetInt("whiteCross", wc.isOn ? 1 : 0);
			flag = true;
		}
	}

	public void ResetSettings() {
		PlayerPrefs.DeleteAll();
		InitializeSettings();
	}

	public void GitHubLink() {
		Application.OpenURL("https://github.com/manojbhatt101010/cubiks");
	}
}