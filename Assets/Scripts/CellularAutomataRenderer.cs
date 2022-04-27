using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class CellularAutomataRenderer : MonoBehaviour {

    ParticleSystem system;
    ParticleSystem.Particle[] cells;
    List<ParticleSystem.Particle> cellsList = new List<ParticleSystem.Particle>();

    public float cellScale = 0.1f;
    public float scale = 1f;

    private void Start() {
        system = GetComponent<ParticleSystem>();
    }

    public void SetCells(Color[,,] colors) {

        for (int i = 0; i < colors.GetLength(0); i++) {
            for (int j = 0; j < colors.GetLength(1); j++) {
                for (int k = 0; k < colors.GetLength(2); k++) {

                    //Check if the cell should be rendered here
                    if (colors[i, j, k] != Color.clear) {

                        ParticleSystem.Particle part = new ParticleSystem.Particle();
                        part.position = new Vector3(i, j, k) * scale;
                        part.startColor = colors[i, j, k];
                        part.startSize = cellScale;

                        cellsList.Add(part);
                    }
                }
            }
        }

        //Initialize cells array and update the particle system
        cells = new ParticleSystem.Particle[cellsList.Count];
        cells = cellsList.ToArray();
        system.SetParticles(cells, cells.Length);
        //Debug.Log("Cells set! Cell count: " + cells.Length);

        //Reset the particles list
        cellsList.Clear();
    }
}
