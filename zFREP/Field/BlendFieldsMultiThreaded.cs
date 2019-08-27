using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using zFREP.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using SpatialSlur;
using SpatialSlur.Fields;
using SpatialSlur.Rhino;

namespace zFREP
{
    public class BlendFieldsMultiThreaded: GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BlendFieldsComponent class.
        /// </summary>
        public BlendFieldsMultiThreaded()
          : base("Blend Fields Multi-threaded", "Blend Fields",
              "Blend fields at specified parameters t",
              "zFREP", "Field")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Base Mesh", "Mesh", "Base Mesh.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Field 0", "Field 0", "First Field.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Field 1", "Field 1", "Second Field.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Blend Parameters", "t", "Parameters at which blend is sampled", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Blend Fields", "Field", "Blend Fields.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> mesh = new List<Mesh>();
            IField3d<double> f0 = null;
            IField3d<double> f1 = null;
            List<double> t = new List<double>();

            if (!DA.GetDataList(0, mesh)) return;
            if (!DA.GetData(1, ref f0)) return;
            if (!DA.GetData(2, ref f1)) return;
            if (!DA.GetDataList(3, t)) return;

            List<MeshField3d> fields = new List<MeshField3d>();

            for (int i = 0; i < t.Count; i++)
            {
                var currentField = MeshField3d.Double.Create(mesh[i].ToHeMesh());
                double[] currentBlend = new double[currentField.Mesh.Vertices.Count];

                Parallel.For(0, currentField.Mesh.Vertices.Count, j =>
                {
                    currentBlend[j] = SlurMath.Lerp(f0.ValueAt(currentField.Mesh.Vertices[j].Position),
                        f1.ValueAt(currentField.Mesh.Vertices[j].Position), t[i]);
                });

                currentField.Set(currentBlend);
                fields.Add(currentField);
            }

            DA.SetDataList(0, fields);

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
                // return Resources.IconForThisComponent;
                return Resources.blendFields;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6aba6ae9-faf2-47fc-9869-d34edf2f214d"); }
        }
    }
}

