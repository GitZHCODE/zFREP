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
    public class MeshFieldCurve: GH_Component
    {
        

        /// <summary>
        /// Initializes a new instance of the MeshFieldComponent class.
        /// </summary>
        public MeshFieldCurve()
          : base("Mesh Field Curve", "Mesh Field Curve",
              "Create a Curve Mesh Field",
              "zFREP", "Field")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Base Mesh", "Mesh", "Base Mesh", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Curve Input", "Input", "Input Curve", GH_ParamAccess.list);
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
            List<Curve> curves = new List<Curve>();
            double iso = 0d;

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataList<Curve>(1, curves)) return;
            if (!DA.GetData(2, ref iso)) return;

            var hem = mesh.ToHeMesh();
            var field = MeshField3d.Double.Create(hem);
         
            List<Polyline> polys = new List<Polyline>();
            for (int i = 0; i < curves.Count; i++)
            {
                Polyline poly;
                if (!curves[i].TryGetPolyline(out poly))
                    throw new ArgumentException();
                else polys.Add(poly);
            }

            List<Mesh> poly_meshes = new List<Mesh>();
            for (int i = 0; i < polys.Count; i++)
                poly_meshes.Add(RhinoFactory.Mesh.CreateExtrusion(polys[i], new Rhino.Geometry.Vector3d(0, 0, 0)));

            List<double> val = new List<double>();

            foreach (Point3d v in mesh.Vertices)
            {
                int idClosestMesh = -1;
                double least = Math.Pow(10, 10);
                for (int i = 0; i < poly_meshes.Count; i++)
                {
                    Point3d closestPt = poly_meshes[i].ClosestPoint(v);

                    double dist = v.DistanceTo(closestPt);

                    if (dist < least)
                    {
                        idClosestMesh = i;
                        least = dist;
                    }
                }

                Point3d point = poly_meshes[idClosestMesh].ClosestPoint(v);
                double d = v.DistanceTo(point);
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
                //return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9852d8d8-520b-4e1a-a84e-4a5ae8cae88d"); }
        }
    }
}