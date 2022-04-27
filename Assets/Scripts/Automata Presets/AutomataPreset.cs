using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CellularAutomataPreset", menuName = "Cellular Automata/New Cellular Automata Preset")]
public class AutomataPreset : ScriptableObject {

    [Header("Cellular Automata Settings")]
    public Vector3Int boundingBoxSize = new Vector3Int(50,50,50); // The size of the simulation space
    public bool[] survival = new bool[26]; // The number of neighbours that allow for survival
    public bool[] birth = new bool[26]; // The number of neighbours that allow for birth
    public int deathTime; // The amount of ticks it takes for a cell to die
    public NeighbourMethod method; // The method used to determine the cells neighbours

    [Header("Cellular Automata Spawn Settings")]
    public float middleSpawnPercent = 0.2f; // Percent of bounding box occupied at start
    public float middleSpawnChance = 0.33f; // The chance that a cell in the middle percent will be alive at start

    [Header("Cellular Automata Rendering Settings")]
    public Color aliveColor = new Color(202,255,114);
    public Color deadColor = new Color(255, 176, 160);

}
