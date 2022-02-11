﻿public class Gaussian
{
    private readonly float[,] _coefficients;
    private readonly int _diameter;
    
    public Gaussian(float[,] coefficients, int diameter)
    {
        _coefficients = coefficients;
        _diameter = diameter;
    }
    
    public float[,] Apply(int[,] grid, int gridWidth, int gridHeight)
    {
        var newGridValues = new float[gridHeight, gridWidth];
        var radius = _diameter / 2;

        for (var i = 0; i < gridWidth; i++)
        {
            for (var j = 0; j < gridHeight; j++)
            {
                if (grid[j, i] == 0)
                    continue;

                for (var dx = -radius; dx <= radius; dx++)
                {
                    for (var dy = -radius; dy <= radius; dy++)
                    {
                        var x = i + dx;
                        var y = j + dy;

                        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
                            continue;

                        newGridValues[y, x] += grid[j, i] * _coefficients[dy + radius, dx + radius];
                    }
                }
            }
        }

        return newGridValues;
    }

}