var elementsPerProcessor = 500;

MPI.Environment.Run(ref args, comm =>
{
    if(comm.Rank is 0) {
        int[][] vector = new int[comm.Size][];
        var rnd = new Random();
        for (int i = 0; i < comm.Size; i++)
        {
            vector[i] = new int[elementsPerProcessor];
            for (int j = 0; j < elementsPerProcessor; j++)
            {
                vector[i][j] = rnd.Next();
            }
        }
        
        var subVector = comm.Scatter(vector, 0);
        var minValueSubVector = subVector.Min();
        var maxValueSubVector = subVector.Max();
        Console.WriteLine($"Processor with rank {comm.Rank} counted min value {minValueSubVector}");
        Console.WriteLine($"Processor with rank {comm.Rank} counted max value {maxValueSubVector}");
        var minMaxTuple = new Tuple<int, int>(minValueSubVector, maxValueSubVector);
        var minMaxValues = comm.Gather(minMaxTuple, 0);
        var minValues = minMaxValues.Select(t => t.Item1);
        var maxValues = minMaxValues.Select(t => t.Item2);
        Console.WriteLine($"Minimal value of vector is {minValues.Min()}");
        Console.WriteLine($"Maximum value of vector is {maxValues.Max()}");
    }
    else
    {
        var subVector = comm.Scatter<int[]>(0);
        var minValueSubVector = subVector.Min();
        var maxValueSubVector = subVector.Max();
        Console.WriteLine($"Processor with rank {comm.Rank} counted min value {minValueSubVector}");
        Console.WriteLine($"Processor with rank {comm.Rank} counted max value {maxValueSubVector}");
        var minMaxTuple = new Tuple<int, int>(minValueSubVector, maxValueSubVector);
        comm.Gather(minMaxTuple, 0);
    }
});