using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public static int row = 20, col = 10;
    public Block[,] grid = new Block[row, col];
    public TetrisBlock currBlock;
    public TetrisBlock deadBlock;
    public GhostBlock[] Ghosts;
    public GhostBlock ghostBlock;
    public int nextBlock;
    private HashSet<int> deck = new HashSet<int>();
    public TetrisBlock[] Blocks;
    public TetrisBlock nextBlockObject;
    public TetrisBlock nextBlockOb;
    public readonly Vector3[] Pivots = new[] { new Vector3(-0.33f, 0f, 0f), new Vector3(-0.27f, -0.15f, 0f), new Vector3(-0.27f, 0.1f, 0f), new Vector3(-0.12f, -0.1f, 0f), new Vector3(-0.22f, -0.1f, 0f), new Vector3(-0.02f, -0.1f, 0f), new Vector3(-0.2f, 0.1f, 0f) };

    public bool isShowingAnimation;

    public bool isAnimating;

    public List<int> deletingRow = new List<int>();

    public int linesDeleted;
    public GameController gameController;

    private void Awake()
    {
        gameController = FindObjectOfType<GameController>();
    }
    public void CreateDeadBlock()
    {
        foreach (Transform children in currBlock.transform)
        {
            int roundedY = Mathf.RoundToInt(children.transform.position.y);
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            Color currColor = children.GetComponent<SpriteRenderer>().color;
            if (grid[roundedY, roundedX] == null)
            {
                TetrisBlock curr = Instantiate(deadBlock, new Vector3(roundedX, roundedY, 0), Quaternion.identity);
                curr.sprite.GetComponent<SpriteRenderer>().color = currColor;
                grid[roundedY, roundedX] = curr;
            }
        }
    }
    public void NewGhost()
    {
        print("newghost start");
        if (ghostBlock != null)
        {
            ghostBlock.Destroy();
            ghostBlock = Instantiate(Ghosts[nextBlock], currBlock.transform.position, Quaternion.identity);
        }
        else
        {
            ghostBlock = Instantiate(Ghosts[nextBlock], currBlock.transform.position, Quaternion.identity);
        }
        print("newghostend");
    }
    public void NextBlock()
    {
        
        print("nextblock start");
        if (deck.Count == Blocks.Length) deck.Clear();
        do nextBlock = Random.Range(0, Blocks.Length);
        while (deck.Contains(nextBlock));
        deck.Add(nextBlock);

        if (nextBlockObject != null) nextBlockObject.Destroy();
        nextBlockObject = Instantiate(Blocks[nextBlock]);
        nextBlockObject.transform.parent = nextBlockOb.transform;
        nextBlockObject.transform.localPosition = Pivots[nextBlock];
        
           
        
        print("nextblock end");
    }
    public void CheckForLines()
    {
        isShowingAnimation = true;
        deletingRow.Clear();

        for (int y = row - 1; y >= 0; y--)
        {
            if (HasLine(y))
            {
                deletingRow.Add(y);
            }
        }

        linesDeleted += deletingRow.Count;
        if (deletingRow != null)
        {
            isAnimating = true;
        }
    }
    private bool HasLine(int y)
    {
        for (int x = 0; x < col; x++)
        {
            if (grid[y, x] == null) return false;
        }
        return true;
    }
    public void NewBlock()
    {
        print("newblock start");
        currBlock = Instantiate(Blocks[nextBlock], gameController.startPos, Quaternion.identity);
        NewGhost();
        NextBlock();
        isShowingAnimation = false;
        if (grid[18, 4] != null)
        {
            print("going to gameover");
            gameController.gameOver = true;
            gameController.GameOver();
        }
        print("newblock end");
    }
    public IEnumerator DeleteLine(int y)
    {
        print("deleteline");
        gameController.isDestroying = true;
        int[] destroyedBlocks = new int[1];
        destroyedBlocks[0] = 0;
        for (int x = 0; x < col; x++)
        {
            if (grid[y, x] != null)
            {
                StartCoroutine(gameController.DeleteLineEffect(grid[y, x], destroyedBlocks));
            }
        }

        while (destroyedBlocks[0] < 10)
        {
            yield return new WaitForSeconds(0.1f);
        }
        for (int x = 0; x < col; x++)
        {
            if (grid[y, x] == null) continue;
            grid[y, x].Destroy();
            grid[y, x] = null;
        }
        gameController. isDestroying = false;
        destroyedBlocks[0] = 0;
    }
    public void RowDown(int deletedLine)
    {
        print("rowdown");

        gameController.isRowDown = true;
        for (int y = deletedLine; y < row; y++)
        {
            for (int x = 0; x < col; x++)
            {
                if (y == deletedLine)
                {
                    grid[y, x] = null;
                }
                if (grid[y, x] != null)
                {
                    grid[y - 1, x] = grid[y, x];
                    grid[y, x] = null;
                    grid[y - 1, x].transform.position -= Vector3.up;
                }
            }
        }
        gameController. isRowDown = false;
    }

}
