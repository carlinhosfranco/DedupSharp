using System;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace DedupSharp.Stages
{
    public class OutpuStage
    {
        
        public ActionBlock<(int, double)[]> PrintConsole()
        {
            return new ActionBlock<(int, double)[]>( num =>
                            {
                                StringBuilder result = new StringBuilder();
                                result.Append("Numbers Resut: ");
                                for (int i = 0; i < num.Length; i++)
                                    result.Append("A Raiz de: " + num[i].Item1.ToString() + " é " + string.Format("{0:0.000}", num[i].Item2 ));
                                
                                Console.WriteLine(result.ToString());
                            }
                        );            
        }
    }
}