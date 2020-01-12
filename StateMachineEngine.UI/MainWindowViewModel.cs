﻿using DataStructures;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StateMachineEngine.UI
{
    public class MainWindowViewModel : Prism.Mvvm.BindableBase
    {
        readonly StateMachineEngine _stateMachine = new StateMachineEngine();
        public MainWindowViewModel()
        {
            PropertyChanged += Window1ViewModel_PropertyChanged;
            Graph = new Graph();
        }

        private void Window1ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(Graph).Equals(e.PropertyName) && Graph != null)
            {
                if (Graph != null)
                {
                    Graph.CreateVertexFunc = VertexFactory;
                    OnRunStateMachineCommand(Graph.Start as IVertex<StateModule>);
                }
            }
        }

        protected IVertex<IState> VertexFactory()
        {
            return new Vertex<IState>();
        }
        private ICommand _ClickCommand;
        public ICommand ClickCommand => _ClickCommand ?? (_ClickCommand = new DelegateCommand<IVertex>(OnClickCommand));

        protected void OnClickCommand(IVertex param)
        {
            if (param != null)
            {
                StateModuleWindow moduleFunctionWindow = new StateModuleWindow();
                var moduleFunctionWindowViewModel = new StateModuleWindowViewModel();
                moduleFunctionWindowViewModel.Vertex = param;
                moduleFunctionWindow.DataContext = moduleFunctionWindowViewModel;
                moduleFunctionWindow.ShowDialog();
            }
        }

        private ICommand _RunStateMachineCommand;
        public ICommand RunStateMachineCommand => _RunStateMachineCommand ?? (_RunStateMachineCommand = new DelegateCommand<IVertex>(OnRunStateMachineCommand));

        protected async void OnRunStateMachineCommand(IVertex param)
        {
            try
            {
                param = Graph.Start;
                await _stateMachine?.Run(param as IVertex<StateModule>);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                System.Diagnostics.Debug.WriteLine(e);
            }

        }

        private Graph _Graph;
        public Graph Graph
        {
            get { return _Graph; }
            set { SetProperty(ref _Graph, value, nameof(Graph)); }
        }


    }
}
