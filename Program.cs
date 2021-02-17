using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DedupSharp.Stages;

namespace DedupSharp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //stages 
            var inputStage = new InputStage<int[]>();
            var fragmentStage = new FragmentStage<int[]>();
            var fragmentTask = fragmentStage.ConsumeAsync(inputStage.BufferStage);

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 5; j++)
                    inputStage.FillInternalBuffer(new[] { i*1, i*2, i*3, i*4, i*5, i*6 });
            }
            
            inputStage.BufferStage.Complete();

            var result = await fragmentTask;
            
            fragmentStage.Result(result);
        }
    }
}
