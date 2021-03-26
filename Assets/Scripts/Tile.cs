using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour 
{
	private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
	private static Tile previousSelected = null;

	private SpriteRenderer render;
	private bool isSelected = false;

	private Vector2[] adjacentDirections = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

	private bool matchFound = false;

	void Awake() 
	{
		render = GetComponent<SpriteRenderer>();
    }

	private void Select() 
	{
		isSelected = true;
		render.color = selectedColor;
		previousSelected = gameObject.GetComponent<Tile>();
		//SFXManager.instance.PlaySFX(Clip.Select);
	}

	private void Deselect() 
	{
		isSelected = false;
		render.color = Color.white;
		previousSelected = null;
	}

	void OnMouseDown() 
	{
		// Make sure the game is permitting tile selections
		if (render.sprite == null || BoardManager.instance.IsShifting) return;

		if (isSelected) // Determines whether to select or deselect the tile
		{
			Deselect();
		} 
		else 
		{
			if (previousSelected == null) // Check if there's already another tile selected
			{
				Select();
				Debug.Log("First");
			} 
			else 
			{
				if (GetAllAdjacentTiles().Contains(previousSelected.gameObject)) // Check if the previousSelected game object is in the returned adjacent tiles list
				{
					Debug.Log("Is an adjacent tile");
					SwapSprite(previousSelected.render); // Swap the sprite of the tile
					previousSelected.ClearAllMatches();
					previousSelected.Deselect(); // If it wasn't the first one that was selected, deselect all tiles
					ClearAllMatches();
				} 
				else // The tile isn't next to the previously selected one
				{
					Debug.Log("Is NOT an adjacent tile");
					previousSelected.GetComponent<Tile>().Deselect(); // Deselect the previous one
					Select(); // Select the newly selected tile
				}
			}
    	}
	}

	public void SwapSprite(SpriteRenderer render2) 
	{
		//if (render.sprite == render2.sprite) return; // Check render2 against the SpriteRenderer of the current tile

		Sprite tempSprite = render2.sprite; // Create a tempSprite to hold the sprite of render2
		render2.sprite = render.sprite; // Swap out the second sprite by setting it to the first
		render.sprite = tempSprite; // Swap out the first sprite by setting it to the second
		//SFXManager.instance.PlaySFX(Clip.Swap); // Play a sound effect
	}

	private GameObject GetAdjacent(Vector2 castDir) // Retrieve a single adjacent tile by sending a raycast in the target specified by castDir
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
		if (hit.collider != null) 
		{
			return hit.collider.gameObject;
		}
		return null;
	}

	private List<GameObject> GetAllAdjacentTiles() // Uses GetAdjacent() to generate a list of tiles surrounding the current tile
	{
		List<GameObject> adjacentTiles = new List<GameObject>();
		for (int i = 0; i < adjacentDirections.Length; i++) 
		{
			adjacentTiles.Add(GetAdjacent(adjacentDirections[i]));
		}
		return adjacentTiles;
	}

	private List<GameObject> FindMatch(Vector2 castDir) 
	{
		List<GameObject> matchingTiles = new List<GameObject>(); // Create a new list of GameObjects to hold all matching tiles
		RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir); // Fire a ray from the tile towards the castDir direction
		while (hit.collider != null && hit.collider.GetComponent<SpriteRenderer>().sprite == render.sprite) // Keep firing new raycasts until either your raycast hits nothing, or the tiles sprite differs from the returned object sprite
		{
			// If both conditions are met, it can be considered as a match, add it to the list
			matchingTiles.Add(hit.collider.gameObject);
			hit = Physics2D.Raycast(hit.collider.transform.position, castDir);
		}
		return matchingTiles; // Return the list of matching sprites
	}

	private void ClearMatch(Vector2[] paths)
	{
		List<GameObject> matchingTiles = new List<GameObject>(); // Create a GameObject list to hold the matches
		for (int i = 0; i < paths.Length; i++) // Iterate through the list of paths
		{
			matchingTiles.AddRange(FindMatch(paths[i])); // Add any matches to the matchingTiles list
		}
		if (matchingTiles.Count >= 2) // Continue if a match with 2 or more tiles was found
		{
			for (int i = 0; i < matchingTiles.Count; i++) // Iterate through all matching tiles
			{
				matchingTiles[i].GetComponent<SpriteRenderer>().sprite = null; // Remove their sprites by setting it null
			}
			matchFound = true; // Set the matchFound flag to true
		}
	}

	public void ClearAllMatches() 
	{
		if (render.sprite == null) return;

		ClearMatch(new Vector2[2] { Vector2.left, Vector2.right }); // Calls ClearMatch for horizontal matches
		ClearMatch(new Vector2[2] { Vector2.up, Vector2.down }); // calls ClearMatch for vertical matches
		if (matchFound) // If a match is found
		{
			render.sprite = null; // Set the current sprite to null
			matchFound = false; // Reset matchFound to false
			StopCoroutine(BoardManager.instance.FindNullTiles()); // Stop the FindNullTiles coroutine and start it again from the start
			StartCoroutine(BoardManager.instance.FindNullTiles());
			//SFXManager.instance.PlaySFX(Clip.Clear); // Play a sound effect
			GUIManager.instance.MoveCounter--; // Decrement MoveCounter every time a sprite is swapped
		}
	}
}