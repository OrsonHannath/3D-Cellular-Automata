using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NeighbourMethod {
    Moore,
    VonNeuman
}

public class CellularAutomataHandler : MonoBehaviour
{

    [Header("Cellular Automata Incrementation")]
    public bool autoIncrement = false; // If this is true will play regularly
    public bool increment = false;

    [Header("Cellular Automata Preset")]
    public AutomataPreset automataPreset; // Allow for use of predefined Automatas
    public bool loadPreset = true; // Have set to true at beggining so that the values are populated

    [Header("Cellular Automata Settings")]
    public Vector3Int boundingBoxSizeX = new Vector3Int(25,25,25); // The size of the simulation space
    public bool[] survivalX = new bool[26]; // The number of neighbours that allow for survival
    public bool[] birthX = new bool[26]; // The number of neighbours that allow for birth
    public int deathTimeX; // The amount of ticks it takes for a cell to die
    public NeighbourMethod methodX; // The method used to determine the cells neighbours

    [Header("Cellular Automata Spawn Settings")]
    public float middleSpawnPercentX = 0.2f; // Percent of bounding box occupied at start
    public float middleSpawnChanceX = 0.33f; // The chance that a cell in the middle percent will be alive at start

    [Header("Cellular Automata Rendering")]
    public GameObject cellularAutomataRend;
    public Color aliveColorX;
    public Color deadColorX;
    private CellularAutomataRenderer cellAutomRendererComp;

    [Header("UI STUFF")]
    public GameObject playButton;
    public GameObject pauseButton;

    private class Cell {

        //Default to these values
        public bool alive = false;
        public bool dead = true;
        public bool dying = false;
        public int deathTimer = 0;
        public int numOfNeighbours = 0;
    }

    private Cell[,,] cellArray; //Cell[xPos, yPos, zPos]
    private Color[,,] cellColorArray;

    // Start is called before the first frame update
    void Start() {

        cellAutomRendererComp = cellularAutomataRend.GetComponent<CellularAutomataRenderer>();

        if (loadPreset) {

            //Set all the local variables to the corresponding preset variables
            boundingBoxSizeX = automataPreset.boundingBoxSize;
            survivalX = automataPreset.survival;
            birthX = automataPreset.birth;
            deathTimeX = automataPreset.deathTime;
            methodX = automataPreset.method;

            middleSpawnChanceX = automataPreset.middleSpawnChance;
            middleSpawnPercentX = automataPreset.middleSpawnPercent;

            aliveColorX = automataPreset.aliveColor;
            deadColorX = automataPreset.deadColor;
        }

        //Initialize the cell array and color array
        cellArray = new Cell[boundingBoxSizeX.x, boundingBoxSizeX.y, boundingBoxSizeX.z];
        cellColorArray = new Color[boundingBoxSizeX.x, boundingBoxSizeX.y, boundingBoxSizeX.z];

        for (int x = 0; x < boundingBoxSizeX.x; x++) {
            for (int y = 0; y < boundingBoxSizeX.y; y++) {
                for (int z = 0; z < boundingBoxSizeX.z; z++) {

                    cellArray[x, y, z] = new Cell();
                    cellColorArray[x, y, z] = Color.clear;
                }
            }
        }

        // Set the middle X% of cells to alive
        int middleSizeX = Mathf.FloorToInt(boundingBoxSizeX.x * middleSpawnPercentX); // The number of cells in middle on x axis
        int middleSizeY = Mathf.FloorToInt(boundingBoxSizeX.y * middleSpawnPercentX); // The number of cells in middle on y axis
        int middleSizeZ = Mathf.FloorToInt(boundingBoxSizeX.z * middleSpawnPercentX); // The number of cells in middle on z axis

        int middlePosX = Mathf.FloorToInt(boundingBoxSizeX.x / 2); // the center point along x axis
        int middlePosY = Mathf.FloorToInt(boundingBoxSizeX.y / 2); // the center point along y axis
        int middlePosZ = Mathf.FloorToInt(boundingBoxSizeX.z / 2); // the center point along z axis

        for (int x = 0; x < middleSizeX; x++) {
            for (int y = 0; y < middleSizeY; y++) {
                for (int z = 0; z < middleSizeZ; z++) {
                    

                    //determine the true x,y,z values of the cell
                    int trueX = (middlePosX - Mathf.FloorToInt(middleSizeX / 2)) + x;
                    int trueY = (middlePosY - Mathf.FloorToInt(middleSizeY / 2)) + y;
                    int trueZ = (middlePosZ - Mathf.FloorToInt(middleSizeZ / 2)) + z;

                    //Make sure the cell is in bounds
                    if (trueX >= 0 && trueX < boundingBoxSizeX.x && trueY >= 0 && trueY < boundingBoxSizeX.y && trueZ >= 0 && trueZ < boundingBoxSizeX.z) {

                        if (Random.Range(0f, 1f) < middleSpawnChanceX) {

                            setCellAlive(trueX, trueY, trueZ);
                            //Debug.Log("Alive Cell at: x = " + trueX + ", y = " + trueY + ", z = " + trueZ);
                        }
                    }
                }
                //int remZ = middleSizeZ % 2;
            }
            //int remY = middleSizeY % 2;
        }
        //int remX = middleSizeX % 2;

        cellAutomRendererComp.SetCells(cellColorArray);
    }

