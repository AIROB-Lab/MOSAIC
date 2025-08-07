using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using GraphX.Common.Enums;
using GraphX.Common.Models;
using GraphX.Controls;
using GraphX.Logic.Algorithms.LayoutAlgorithms;

using QuickGraph;

namespace MosaicLibary
{
    // a BlockTable is a Simulink-style GUI to the block structure,
    // created out of a dictionary of Blocks, enforcing basic interaction capabilities.
    public class BlockTable
    {
        public BlockTableArea ImArea { get; private set; }
        static BlockTable_window BlockTableView = null;

        public BlockTable()
        {
            BlockTableView = new BlockTable_window(GenerateGraphData());

            BlockTableView.Show();
            BlockTableView.Activate();
        }

        GraphData GenerateGraphData()
        {
            GraphData _tmpGraphData = new GraphData();

            // for each block, add a vertex
            int vertexID = 0;
            foreach (Block b in Blocks.Instance.Values) 
                _tmpGraphData.AddVertex(new Vertex(b) { ID = vertexID++ });

            // for each block, add edges from input-vertices to this one
            int edgeID = 0;
            foreach (Block b in Blocks.Instance.Values)
            {
                Vertex thisVertex = _tmpGraphData.Vertices.Where(v => v.block == b).First();
                foreach (Block inp in Blocks.Instance.Values.Where(bb => b.HasInput(bb)))
                {
                    Vertex parentVertex = _tmpGraphData.Vertices.Where(v => v.block == inp).First();
                    // edges are labelled with the output type and desired rate of the source block
                    _tmpGraphData.AddEdge(new Edge(parentVertex, thisVertex) { ID = edgeID++, Weight = 0.1 });
                }
            }
            return _tmpGraphData;
        }
    }

    public class Vertex : VertexBase
    {
        public Block block { get; private set; }
        public string text { get; set; }
        public override string ToString() { return text; }

        // a vertex takes its characteristics from a block
        public Vertex(Block b) { block = b; text = (b.DesiredRate == 0 ? b.Name : b.Name + "@" + b.DesiredRate); }
        // default parameterless constructor for this class (required for YAXLib serialization)
        public Vertex() : this(null) { }
    }

    public class Edge : EdgeBase<Vertex>
    {
        string _text;
        public string Text { get { return _text; } set { _text = value; OnPropertyChanged("Text"); } }
        public override string ToString() { return Text; }

