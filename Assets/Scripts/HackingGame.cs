using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;

public class HackingGame : MonoBehaviour
{
    public TextAsset commonFourLetterWordsText;
    public TextAsset commonFiveLetterWordsText;
    public TextAsset commonSixLetterWordsText;
    public TextAsset commonSevenLetterWordsText;
	public string[] commonFourLetterWords;
	public string[] commonFiveLetterWords;
	public string[] commonSixLetterWords;
	public string[] commonSevenLetterWords;
	public GameObject hackingLetterPrefab;
	public Transform letterParent;
	private string currentWord;
	private int currentWordLength;
	private int currentGuess;
	private List<HackingLetter> hackingLetters = new List<HackingLetter>();
	public Vector2 hackingLetterSize;
	public Vector2 distanceBetweenHackingLetters;
	public int lettersFilledIn;
	public MovingButton[] keyboardButtons; // 0 - 25 = a - z, 26 = backspace, 27 = enter, 28 = decrease word length, 29 = increase word length
	public Label currentWordLengthLabel;
	public Label hacksCompletedLabel;
	public Color defaultButtonColor;
	public Color incorrectLetterColor;
	public Color correctLetterColor;
	public Color wrongSpaceLetterColor;
	private Dictionary<string, int> lettersInCurrentWord = new Dictionary<string, int>();
	public GameObject errorMessageGO;
	public Label errorMessageLabel;
	public float errorMessageDisplayTime;
	public bool inputEnabled;
	public TextAsset collinsScrabbleWordsText;
	public TextAsset acceptableFourLetterWordsText;
	public TextAsset acceptableFiveLetterWordsText;
	public TextAsset acceptableSixLetterWordsText;
	public TextAsset acceptableSevenLetterWordsText;
	public int numberOfGuesses;
	HashSet<string> acceptableFourLetterWordsHashSet;
	HashSet<string> acceptableFiveLetterWordsHashSet;
	HashSet<string> acceptableSixLetterWordsHashSet;
	HashSet<string> acceptableSevenLetterWordsHashSet;
	public string previousGuess;
	public List<string> knownIncorrectLetters;
	public Dictionary<int, string> previousWrongSpaceLetters = new Dictionary<int, string>();
	public Dictionary<string, int> previousWrongSpaceLettersCount = new Dictionary<string, int>();
	public string testWord;
	public int wordsDiscovered;
	
	public static Dictionary<char, int> LetterToPosition = new Dictionary<char, int>
    {
        { 'Q', 0 },
        { 'W', 1 },
        { 'E', 2 },
        { 'R', 3 },
        { 'T', 4 },
        { 'Y', 5 },
        { 'U', 6 },
        { 'I', 7 },
        { 'O', 8 },
        { 'P', 9 },
        { 'A', 10 },
        { 'S', 11 },
        { 'D', 12 },
        { 'F', 13 },
        { 'G', 14 },
        { 'H', 15 },
        { 'J', 16 },
        { 'K', 17 },
        { 'L', 18 },
        { 'Z', 19 },
        { 'X', 20 },
        { 'C', 21 },
        { 'V', 22 },
        { 'B', 23 },
        { 'N', 24 },
        { 'M', 25 }
    };
	
	void Start()
	{
		PopulateWordArrays();
		SetupNewGame(5);
		// GetWordsOfLengthFromFile(7, collinsScrabbleWordsText, "AcceptableSevenLetterWods");
	}
	
	public void GetWordsOfLengthFromFile(int length, TextAsset file, string outputFileName)
	{
		string outputFilePath = $"{Application.persistentDataPath}/{outputFileName}.txt";
		Debug.Log($"outputFilePath = {outputFilePath}");
		if(File.Exists(outputFilePath))
		{
			File.WriteAllText(outputFilePath, "");
		}
		string[] words = file.text.Split('\n');
		StreamWriter writer = new StreamWriter(outputFilePath, true);
		for(int i = 0; i < words.Length; i++)
		{
			if(words[i].Trim().Length == length)
			{
				writer.WriteLine(words[i].Trim());
			}
		}
		writer.Close();
	}
	