    // Update is called once per frame
    void Update() {

        //Check if the play or pause button should be showing
        if (autoIncrement) {

            pauseButton.SetActive(true);
            playButton.SetActive(false);
        } else {

            pauseButton.SetActive(false);
            playButton.SetActive(true);
        }

        if (autoIncrement || increment) {

            //Determine current number of cell neighbours
            for (int x = 0; x < cellArray.GetLength(0); x++) {
                for (int y = 0; y < cellArray.GetLength(1); y++) {
                    for (int z = 0; z < cellArray.GetLength(2); z++) {

                        int numOfNeighbours = 0;

                        if (methodX == NeighbourMethod.Moore) {

                            numOfNeighbours = NeighbourCountMoore(x, y, z); //Determine the number of Moore Neighbours
                        } else if (methodX == NeighbourMethod.VonNeuman) {

                            numOfNeighbours = NeighbourCountVon(x, y, z); //Determine the number of Moore Neighbours
                        }

                        cellArray[x, y, z].numOfNeighbours = numOfNeighbours;
                        //Debug.Log(numOfNeighbours);
                    }
                }
            }

            if (true) { 
                //Loop through each of the cells and determine what they need to do
                for (int x = 0; x < cellArray.GetLength(0); x++) {
                    for (int y = 0; y < cellArray.GetLength(1); y++) {
                        for (int z = 0; z < cellArray.GetLength(2); z++) {

                            //Check if the cell is alive
                            if (cellArray[x, y, z].alive == true) {

                                bool cellSurvived = false;

                                // Check if this number of neighbours exists in the survival array
                                for (int i = 0; i < survivalX.Length; i++) {
                                    //Checks if survival is true for a given number and if it is checks if that number is the same as cells num of neighbours
                                    if (survivalX[i] && i + 1 == cellArray[x, y, z].numOfNeighbours) {
                                        cellSurvived = true;
                                        continue;
                                    }
                                }

                                if (!cellSurvived) {

                                    setCellDying(x, y, z);
                                }

                                continue;
                            }

                            //Check if the cell is dead
                            if (cellArray[x, y, z].dead == true) {

                                bool cellBirthed = false;

                                // Check if this number of neighbours exists in the birth array
                                for (int i = 0; i < birthX.Length; i++) {
                                    //Checks if survival is true for a given number and if it is checks if that number is the same as cells num of neighbours
                                    if (birthX[i] && i + 1 == cellArray[x, y, z].numOfNeighbours) {
                                        cellBirthed = true;
                                        continue;
                                    }
                                }

                                if (cellBirthed) {

                                    setCellAlive(x, y, z);
                                }

                                continue;
                            }

                            //Check if the cell is dying
                            if (cellArray[x, y, z].dying == true) {

                                decrementDying(x, y, z);
                                continue;
                            }
                        }
                    }
                }
            }

            //Update the rendered cells
            cellAutomRendererComp.SetCells(cellColorArray);

            increment = false;
        }

    }

