using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour 
{
	public static BoardManager instance; // Singleton
	public List<Sprite> characters = new List<Sprite>(); // A list of sprites used as tile pieces.
	public GameObject tile; // The prefab instantiated when creating the board
	public int xSize, ySize; // X and Y dimensions of the board.

	private GameObject[,] tiles; // Used to store the tiles in the board

	public bool IsShifting { get; set; } // Tell the game when a match is found and the board is re-filling

	void Start () 
	{
		instance = GetComponent<BoardManager>(); // Sets the singleton with reference of the BoardManager

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
				possibleCharacters.AddRange(characters); // Add all characters to the list
				
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
}