	void PopulateWordArrays()
	{
		commonFourLetterWords = commonFourLetterWordsText.text.Split('\n');
		string[] acceptableFourLetterWordsStringArray = acceptableFourLetterWordsText.text.Split('\n');
		acceptableFourLetterWordsHashSet = new HashSet<string>(acceptableFourLetterWordsStringArray.Select(word => word.Trim().ToUpper()));
		commonFiveLetterWords = commonFiveLetterWordsText.text.Split('\n');
		string[] acceptableFiveLetterWordsStringArray = acceptableFiveLetterWordsText.text.Split('\n');
		acceptableFiveLetterWordsHashSet = new HashSet<string>(acceptableFiveLetterWordsStringArray.Select(word => word.Trim().ToUpper()));
		commonSixLetterWords = commonSixLetterWordsText.text.Split('\n');
		string[] acceptableSixLetterWordsStringArray = acceptableSixLetterWordsText.text.Split('\n');
		acceptableSixLetterWordsHashSet = new HashSet<string>(acceptableSixLetterWordsStringArray.Select(word => word.Trim().ToUpper()));
		commonSevenLetterWords = commonSevenLetterWordsText.text.Split('\n');
		string[] acceptableSevenLetterWordsStringArray = acceptableSevenLetterWordsText.text.Split('\n');
		acceptableSevenLetterWordsHashSet = new HashSet<string>(acceptableSevenLetterWordsStringArray.Select(word => word.Trim().ToUpper()));
	}
	
	public IEnumerator DestroyOldLetters()
	{
		var oldLetters = new List<GameObject>();
		foreach(Transform child in letterParent)
		{
			oldLetters.Add(child.gameObject);
		}
		// oldLetters.ForEach(child => Destroy(child));
		foreach(GameObject childGO in oldLetters)
		{
			childGO.SetActive(false);
		}
		foreach(GameObject childGO in oldLetters)
		{
			Destroy(childGO);
			yield return null;
		}
	}
	
	public IEnumerator SpawnNewLetters(int numLetters)
	{
		for(int chance = 0; chance < numberOfGuesses; chance++)
		{
			for(int letter = 0; letter < numLetters; letter++)
			{
				GameObject newHackingLetterGO = Instantiate(hackingLetterPrefab, letterParent);
				HackingLetter newHackingLetter = newHackingLetterGO.GetComponent<HackingLetter>();
				newHackingLetter.rt.anchoredPosition = new Vector2((-numLetters + 1) * (hackingLetterSize.x + distanceBetweenHackingLetters.x) / 2 + (letter * (hackingLetterSize.x + distanceBetweenHackingLetters.x)), -chance * (hackingLetterSize.y + distanceBetweenHackingLetters.y));
				hackingLetters.Add(newHackingLetter);
				yield return null;
			}
		}
		inputEnabled = true;
		UpdateChangeWordLengthButtons();
	}
	
	public IEnumerator DisplayErrorMessage(string errorMessage)
	{
		errorMessageGO.SetActive(true);
		errorMessageLabel.ChangeText(errorMessage);
		yield return new WaitForSeconds(errorMessageDisplayTime);
		errorMessageGO.SetActive(false);
	}
	
	public void ChangeWordLength(bool decrement)
	{
		keyboardButtons[28].ChangeDisabled(true);
		keyboardButtons[29].ChangeDisabled(true);
		if(decrement)
		{
			SetupNewGame(currentWordLength - 1);
		}
		else
		{
			SetupNewGame(currentWordLength + 1);
		}
		currentWordLengthLabel.ChangeText(currentWordLength.ToString());
	}
	
	public void UpdateChangeWordLengthButtons()
	{
		if(currentWordLength == 7)
		{
			keyboardButtons[29].ChangeDisabled(true);
		}
		else
		{
			keyboardButtons[29].ChangeDisabled(false);
		}
		if(currentWordLength == 4)
		{
			keyboardButtons[28].ChangeDisabled(true);
		}
		else
		{
			keyboardButtons[28].ChangeDisabled(false);
		}		
	}
	
	public void SetupNewGame(int numLetters)
	{
		StartCoroutine(DestroyOldLetters());
		hackingLetters.Clear();
		StartCoroutine(SpawnNewLetters(numLetters));
		currentWordLength = numLetters;
		currentGuess = 0;
		lettersFilledIn = 0;
		keyboardButtons[26].ChangeDisabled(true);
		keyboardButtons[27].ChangeDisabled(true);
		knownIncorrectLetters.Clear();
		previousWrongSpaceLetters.Clear();
		previousWrongSpaceLettersCount.Clear();
		previousGuess = string.Empty;
		if(numLetters == 4)
		{
			currentWord = commonFourLetterWords[Random.Range(0, commonFourLetterWords.Length)].ToUpper();
		}
		else if(numLetters == 5)
		{
			currentWord = commonFiveLetterWords[Random.Range(0, commonFiveLetterWords.Length)].ToUpper();
			if(testWord.Length == 5)
			{
				currentWord = testWord.ToUpper();
			}
		}
		else if(numLetters == 6)
		{
			currentWord = commonSixLetterWords[Random.Range(0, commonSixLetterWords.Length)].ToUpper();
		}
		else if(numLetters == 7)
		{
			currentWord = commonSevenLetterWords[Random.Range(0, commonSevenLetterWords.Length)].ToUpper();
		}
		lettersInCurrentWord.Clear();
		for(int letter = 0; letter < numLetters; letter++)
		{
			if(lettersInCurrentWord.ContainsKey(currentWord.Substring(letter, 1)))
			{
				lettersInCurrentWord[currentWord.Substring(letter, 1)]++;
			}
			else
			{
				lettersInCurrentWord.Add(currentWord.Substring(letter, 1), 1);
			}
		}
		for(int i = 0; i < 26; i++)
		{
			keyboardButtons[i].ChangeDisabled(false);
			keyboardButtons[i].ChangeColor(defaultButtonColor);
		}
	}
	
