using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DedupSharp.Queues;

namespace DedupSharp.Stages
{
    public class FragmentStage
    {

        private TransformBlock<int[], (int, double)[]> _proc;
        private BufferBlock<int[]> _buff; 
        private ActionBlock<(int, double)[]> _out;
    
        public FragmentStage(BufferBlock<int[]> buff, ActionBlock<(int, double)[]> outQ ) 
        {
            _proc = new TransformBlock<int[], (int, double)[]>( num => {
                            var size = num.Length;
                            var result = new (int, double)[size];

                            for (int i = 0; i < size; i++)
                                result[i] = (num[i], Math.Sqrt(num[i]) );

                            return result;
            } );  
            
            _buff = buff;
            _out = outQ;
        }

        public void Process()
        {
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            var stageProcess = new StageProcess<int>();

            stageProcess.LinkTo(_buff, linkOptions);

            stageProcess.Completion.ContinueWith(delegate  { _proc.Complete(); });

            //var completion = _proc.Completion.ContinueWith(delegate { Cons }  );

            _buff.LinkTo(_proc);            
            _proc.LinkTo(_out);
            
            //_buff.Complete();
            
            stageProcess.Complete();
        }

        
       
    }
}