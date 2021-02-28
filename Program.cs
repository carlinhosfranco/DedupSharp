using System;
using System.Collections.Concurrent;
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
        public static void Main(string[] args)
        {
            int BufferSize = 1;
            var inputBuffer = new BlockingCollection<int[][]>(BufferSize);
            var fragBuffer = new BlockingCollection<int[]>(BufferSize);

            var farmTask = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

            var stage1 = farmTask.StartNew(() => InputStage.ReadArrayNumbers(inputBuffer, 5));
            var stage2 = farmTask.StartNew(() => FragmentStage.FragIntoSingleNumbers(inputBuffer, fragBuffer));
            var stage3 = farmTask.StartNew(() => OutpuStage.Output(fragBuffer));

            Task.WaitAll(stage1, stage2, stage3);
        }
    }
}
