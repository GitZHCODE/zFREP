using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using zFREP.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

using SpatialSlur.Fields;
using SpatialSlur.Rhino;

namespace zFREP
{
    public class PlaneTrimMultiThreaded: GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PlaneTrimComponent class.
        /// </summary>
        public PlaneTrimMultiThreaded()
          : base("Plane Trim Multithreaded", "Plane Trim Multithreaded",
              "Trim field with plane.",
              "zFREP", "Booleans")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Base Mesh", "Mesh", "Base Mesh.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Field", "Field", "Field to be trimmed.", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Trim Plane", "Plane", "Trim Plane.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Trim Value", "Trim Value", "Optional value set for all points in trim.", GH_ParamAccess.item, 1.00d);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Trimmed Field", "Field", "Trimmed Field.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            IField3d<double> f = null;
            Plane plane = new Plane();
            double val = 1.00d;

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetData(1, ref f)) return;
            if (!DA.GetData(2, ref plane)) return;
            if (!DA.GetData(3, ref val)) return;

            var hem = mesh.ToHeMesh();
            var field = MeshField3d.Double.Create(hem);

            double[] vals = new double[field.Mesh.Vertices.Count];

            Parallel.ForEach(Partitioner.Create(0, mesh.Vertices.Count), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    Vector3d vec = plane.Origin - mesh.Vertices[i];
                    double proj = vec * plane.Normal; 

                    if (proj > 0) vals[i] = val;

                    else vals[i] = f.ValueAt(mesh.Vertices[i]);
                }
            });

            field.Set(vals);

            DA.SetData(0, field);

        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Resources.PTrim;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2aac0dc6-9de9-41e2-a6d7-853fb5ee550c"); }
        }
    }
}