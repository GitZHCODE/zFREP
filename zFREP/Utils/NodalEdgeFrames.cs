using System;
using System.Linq;
using System.Collections.Generic;
using zFREP.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

using SpatialSlur;
using SpatialSlur.Fields;
using SpatialSlur.Meshes;

using Vec3d = SpatialSlur.Vector3d;
using Vector3d = Rhino.Geometry.Vector3d;

namespace zFREP
{
    public class NodalEdgeFrames: GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the component class.
        /// </summary>
        public NodalEdgeFrames()
          : base("Nodal Edge Frames", "Nodal Edge Frames",
              "Compute a series of edge aligned frames starting from the nodes",
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
            pManager.AddGenericParameter("Length", "Length", "Length of layers sum", GH_ParamAccess.item);
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
            double length = 0;

            if (!DA.GetData(0, ref graph)) return;
            if (!DA.GetData(1, ref layerHeigth)) return;
            if (!DA.GetData(2, ref length)) return;

            var f = new DataTree<Plane>();
            var tparams = new DataTree<double>();

            List<GH_Path> branches = new List<GH_Path>();

            for (int i = 0; i < graph.Edges.Count; i++)
                branches.Add(new GH_Path(i));

            for (int i = 0; i < graph.Vertices.Count; i++)
            {
                for (int j = 0; j < graph.Vertices[i].ConnectedVertices.ToList().Count; j++)
                {
                    var v0 = graph.Vertices[i].Position;
                    var v1 = graph.Vertices[i].ConnectedVertices.ToList()[j].Position;

                    var eVec = (v0 - v1).Unit;
                    var dist = v0.DistanceTo(v1);

                    var zUnit = new Vec3d(1, 0, 0);
                    var n = Vec3d.Cross(eVec, zUnit).Unit;
                    var bn = Vec3d.Cross(eVec, n).Unit;
                    var bbn = Vec3d.Cross(eVec, bn).Unit;

                    var frames = new List<Rhino.Geometry.Plane>();
                    var edgeId = graph.Vertices[i].FindHalfedgeTo(graph.Vertices[i].ConnectedVertices.ToList()[j]).Index >> 1;

                    var branch = new GH_Path(edgeId);

                    int cnt = 0;
                    double l = 0;

                    do
                    {
                        Point3d pt = v0 - eVec * cnt * layerHeigth;
                        l += v0.DistanceTo((Vec3d)pt);

                        var frame = new Rhino.Geometry.Plane((Point3d)pt, (Vector3d) bn, (Vector3d) bbn);

                        frames.Add(frame);
                        cnt++;
                    }

                    while (length > l);

                    f.AddRange(frames, branch);
                }
            }

            for (int i = 0; i < graph.Edges.Count; i++)
            {
                var count = f.Branch(i).Count;
                var t0 = 1.0 / (double)count;

                for (int j = 0; j < count; j++)
                    tparams.Add((j * t0), new GH_Path(i));
            }

            DA.SetDataTree(0, f);
            DA.SetDataTree(1, tparams);
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
            get { return new Guid("4D7A62CE-86E9-44BE-9A20-BBAEC3063C1C"); }
        }
    }
}