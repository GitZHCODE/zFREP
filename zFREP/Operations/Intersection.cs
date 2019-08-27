using System;
using System.Collections.Generic;
using zFREP.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

using SpatialSlur.Fields;

namespace zFREP
{
    public class Intersection: GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the IntersectionComponent class.
        /// </summary>
        public Intersection()
          : base("Field Intersection", "Field Intersection",
              "Perform the boolean intersection of two fields.",
              "zFREP", "Booleans")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Field 0", "Field 0", "First Field", GH_ParamAccess.item);
            pManager.AddGenericParameter("Field 1", "Field 1", "Second Field", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Field Result", "Field", "Intersection Field", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IField3d<double> f0 = null;
            IField3d<double> f1 = null;

            if (!DA.GetData(0, ref f0)) return;
            if (!DA.GetData(1, ref f1)) return;


            DA.SetData(0, Field3d.CreateIntersection(f0, f1));
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
                return Resources.intersect;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("11797900-2118-4f76-8879-2213f83255e2"); }
        }
    }
}