	void Update()
	{
		if(inputEnabled)
		{
			for(char letter = 'a'; letter <= 'z'; letter++)
			{
				if(Input.GetKeyDown(letter.ToString()))
				{
					KeyboardLetterClicked(letter.ToString());
				}
			}
			if(Input.GetKeyDown(KeyCode.Backspace))
			{
				BackspaceClicked();
			}
			if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				EnterClicked();
			}
			if(Input.GetKeyDown(KeyCode.Keypad1))
			{
				Debug.Log($"currentWord= {currentWord}");
			}
		}
	}
	
	public void KeyboardLetterClicked(string letterClicked)
	{
		if(lettersFilledIn >= currentWordLength)
		{
			return;
		}
		hackingLetters[currentGuess * currentWordLength + lettersFilledIn].label.gameObject.SetActive(true);
		hackingLetters[currentGuess * currentWordLength + lettersFilledIn].label.ChangeText(letterClicked.ToUpper());
		hackingLetters[currentGuess * currentWordLength + lettersFilledIn].letter = letterClicked.ToUpper();
		lettersFilledIn++;
		keyboardButtons[26].ChangeDisabled(false);
		if(lettersFilledIn == currentWordLength)
		{
			keyboardButtons[27].ChangeDisabled(false);
		}
	}
	
	public void BackspaceClicked()
	{
		if(lettersFilledIn == 0)
		{
			return;
		}
		lettersFilledIn--;
		hackingLetters[currentGuess * currentWordLength + lettersFilledIn].label.gameObject.SetActive(false);
		keyboardButtons[27].ChangeDisabled(true);
		if(lettersFilledIn == 0)
		{
			keyboardButtons[26].ChangeDisabled(true);
		}
	}
	
	public void EnterClicked()
	{
		if(lettersFilledIn != currentWordLength)
		{
			StartCoroutine(DisplayErrorMessage($"Must Input {currentWordLength} Letters"));
			return;
		}
		string inputWord = string.Empty;
		for(int letter = currentGuess * currentWordLength; letter < currentGuess * currentWordLength + currentWordLength; letter++)
		{
			inputWord += hackingLetters[letter].letter;
		}
		if((currentWordLength == 4 && !acceptableFourLetterWordsHashSet.Contains(inputWord)) || (currentWordLength == 5 && !acceptableFiveLetterWordsHashSet.Contains(inputWord)) || (currentWordLength == 6 && !acceptableSixLetterWordsHashSet.Contains(inputWord)) || (currentWordLength == 7 && !acceptableSevenLetterWordsHashSet.Contains(inputWord)))
		{
			StartCoroutine(DisplayErrorMessage($"{inputWord} is not a Valid Word"));
			return;
		}
		Dictionary<string, int> lettersUsed = new Dictionary<string, int>();
		Dictionary<string, int> letterInventory = new Dictionary<string, int>();
		
		for(int letter = 0; letter < currentWordLength; letter++)
		{
			if(currentGuess > 0)
			{
				if(previousGuess.Substring(letter, 1) == currentWord.Substring(letter, 1) && inputWord.Substring(letter, 1) != currentWord.Substring(letter, 1))
				{
					StartCoroutine(DisplayErrorMessage("Letter in Correct Position Unused"));
					return;
				}
				if(previousWrongSpaceLetters.ContainsKey(letter) && previousWrongSpaceLetters[letter] == inputWord.Substring(letter, 1))
				{
					StartCoroutine(DisplayErrorMessage("Unmoved Letter that is in Wrong Position"));
					return;
				}
				if(knownIncorrectLetters.Contains(inputWord.Substring(letter, 1)))
				{
					StartCoroutine(DisplayErrorMessage("Cannot Use Letters Previously Eliminated"));
					return;
				}
			}
			if(lettersUsed.ContainsKey(inputWord.Substring(letter, 1)))
			{
				lettersUsed[inputWord.Substring(letter, 1)]++;
			}
			else
			{
				lettersUsed.Add(inputWord.Substring(letter, 1), 1);
			}
			if(letterInventory.ContainsKey(currentWord.Substring(letter, 1)))
			{
				letterInventory[currentWord.Substring(letter, 1)]++;
			}
			else
			{
				letterInventory.Add(currentWord.Substring(letter, 1), 1);
			}
		}
		foreach(KeyValuePair<string, int> entry in previousWrongSpaceLettersCount)
		{
			// Debug.Log($"Checking {entry.Key}, {entry.Value}");
			if(!lettersUsed.ContainsKey(entry.Key) || lettersUsed[entry.Key] < entry.Value)
			{
				StartCoroutine(DisplayErrorMessage("Must Use Previously Discovered Letters"));
				return;
			}
		}
		previousWrongSpaceLetters.Clear();
		previousWrongSpaceLettersCount.Clear();
		previousGuess = inputWord;
		int lettersCorrect = 0;
		foreach(KeyValuePair<string, int> entry in lettersUsed)
		{
			// Debug.Log($"lettersUsed[{entry.Key}] = {entry.Value}");
		}
		for(int letter = currentGuess * currentWordLength; letter < currentGuess * currentWordLength + currentWordLength; letter++)
		{
			// Debug.Log($"hackingLetters.Count= {hackingLetters.Count} letter= {letter}");
			// Debug.Log($"letter= {letter} = '{hackingLetters[letter].letter}' correct letter = '{currentWord.Substring(letter - currentGuess * currentWordLength, 1)}' correct word= {currentWord}");
			if(hackingLetters[letter].letter == currentWord.Substring(letter - currentGuess * currentWordLength, 1))
			{
				// Debug.Log($"Setting Letter {letter}, which is {hackingLetters[letter].letter} to green");
				hackingLetters[letter].fill.color = correctLetterColor;
				keyboardButtons[LetterToPosition[char.Parse(hackingLetters[letter].letter)]].ChangeColor(correctLetterColor);
				lettersCorrect++;
				letterInventory[hackingLetters[letter].letter]--;
				hackingLetters[letter].isCorrect = true;
			}
		}
		for(int letter = currentGuess * currentWordLength; letter < currentGuess * currentWordLength + currentWordLength; letter++)
		{
			if(hackingLetters[letter].isCorrect)
			{
				continue;
			}
			if(letterInventory.ContainsKey(hackingLetters[letter].letter) && letterInventory[hackingLetters[letter].letter] > 0)
			{
				// Debug.Log($"Setting Letter {letter}, which is {hackingLetters[letter].letter} to yellow");
				hackingLetters[letter].fill.color = wrongSpaceLetterColor;
				keyboardButtons[LetterToPosition[char.Parse(hackingLetters[letter].letter)]].ChangeColor(wrongSpaceLetterColor);
				previousWrongSpaceLetters.Add(letter, hackingLetters[letter].letter);
				letterInventory[hackingLetters[letter].letter]--;
				if(previousWrongSpaceLettersCount.ContainsKey(hackingLetters[letter].letter))
				{
					previousWrongSpaceLettersCount[hackingLetters[letter].letter]++;
				}
				else
				{
					previousWrongSpaceLettersCount.Add(hackingLetters[letter].letter, 1);
				}
			}
			else
			{
				// Debug.Log($"Setting Letter {letter}, which is {hackingLetters[letter].letter} to gray");
				if(!lettersInCurrentWord.ContainsKey(hackingLetters[letter].letter))
				{
					if(!knownIncorrectLetters.Contains(hackingLetters[letter].letter))
					{
						knownIncorrectLetters.Add(hackingLetters[letter].letter);
					}
					keyboardButtons[LetterToPosition[char.Parse(hackingLetters[letter].letter)]].ChangeDisabled(true);
				}
				hackingLetters[letter].fill.color = incorrectLetterColor;
			}
		}
		keyboardButtons[26].ChangeDisabled(true);
		keyboardButtons[27].ChangeDisabled(true);
		if(lettersCorrect == currentWordLength)
		{
			StartCoroutine(DisplayErrorMessage("System Hacked"));
			StartCoroutine(StartNewGame(2));
			inputEnabled = false;
			wordsDiscovered++;
			hacksCompletedLabel.ChangeText(wordsDiscovered.ToString());
		}
		else
		{
			lettersFilledIn = 0;
			currentGuess++;
			if(currentGuess == numberOfGuesses)
			{
				inputEnabled = false;
				StartCoroutine(DisplayErrorMessage("Unable to Complete Hack"));
				StartCoroutine(StartNewGame(2));
			}
		}
	}
	
	public IEnumerator StartNewGame(float delay)
	{
		keyboardButtons[28].ChangeDisabled(true);
		keyboardButtons[29].ChangeDisabled(true);
		yield return new WaitForSeconds(delay);
		SetupNewGame(currentWordLength);
	}
}