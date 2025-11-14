using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;

public class HackingGameOld : MonoBehaviour
{
    public TextAsset commonFiveLetterWordsText;
	public string[] commonFiveLetterWords;
	public GameObject hackingLetterPrefab;
	public Transform letterParent;
	private string currentWord;
	private int currentWordLength;
	private int currentGuess;
	private List<HackingLetter> hackingLetters = new List<HackingLetter>();
	public Vector2 hackingLetterSize;
	public Vector2 distanceBetweenHackingLetters;
	public int lettersFilledIn;
	public MovingButton[] keyboardButtons; // 0 - 25 = a - z, 26 = backspace, 27 = enter
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
	public TextAsset acceptableFiveLetterWordsText;
	public int numberOfGuesses;
	HashSet<string> acceptableFiveLetterWordsHashSet;
	public string previousGuess;
	public List<string> knownIncorrectLetters;
	public Dictionary<int, string> previousWrongSpaceLetters = new Dictionary<int, string>();
	public Dictionary<string, int> previousWrongSpaceLettersCount = new Dictionary<string, int>();
	public string testWord;
	
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
		// GetWordsOfLengthFromFile(5, collinsScrabbleWordsText, "AcceptableFiveLetterWods");
	}
	
	public void GetWordsOfLengthFromFile(int length, TextAsset file, string outputFileName)
	{
		string outputFilePath = $"{Application.persistentDataPath}/{outputFileName}.txt";
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
		commonFiveLetterWords = commonFiveLetterWordsText.text.Split('\n');
		string[] acceptableFiveLetterWordsStringArray = acceptableFiveLetterWordsText.text.Split('\n');
		acceptableFiveLetterWordsHashSet = new HashSet<string>(acceptableFiveLetterWordsStringArray.Select(word => word.Trim().ToUpper()));
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
	}
	
	public IEnumerator DisplayErrorMessage(string errorMessage)
	{
		errorMessageGO.SetActive(true);
		errorMessageLabel.ChangeText(errorMessage);
		yield return new WaitForSeconds(errorMessageDisplayTime);
		errorMessageGO.SetActive(false);
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
		// previousGuesses.Clear();
		previousGuess = string.Empty;
		if(numLetters == 5)
		{
			currentWord = commonFiveLetterWords[Random.Range(0, commonFiveLetterWords.Length)].ToUpper();
			if(testWord.Length == 5)
			{
				currentWord = testWord.ToUpper();
			}
		}
		Debug.Log($"currentWord= {currentWord}");
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
		if(currentWordLength == 5 && !acceptableFiveLetterWordsHashSet.Contains(inputWord))
		{
			StartCoroutine(DisplayErrorMessage($"{inputWord} is not a Valid Word"));
			return;
		}
		Dictionary<string, int> lettersUsed = new Dictionary<string, int>();
		
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
		// Dictionary<string, int> lettersGuessed = new Dictionary<string, int>();
		int lettersCorrect = 0;
		foreach(KeyValuePair<string, int> entry in lettersUsed)
		{
			Debug.Log($"lettersUsed[{entry.Key}] = {entry.Value}");
		}
		for(int letter = currentGuess * currentWordLength; letter < currentGuess * currentWordLength + currentWordLength; letter++)
		{
			// Debug.Log($"hackingLetters.Count= {hackingLetters.Count} letter= {letter}");
			// Debug.Log($"letter= {letter} = '{hackingLetters[letter].letter}' correct letter = '{currentWord.Substring(letter - currentGuess * currentWordLength, 1)}' correct word= {currentWord}");
			if(hackingLetters[letter].letter == currentWord.Substring(letter - currentGuess * currentWordLength, 1))
			{
				hackingLetters[letter].fill.color = correctLetterColor;
				keyboardButtons[LetterToPosition[char.Parse(hackingLetters[letter].letter)]].ChangeColor(correctLetterColor);
				lettersCorrect++;
			}
			else
			{
				if(lettersInCurrentWord.ContainsKey(hackingLetters[letter].letter))
				{
					// if(lettersGuessed.ContainsKey(hackingLetters[letter].letter))
					/* if(lettersUsed.ContainsKey(hackingLetters[letter].letter))
					{
						// if(lettersGuessed[hackingLetters[letter].letter] < lettersInCurrentWord[hackingLetters[letter].letter])
						{
							hackingLetters[letter].fill.color = wrongSpaceLetterColor;
							keyboardButtons[LetterToPosition[char.Parse(hackingLetters[letter].letter)]].ChangeColor(wrongSpaceLetterColor);
							previousWrongSpaceLetters.Add(letter, hackingLetters[letter].letter);
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
							hackingLetters[letter].fill.color = incorrectLetterColor;
						}
					} */
					int numberOfTimesLetterIsInCorrectWord = 0;
					int numberOfTimesLetterIsInGuess = 0;
					int numberOfTimesLetterIsInGuessInWrongSpot = 0;
					/* if(lettersUsed.ContainsKey(hackingLetters[letter].letter))
					{
						numberOfTimesLetterIsInGuess = lettersUsed[hackingLetters[letter].letter];
					} */
					int numberOfTimesLetterIsInCorrectPositionInCorrectWord = 0;
					int indexOfLetterInGuess = 0;
					for(int l = currentGuess * currentWordLength; l < currentGuess * currentWordLength + currentWordLength; l++)
					{
						if(hackingLetters[l].letter == hackingLetters[letter].letter)
						{
							if(l == letter)
							{
								// indexOfLetterInGuess = numberOfTimesLetterIsInGuess;
								indexOfLetterInGuess = numberOfTimesLetterIsInGuessInWrongSpot;
							}
							if(hackingLetters[l].letter != currentWord.Substring(l - currentGuess * currentWordLength))
							{
								numberOfTimesLetterIsInGuessInWrongSpot++;
							}
							numberOfTimesLetterIsInGuess++;
						}
						if(hackingLetters[l].letter == hackingLetters[letter].letter && hackingLetters[l].letter == currentWord.Substring(l - currentGuess * currentWordLength, 1))
						{
							numberOfTimesLetterIsInCorrectPositionInCorrectWord++;
						}
						if(currentWord.Substring(l - currentGuess * currentWordLength, 1) == hackingLetters[letter].letter)
						{
							numberOfTimesLetterIsInCorrectWord++;
						}
					}
					// Debug.Log($"letter = {letter}, numberOfTimesLetterIsInGuess = {numberOfTimesLetterIsInGuess}, numberOfTimesLetterIsInGuessInWrongSpot = {numberOfTimesLetterIsInGuessInWrongSpot}, numberOfTimesLetterIsInCorrectWord = {numberOfTimesLetterIsInCorrectWord}, indexOfLetterInGuess = {indexOfLetterInGuess}, numberOfTimesLetterIsInCorrectPositionInCorrectWord = {numberOfTimesLetterIsInCorrectPositionInCorrectWord}, result = {numberOfTimesLetterIsInGuessInWrongSpot - numberOfTimesLetterIsInCorrectWord - indexOfLetterInGuess - numberOfTimesLetterIsInCorrectPositionInCorrectWord}");
					// Debug.Log($"letter = {letter}, numberOfTimesLetterIsInGuessInWrongSpot = {numberOfTimesLetterIsInGuessInWrongSpot}, indexOfLetterInGuess = {indexOfLetterInGuess}, numberOfTimesLetterIsInCorrectWord = {numberOfTimesLetterIsInCorrectWord},  numberOfTimesLetterIsInCorrectPositionInCorrectWord = {numberOfTimesLetterIsInCorrectPositionInCorrectWord}, {numberOfTimesLetterIsInGuessInWrongSpot - indexOfLetterInGuess} <=  {numberOfTimesLetterIsInCorrectWord - numberOfTimesLetterIsInCorrectPositionInCorrectWord} = {numberOfTimesLetterIsInGuessInWrongSpot - indexOfLetterInGuess <= numberOfTimesLetterIsInCorrectWord - numberOfTimesLetterIsInCorrectPositionInCorrectWord}");
					Debug.Log($"letter = {letter}, numberOfTimesLetterIsInGuessInWrongSpot = {numberOfTimesLetterIsInGuessInWrongSpot}, indexOfLetterInGuess = {indexOfLetterInGuess}, numberOfTimesLetterIsInCorrectWord = {numberOfTimesLetterIsInCorrectWord}, {numberOfTimesLetterIsInGuessInWrongSpot - indexOfLetterInGuess} <=  {numberOfTimesLetterIsInCorrectWord - numberOfTimesLetterIsInCorrectPositionInCorrectWord} = {numberOfTimesLetterIsInGuessInWrongSpot - indexOfLetterInGuess <= numberOfTimesLetterIsInCorrectWord}");
					// if(numberOfTimesLetterIsInGuessInWrongSpot - numberOfTimesLetterIsInCorrectWord - indexOfLetterInGuess - numberOfTimesLetterIsInCorrectPositionInCorrectWord < 0)
					// if(numberOfTimesLetterIsInGuessInWrongSpot - (numberOfTimesLetterIsInCorrectWord - numberOfTimesLetterIsInCorrectPositionInCorrectWord) - indexOfLetterInGuess < 0)
					// if(numberOfTimesLetterIsInGuessInWrongSpot - indexOfLetterInGuess <= numberOfTimesLetterIsInCorrectWord - numberOfTimesLetterIsInCorrectPositionInCorrectWord)
					if(numberOfTimesLetterIsInGuessInWrongSpot - indexOfLetterInGuess <= numberOfTimesLetterIsInCorrectWord)
					{
						Debug.Log("Setting Letter to gray");
						hackingLetters[letter].fill.color = incorrectLetterColor;
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
						Debug.Log("Setting Letter to yellow");
						hackingLetters[letter].fill.color = wrongSpaceLetterColor;
						keyboardButtons[LetterToPosition[char.Parse(hackingLetters[letter].letter)]].ChangeColor(wrongSpaceLetterColor);
						previousWrongSpaceLetters.Add(letter, hackingLetters[letter].letter);
						if(previousWrongSpaceLettersCount.ContainsKey(hackingLetters[letter].letter))
						{
							previousWrongSpaceLettersCount[hackingLetters[letter].letter]++;
						}
						else
						{
							previousWrongSpaceLettersCount.Add(hackingLetters[letter].letter, 1);
						}
					}
				}
				else
				{
					if(!knownIncorrectLetters.Contains(hackingLetters[letter].letter))
					{
						knownIncorrectLetters.Add(hackingLetters[letter].letter);
					}
					hackingLetters[letter].fill.color = incorrectLetterColor;
					keyboardButtons[LetterToPosition[char.Parse(hackingLetters[letter].letter)]].ChangeDisabled(true);
				}
			}
			/* if(lettersGuessed.ContainsKey(hackingLetters[letter].letter))
			{
				lettersGuessed[hackingLetters[letter].letter]++;
			}
			else
			{
				lettersGuessed.Add(hackingLetters[letter].letter, 1);
			} */
		}
		keyboardButtons[26].ChangeDisabled(true);
		keyboardButtons[27].ChangeDisabled(true);
		if(lettersCorrect == currentWordLength)
		{
			StartCoroutine(DisplayErrorMessage("System Hacked"));
			StartCoroutine(StartNewGame(2));
			inputEnabled = false;
		}
		else
		{
			lettersFilledIn = 0;
			currentGuess++;
			if(currentGuess == numberOfGuesses - 1)
			{
				inputEnabled = false;
				StartCoroutine(DisplayErrorMessage("Unable to Complete Hack"));
				StartCoroutine(StartNewGame(2));
			}
		}
	}
	
	public IEnumerator StartNewGame(float delay)
	{
		yield return new WaitForSeconds(delay);
		SetupNewGame(5);
	}
}
