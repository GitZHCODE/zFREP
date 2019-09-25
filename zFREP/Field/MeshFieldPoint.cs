using System;
using System.Collections.Generic;
using zFREP.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Linq;
using SpatialSlur;
using SpatialSlur.Collections;
using SpatialSlur.Fields;
using SpatialSlur.Rhino;

namespace zFREP
{
    public class MeshFieldPoint: GH_Component
    {
        

        /// <summary>
        /// Initializes a new instance of the MeshFieldComponent class.
        /// </summary>
        public MeshFieldPoint()
          : base("Mesh Field Point", "MeshField Point",
              "Create a Point Mesh Field.",
              "zFREP", "Field")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Base Mesh", "Mesh", "Base Mesh", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Point Input", "Input", "Input Point", GH_ParamAccess.list);
            pManager.AddNumberParameter("Iso Value", "Iso Value", "Zero Contour Value", GH_ParamAccess.item);
            
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            
            pManager.AddGenericParameter("Mesh Field", "Field", "Mesh Field Result", GH_ParamAccess.item);
            
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {        
            Mesh mesh = null;
            List<Point3d> points = new List<Point3d>();
            double iso = 0d;

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataList<Point3d>(1, points)) return;
            if (!DA.GetData(2, ref iso)) return;

            var hem = mesh.ToHeMesh();
            var field = MeshField3d.Double.Create(hem);

            var vecs = points.Select(p => (SpatialSlur.Vector3d)p).ToList();

            var kdTree = KdTree.CreateBalanced(vecs.Select(v => (v.XY).ToArray()));
            var nearest = hem.Vertices.Select(p => kdTree.NearestL2(new double[] { p.Position.X, p.Position.Y })).ToList();

            List<double> val = new List<double>();

            for (int i = 0; i < hem.Vertices.Count; i++)
            {
                double d = hem.Vertices[i].Position.DistanceTo(points[nearest[i]]);
                val.Add(d);
            }

            Interval interval = GetInterval(val);

            for (int i = 0; i < val.Count; i++)
                val[i] = (SlurMath.Remap(val[i], interval.T0, interval.T1, -1, 1)) - iso;

            field.Set(val);

            DA.SetData(0, Field3d.Create(v => field.ValueAt(v)));
        }

        private Interval GetInterval(List<double> val)
        {
            List<double> tempval = new List<double>(val);
            tempval.Sort();
            var interval = new Interval(tempval[0], tempval[tempval.Count - 1]);
            return interval;
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
                //return Resources.IconForThisComponent;
                return Resources.MField;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8ad2d2c0-54c1-44a4-b700-88a2b74d2b23"); }
        }
    }
}