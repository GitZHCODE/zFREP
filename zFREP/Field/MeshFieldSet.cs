using System;
using System.Collections.Generic;
using zFREP.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using SpatialSlur;
using SpatialSlur.Fields;
using SpatialSlur.Rhino;

namespace zFREP
{
    public class MeshFieldSet: GH_Component
    {
        

        /// <summary>
        /// Initializes a new instance of the MeshFieldComponent class.
        /// </summary>
        public MeshFieldSet()
          : base("Mesh Field Set", "Mesh Field Set",
              "Create a Mesh Field from Values",
              "zFREP", "Field")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Base Mesh", "Mesh", "Base Mesh", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Values", "Input", "Input Values", GH_ParamAccess.list);
            pManager.AddNumberParameter("Iso Value", "Iso Value", "Zero Contour Value", GH_ParamAccess.item);           
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {          
            pManager.AddGenericParameter("Mesh Field", "Mesh Field", "Mesh Field Result", GH_ParamAccess.item);         
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {        
            Mesh mesh = null;
            List<double> values = new List<double>();
            double iso = 0d;

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataList<double>(1, values)) return;
            if (!DA.GetData(2, ref iso)) return;

            var hem = mesh.ToHeMesh();
            var field = MeshField3d.Double.Create(hem);            

            Interval interval = GetInterval(values);

            for (int i = 0; i < values.Count; i++)
                values[i] = (SlurMath.Remap(values[i], interval.T0, interval.T1, -1, 1)) - iso;

            field.Set(values);
            
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
            get { return new Guid("4db7be34-b215-4a6d-91dc-3d4b0f0e2e01"); }
        }
    }
}