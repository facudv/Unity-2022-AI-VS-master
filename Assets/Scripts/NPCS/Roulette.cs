using System.Collections.Generic;
using UnityEngine;

public class Roulette 
{

    private List<string> posibleOptions;
    private List<int> posibleWeights;
    
    public Roulette(List<string> options, List<int> weights)
    {
        posibleOptions = options;
        posibleWeights = weights;
    }

    public string RouletteWheelSelection()
    {
        float totalWeights = 0f;
        foreach (float weight in posibleWeights)
            totalWeights += weight;

        float accumulator = 0f;
        float randomNumber = Random.value;

        for (int i = 0; i < posibleOptions.Count; i++)
        {
            float probability = posibleWeights[i] / totalWeights;
            accumulator += probability;
            if (randomNumber <= accumulator)
                return posibleOptions[i];
        }

        return default (string);
    }
}
