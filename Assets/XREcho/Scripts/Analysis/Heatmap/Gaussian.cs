public class Gaussian
{
    public readonly float[,] Coefficients;
    public readonly int Radius;

    public Gaussian(float[,] coefficients, int radius)
    {
        Coefficients = coefficients;
        Radius = radius;
    }

    // TODO: Use Compute Shaders to make this run faster (takes approximately 95% of the exec time on the generation of a heatmap)
    public float[,] Apply(float[,] grid)
    {
        var gridHeight = grid.GetLength(0);
        var gridWidth = grid.GetLength(1);
        var newGridValues = new float[gridHeight, gridWidth];

        for (var i = 0; i < gridWidth; i++)
        {
            for (var j = 0; j < gridHeight; j++)
            {
                for (var dx = -Radius; dx <= Radius; dx++)
                {
                    for (var dy = -Radius; dy <= Radius; dy++)
                    {
                        var x = i + dx;
                        var y = j + dy;

                        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
                            continue;

                        newGridValues[y, x] += grid[j, i] * Coefficients[dy + Radius, dx + Radius];
                    }
                }
            }
        }

        return newGridValues;
    }
}