using System;
using System.Collections.Generic;
using System.Drawing;
using zFREP.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

using SpatialSlur;
using SpatialSlur.Fields;
using SpatialSlur.Rhino;

namespace zFREP
{
    public class FieldDisplay : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FieldDisplayComponent class.
        /// </summary>
        public FieldDisplay()
          : base("Field Display", "Field Display",
              "Displays scalar field with specified color range.",
              "zFREP", "Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Base mesh.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Field", "Field", "Field", GH_ParamAccess.item);
            pManager.AddColourParameter("Colors", "Colors", "Color Range", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Field Mesh", "Field Mesh", "Mesh.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            IField3d<double> f = null;
            Mesh mesh = null;
            List<Color> colors = new List<Color>();

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetData(1, ref f)) return;
            if (!DA.GetDataList(2, colors)) return;

            mesh.ColorVertices(i => { return colors.Lerp(f.ValueAt(mesh.Vertices[i])); }, true);

            DA.SetData(0, mesh);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Resources.FDisplay;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f64f64f2-65f4-405b-ba3b-4a4ea82a2cec"); }
        }
    }
}