        // Default parameterless constructor for this class (required for YAXLib serialization)
        public Edge() : base(null, null, 1) { }
        public Edge(Vertex source, Vertex target, double weight = 1) : base(source, target, weight) { }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name) { if (PropertyChanged != null) PropertyChanged.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name)); }
    }

    // GraphArea is the visual panel component responsible for drawing visuals (vertices and edges).
    public class BlockTableArea : GraphArea<Vertex, Edge, BidirectionalGraph<Vertex, Edge>> { }

    // GraphData stores vertices and edges data that is used by GraphArea and end-user for a variety of operations.
    // Data graph content handled manually by user (add/remove objects). The main idea is that you can dynamicaly
    // remove/add objects into the GraphArea layout and then use data graph to restore original layout content.
    public class GraphData : BidirectionalGraph<Vertex, Edge> { }

    // Logics core object which contains all algorithms and logic settings
    public class GXLogicCore : GraphX.Logic.Models.GXLogicCore<Vertex, Edge, BidirectionalGraph<Vertex, Edge>> { }

    public partial class BlockTable_window : Window
    {
        GraphData currentGraphData = null;
        Point clickedElementPreviousPosition;

        public BlockTable_window(GraphData initialGraphData)
        {
            currentGraphData = initialGraphData;

            InitializeComponent();

            // customize Zoombox a bit - set the minimap (overview) window to be hidden by default
            ZoomControl.SetViewFinderVisibility(zoomctrl, Visibility.Hidden);
            // setup GraphArea settings
            GraphAreaSetup();

            // callback: the window gets laoded
            Loaded += BlockTable_window_Loaded;

            // callback: block table refresh
            Utils.dtmRefresh.Tick += BlockTable_refresh;
        }

        private void BlockTable_window_Loaded(object sender, RoutedEventArgs e)
        {
            // Generates graph with data loaded during setup
            // if edges are to be created later, first argument is false, then call Area.GenerateAllEdges()
            Area.GenerateGraph(false, true);
            Area.GenerateAllEdges();

            // create callbacks for vertices
            foreach (var vc in Area.VertexList.Values)
            {
                vc.MouseLeftButtonDown += (vc_, args) => { clickedElementPreviousPosition = (vc_ as VertexControl).GetPosition(); };
                vc.MouseLeftButtonUp += LeftClickUpOnBlockHandler;
                vc.MouseRightButtonUp += RightClickUpOnBlockHandler;
            }

            Area.SetEdgesDashStyle(EdgeDashStyle.Solid);
            Area.ShowAllEdgesArrows(true);
            Area.ShowAllEdgesLabels(true);
            Area.SetVerticesDrag(true, true);

            zoomctrl.ZoomToFill();
        }

        // this function prettyprints stuff on the blocktable. it gets called at every tick of the global timer.
        private void BlockTable_refresh(object sender, EventArgs e)
        {
            // set a vertex's bg colour according to block's status: idle(grey), running(green), lagging(red), stumbling(magenta)
            foreach (var v in Area.VertexList.Keys)
                Area.VertexList[v].Background = new SolidColorBrush(
                    v.block.IsIdle ? Color.FromRgb(0xCC, 0xCC, 0xCC) :
                       v.block.IsLagging ? Color.FromRgb(0xFF, 0x77, 0x77) :
                          v.block.IsStumbling ? Color.FromRgb(0xFF, 0x77, 0xFF) :
                            Color.FromRgb(0x77, 0xFF, 0x77)
                );
            // decorate edge labels: if A is subscribed to B, write output type, rate and idle/lagging/stumbling; otherwise a cross.
            foreach (var ed in Area.EdgesList.Keys)
                ed.Text =
                    ed.Target.block.IsSubscribedTo(ed.Source.block) ?
                    $"{ed.Source.block.Rate:0}" +
                        (ed.Source.block.IsIdle ? "Y" : "n") +
                        (ed.Source.block.IsLagging ? "Y" : "n") +
                        (ed.Source.block.IsStumbling ? "Y" : "n") :
                    "x";
            // regenerate edges
            Area.GenerateAllEdges();
        }

        // called upon left mouse click on a vertex
        private void LeftClickUpOnBlockHandler(object sender, RoutedEventArgs e)
        {
            VertexControl vControl = sender as VertexControl;
            Vertex v = vControl.DataContext as Vertex;

            // if mouse button released after drag operation, just update its previous position
            if (!vControl.GetPosition().Equals(clickedElementPreviousPosition)) { clickedElementPreviousPosition = vControl.GetPosition(); }
            // otherwise, show/hide the vertex's block
            else v.block.MouseLeftClick();
        }
        // called upon right mouse click on a vertex
        private void RightClickUpOnBlockHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            VertexControl vControl = sender as VertexControl;
            Vertex v = vControl.DataContext as Vertex;
            v.block.MouseRightClick();
        }

        public void Dispose()
        {
            // if you plan to dynamically create and destroy GraphArea it is wise to use Dispose() method
            // that ensures that all potential memory-holding objects will be released.
            Area.Dispose();
        }

        private void GraphAreaSetup()
        {
            // instantiate logic core
            var logicCore = new GXLogicCore() { Graph = currentGraphData };

            // choose a layout algorithm
            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.EfficientSugiyama;

            // some experimental sets of parameters for different layout algorithms - 
            // you need to specify a different parameter type for every algorithm, in order to change the parameters
            switch (logicCore.DefaultLayoutAlgorithm)
            {
                case LayoutAlgorithmTypeEnum.KK:
                    logicCore.DefaultLayoutAlgorithmParams = logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.KK);
                    ((KKLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).AdjustForGravity = true;
                    ((KKLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).LengthFactor = 0.3;
                    break;
                case LayoutAlgorithmTypeEnum.Sugiyama:
                    logicCore.DefaultLayoutAlgorithmParams = logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.Sugiyama);
                    ((SugiyamaLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).VerticalGap = 50;
                    break;
                case LayoutAlgorithmTypeEnum.EfficientSugiyama:
                    logicCore.DefaultLayoutAlgorithmParams = logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.EfficientSugiyama);
                    ((EfficientSugiyamaLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).EdgeRouting = SugiyamaEdgeRoutings.Traditional;
                    ((EfficientSugiyamaLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).MinimizeEdgeLength = true;
                    ((EfficientSugiyamaLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).VertexDistance = 50;
                    ((EfficientSugiyamaLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).LayerDistance = 60;
                    ((EfficientSugiyamaLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).WidthPerHeight = 1;
                    ((EfficientSugiyamaLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).OptimizeWidth = false;
                    break;
            }

            // vertex overlap removal algorithm
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;

            // enable parallel and curved edges (useful if you have cycles)
            logicCore.EdgeCurvingEnabled = true;
            logicCore.EnableParallelEdges = true;

            // default parameters are created automaticaly when new default algorithm is set and previous params were NULL
            logicCore.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 50;
            logicCore.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 50;

            // edge routing algorithm that is used to build route paths
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
            //((PathFinderEdgeRoutingParameters)logicCore.DefaultEdgeRoutingAlgorithmParams).UseDiagonals = true;
            //((PathFinderEdgeRoutingParameters)logicCore.DefaultEdgeRoutingAlgorithmParams).UseTieBreaker = false;
            //((PathFinderEdgeRoutingParameters)logicCore.DefaultEdgeRoutingAlgorithmParams).PunishChangeDirection = true;
            //((PathFinderEdgeRoutingParameters)logicCore.DefaultEdgeRoutingAlgorithmParams).PathFinderAlgorithm = PathFindAlgorithm.Manhattan;

            // this property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            // will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            // Area.RelayoutFinished and Area.GenerateGraphFinished.
            logicCore.AsyncAlgorithmCompute = false;

            // lastly, assign logic core to GraphArea object
            Area.LogicCore = logicCore;
        }

        private void BlockTable_window_Closed(object sender, EventArgs e) { System.Windows.Forms.Application.Exit(); }
    }
}
