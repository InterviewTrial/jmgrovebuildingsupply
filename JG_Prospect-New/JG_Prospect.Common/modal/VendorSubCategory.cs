﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JG_Prospect.Common.modal
{
    public class VendorSubCategory
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _vendorcatid;
        public string VendorCatId
        {
            get { return _vendorcatid; }
            set { _vendorcatid = value; }
        }

        public bool IsRetail_Wholesale { get; set; }
        public bool IsManufacturer { get; set; }
    }
}
