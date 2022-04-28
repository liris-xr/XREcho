﻿using UnityEngine;

public class GaussianProvider : MonoBehaviour, IGaussianProvider
{
    public float standardDeviation = 5f;

    public Gaussian CreateGaussian()
    {
        var radius = Mathf.RoundToInt(3 * standardDeviation);
        var diameter = 2 * radius + 1;

        var coefficients = new float[diameter, diameter];

        for (var i = -radius; i <= radius; i++)
        {
            for (var j = -radius; j <= radius; j++)
            {
                var dist = i * i + j * j;
            
                if (dist > radius * radius)
                {
                    coefficients[j + radius, i + radius] = 0;
                    continue;
                }

                coefficients[j + radius, i + radius] = Mathf.Exp(-dist / (2 * standardDeviation * standardDeviation)) / standardDeviation;
            }
        }

        return new Gaussian(coefficients, radius);
    }
}