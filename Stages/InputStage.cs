using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DedupSharp.Stages
{
    public static class InputStage
    {
        public static void ReadArrayNumbers(BlockingCollection<int[][]> output, int seed)
        {
            try
            {
                for (int i = 0; i < seed; i++)
                    for (int j = 0; j < 5; j++)
                    {
                        output.Add(
                            new[]{
                                new[] { i*1, i*2, i*3, i*4, i*5, i*6 },
                                new[] { i*7, i*8, i*9, i*10, i*11, i*12 },
                                new[] { i*13, i*14, i*15, i*16, i*17, i*18 },
                                new[] { i*19, i*20, i*21, i*22, i*23, i*24 },
                            }
                        );
                    }
            }
            finally
            {
                output.CompleteAdding();
            }
        }
    }
}