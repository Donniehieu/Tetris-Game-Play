using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameController : MonoBehaviour {
    
    public float fallTime = 0.8f;
    public Vector3 startPos = new Vector3();
    private float previousTime, previousToLeft, previousToRight;   
    public Grid gridMap;
   
    public bool gameOver, isDestroying;
    public bool hardDropped, gameClear, isPaused, isRowDown, isEndTurn; 
    

    void Start() {
       
        gridMap = FindObjectOfType<Grid>();
        InitGame();
    }

    void InitGame() {
        FindObjectOfType<AudioManager>().Play("GameStart");
      
        gameOver = false;
        gameClear = false;
        gridMap.isShowingAnimation = false;
        isEndTurn = false;
        gridMap.isAnimating = false;
        gridMap.linesDeleted = 0;      
        if (gridMap.currBlock != null) gridMap.currBlock.Destroy();
        gridMap.NextBlock();
        gridMap.NewBlock();
    }

    public void Pause() {
        isPaused = true;
       
        FindObjectOfType<AudioManager>().Mute("GameStart", true);
    }

    public void Resume() {
        isPaused = false;
      
        FindObjectOfType<AudioManager>().Mute("GameStart", false);
    }

    public void Mute(bool isMute) {
        FindObjectOfType<AudioManager>().Mute("GameStart", isMute);
        if (isMute) {
            
        } else {
           
        }
    }

    void Update() {
        if (isPaused && Input.GetKeyDown(KeyCode.P)) Resume();
        else if (!isEndTurn && !gameOver && !gameClear && !isPaused && !gridMap.isShowingAnimation) {
            if (Input.GetKey(KeyCode.LeftArrow) && Time.time - previousToLeft > 0.1f) {
                HorizontalMove(Vector3.left);
                previousToLeft = Time.time;
            } else if (Input.GetKey(KeyCode.RightArrow) && Time.time - previousToRight > 0.1f) {
                HorizontalMove(Vector3.right);
                previousToRight = Time.time;
            } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
                Rotate();
            } else if (Input.GetKeyDown(KeyCode.Space)) {
                while (ValidMove(gridMap.currBlock.transform) && !hardDropped) VerticalMove(Vector3.down);
            } else if (Input.GetKeyUp(KeyCode.Space)) {
                hardDropped = false;
            } else if (Input.GetKeyDown(KeyCode.P)) {
                Pause();
            }

            if (Time.time - previousTime > (Input.GetKey(KeyCode.DownArrow) ? fallTime / 10 : fallTime)) {
                VerticalMove(Vector3.down);
                previousTime = Time.time;
            }
            if (gridMap.isAnimating && !isEndTurn) {
                EndTurn();
                isEndTurn = false;
            }
         
            GhostBlockImgUpdate();
          
        }
    }
    private void GhostBlockImgUpdate() {
        if (!gridMap.ghostBlock.IsDestroyed()) {
            gridMap.ghostBlock.transform.position = GhostPosition(gridMap.currBlock.transform.position);
        }
    }

    void Rotate() {
        Transform currTransform = gridMap.currBlock.transform;
        currTransform.RotateAround(currTransform.TransformPoint(gridMap.currBlock.rotationPoint), Vector3.forward, 90);
        gridMap.ghostBlock.transform.RotateAround(gridMap.ghostBlock.transform.TransformPoint(gridMap.currBlock.rotationPoint), Vector3.forward, 90);

        if (!ValidMove(gridMap.currBlock.transform)) {
            currTransform.RotateAround(currTransform.TransformPoint(gridMap.currBlock.rotationPoint), Vector3.forward, -90);
            gridMap.ghostBlock.transform.RotateAround(gridMap.ghostBlock.transform.TransformPoint(gridMap.currBlock.rotationPoint), Vector3.forward, -90);

        }
    }

    void HorizontalMove(Vector3 nextMove) {
        gridMap.currBlock.transform.position += nextMove;
        if (!ValidMove(gridMap.currBlock.transform)) {
            gridMap.currBlock.transform.position -= nextMove;
        }
    }
   

    void VerticalMove(Vector3 nextMove) {
        gridMap.currBlock.transform.position += nextMove;
        if (!ValidMove(gridMap.currBlock.transform)) {
            gridMap.currBlock.transform.position -= nextMove;
            gridMap.CreateDeadBlock();
            DestroyCurrBlock();
            gridMap.CheckForLines();
        }
    }
    private void DestroyCurrBlock() {
     
        
            gridMap.currBlock.Destroy();
            gridMap.ghostBlock.Destroy();
        
        
    }
    private void EndTurn() {
        isEndTurn = true;
        print("EndTurn");
        FindObjectOfType<AudioManager>().Play("Blip");
        hardDropped = true;
        foreach (var y in gridMap.deletingRow) {
            StartCoroutine(gridMap.DeleteLine(y));
            StartCoroutine(WaitForRowDown(y));
        }
        StartCoroutine(WaitForNewBlock());
        gridMap.isAnimating = false;
    }
    public IEnumerator DeleteLineEffect(Block dead, int[] destroyedBlocks) {
        Color tmp = dead.sprite.GetComponent<SpriteRenderer>().color;
        float _progress = 1f;

        while (_progress > 0.0f) {
            dead.sprite.GetComponent<SpriteRenderer>().color = new Color(tmp.r, tmp.g, tmp.b, tmp.a * _progress);
            _progress -= 0.1f;
            yield return new WaitForSeconds(0.03f);
        }

        if (_progress < 0.0f && dead != null) {
            destroyedBlocks[0]++;
        }
    }

    private IEnumerator WaitForRowDown(int y) {
        while (isDestroying) {
            yield return new WaitForSeconds(0.01f);
        }
        gridMap.RowDown(y);
    }

    private IEnumerator WaitForNewBlock() {
        while (isDestroying || isRowDown) {
            yield return new WaitForSeconds(0.01f);
        }
        gridMap.NewBlock();
    }

    
    bool ValidMove(Transform transform) {
        foreach (Transform children in transform) {
            int roundedY = Mathf.RoundToInt(children.transform.position.y);
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            if (roundedX < 0 || roundedX >= Grid.col|| roundedY < 0 || roundedY >= Grid.row) {
                return false;
            }
            if (gridMap.grid[roundedY, roundedX] != null) {
                return false;
            }
        }
        return true;
    }

    public Vector3 GhostPosition(Vector3 vec) {
        int x = Mathf.RoundToInt(vec.x), y = Math.Max(Mathf.RoundToInt(vec.y), 0), z = Mathf.RoundToInt(vec.z);
        gridMap.ghostBlock.transform.position = new Vector3(x, y, z);
        while (ValidMove(gridMap.ghostBlock.transform)) gridMap.ghostBlock.transform.position += Vector3.down;

        return gridMap.ghostBlock.transform.position + Vector3.up;
    }
    public void GameOver() {
        print("GAME OVER!!!");
        if (gridMap.ghostBlock != null) gridMap.ghostBlock.Destroy();
      
        FindObjectOfType<AudioManager>().Stop("GameStart");
      
    }

    private void GameClear() {
        print("GameClear");
        if (gridMap.ghostBlock != null) gridMap.ghostBlock.Destroy();
       
        FindObjectOfType<AudioManager>().Stop("GameStart");
        FindObjectOfType<AudioManager>().Play("GameClear");
    }
}


