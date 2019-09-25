using System;
using System.Collections.Generic;
using zFREP.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

using SpatialSlur.Fields;
using SpatialSlur.Meshes;

namespace zFREP
{
    public class EdgeFrames: GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the component class.
        /// </summary>
        public EdgeFrames()
          : base("Edge Frames", "Edge Frames",
              "Compute a series of edge aligned frames",
              "zFREP", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Graph", "Graph", "Input Graph", GH_ParamAccess.item);
            pManager.AddGenericParameter("Layer Heigth", "Layer Heigth", "Layer Heigth Value", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frames Result", "Frames", "Frames Result", GH_ParamAccess.tree);
            pManager.AddGenericParameter("t Parameters Result", "t", "t Parameters Result", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            HeGraph3d graph = null;
            double layerHeigth = 0;

            if (!DA.GetData(0, ref graph)) return;
            if (!DA.GetData(1, ref layerHeigth)) return;

            var frames = new DataTree<Plane>();
            var tparams = new DataTree<double>();

            for (int i = 0; i < graph.Edges.Count; i++)
                frames.AddRange(getEdgeFrames(graph, i, layerHeigth), new GH_Path(i));

            for (int i = 0; i < graph.Edges.Count; i++)
            {
                var count = frames.Branch(i).Count;
                var t0 = 1.0 / (double)count;

                for (int j = 0; j < count; j++)
                {
                    tparams.Add((j * t0), new GH_Path(i));
                }
            }

            DA.SetDataTree(0, frames);
            DA.SetDataTree(1, tparams);
        }

        public List<Plane> getEdgeFrames(HeGraph3d _graph, int _edgeId, double _height)
        {
            var e0 = _graph.Edges[_edgeId];

            var v0 = e0.End.Position;
            var v1 = e0.Start.Position;

            var eVec = (v0 - v1).Unit;
            var dist = v0.DistanceTo(v1);

            var zUnit = new SpatialSlur.Vector3d(1, 0, 0);
            var n = SpatialSlur.Vector3d.Cross(eVec, zUnit).Unit;
            var bn = SpatialSlur.Vector3d.Cross(eVec, n).Unit;

            var frames = new List<Rhino.Geometry.Plane>();

            double num = e0.GetLength() / _height;

            for (int i = 0; i < num + 1; i++)
            {
                var pt = e0.Start.Position + eVec * (dist / num) * i;

                var frame = new Rhino.Geometry.Plane((Point3d)pt,
                  (Vector3d)n, (Vector3d)bn);

                frames.Add(frame);
            }

            return frames;
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Resources.isoContour;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("38FAE3F9-6F0E-465D-B723-E8D41BE2B613"); }
        }
    }
}