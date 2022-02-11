﻿public class Gaussian
{
    private readonly float[,] _coefficients;
    private readonly int _diameter;
    
    public Gaussian(float[,] coefficients, int diameter)
    {
        _coefficients = coefficients;
        _diameter = diameter;
    }
    
    public float[,] Apply(float[,] grid)
    {
        var gridHeight = grid.GetLength(0);
        var gridWidth = grid.GetLength(1);
        var newGridValues = new float[gridHeight, gridWidth];
        var radius = _diameter / 2;

        for (var i = 0; i < gridWidth; i++)
        {
            for (var j = 0; j < gridHeight; j++)
            {
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