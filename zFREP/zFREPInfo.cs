﻿using System;
using System.Drawing;
using zFREP.Properties;
using Grasshopper.Kernel;

namespace zFREP
{
    public class zFREPInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "zFREP";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Resources.zFREP;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Function Representation Toolkit for 3d Printing.";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("32e793e0-f67c-44fe-95f5-55ad85ceecc1");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "ZHCODE";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
