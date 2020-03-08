using DataStructures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace StateMachineEngine
{
    public class StateMachineEngine : INotifyPropertyChanged
    {
        private readonly UnityContainer _UnityContainer;

        public StateMachineEngine()
        {
            _UnityContainer = new UnityContainer();
        }

        private IVertex<IState> _Current;
        public IVertex<IState> Current
        {
            get { return _Current; }
            protected set
            {
                _Current = value;
                NotifyPropertyChanged(nameof(Current));
            }
        }

        public StateModule CurrentState { get; protected set; }
        protected StateModule LastVertex { get; set; }
        public async Task<object> Run(IVertex<IState> startVertex)
        {
            if (startVertex == null) throw new ArgumentNullException(nameof(startVertex));
            Current = startVertex;
            CurrentState = Current.Value as StateModule;
            object stateResult = null;
            while (Current != null && CurrentState != null)
            {
                stateResult = await RunState(CurrentState, stateResult);
                LastVertex = CurrentState;
                foreach (IEdge edge in Current.Edges)
                {
                    var vertex = edge.V as IVertex<IState>;
                    if (vertex != null && vertex.Value != null && vertex.Value.Condition(stateResult))
                    {
                        Current = vertex;
                        CurrentState = Current.Value as StateModule;
                    }
                }
                //there is no edge which contains a state which can be used - exit loop
                if (LastVertex == CurrentState) CurrentState = null;
            }


            return null;
        }
        private async Task<object> RunState(StateModule stateModule, object previousComputedStateResult)
        {
            //load assembly
            if (stateModule.Assembly == null)
            {
                stateModule.LoadAssembly();
            }
            MethodInfo methodInfo = stateModule.LoadMethodTyp();
            object classInstanceToInvokeMethod = null;
            if (methodInfo.IsStatic == false)
            {
                if (!_UnityContainer.IsRegistered(methodInfo.DeclaringType))
                {
                    _UnityContainer.RegisterType(methodInfo.DeclaringType);
                }
                classInstanceToInvokeMethod = _UnityContainer.Resolve(methodInfo.DeclaringType);
            }

            if (classInstanceToInvokeMethod != null || methodInfo.IsStatic)
            {
                var methodParametersInfo = stateModule.GenerateMethodParameters().OrderBy(a => a.Position);
                List<object> param = new List<object>();
                foreach (ParameterInfo paramInfo in methodParametersInfo)
                {
                    //find the method parameter value from the current state 
                    MethodParameter methodParameter = stateModule.MethodParameters.FirstOrDefault(a => a.Position == paramInfo.Position);
                    string rawValue = methodParameter.ParameterValue;
                    //generate value
                    if (methodParameter.UsePreviousStateValue)
                    {
                        param.Add(previousComputedStateResult);
                    }
                    else
                    {
                        object val = InitDefaultValue(paramInfo.ParameterType, methodParameter.ParameterValue);
                        param.Add(val);
                    }
                }
                object resultMethod = null;
                if (methodInfo.IsStatic)
                {
                    resultMethod = methodInfo.Invoke(null, param.ToArray());
                }
                else
                {
                    resultMethod = methodInfo.Invoke(classInstanceToInvokeMethod, param.ToArray());
                }
                if (resultMethod is Task)
                {
                    Task resultMethodTask = (resultMethod as Task);
                    await resultMethodTask;

                    if (methodInfo.ReturnType.IsGenericType)
                    {
                        var resultProperty = methodInfo.ReturnType.GetProperty("Result");
                        var x = resultProperty.GetValue(resultMethodTask);
                        return x;
                    }


                }
                else
                {
                    return Task.FromResult(resultMethod);
                }
            }
            return null;
        }
        //private async Task<object> RunState(StateModule stateModule, object previousComputedStateResult)
        //{
        //    stateModule = Current.Value;
        //    //load assembly
        //    if (stateModule.Assembly == null)
        //    {
        //        Current?.Value?.LoadAssembly();
        //    }
        //    //load method
        //    MethodInfo methodInfo = stateModule.LoadMethodTyp();
        //    object classInstanceToInvokeMethod = null;
        //    if (methodInfo.IsStatic == false)
        //    {
        //        classInstanceToInvokeMethod = Activator.CreateInstance(methodInfo.DeclaringType, null);
        //    }
        //    if (classInstanceToInvokeMethod != null || methodInfo.IsStatic)
        //    {
        //        var methodParametersInfo = stateModule.GenerateMethodParameters().OrderBy(a => a.Position);
        //        List<object> param = new List<object>();
        //        foreach (ParameterInfo paramInfo in methodParametersInfo)
        //        {
        //            //find the method parameter value from the current state 
        //            MethodParameter methodParameter = stateModule.MethodParameters.FirstOrDefault(a => a.Position == paramInfo.Position);
        //            string rawValue = methodParameter.ParameterValue;
        //            //generate value
        //            if (methodParameter.UsePreviousStateValue)
        //            {
        //                param.Add(previousComputedStateResult);
        //            }
        //            else
        //            {
        //                object val = InitDefaultValue(paramInfo.ParameterType, methodParameter.ParameterValue);
        //                param.Add(val);
        //            }
        //        }
        //        object resultMethod = null;
        //        if (methodInfo.IsStatic)
        //        {
        //            resultMethod = methodInfo.Invoke(null, param.ToArray());
        //        }
        //        else
        //        {
        //            resultMethod = methodInfo.Invoke(classInstanceToInvokeMethod, param.ToArray());
        //        }
        //        if (resultMethod is Task)
        //        {
        //            Task resultMethodTask = (resultMethod as Task);
        //            await resultMethodTask;

        //            if (methodInfo.ReturnType.IsGenericType)
        //            {
        //                var resultProperty = methodInfo.ReturnType.GetProperty("Result");
        //                var x = resultProperty.GetValue(resultMethodTask);
        //                return x;
        //            }


        //        }
        //        else
        //        {
        //            return Task.FromResult(resultMethod);
        //        }
        //    }
        //    return null;
        //}

        private object InitDefaultValue(Type memberType, object value)
        {
            if (memberType.IsValueType)
            {
                if (memberType == typeof(System.Int32))
                {
                    return Convert.ToInt32(value);
                }

                object x = memberType.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, new object[0]);



                return x;
            }
            else
            {
                if (memberType == typeof(string))
                {
                    return $"{value}";
                }

                object x = Activator.CreateInstance(memberType, null);
                return x;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }



    }
}
