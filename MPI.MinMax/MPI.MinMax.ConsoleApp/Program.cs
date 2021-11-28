// See https://aka.ms/new-console-template for more information

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
        Console.WriteLine($"Processor with rank {comm.Rank} counted min value {minValueSubVector}");
        var minValues = comm.Gather(minValueSubVector, 0);
        Console.WriteLine(minValues);
        Console.WriteLine($"Minimal value of vector is {minValues.Min()}");
    }
    else
    {
        var subVector = comm.Scatter<int[]>(0);
        var minValueSubVector = subVector.Min();
        Console.WriteLine($"Processor with rank {comm.Rank} counted min value {minValueSubVector}");
        comm.Gather(minValueSubVector, 0);
    }
});