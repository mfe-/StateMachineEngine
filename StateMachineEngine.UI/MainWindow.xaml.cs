using DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace StateMachineEngine.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _GraphVisualization.DataContractSerializerSettingsActionInvoker = (DataContractSerializerSettings a) =>
            {
                a.KnownTypes = new List<Type>()
                {
                    typeof(Graph),
                    typeof(Vertex<StateModule>),
                    typeof(Vertex<IState>),
                    typeof(Edge<StateModule>),
                    typeof(StateModule),
                };
                a.DataContractResolver = new StateResolver();
            };

            _GraphVisualization.LoadGraphFunc = Load;
            _GraphVisualization.GraphSaveFunc = Save;
        }
        public static XElement Save(DataStructures.Graph g, Action<DataContractSerializerSettings> DataContractSerializerSettingsActionInvokrer = null)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(ms))
                {
                    DataContractSerializerSettings dataContractSerializerSettings = GetDataContractSerializerSettings();
                    DataContractSerializerSettingsActionInvokrer?.Invoke(dataContractSerializerSettings);
                    DataContractSerializer serializer = new DataContractSerializer(g.GetType(), dataContractSerializerSettings);
                    serializer.WriteObject(writer, g);
                    writer.Flush();
                    ms.Position = 0;
                    return XElement.Load(ms);
                }
            }
        }
        public static DataStructures.Graph Load(XElement e, Action<DataContractSerializerSettings> DataContractSerializerSettingsActionInvokrer = null)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            DataStructures.Graph g = new DataStructures.Graph();
            //load graph
            using (MemoryStream memoryStream = new MemoryStream())
            {
                e.Save(memoryStream);
                memoryStream.Position = 0;
                DataContractSerializerSettings dataContractSerializerSettings = GetDataContractSerializerSettings();
                DataContractSerializerSettingsActionInvokrer?.Invoke(dataContractSerializerSettings);
                DataContractSerializer ndcs = new DataContractSerializer(g.GetType(), dataContractSerializerSettings);
                DataStructures.Graph u = ndcs.ReadObject(memoryStream) as DataStructures.Graph;

                g.Directed = u.Directed;
                foreach (IVertex v in u.Vertices)
                {
                    g.Vertices.Add(v);
                }
                g.Start = u.Start;
                return g;
            }
        }
        public static DataContractSerializerSettings GetDataContractSerializerSettings()
        {
            return GetDataContractSerializerSettings(new List<Type>());
        }
        public static DataContractSerializerSettings GetDataContractSerializerSettings(List<Type> knownTypes, DataContractResolver dataContractResolver = null)
        {
            List<Type> types = new List<Type>() { typeof(Vertex<object>), typeof(Edge<object>) };
            if (knownTypes != null)
            {
                types.AddRange(knownTypes);
            }
            var dataContractSerializerSettings = new DataContractSerializerSettings();
            dataContractSerializerSettings.PreserveObjectReferences = true;
            dataContractSerializerSettings.KnownTypes = types;
            dataContractSerializerSettings.DataContractResolver = dataContractResolver;
            return dataContractSerializerSettings;
        }
    }
}
