using DataStructures;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachineEngine.UI
{
    public class TransationWindowViewModel : BindableBase
    {
        private IEdge _Edge;
        public IEdge Edge
        {
            get { return _Edge; }
            set
            {
                SetProperty(ref _Edge, value, nameof(Edge));
                if (Transation == null)
                {
                    Transation = new Transation();
                }
                else
                {
                }

            }
        }
        public Transation Transation
        {
            get; set;
        }
    }
}
