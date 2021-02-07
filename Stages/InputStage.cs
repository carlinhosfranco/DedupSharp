using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks.Dataflow;
using DedupSharp.Helpers.IO;

namespace DedupSharp.Stages
{
    public class InputStage 
    {
        private readonly BufferBlock<int[]> _bufferStage;
        private const int WS = 10;
        private int[] _input;
        private Random _ran = new Random();
        public InputStage()
        {
            
        }

        public BufferBlock<T[]> CreateQueue<T>()
        {
            var source = new BufferBlock<T[]>();
            var queue = new Queue<T>();
            
            var target = new ActionBlock<T>(item =>
            {
                // Add the item to the queue.
                queue.Enqueue(item);
                // Remove the oldest item when the queue size exceeds the window size.
                if (queue.Count > WS)
                    queue.Dequeue();
                
                // Post the data in the queue to the source block when the queue size
                // equals the window size.
                if (queue.Count == WS)
                    source.Post(queue.ToArray());
            });

            target.Completion.ContinueWith(delegate {
                if (queue.Count > 0 && queue.Count < WS)
                    source.Post(queue.ToArray());
                
                source.Complete();
            });

            return source;
        }

        // public void Process(ITargetBlock<int[]> target)
        // {            
        //     var num = GetNumbers();            
        //     target.Post(num);
        //     target.Complete();
        //     target.Completion.Wait();
        // }

        public int[] GetNumbers()
        {
            _input = new int[WS];

            for (int i = 0; i < WS; i++)
                _input[i] = _ran.Next(1, 500);
            
            return _input;            
        }

        // public TransformBlock<double[], double[]> Que()
        // {
        //     return new TransformBlock<double[], double[]>( q => {
        //                     for (int i = 0; i < q.Length; i++)
        //                         Console.WriteLine($"Source: {q[i]}\n");
        //                     return _input;
        //                 });
        // }

        // public void ReadFilesFromDisk( string path, ref ITargetBlock<FileStream> queue)
        // {
        //     string[] files = ReadFiles.Get(path);

        //     if (files != null)
        //     {
        //         for (int i = 0; i < files.Length; i++)
        //         {
        //             FileStream file = File.Open(files[i], FileMode.Open, FileAccess.Read);                                        
        //             queue.Post(file);
        //         }
        //         queue.Complete();               
        //     }
        // }        
    }
}