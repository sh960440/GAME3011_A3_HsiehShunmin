using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour 
{
	public static BoardManager instance;
	[Header("Animal Sprites")]
	public List<Sprite> allAnimals = new List<Sprite>(); // A list of sprites used as tile pieces
	public List<Sprite> animalsInThisRound;

	[Header("Tile Generation")]
	public GameObject tile;
	public int xSize, ySize;

	private GameObject[,] tiles; // Used to store the tiles in the board

	public bool IsShifting { get; set; } // Tell the game when a match is found and the board is re-filling

	[Header("Sound Effects")]
	public AudioSource selectSound;
	public AudioSource swapSound;
	public AudioSource clearSound;

	void Start() 
	{
		instance = GetComponent<BoardManager>(); // Sets the singleton with reference of the BoardManager
    }

	public void StartNewGame(int count)
	{
		animalsInThisRound = new List<Sprite>(allAnimals);
		
		if (count >= 0)
		{
			for (int i = 0; i < count; i++)
			{
				animalsInThisRound.Remove(animalsInThisRound[animalsInThisRound.Count-1]);
			}
		} 	
		
		Vector2 offset = tile.GetComponent<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);
	}

	private void CreateBoard (float xOffset, float yOffset) {
		tiles = new GameObject[xSize, ySize]; // The 2D array tiles gets initialized

		// Find the starting positions for the board generation
        float startX = transform.position.x;
		float startY = transform.position.y;

		Sprite[] previousLeft = new Sprite[ySize];
    	Sprite previousBelow = null;


		// Loop through xSize and ySize, instantiating a newTile every iteration to achieve a grid of rows and columns
		for (int x = 0; x < xSize; x++) 
		{
			for (int y = 0; y < ySize; y++) 
			{
				GameObject newTile = Instantiate(tile, new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0), tile.transform.rotation);
				tiles[x, y] = newTile;

				newTile.transform.parent = transform; // Parent all the tiles to BoardManager
				
				List<Sprite> possibleCharacters = new List<Sprite>(); // Create a list of possible characters for this sprite
				possibleCharacters.AddRange(animalsInThisRound); // Add all characters to the list
				
				// Remove the characters that are on the left and below the current sprite from the list
				possibleCharacters.Remove(previousLeft[y]);
				possibleCharacters.Remove(previousBelow);
				Sprite newSprite = possibleCharacters[Random.Range(0, possibleCharacters.Count)]; // Randomly choose a sprite
				newTile.GetComponent<SpriteRenderer>().sprite = newSprite; // Set the newly created tile's sprite to the randomly chosen sprite

				previousLeft[y] = newSprite;
				previousBelow = newSprite;
			}
        }
    }

	public IEnumerator FindNullTiles() 
	{
		// Loop through the entire board in search of tile pieces with null sprites
		for (int x = 0; x < xSize; x++) 
		{
			for (int y = 0; y < ySize; y++) 
			{
				if (tiles[x, y].GetComponent<SpriteRenderer>().sprite == null)
				{
					yield return StartCoroutine(ShiftTilesDown(x, y));
					break;
				}
			}
		}

		// Locate any possible combos that may have been created during the shifting process
		for (int x = 0; x < xSize; x++) 
		{
			for (int y = 0; y < ySize; y++) 
			{
				tiles[x, y].GetComponent<Tile>().ClearAllMatches();
			}
		}
	}

	private IEnumerator ShiftTilesDown(int x, int yStart, float shiftDelay = .03f) 
	{
		IsShifting = true;
		List<SpriteRenderer> renders = new List<SpriteRenderer>();
		int nullCount = 0;

		for (int y = yStart; y < ySize; y++) // Loop through and finds how many spaces it needs to shift downwards
		{
			SpriteRenderer render = tiles[x, y].GetComponent<SpriteRenderer>();
			if (render.sprite == null) // Store the number of spaces in an integer named nullCount
			{
				nullCount++;
			}
			renders.Add(render);
		}

		for (int i = 0; i < nullCount; i++) // Loop again to begin the actual shifting
		{
			UIManager.instance.Score += 5;
			yield return new WaitForSeconds(shiftDelay); // Pause for shiftDelay seconds
			for (int k = 0; k < renders.Count - 1; k++) // Loop through every SpriteRenderer in the list of renders
			{
				renders[k].sprite = renders[k + 1].sprite; // Swap each sprite with the one above it
				renders[k + 1].sprite = GetNewSprite(x, ySize - 1); // Refill
			}
		}
		IsShifting = false;
	}

	private Sprite GetNewSprite(int x, int y) 
	{
		List<Sprite> possibleCharacters = new List<Sprite>();
		possibleCharacters.AddRange(animalsInThisRound);
		
		if (x > 0) 
		{
			possibleCharacters.Remove(tiles[x - 1, y].GetComponent<SpriteRenderer>().sprite); // Remove possible duplicates that could cause an accidental match
		}
		if (x < xSize - 1) 
		{
			possibleCharacters.Remove(tiles[x + 1, y].GetComponent<SpriteRenderer>().sprite); // Remove possible duplicates that could cause an accidental match
		}
		if (y > 0) 
		{
			possibleCharacters.Remove(tiles[x, y - 1].GetComponent<SpriteRenderer>().sprite); // Remove possible duplicates that could cause an accidental match
		}

		return possibleCharacters[Random.Range(0, possibleCharacters.Count)]; // Return a random sprite from the possible sprite list
	}

	public static void ClearBoard()
	{
		Tile[] tiles = FindObjectsOfType<Tile>();
		foreach(Tile tile in tiles)
		{
			Destroy(tile.gameObject);			
		}
	}
}