    private int NeighbourCountMoore(int x, int y, int z) {

        int count = 0;
        float counter = Time.realtimeSinceStartup;

        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                for (int k = 0; k < 3; k++) {

                    Vector3Int nPos = new Vector3Int();
                    nPos.x = (x - 1) + i;
                    nPos.y = (y - 1) + j;
                    nPos.z = (z - 1) + k;

                    if (CellInBounds(nPos.x, nPos.y, nPos.z)) {
                        if (cellArray[nPos.x, nPos.y, nPos.z].alive == true) {

                            count++;
                        }
                    }
                }
            }
        }

        //Debug.Log("Moore: " + (Time.realtimeSinceStartup - counter));
        return count;
    }

    private int NeighbourCountVon(int x, int y, int z) {

        int count = 0;
        float counter = Time.realtimeSinceStartup;

        //Top
        Vector3Int top = new Vector3Int(x, y, z) + Vector3Int.up;
        if (CellInBounds(top.x, top.y, top.z)) {

            if (cellArray[top.x, top.y, top.z].alive == true) {

                count++;
            }
        }

        //Bot
        Vector3Int bot = new Vector3Int(x, y, z) + Vector3Int.down;
        if (CellInBounds(bot.x, bot.y, bot.z)) {

            if (cellArray[bot.x, bot.y, bot.z].alive == true) {

                count++;
            }
        }

        //Left
        Vector3Int left = new Vector3Int(x, y, z) + Vector3Int.left;
        if (CellInBounds(left.x, left.y, left.z)) {

            if (cellArray[left.x, left.y, left.z].alive == true) {

                count++;
            }
        }

        //Right
        Vector3Int right = new Vector3Int(x, y, z) + Vector3Int.right;
        if (CellInBounds(right.x, right.y, right.z)) {

            if (cellArray[right.x, right.y, right.z].alive == true) {

                count++;
            }
        }

        //Forward
        Vector3Int forw = new Vector3Int(x, y, z) + Vector3Int.forward;
        if (CellInBounds(forw.x, forw.y, forw.z)) {

            if (cellArray[forw.x, forw.y, forw.z].alive == true) {

                count++;
            }
        }

        //Back
        Vector3Int back = new Vector3Int(x, y, z) + Vector3Int.back;
        if (CellInBounds(back.x, back.y, back.z)) {

            if (cellArray[back.x, back.y, back.z].alive == true) {

                count++;
            }
        }


        //Debug.Log("Von: " + (Time.realtimeSinceStartup - counter));
        return count;
    }

    private bool CellInBounds(int x, int y, int z) {

        //Simply checks whether the cell is within the bounds of the cellArray
        if (x >= 0 && x < cellArray.GetLength(0) && y >= 0 && y < cellArray.GetLength(1) && z >= 0 && z < cellArray.GetLength(2)) {

            return true;
        }

        return false;
    }

    private void setCellAlive(int x, int y, int z) {

        cellArray[x, y, z].alive = true;
        cellArray[x, y, z].dead = false;
        cellArray[x, y, z].dying = false;
        cellArray[x, y, z].deathTimer = 0;

        cellColorArray[x, y, z] = aliveColorX;
    }

    private void setCellDead(int x, int y, int z) {

        cellArray[x, y, z].alive = false;
        cellArray[x, y, z].dead = true;
        cellArray[x, y, z].dying = false;
        cellArray[x, y, z].deathTimer = 0;

        cellColorArray[x, y, z] = Color.clear;
    }

    private void setCellDying(int x, int y, int z) {

        cellArray[x, y, z].alive = false;
        cellArray[x, y, z].dead = false;
        cellArray[x, y, z].dying = true;
        cellArray[x, y, z].deathTimer = deathTimeX;

        cellColorArray[x, y, z] = aliveColorX;
    }

    private void decrementDying(int x, int y, int z) {

        cellArray[x, y, z].alive = false;
        cellArray[x, y, z].dead = false;
        cellArray[x, y, z].dying = true;
        cellArray[x, y, z].deathTimer = cellArray[x, y, z].deathTimer - 1;

        cellColorArray[x, y, z] = Color.Lerp(deadColorX, aliveColorX, (cellArray[x, y, z].deathTimer / (deathTimeX + 0.0001f)));

        //if the cells death timer drops to 0 then kill the cell
        if (cellArray[x, y, z].deathTimer <= 0) {

            setCellDead(x, y, z);
        }
    }

    public void ResetAutomata() {

        cellAutomRendererComp = cellularAutomataRend.GetComponent<CellularAutomataRenderer>();

        if (loadPreset) {

            //Set all the local variables to the corresponding preset variables
            boundingBoxSizeX = automataPreset.boundingBoxSize;
            survivalX = automataPreset.survival;
            birthX = automataPreset.birth;
            deathTimeX = automataPreset.deathTime;
            methodX = automataPreset.method;

            middleSpawnChanceX = automataPreset.middleSpawnChance;
            middleSpawnPercentX = automataPreset.middleSpawnPercent;

            aliveColorX = automataPreset.aliveColor;
            deadColorX = automataPreset.deadColor;
        }

        //Initialize the cell array and color array
        cellArray = new Cell[boundingBoxSizeX.x, boundingBoxSizeX.y, boundingBoxSizeX.z];
        cellColorArray = new Color[boundingBoxSizeX.x, boundingBoxSizeX.y, boundingBoxSizeX.z];

        for (int x = 0; x < boundingBoxSizeX.x; x++) {
            for (int y = 0; y < boundingBoxSizeX.y; y++) {
                for (int z = 0; z < boundingBoxSizeX.z; z++) {

                    cellArray[x, y, z] = new Cell();
                    cellColorArray[x, y, z] = Color.clear;
                }
            }
        }

        // Set the middle X% of cells to alive
        int middleSizeX = Mathf.FloorToInt(boundingBoxSizeX.x * middleSpawnPercentX); // The number of cells in middle on x axis
        int middleSizeY = Mathf.FloorToInt(boundingBoxSizeX.y * middleSpawnPercentX); // The number of cells in middle on y axis
        int middleSizeZ = Mathf.FloorToInt(boundingBoxSizeX.z * middleSpawnPercentX); // The number of cells in middle on z axis

        int middlePosX = Mathf.FloorToInt(boundingBoxSizeX.x / 2); // the center point along x axis
        int middlePosY = Mathf.FloorToInt(boundingBoxSizeX.y / 2); // the center point along y axis
        int middlePosZ = Mathf.FloorToInt(boundingBoxSizeX.z / 2); // the center point along z axis

        for (int x = 0; x < middleSizeX; x++) {
            for (int y = 0; y < middleSizeY; y++) {
                for (int z = 0; z < middleSizeZ; z++) {


                    //determine the true x,y,z values of the cell
                    int trueX = (middlePosX - Mathf.FloorToInt(middleSizeX / 2)) + x;
                    int trueY = (middlePosY - Mathf.FloorToInt(middleSizeY / 2)) + y;
                    int trueZ = (middlePosZ - Mathf.FloorToInt(middleSizeZ / 2)) + z;

                    //Make sure the cell is in bounds
                    if (trueX >= 0 && trueX < boundingBoxSizeX.x && trueY >= 0 && trueY < boundingBoxSizeX.y && trueZ >= 0 && trueZ < boundingBoxSizeX.z) {

                        if (Random.Range(0f, 1f) < middleSpawnChanceX) {

                            setCellAlive(trueX, trueY, trueZ);
                            //Debug.Log("Alive Cell at: x = " + trueX + ", y = " + trueY + ", z = " + trueZ);
                        }
                    }
                }
                //int remZ = middleSizeZ % 2;
            }
            //int remY = middleSizeY % 2;
        }
        //int remX = middleSizeX % 2;

        cellAutomRendererComp.SetCells(cellColorArray);
    }

    public void TogglePlay() {

        autoIncrement = !autoIncrement;
    }

    public void IncrementOneStep() {

        increment = true;
    }
}
