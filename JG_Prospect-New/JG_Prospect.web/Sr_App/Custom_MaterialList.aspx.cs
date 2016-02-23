﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using JG_Prospect.Common;
using JG_Prospect.Common.modal;
using JG_Prospect.BLL;

using System.Net.Mail;
using System.Text;
using System.Web.Services;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using System.Configuration;
using System.Net.Configuration;
using System.Net;
using JG_Prospect.Common.Logger;
//using System.Diagnostics;

namespace JG_Prospect.Sr_App
{
    public partial class Custom_MaterialList : System.Web.UI.Page
    {
        string jobId = string.Empty;
        private Boolean IsPageRefresh = false;
        int estimateId = 0, customerId = 0, productTypeId = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            jobId = Session[SessionKey.Key.JobId.ToString()].ToString();
            //if (Request.QueryString[QueryStringKey.Key.ProductId.ToString()] != null)
            //{
            //    estimateId = Convert.ToInt16(Request.QueryString[QueryStringKey.Key.ProductId.ToString()]);
            //}
            //if (Request.QueryString[QueryStringKey.Key.CustomerId.ToString()] != null)
            //{
            //    customerId = Convert.ToInt16(Request.QueryString[QueryStringKey.Key.CustomerId.ToString()]);
            //}
            //if (Request.QueryString[QueryStringKey.Key.ProductTypeId.ToString()] != null)
            //{
            //    productTypeId = Convert.ToInt16(Request.QueryString[QueryStringKey.Key.ProductTypeId.ToString()]);
            //}

            setPermissions();
            if (!IsPostBack)
            {
                bindMaterialList();
                SetButtonText();
                bind();
                lnkVendorCategory.ForeColor = System.Drawing.Color.DarkGray;
                lnkVendorCategory.Enabled = false;
                lnkVendor.Enabled = true;
                lnkVendor.ForeColor = System.Drawing.Color.Blue;
                //if (Request.QueryString[QueryStringKey.Key.ProductTypeId.ToString()] != null)
                //{
                //   // productTypeId = Convert.ToInt16(Request.QueryString[QueryStringKey.Key.ProductTypeId.ToString()]);
                //    string prodName = UserBLL.Instance.GetProductNameByProductId(productTypeId);
                //    h1Heading.InnerText = "Material List - " + prodName;
                //}
            }
            else
            {
                IsPageRefresh = true;
            }
           
        }

        DataSet DS = new DataSet();

        public void SetButtonText()
        {
            string EmailStatus = CustomBLL.Instance.GetEmailStatusOfCustomMaterialList(jobId);//, productTypeId, estimateId);
            int result = CustomBLL.Instance.WhetherCustomMaterialListExists(jobId);//, productTypeId, estimateId);
            if (result == 0) //if list doesn't exists
            {
                btnSendMail.Text = "Save";
                showVendorCategoriesPermissions();
            }
            else  //if list exists
            {
                if (EmailStatus == JGConstant.EMAIL_STATUS_NONE || EmailStatus == string.Empty)       //if no email was sent
                {
                    int permissionStatusCategories = CustomBLL.Instance.CheckPermissionsForCategories(jobId);//, productTypeId, estimateId);
                    if (permissionStatusCategories == 0)        //if no permissions were granted for categories
                    {
                        btnSendMail.Text = "Save";
                    }
                    else                //if permissions were granted for categories
                    {
                        btnSendMail.Text = "Send Mail To Vendor Category(s)";
                        grdcustom_material_list.Columns[6].Visible = false;
                    }
                    showVendorCategoriesPermissions();
                }

                else if (EmailStatus == JGConstant.EMAIL_STATUS_VENDOR)    //if both mails are sent
                {
                    setControlsAfterSendingBothMails();
                    showVendorPermissions();
                }
                else        //if mails were sent to categories
                {
                    int permissionStatus = CustomBLL.Instance.CheckPermissionsForVendors(jobId);//, productTypeId, estimateId);
                    if (permissionStatus == 0)  //if permissions were not granted for vendors
                    {
                        btnSendMail.Text = "Save";
                        showVendorPermissions();
                        EnableVendorNameAndAmount();
                        grdcustom_material_list.Columns[6].Visible = true;
                    }
                    else         //if permissions were granted for vendors
                    {
                        btnSendMail.Text = "Send Mail To Vendor(s)";
                        setControlsForVendorsAfterSave();
                        showVendorPermissions();
                        EnableVendorNameAndAmount();
                    }
                }
            }
        }
        private void EnableVendorNameAndAmount()
        {
            foreach (GridViewRow r in grdcustom_material_list.Rows)
            {
                DropDownList ddlVendorName = (DropDownList)r.FindControl("ddlVendorName");
                ddlVendorName.Enabled = true;
                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
                txtAmount.Enabled = true;
            }
        }

        private void DisableVendorNameAndAmount()
        {
            foreach (GridViewRow r in grdcustom_material_list.Rows)
            {
                DropDownList ddlVendorName = (DropDownList)r.FindControl("ddlVendorName");
                ddlVendorName.Enabled = false;
                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
                txtAmount.Enabled = false;
            }
        }
        public void setPermissions()
        {
            DataSet ds = CustomBLL.Instance.GetAllPermissionOfCustomMaterialList(jobId);//, productTypeId, estimateId);
            if (ds.Tables[0].Rows.Count > 0)
            {
                if (lnkForemanPermission.Visible == true)
                {
                    if (Convert.ToChar(ds.Tables[0].Rows[0]["IsForemanPermission"].ToString().Trim()) == JGConstant.PERMISSION_STATUS_GRANTED)
                    {
                        lnkForemanPermission.Enabled = false;
                        lnkForemanPermission.ForeColor = System.Drawing.Color.DarkGray;
                        popupForeman_permission.TargetControlID = "hdnForeman";
                    }
                    if (Convert.ToChar(ds.Tables[0].Rows[0]["IsSrSalemanPermissionF"].ToString().Trim()) == JGConstant.PERMISSION_STATUS_GRANTED)
                    {
                        lnkSrSalesmanPermissionF.Enabled = false;
                        lnkSrSalesmanPermissionF.ForeColor = System.Drawing.Color.DarkGray;
                        popupSrSalesmanPermissionF.TargetControlID = "hdnSrF";
                    }
                }
                if (lnkAdminPermission.Visible == true)
                {
                    if (Convert.ToChar(ds.Tables[0].Rows[0]["IsAdminPermission"].ToString().Trim()) == JGConstant.PERMISSION_STATUS_GRANTED)
                    {
                        lnkAdminPermission.Enabled = false;
                        lnkAdminPermission.ForeColor = System.Drawing.Color.DarkGray;
                        popupAdmin_permission.TargetControlID = "hdnAdmin";
                    }
                    if (Convert.ToChar(ds.Tables[0].Rows[0]["IsSrSalemanPermissionA"].ToString().Trim()) == JGConstant.PERMISSION_STATUS_GRANTED)
                    {
                        lnkSrSalesmanPermissionA.Enabled = false;
                        lnkSrSalesmanPermissionA.ForeColor = System.Drawing.Color.DarkGray;
                        popupSrSalesmanPermissionA.TargetControlID = "hdnSrA";
                    }
                }
            }
        }
        public void showVendorCategoriesPermissions()
        {
            lnkForemanPermission.Visible = true;
            lnkSrSalesmanPermissionF.Visible = true;
            lnkAdminPermission.Visible = false;
            lnkSrSalesmanPermissionA.Visible = false;
            setPermissions();
        }
        public void showVendorPermissions()
        {
            lnkForemanPermission.Visible = false;
            lnkSrSalesmanPermissionF.Visible = false;
            lnkAdminPermission.Visible = true;
            lnkSrSalesmanPermissionA.Visible = true;

            setPermissions();
        }

        protected void setControlsForVendorCategoriesAfterSave()
        {
            foreach (GridViewRow gr in grdcustom_material_list.Rows)
            {
                TextBox txtMateriallist = (TextBox)gr.FindControl("txtMateriallist");
                txtMateriallist.Enabled = false;

                TextBox txtAmount = (TextBox)gr.FindControl("txtAmount");
                txtAmount.Enabled = false;
                DropDownList ddlVendorCategory = (DropDownList)gr.FindControl("ddlVendorCategory");
                ddlVendorCategory.Enabled = false;

                DropDownList ddlVendorName = (DropDownList)gr.FindControl("ddlVendorName");
                ddlVendorName.Enabled = false;
            }
        }

        protected void setControlsForVendorsAfterSave()
        {
            foreach (GridViewRow gr in grdcustom_material_list.Rows)
            {
                TextBox txtMateriallist = (TextBox)gr.FindControl("txtMateriallist");
                txtMateriallist.Enabled = false;

                TextBox txtAmount = (TextBox)gr.FindControl("txtAmount");
                txtAmount.Enabled = false;
                DropDownList ddlVendorCategory = (DropDownList)gr.FindControl("ddlVendorCategory");
                ddlVendorCategory.Enabled = false;
                int selectedCategoryID = Convert.ToInt16(ddlVendorCategory.SelectedItem.Value);

                DropDownList ddlVendorName = (DropDownList)gr.FindControl("ddlVendorName");
                ddlVendorName.Enabled = false;
            }
            grdcustom_material_list.Columns[6].Visible = false;
        }
        [WebMethod]
        public static string Exists(string value)
        {
            if (value == AdminBLL.Instance.GetAdminCode())
            {
                return "True";
            }
            else
            {
                return "false";
            }
        }

        public DataSet GetVendorCategories()
        {
            DataSet ds = new DataSet();
            ds = VendorBLL.Instance.fetchAllVendorCategoryHavingVendors();
            return ds;
        }

        public DataSet GetVendorNames(int vendorcategoryId)
        {
            DataSet ds = new DataSet();
            ds = VendorBLL.Instance.fetchVendorNamesByVendorCategory(vendorcategoryId);
            return ds;
        }

        private void bindMaterialList()
        {
            DataSet ds = CustomBLL.Instance.GetCustom_MaterialList(jobId.ToString());//,productTypeId,estimateId);
            List<CustomMaterialList> cmList = new List<CustomMaterialList>();
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                {
                    DataRow dr = ds.Tables[0].Rows[j];
                    CustomMaterialList cm = new CustomMaterialList();
                    cm.Id = Convert.ToInt16(dr["Id"]);
                    cm.MaterialList = dr["MaterialList"].ToString();
                    cm.VendorCategoryId = Convert.ToInt16(dr["VendorCategoryId"]);
                    cm.VendorCategoryName = dr["VendorCategoryNm"].ToString();
                    if (dr["VendorId"].ToString() != "")
                        cm.VendorId = Convert.ToInt16(dr["VendorId"]);
                    cm.VendorName = dr["VendorName"].ToString();
                    if (dr["Amount"].ToString() != "")
                        cm.Amount = Convert.ToDecimal(dr["Amount"]);
                    cm.DocName = dr["DocName"].ToString();
                    cm.TempName = dr["TempName"].ToString();
                    cm.IsForemanPermission = dr["IsForemanPermission"].ToString();
                    cm.IsSrSalemanPermissionF = dr["IsSrSalemanPermissionF"].ToString();
                    cm.IsAdminPermission = dr["IsAdminPermission"].ToString();
                    cm.IsSrSalemanPermissionA = dr["IsSrSalemanPermissionA"].ToString();
                    cm.Status = JGConstant.CustomMaterialListStatus.Unchanged;
                    cmList.Add(cm);
                }

                ViewState["CustomMaterialList"] = cmList;

                BindCustomMaterialList(cmList);
            }
            else
            {
                //List<CustomMaterialList> cmList1 = new List<CustomMaterialList>();

                //CustomMaterialList cm1 = new CustomMaterialList();
                //cm1.Id = 0;
                //cm1.MaterialList = "";
                //cm1.VendorCategoryId = 0;
                //cm1.VendorCategoryName = "";
                //cm1.VendorId = 0;
                //cm1.VendorName = "";
                //cm1.Amount = 0;
                //cm1.DocName = "";
                //cm1.TempName = "";
                //cm1.IsForemanPermission = "";
                //cm1.IsSrSalemanPermissionF = "";
                //cm1.IsAdminPermission = "";
                //cm1.IsSrSalemanPermissionA = "";
                //cm1.Status = JGConstant.CustomMaterialListStatus.Unchanged;
                //cmList1.Add(cm1);
                 List<CustomMaterialList> cmList1 = BindEmptyRowToMaterialList();

                ViewState["CustomMaterialList"] = cmList1;
                BindCustomMaterialList(cmList1);
            }

        }

        private List<CustomMaterialList> BindEmptyRowToMaterialList()
        {
            List<CustomMaterialList> cmList1 = new List<CustomMaterialList>();
            cmList1 = GetMaterialListFromGrid();
            CustomMaterialList cm1 = new CustomMaterialList();
            cm1.Id = 0;
            cm1.MaterialList = "";
            cm1.VendorCategoryId = 0;
            cm1.VendorCategoryName = "";
            cm1.VendorId = 0;
            cm1.VendorName = "";
            cm1.Amount = 0;
            cm1.DocName = "";
            cm1.TempName = "";
            cm1.IsForemanPermission = "";
            cm1.IsSrSalemanPermissionF = "";
            cm1.IsAdminPermission = "";
            cm1.IsSrSalemanPermissionA = "";
            cm1.Status = JGConstant.CustomMaterialListStatus.Unchanged;
            cmList1.Add(cm1);

            return cmList1;
        }
        private List<CustomMaterialList> GetMaterialListFromGrid()
        {
            List<CustomMaterialList> itemList = new List<CustomMaterialList>();

            for (int i = 0; i < grdcustom_material_list.Rows.Count; i++)
            {
                CustomMaterialList cm = new CustomMaterialList();
                HiddenField hdnEmailStatus = (HiddenField)grdcustom_material_list.Rows[i].FindControl("hdnEmailStatus");
                if (hdnEmailStatus.Value.ToString() != "")
                    cm.EmailStatus = hdnEmailStatus.Value;

                HiddenField hdnForemanPermission = (HiddenField)grdcustom_material_list.Rows[i].FindControl("hdnForemanPermission");
                if (hdnForemanPermission.Value.ToString() != "")
                    cm.IsForemanPermission = hdnForemanPermission.Value;

                HiddenField hdnSrSalesmanPermissionF = (HiddenField)grdcustom_material_list.Rows[i].FindControl("hdnSrSalesmanPermissionF");
                if (hdnSrSalesmanPermissionF.Value.ToString() != "")
                    cm.IsSrSalemanPermissionF = hdnSrSalesmanPermissionF.Value;

                HiddenField hdnAdminPermission = (HiddenField)grdcustom_material_list.Rows[i].FindControl("hdnAdminPermission");
                if (hdnAdminPermission.Value.ToString() != "")
                    cm.IsAdminPermission = hdnAdminPermission.Value;

                HiddenField hdnSrSalesmanPermissionA = (HiddenField)grdcustom_material_list.Rows[i].FindControl("hdnSrSalesmanPermissionA");
                if (hdnSrSalesmanPermissionA.Value.ToString() != "")
                    cm.IsSrSalemanPermissionA = hdnSrSalesmanPermissionA.Value;

                HiddenField hdnMaterialListId = (HiddenField)grdcustom_material_list.Rows[i].FindControl("hdnMaterialListId");
                if (hdnMaterialListId.Value.ToString() != "")
                    cm.Id = Convert.ToInt16(hdnMaterialListId.Value);
                TextBox txtMateriallist = (TextBox)grdcustom_material_list.Rows[i].FindControl("txtMateriallist");
                cm.MaterialList = txtMateriallist.Text;

                DropDownList ddlVendorCategory = (DropDownList)grdcustom_material_list.Rows[i].FindControl("ddlVendorCategory");
                if (ddlVendorCategory.SelectedIndex != -1)
                {
                    cm.VendorCategoryId = Convert.ToInt16(ddlVendorCategory.SelectedValue);
                    cm.VendorCategoryName = ddlVendorCategory.SelectedItem.Text;
                }
                DropDownList ddlVendorName = (DropDownList)grdcustom_material_list.Rows[i].FindControl("ddlVendorName");
                if (ddlVendorName.SelectedIndex != -1)
                {
                    cm.VendorId = Convert.ToInt16(ddlVendorName.SelectedValue);
                    cm.VendorName = ddlVendorName.SelectedItem.Text;
                }

                LinkButton lnkQuote = (LinkButton)grdcustom_material_list.Rows[i].FindControl("lnkQuote");
                if (lnkQuote.Text != "")
                {
                    cm.DocName = lnkQuote.Text;
                    cm.TempName = lnkQuote.CommandArgument;
                }
                TextBox txtAmount = (TextBox)grdcustom_material_list.Rows[i].FindControl("txtAmount");
                if (txtAmount.Text != "")
                    cm.Amount = Convert.ToDecimal(txtAmount.Text);

                itemList.Add(cm);
            }
            return itemList;
        }

        protected void Add_Click(object sender, EventArgs e)
        {
            List<CustomMaterialList> cmList = BindEmptyRowToMaterialList();
            ViewState["CustomMaterialList"] = cmList;
            BindCustomMaterialList(cmList);
        }
        private List<CustomMaterialList> GetMaterialListFromViewState()
        {
            List<CustomMaterialList> itemList = null;

            if (ViewState["CustomMaterialList"] == null)
            {
                itemList = new List<CustomMaterialList>();
            }
            else
            {
                itemList = ViewState["CustomMaterialList"] as List<CustomMaterialList>;
            }
            return itemList;
        }

        protected void UpdateMaterialList(CustomMaterialList item, int rowIndex = 0)
        {
            List<CustomMaterialList> itemList = GetMaterialListFromGrid();

            switch (item.Status)
            {
                case JGConstant.CustomMaterialListStatus.Unchanged:
                    break;
                case JGConstant.CustomMaterialListStatus.Added:
                    itemList.Add(item);
                    break;
                case JGConstant.CustomMaterialListStatus.Deleted:
                    itemList[rowIndex].Status = JGConstant.CustomMaterialListStatus.Deleted;
                    break;
                case JGConstant.CustomMaterialListStatus.Modified:
                    itemList[rowIndex] = item;
                    break;
                default:
                    break;
            }

            ViewState["CustomMaterialList"] = itemList;
            BindCustomMaterialList(itemList);
        }

        protected void BindCustomMaterialList(List<CustomMaterialList> itemList = null)
        {
            if (itemList == null)
            {
                itemList = GetMaterialListFromViewState();
            }
            List<CustomMaterialList> cmList = itemList.Where(c => c.Status != JGConstant.CustomMaterialListStatus.Deleted).ToList();
            grdcustom_material_list.DataSource = cmList;
            grdcustom_material_list.DataBind();
            int j = 0;
            string emailStatus = CustomBLL.Instance.GetEmailStatusOfCustomMaterialList(jobId);//, productTypeId, estimateId);

            foreach (GridViewRow r in grdcustom_material_list.Rows)
            {
                CustomMaterialList cml = cmList[j];
                if (cml.Status != JGConstant.CustomMaterialListStatus.Deleted)
                {
                    Label lblsrno = (Label)r.FindControl("lblsrno");

                    DropDownList ddlVendorCategory1 = (DropDownList)r.FindControl("ddlVendorCategory");
                    DropDownList ddlVendorName = (DropDownList)r.FindControl("ddlVendorName");
                    TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
                    LinkButton lnkQuote = (LinkButton)r.FindControl("lnkQuote");
                    HiddenField hdnMaterialListId = (HiddenField)r.FindControl("hdnMaterialListId");
                    HiddenField hdnEmailStatus = (HiddenField)r.FindControl("hdnEmailStatus");
                    HiddenField hdnForemanPermission = (HiddenField)r.FindControl("hdnForemanPermission");
                    HiddenField hdnSrSalesmanPermissionF = (HiddenField)r.FindControl("hdnSrSalesmanPermissionF");
                    HiddenField hdnAdminPermission = (HiddenField)r.FindControl("hdnAdminPermission");
                    HiddenField hdnSrSalesmanPermissionA = (HiddenField)r.FindControl("hdnSrSalesmanPermissionA");

                    lblsrno.Text = (j + 1).ToString();
                    if (cml.VendorCategoryId.ToString() != "")
                    {
                        ddlVendorCategory1.SelectedValue = cml.VendorCategoryId.ToString();
                    }
                    else
                    {
                        ddlVendorCategory1.SelectedIndex = -1;
                    }
                    if (cml.VendorId.ToString() != "")
                    {
                        int selectedCategoryID = Convert.ToInt16(ddlVendorCategory1.SelectedItem.Value);
                        DataSet ds = GetVendorNames(selectedCategoryID);
                        ddlVendorName.DataSource = ds;
                        ddlVendorName.SelectedIndex = -1;
                        ddlVendorName.DataTextField = "VendorName";
                        ddlVendorName.DataValueField = "VendorId";
                        ddlVendorName.DataBind();
                        ddlVendorName.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "0"));

                        ddlVendorName.SelectedValue = cml.VendorId.ToString();

                    }
                    else
                    {
                        ddlVendorName.SelectedIndex = -1;
                    }

                    if (cml.Amount.ToString() != "")
                    {
                        txtAmount.Text = cml.Amount.ToString();

                    }
                    else
                    {
                        txtAmount.Text = string.Empty;
                    }
                    if (Convert.ToInt16(cml.Id.ToString()) != 0)
                    {
                        hdnMaterialListId.Value = cml.Id.ToString();
                    }
                    else
                    {
                        hdnMaterialListId.Value = "0";
                    }
                    if (cml.IsForemanPermission != "")
                    {
                        hdnForemanPermission.Value = cml.IsForemanPermission;
                    }
                    else
                    {
                        hdnForemanPermission.Value = "";
                    }
                    if (cml.IsSrSalemanPermissionF != "")
                    {
                        hdnSrSalesmanPermissionF.Value = cml.IsSrSalemanPermissionF;
                    }
                    else
                    {
                        hdnSrSalesmanPermissionF.Value = "";
                    }
                    if (cml.IsAdminPermission != "")
                    {
                        hdnAdminPermission.Value = cml.IsAdminPermission;
                    }
                    else
                    {
                        hdnAdminPermission.Value = "";
                    }
                    if (cml.IsSrSalemanPermissionA != "")
                    {
                        hdnSrSalesmanPermissionA.Value = cml.IsSrSalemanPermissionA;
                    }
                    else
                    {
                        hdnSrSalesmanPermissionA.Value = "";
                    }
                    if (cml.EmailStatus != "")
                    {
                        hdnEmailStatus.Value = cml.EmailStatus;
                    }
                    else
                    {
                        hdnEmailStatus.Value = "";
                    }
                }
                if (emailStatus == JGConstant.EMAIL_STATUS_VENDORCATEGORIES)
                {
                    EnableVendorNameAndAmount();
                }
                j++;
            }
        }

        protected void ddlVendorName_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlVendorName = (DropDownList)sender;
            string selectedName = ddlVendorName.SelectedItem.Text;
            int vendorId = Convert.ToInt16(ddlVendorName.SelectedValue.ToString());

            foreach (GridViewRow r in grdcustom_material_list.Rows)
            {
                if (selectedName == "Select")
                {
                    ddlVendorName.SelectedIndex = -1;
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Please select a vendor name');", true);

                }
            }
            DataSet ds = VendorBLL.Instance.GetVendorQuoteByVendorId(jobId, vendorId);
            if (ds.Tables[0].Rows.Count <= 0)
            {
                ddlVendorName.SelectedIndex = -1;
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First attach quote for this vendor');", true);

            }

            foreach (GridViewRow r in grdcustom_material_list.Rows)
            {
                DropDownList ddlVendorName1 = (DropDownList)r.FindControl("ddlVendorName");
                {
                    DataSet dsVendorQuoute = VendorBLL.Instance.GetVendorQuoteByVendorId(jobId, Convert.ToInt16(ddlVendorName1.SelectedValue));
                    LinkButton lnkQuote = (LinkButton)r.FindControl("lnkQuote");
                    if (dsVendorQuoute.Tables[0].Rows.Count > 0)
                    {
                        lnkQuote.Text = dsVendorQuoute.Tables[0].Rows[0]["DocName"].ToString();
                        lnkQuote.CommandArgument = dsVendorQuoute.Tables[0].Rows[0]["TempName"].ToString();
                    }
                    else
                    {
                        lnkQuote.Text = "";
                        lnkQuote.CommandArgument = "";
                    }
                }
            }
        }
        protected void ddlVendorCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlVendorCategory = (DropDownList)sender;

            string selectedCategory = ddlVendorCategory.SelectedItem.Text;
            string emailStatus = CustomBLL.Instance.GetEmailStatusOfCustomMaterialList(jobId);//, productTypeId, estimateId);
            int counter = 1;
            foreach (GridViewRow r in grdcustom_material_list.Rows)
            {
                if (selectedCategory == "Select")
                {
                    ddlVendorCategory.SelectedIndex = -1;
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Please select a vendor category');", true);

                }
                else if (((DropDownList)r.FindControl("ddlVendorCategory")).SelectedItem.Text == selectedCategory)
                {
                    if (counter == 2)
                    {
                        ddlVendorCategory.SelectedIndex = -1;
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('This Vendor Category is already selected');", true);

                    }
                    counter++;

                }
                if (emailStatus == JGConstant.EMAIL_STATUS_VENDORCATEGORIES)
                {
                    DropDownList ddlVendorName = (DropDownList)r.FindControl("ddlVendorName");
                    DropDownList ddlVendorCategorySelected = (DropDownList)r.FindControl("ddlVendorCategory");
                    LinkButton lnkQuote = (LinkButton)r.FindControl("lnkQuote");
                    if (ddlVendorCategory == ddlVendorCategorySelected)
                    {
                        int selectedCategoryID = Convert.ToInt16(ddlVendorCategory.SelectedItem.Value);
                        DataSet ds = GetVendorNames(selectedCategoryID);
                        ddlVendorName.DataSource = ds;
                        ddlVendorName.SelectedIndex = -1;
                        ddlVendorName.DataTextField = "VendorName";
                        ddlVendorName.DataValueField = "VendorId";
                        ddlVendorName.DataBind();
                        ddlVendorName.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "0"));
                        ddlVendorName.SelectedIndex = 0;

                        lnkQuote.Text = "";
                    }
                }
            }
        }
        protected void grdcustom_material_list_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            List<CustomMaterialList> cmList = GetMaterialListFromGrid();
            if (cmList.Count > 1)
            {
                CustomMaterialList cm = cmList[e.RowIndex];
                cm.Status = JGConstant.CustomMaterialListStatus.Deleted;
                UpdateMaterialList(cm, e.RowIndex);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Atleast one row must be there in Custom- Material List');", true);
            }
        }

        protected void lnkVendorCategory_Click(object sender, EventArgs e)
        {
            pnlEmailTemplateForVendorCategories.Visible = true;
            pnlEmailTemplateForVendors.Visible = false;
            lnkVendorCategory.ForeColor = System.Drawing.Color.DarkGray;
            lnkVendorCategory.Enabled = false;
            lnkVendor.Enabled = true;
            lnkVendor.ForeColor = System.Drawing.Color.Blue;
            bind();
        }
        protected void bindVendorTemplate()
        {
            DataSet ds = new DataSet();
            ds = AdminBLL.Instance.FetchContractTemplate(100);
            if (ds != null)
            {
                HeaderEditorVendor.Content = ds.Tables[0].Rows[0][0].ToString();
                lblMaterialsVendor.Text = ds.Tables[0].Rows[0][1].ToString();
                FooterEditorVendor.Content = ds.Tables[0].Rows[0][2].ToString();
            }
        }
        protected void lnkVendor_Click(object sender, EventArgs e)
        {
            pnlEmailTemplateForVendors.Visible = true;
            pnlEmailTemplateForVendorCategories.Visible = false;
            lnkVendor.ForeColor = System.Drawing.Color.DarkGray;
            lnkVendor.Enabled = false;
            lnkVendorCategory.Enabled = true;
            lnkVendorCategory.ForeColor = System.Drawing.Color.Blue;
            bindVendorTemplate();
        }
        protected void lnkdelete_Click(object sender, EventArgs e)
        {
            if (grdcustom_material_list.Rows.Count > 1)
            {
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Atleast one row must be there in Custom- Material List');", true);
            }

        }

        protected void grdcustom_material_list_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label lblsrno = (Label)e.Row.FindControl("lblsrno");
                lblsrno.Text = Convert.ToString(Convert.ToInt16(lblsrno.Text) + 1);
                DropDownList ddlVendorCategory = (DropDownList)e.Row.FindControl("ddlVendorCategory");
                DataSet dsVendorCategory = GetVendorCategories();
                ddlVendorCategory.DataSource = GetVendorCategories();
                ddlVendorCategory.DataSource = dsVendorCategory;
                ddlVendorCategory.DataTextField = "VendorCategoryNm";
                ddlVendorCategory.DataValueField = "VendorCategpryId";
                ddlVendorCategory.DataBind();
                ddlVendorCategory.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "0"));
                ddlVendorCategory.SelectedIndex = 0;
            }
            if (btnSendMail.Text == "Send Mail To Vendor(s)")
            {
                grdcustom_material_list.Columns[6].Visible = false;
            }

        }
        protected void btnClose_Click(object sender, EventArgs e)
        {
            Response.Redirect("Procurement.aspx");
        }

        protected void btnXForeman_Click(object sender, EventArgs e)
        {
            popupForeman_permission.Hide();
        }
        protected void btnXSrSalesmanF_Click(object sender, EventArgs e)
        {
            popupSrSalesmanPermissionF.Hide();
        }

        protected void btnXSrSalesmanA_Click(object sender, EventArgs e)
        {
            popupSrSalesmanPermissionA.Hide();
        }

        protected void btnXAdmin_Click(object sender, EventArgs e)
        {
            popupAdmin_permission.Hide();
        }

        protected void VerifyForemanPermission(object sender, EventArgs e)
        {
            int cResult = CustomBLL.Instance.WhetherCustomMaterialListExists(jobId);//, productTypeId, estimateId);
            if (cResult == 1)
            {
                if (!string.IsNullOrEmpty(txtForemanPassword.Text))
                {
                    string adminCode = AdminBLL.Instance.GetForemanCode();
                    if (adminCode != txtForemanPassword.Text.Trim())
                    {
                        CVForeman.ErrorMessage = "Invalid Foreman Code";
                        CVForeman.ForeColor = System.Drawing.Color.Red;
                        CVForeman.IsValid = false;
                        CVForeman.Visible = true;
                        popupForeman_permission.Show();
                        return;
                    }
                    else
                    {
                        int result = CustomBLL.Instance.UpdateForemanPermissionOfCustomMaterialList(jobId.ToString(), JGConstant.PERMISSION_STATUS_GRANTED);//, productTypeId, estimateId);
                        if (result == 0)
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List');", true);
                        }
                        else
                        {
                            lnkForemanPermission.Enabled = false;
                            lnkForemanPermission.ForeColor = System.Drawing.Color.DarkGray;
                            popupForeman_permission.TargetControlID = "hdnForeman";
                            SetButtonText();
                        }
                    }
                }
                else
                {
                    CVForeman.ErrorMessage = "Please Enter Foreman Code";
                    CVForeman.ForeColor = System.Drawing.Color.Red;
                    CVForeman.IsValid = false;
                    CVForeman.Visible = true;
                    popupForeman_permission.Show();
                    return;
                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List');", true);
            }
        }

        protected void VerifySrSalesmanPermissionF(object sender, EventArgs e)
        {
            int cResult = CustomBLL.Instance.WhetherCustomMaterialListExists(jobId);//, productTypeId, estimateId);
            if (cResult == 1)
            {
                if (!string.IsNullOrEmpty(txtSrSalesmanPasswordF.Text))
                {
                    string salesmanCode = Session["loginpassword"].ToString();
                    if (salesmanCode != txtSrSalesmanPasswordF.Text.Trim())
                    {
                        CVSrSalesmanF.ErrorMessage = "Invalid Sr. Salesman Code";
                        CVSrSalesmanF.ForeColor = System.Drawing.Color.Red;
                        CVSrSalesmanF.IsValid = false;
                        CVSrSalesmanF.Visible = true;
                        popupSrSalesmanPermissionF.Show();
                        return;
                    }
                    else
                    {
                        int result = CustomBLL.Instance.UpdateSrSalesmanPermissionOfCustomMaterialListF(jobId.ToString(), JGConstant.PERMISSION_STATUS_GRANTED);//, productTypeId, estimateId);
                        if (result == 0)
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List');", true);
                        }
                        else
                        {
                            lnkSrSalesmanPermissionF.Enabled = false;
                            lnkSrSalesmanPermissionF.ForeColor = System.Drawing.Color.DarkGray;
                            popupSrSalesmanPermissionF.TargetControlID = "hdnSrF";
                            SetButtonText();
                        }
                    }
                }
                else
                {
                    CVSrSalesmanF.ErrorMessage = "Please Enter Sr. Salesman Code";
                    CVSrSalesmanF.ForeColor = System.Drawing.Color.Red;
                    CVSrSalesmanF.IsValid = false;
                    CVSrSalesmanF.Visible = true;
                    popupSrSalesmanPermissionF.Show();
                    return;

                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List');", true);
            }
        }

        protected void VerifyAdminPermission(object sender, EventArgs e)
        {
            int cResult = CustomBLL.Instance.WhetherVendorInCustomMaterialListExists(jobId);//,productTypeId,estimateId);
            if (cResult == 1)
            {
                if (!string.IsNullOrEmpty(txtAdminPassword.Text))
                {
                    string adminCode = AdminBLL.Instance.GetAdminCode();
                    if (adminCode != txtAdminPassword.Text.Trim())
                    {
                        CVAdmin.ErrorMessage = "Invalid Admin Code";
                        CVAdmin.ForeColor = System.Drawing.Color.Red;
                        CVAdmin.IsValid = false;
                        CVAdmin.Visible = true;
                        popupAdmin_permission.Show();
                        return;
                    }
                    else
                    {
                        int result = CustomBLL.Instance.UpdateAdminPermissionOfCustomMaterialList(jobId.ToString(), JGConstant.PERMISSION_STATUS_GRANTED);//, productTypeId, estimateId);
                        if (result == 0)
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List');", true);
                        }
                        else
                        {
                            lnkAdminPermission.Enabled = false;
                            lnkAdminPermission.ForeColor = System.Drawing.Color.DarkGray;
                            popupAdmin_permission.TargetControlID = "hdnAdmin";
                            SetButtonText();
                            DisableVendorNameAndAmount();
                        }
                    }
                }
                else
                {
                    CVAdmin.ErrorMessage = "Please Enter Admin Code";
                    CVAdmin.ForeColor = System.Drawing.Color.Red;
                    CVAdmin.IsValid = false;
                    CVAdmin.Visible = true;
                    popupAdmin_permission.Show();
                    return;
                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List and enter all vendor names');", true);
            }
            //message mail is not sent to categories
        }

        protected void VerifySrSalesmanPermissionA(object sender, EventArgs e)
        {
            int cResult = CustomBLL.Instance.WhetherVendorInCustomMaterialListExists(jobId);//, productTypeId, estimateId);
            if (cResult == 1)
            {
                if (!string.IsNullOrEmpty(txtSrSalesmanPasswordA.Text))
                {
                    string salesmanCode = Session["loginpassword"].ToString();
                    if (salesmanCode != txtSrSalesmanPasswordA.Text.Trim())
                    {
                        CVSrSalesmanA.ErrorMessage = "Invalid Sr. Salesman Code";
                        CVSrSalesmanA.ForeColor = System.Drawing.Color.Red;
                        CVSrSalesmanA.IsValid = false;
                        CVSrSalesmanA.Visible = true;
                        popupSrSalesmanPermissionA.Show();
                        return;
                    }
                    else
                    {
                        int result = CustomBLL.Instance.UpdateSrSalesmanPermissionOfCustomMaterialList(jobId.ToString(), JGConstant.PERMISSION_STATUS_GRANTED);//, productTypeId, estimateId);
                        if (result == 0)
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List');", true);
                        }
                        else
                        {
                            lnkSrSalesmanPermissionA.Enabled = false;
                            lnkSrSalesmanPermissionA.ForeColor = System.Drawing.Color.DarkGray;
                            popupSrSalesmanPermissionA.TargetControlID = "hdnSrA";
                            SetButtonText();
                            DisableVendorNameAndAmount();
                        }
                    }
                }
                else
                {
                    CVSrSalesmanA.ErrorMessage = "Please Enter Sr. Salesman Code";
                    CVSrSalesmanA.ForeColor = System.Drawing.Color.Red;
                    CVSrSalesmanA.IsValid = false;
                    CVSrSalesmanA.Visible = true;
                    popupSrSalesmanPermissionA.Show();
                    return;
                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First save Material List and enter all vendor names');", true);
            }
        }

        protected void disableAddDeleteLinks()
        {
            foreach (GridViewRow r in grdcustom_material_list.Rows)
            {
                LinkButton lnkAdd = (LinkButton)r.FindControl("lnkAdd");
                lnkAdd.Enabled = false;
                lnkAdd.ForeColor = System.Drawing.Color.DarkGray;
                LinkButton lnkdelete = (LinkButton)r.FindControl("lnkdelete");
                lnkdelete.Enabled = false;
                lnkdelete.ForeColor = System.Drawing.Color.DarkGray;
            }

        }

        protected void btnSendMail_Click(object sender, EventArgs e)
        {
            try
            {
                string status = CustomBLL.Instance.GetEmailStatusOfCustomMaterialList(jobId);//, productTypeId, estimateId);
                List<CustomMaterialList> cmList = new List<CustomMaterialList>();
                foreach (GridViewRow r in grdcustom_material_list.Rows)
                {
                    CustomMaterialList cm = new CustomMaterialList();
                    DropDownList ddlVendorCategory = (DropDownList)r.FindControl("ddlVendorCategory");
                    cm.VendorCategoryId = Convert.ToInt16(ddlVendorCategory.SelectedValue);
                    TextBox txtMateriallist = (TextBox)r.FindControl("txtMateriallist");
                    HiddenField hdnMaterialListId = (HiddenField)r.FindControl("hdnMaterialListId");
                    HiddenField hdnEmailStatus = (HiddenField)r.FindControl("hdnEmailStatus");
                    HiddenField hdnForemanPermission = (HiddenField)r.FindControl("hdnForemanPermission");
                    HiddenField hdnSrSalesmanPermissionF = (HiddenField)r.FindControl("hdnSrSalesmanPermissionF");
                    HiddenField hdnAdminPermission = (HiddenField)r.FindControl("hdnAdminPermission");
                    HiddenField hdnSrSalesmanPermissionA = (HiddenField)r.FindControl("hdnSrSalesmanPermissionA");

                    if (txtMateriallist.Text == "")
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Please fill Material List(s).');", true);
                    }
                    else
                    {
                        cm.MaterialList = txtMateriallist.Text;
                    }

                    if (hdnMaterialListId.Value != "")
                    {
                        cm.Id = Convert.ToInt16(hdnMaterialListId.Value);
                    }
                    else
                    {
                        cm.Id = 0;
                    }
                    DropDownList ddlVendorName = (DropDownList)r.FindControl("ddlVendorName");
                    TextBox txtAmount = (TextBox)r.FindControl("txtAmount");

                    if (status == "C") //mail was already sent to vendor categories
                    {
                        if (ddlVendorName.SelectedItem.Text == "Select")
                        {
                            //ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Please select vendor name.');", true);
                            //return;
                        }
                        else
                        {
                            cm.VendorName = ddlVendorName.SelectedItem.Text;
                            cm.VendorId = Convert.ToInt16(ddlVendorName.SelectedValue);

                            DataSet ds = VendorBLL.Instance.getVendorEmailId(ddlVendorName.SelectedItem.Text);
                            cm.VendorEmail = ds.Tables[0].Rows[0][0].ToString();
                        }

                        if (txtAmount.Text == "")
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Please enter amount.');", true);
                            return;
                        }
                        else
                        {
                            cm.Amount = Convert.ToDecimal(txtAmount.Text);
                        }
                        if (lnkAdminPermission.Enabled == true)
                        {
                            cm.IsAdminPermission = JGConstant.PERMISSION_STATUS_NOTGRANTED.ToString();
                        }
                        else
                        {
                            cm.IsAdminPermission = JGConstant.PERMISSION_STATUS_GRANTED.ToString();
                        }
                        if (lnkSrSalesmanPermissionA.Enabled == true)
                        {
                            cm.IsSrSalemanPermissionA = JGConstant.PERMISSION_STATUS_NOTGRANTED.ToString();
                        }
                        else
                        {
                            cm.IsSrSalemanPermissionA = JGConstant.PERMISSION_STATUS_GRANTED.ToString();
                        }
                        cm.IsForemanPermission = JGConstant.PERMISSION_STATUS_GRANTED.ToString();
                        cm.IsSrSalemanPermissionF = JGConstant.PERMISSION_STATUS_GRANTED.ToString();

                        cm.EmailStatus = JGConstant.EMAIL_STATUS_VENDORCATEGORIES;
                    }
                    else // mail was not sent to vendor categories
                    {
                        cm.VendorName = "";
                        cm.VendorEmail = "";
                        cm.IsAdminPermission = JGConstant.PERMISSION_STATUS_NOTGRANTED.ToString();
                        cm.IsSrSalemanPermissionA = JGConstant.PERMISSION_STATUS_NOTGRANTED.ToString();
                        if (lnkForemanPermission.Enabled == true)
                        {
                            cm.IsForemanPermission = JGConstant.PERMISSION_STATUS_NOTGRANTED.ToString();
                        }
                        else
                        {
                            cm.IsForemanPermission = JGConstant.PERMISSION_STATUS_GRANTED.ToString();
                        }
                        if (lnkSrSalesmanPermissionF.Enabled == true)
                        {
                            cm.IsSrSalemanPermissionF = JGConstant.PERMISSION_STATUS_NOTGRANTED.ToString();
                        }
                        else
                        {
                            cm.IsSrSalemanPermissionF = JGConstant.PERMISSION_STATUS_GRANTED.ToString();
                        }

                        cm.EmailStatus = JGConstant.EMAIL_STATUS_NONE;
                    }
                    cmList.Add(cm);
                }
                if (btnSendMail.Text == "Save")
                {
                    int existsList = CustomBLL.Instance.WhetherCustomMaterialListExists(jobId);//, productTypeId, estimateId);
                    if (existsList == 0)
                    {
                        saveCustom_MaterialList(cmList);
                    }
                    else
                    {
                        EnableVendorNameAndAmount();
                        int permissionStatusCategories = CustomBLL.Instance.CheckPermissionsForCategories(jobId);//, productTypeId, estimateId);
                        if (permissionStatusCategories == 0)
                        {
                            saveCustom_MaterialList(cmList);
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('All lists are saved.');", true);
                            return;
                        }
                        else
                        {
                            int permissionStatusVendors = CustomBLL.Instance.CheckPermissionsForVendors(jobId);//, productTypeId, estimateId);
                            if (permissionStatusVendors == 0)
                            {
                                saveCustom_MaterialList(cmList);
                                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('All lists are saved.');", true);
                                return;
                            }
                            else
                            {
                                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('After giving permissions lists cann't be changed');", true);
                                return;
                            }
                        }
                    }
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('All lists are saved.');", true);
                }

                else if (btnSendMail.Text == "Send Mail To Vendor Category(s)")
                {

                    int permissionStatus = CustomBLL.Instance.CheckPermissionsForCategories(jobId);//, productTypeId, estimateId);
                    if (permissionStatus == 1)
                    {
                        bool emailStatusVendorCategory = sendEmailToVendorCategories(cmList);

                        if (emailStatusVendorCategory == true)
                        {
                            bool result = CustomBLL.Instance.UpdateEmailStatusOfCustomMaterialList(jobId, JGConstant.EMAIL_STATUS_VENDORCATEGORIES);//, productTypeId, estimateId);
                            UpdateEmailStatus(JGConstant.EMAIL_STATUS_VENDORCATEGORIES.ToString());
                            btnSendMail.Text = "Save";
                            setControlsForVendors();
                            grdcustom_material_list.Columns[6].Visible = true;
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Email is sent to all vendor categories');", true);

                        }
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First grant Foreman and Sr. Salesman permission');", true);
                    }
                }
                else
                {
                    int permissionStatus = CustomBLL.Instance.CheckPermissionsForVendors(jobId);//, productTypeId, estimateId);
                    if (permissionStatus == 1)
                    {
                        int statusQuotes = CustomBLL.Instance.WhetherVendorQuotesExists(jobId);
                        if (statusQuotes == 1)
                        {

                            bool emailStatusVendor = sendEmailToVendors(cmList);
                            if (emailStatusVendor == true)
                            {
                                bool result = CustomBLL.Instance.UpdateEmailStatusOfCustomMaterialList(jobId, JGConstant.EMAIL_STATUS_VENDOR);//, productTypeId, estimateId);
                                UpdateEmailStatus(JGConstant.EMAIL_STATUS_VENDOR.ToString());
                                btnSendMail.Text = "Save";
                                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Email is sent to all vendors');", true);
                                setControlsAfterSendingBothMails();
                                string fileName = GenerateWorkOrder();
                                string url = ConfigurationManager.AppSettings["URL"].ToString();
                                ClientScript.RegisterClientScriptBlock(Page.GetType(), "Myscript", "<script language='javascript'>window.open('" + url + "/CustomerDocs/Pdfs/" + fileName + "', null, 'width=487px,height=455px,center=1,resize=0,scrolling=1,location=no');</script>");
                            }
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First attach quotes.');", true);
                        }
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('First grant Admin and Sr. Salesman permission');", true);
                    }

                }
            }
            catch (Exception)
            {

            }
        }

        protected void UpdateEmailStatus(string status)
        {
            List<CustomMaterialList> cmList = GetMaterialListFromGrid();
            foreach (CustomMaterialList cm in cmList)
            {
                cm.EmailStatus = status;
            }
            ViewState["CustomMaterialList"] = cmList;
        }
        protected string GenerateWorkOrder()
        {
            string path = Server.MapPath("/CustomerDocs/Pdfs/");
            string tempWorkOrderFilename = "WorkOrder" + DateTime.Now.Ticks + ".pdf";
            string originalWorkOrderFilename = "WorkOrder" + ".pdf";
            string soldjobId = Session["jobId"].ToString();
           // DataSet dssoldJobs = new_customerBLL.Instance.GetProductAndEstimateIdOfSoldJob(soldjobId);
            int productId = estimateId;// Convert.ToInt16(dssoldJobs.Tables[0].Rows[0]["EstimateId"].ToString());
            GeneratePDF(path, tempWorkOrderFilename, false, createWorkOrder("Work Order-" + customerId.ToString(), productId, soldjobId));

            new_customerBLL.Instance.AddCustomerDocs(Convert.ToInt32(customerId.ToString()), productId, originalWorkOrderFilename, "WorkOrder", tempWorkOrderFilename, productTypeId, 0);
            return tempWorkOrderFilename;
        }
        
        private string createWorkOrder(string InvoiceNo, int estimateId, string soldJobId)
        {
            return pdf_BLL.Instance.CreateWorkOrder(InvoiceNo, estimateId, productTypeId, customerId, soldJobId);
        }

        private void GeneratePDF(string path, string fileName, bool download, string text)//download set to false in calling method
        {
            var document = new Document();
            FileStream FS = new FileStream(path + fileName, FileMode.Create);
            try
            {
                if (download)
                {
                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    PdfWriter.GetInstance(document, Response.OutputStream);
                }
                else
                {
                    PdfWriter.GetInstance(document, FS);
                }
                StringBuilder strB = new StringBuilder();
                strB.Append(text);
                //string filePath = Server.MapPath("/CustomerDocs/Pdfs/wkhtmltopdf.exe");
                //byte[] byteData = ConvertHtmlToByte(strB.ToString(), "", "", filePath);
                //if (byteData != null)
                //{
                //    StreamByteToPDF(byteData, Server.MapPath("/CustomerDocs/Pdfs/") + fileName);
                //}

                using (TextReader sReader = new StringReader(strB.ToString()))
                {
                    document.Open();
                    List<IElement> list = HTMLWorker.ParseToList(sReader, new StyleSheet());
                    foreach (IElement elm in list)
                    {
                        document.Add(elm);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Instance.writeToLog(ex, "Custom", "");
                //LogManager.Instance.WriteToFlatFile(ex.Message, "Custom",1);// Request.ServerVariables["remote_addr"].ToString());
                
            }
            finally
            {
                if (document.IsOpen())
                    document.Close();
            }
        }
        //public static void StreamByteToPDF(byte[] byteData, string filePathPhysical)
        //{

        //    if (byteData != null)
        //    {
        //        if (File.Exists(filePathPhysical))
        //        {
        //            File.Delete(filePathPhysical);

        //        }
        //        // string filename = "C:\\Reports\\Newsamplecif.pdf";
        //        FileStream fs = new FileStream(filePathPhysical, FileMode.Create, FileAccess.ReadWrite);
        //        //Read block of bytes from stream into the byte array
        //        fs.Write(byteData, 0, byteData.Length);

        //        //Close the File Stream
        //        fs.Close();
        //    }
        //}
        //public static byte[] ConvertHtmlToByte(string HtmlData, string headerPath, string footerPath, string filePath)
        //{
        //    Process p;
        //    ProcessStartInfo psi = new ProcessStartInfo();

        //    psi.FileName = filePath;
        //    psi.WorkingDirectory = Path.GetDirectoryName(psi.FileName);

        //    // run the conversion utility
        //    psi.UseShellExecute = false;
        //    psi.CreateNoWindow = true;
        //    psi.RedirectStandardInput = true;
        //    psi.RedirectStandardOutput = true;
        //    psi.RedirectStandardError = true;
        //    // note: that we tell wkhtmltopdf to be quiet and not run scripts
        //    string args = "-q -n ";
        //    args += "--disable-smart-shrinking ";
        //    args += "--orientation Portrait ";
        //    args += "--outline-depth 0 ";
        //    //args += "--header-spacing 140 ";
        //    //args += "--default-header ";

        //    if (footerPath != string.Empty)
        //    {
        //        args += "--footer-html " + footerPath + " ";

        //    }
        //    if (headerPath != string.Empty)
        //    {
        //        args += "--header-spacing 2 ";
        //        args += "--header-html " + headerPath + " ";

        //    }
        //    //args += "--header-font-size  20 ";
        //    args += "--page-size A4 --encoding windows-1250";
        //    args += " - -";

        //    psi.Arguments = args;

        //    p = Process.Start(psi);

        //    try
        //    {
        //        using (StreamWriter stdin = new StreamWriter(p.StandardInput.BaseStream, Encoding.UTF8))
        //        {
        //            stdin.AutoFlush = true;
        //            stdin.Write(HtmlData);
        //        }

        //        //read output
        //        byte[] buffer = new byte[32768];
        //        byte[] file;
        //        using (var ms = new MemoryStream())
        //        {
        //            while (true)
        //            {
        //                int read = p.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);
        //                if (read <= 0)
        //                    break;
        //                ms.Write(buffer, 0, read);
        //            }
        //            file = ms.ToArray();
        //        }

        //        p.StandardOutput.Close();
        //        // wait or exit
        //        p.WaitForExit(60000);

        //        // read the exit code, close process
        //        int returnCode = p.ExitCode;
        //        p.Close();

        //        if (returnCode == 0)
        //            return file;
        //        //else
        //        //    log.Error("Could not create PDF, returnCode:" + returnCode);
        //    }
        //    catch (Exception ex)
        //    {
        //        // log.Error("Could not create PDF", ex);
        //    }
        //    finally
        //    {
        //        p.Close();
        //        p.Dispose();
        //    }
        //    return null;
        //}
        protected void setControlsAfterSendingBothMails()
        {
            btnSendMail.Visible = false;
            grdcustom_material_list.Columns[6].Visible = false;
            lnkAdminPermission.Enabled = false;
            lnkForemanPermission.Enabled = false;
            lnkSrSalesmanPermissionA.Enabled = false;
            lnkSrSalesmanPermissionF.Enabled = false;
            foreach (GridViewRow r in grdcustom_material_list.Rows)
            {
                TextBox txtMateriallist = (TextBox)r.FindControl("txtMateriallist");
                txtMateriallist.Enabled = false;
                DropDownList ddlVendorCategory = (DropDownList)r.FindControl("ddlVendorCategory");
                ddlVendorCategory.Enabled = false;
                DropDownList ddlVendorName = (DropDownList)r.FindControl("ddlVendorName");
                ddlVendorName.Enabled = false;
                TextBox txtAmount = (TextBox)r.FindControl("txtAmount");
                txtAmount.Enabled = false;
                LinkButton lnkQuote = (LinkButton)r.FindControl("lnkQuote");
                lnkQuote.Enabled = true;
            }
        }
        protected void setControlsForVendors()
        {
            DataSet ds1 = CustomBLL.Instance.GetCustom_MaterialList(jobId);//,productTypeId,estimateId);
            decimal amount = 0;
            int vendorId = 0, i = 0;
            foreach (GridViewRow gr in grdcustom_material_list.Rows)
            {
                TextBox txtMateriallist = (TextBox)gr.FindControl("txtMateriallist");
                txtMateriallist.Enabled = false;
                TextBox txtAmount = (TextBox)gr.FindControl("txtAmount");
                txtAmount.Enabled = true;
                DropDownList ddlVendorCategory = (DropDownList)gr.FindControl("ddlVendorCategory");
                ddlVendorCategory.Enabled = false;
                int selectedCategoryID = Convert.ToInt16(ddlVendorCategory.SelectedItem.Value);
                DropDownList ddlVendorName = (DropDownList)gr.FindControl("ddlVendorName");
                ddlVendorName.Enabled = true;
                DataSet ds = GetVendorNames(selectedCategoryID);
                ddlVendorName.DataSource = ds;
                ddlVendorName.SelectedIndex = -1;
                ddlVendorName.DataTextField = "VendorName";
                ddlVendorName.DataValueField = "VendorId";
                ddlVendorName.DataBind();
                ddlVendorName.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select", "0"));
                ddlVendorName.SelectedIndex = 0;
                if (ds1.Tables[0].Rows[i]["Amount"].ToString() != "")
                {
                    amount = Convert.ToDecimal(ds1.Tables[0].Rows[i]["Amount"].ToString());
                    txtAmount.Text = amount.ToString();
                }
                if (ds1.Tables[0].Rows[i]["VendorId"].ToString() != "")
                {
                    ddlVendorName.SelectedIndex = -1;
                    vendorId = Convert.ToInt16(ds1.Tables[0].Rows[i]["VendorId"].ToString());
                    ddlVendorName.SelectedValue = vendorId.ToString();
                }
                i++;
            }
            lnkAdminPermission.Visible = true;
            lnkSrSalesmanPermissionA.Visible = true;
            lnkForemanPermission.Visible = false;
            lnkSrSalesmanPermissionF.Visible = false;
        }

        protected void setControlsForVendorsOnPageLoad()
        {
            foreach (GridViewRow gr in grdcustom_material_list.Rows)
            {
                TextBox txtMateriallist = (TextBox)gr.FindControl("txtMateriallist");
                txtMateriallist.Enabled = false;
                TextBox txtAmount = (TextBox)gr.FindControl("txtAmount");
                txtAmount.Enabled = true;
                DropDownList ddlVendorCategory = (DropDownList)gr.FindControl("ddlVendorCategory");
                ddlVendorCategory.Enabled = false;
                int selectedCategoryID = Convert.ToInt16(ddlVendorCategory.SelectedItem.Value);
                DropDownList ddlVendorName = (DropDownList)gr.FindControl("ddlVendorName");
                ddlVendorName.Enabled = true;
            }
            lnkAdminPermission.Visible = true;
            lnkSrSalesmanPermissionA.Visible = true;
            lnkForemanPermission.Visible = false;
            lnkSrSalesmanPermissionF.Visible = false;
            grdcustom_material_list.Columns[6].Visible = false;
        }

        protected void saveCustom_MaterialList(List<CustomMaterialList> cmList)
        {
            bool result = false;
            CustomBLL.Instance.DeleteCustomMaterialList(jobId);//, productTypeId, estimateId);
            foreach (CustomMaterialList cm in cmList)
            {

                result = CustomBLL.Instance.AddCustomMaterialList(cm, jobId);//,productTypeId,estimateId);
            }

            ViewState["CustomMaterialList"] = cmList;
        }

        public DataSet fetchVendorCategoryEmailTemplate()
        {
            DataSet ds = new DataSet();
            ds = AdminBLL.Instance.FetchContractTemplate(0);
            return ds;
        }
        protected bool sendEmailToVendorCategories(List<CustomMaterialList> cmList)
        {
            bool emailStatus = true;
            string mailNotSendIds = string.Empty;
            string htmlBody = string.Empty;
            int emailCounter = 0;
            try
            {
                //loop for each vendor category on procurement page
                foreach (CustomMaterialList cm in cmList)
                {
                    //to fetch all vendors within a category
                    DataSet dsVendorsListByCategory = VendorBLL.Instance.fetchVendorListByCategoryForEmail(cm.VendorCategoryId);

                    if (dsVendorsListByCategory != null)
                    {
                        //loop for all vendors within a category
                        for (int counter = 0; counter < dsVendorsListByCategory.Tables[0].Rows.Count; counter++)
                        {
                            DataRow dr = dsVendorsListByCategory.Tables[0].Rows[counter];
                            string mailId = dr["Email"].ToString();
                            string vendorName = dr["VendorName"].ToString();

                            MailMessage m = new MailMessage();
                            SmtpClient sc = new SmtpClient();

                            string userName = ConfigurationManager.AppSettings["VendorCategoryUserName"].ToString();
                            string password = ConfigurationManager.AppSettings["VendorCategoryPassword"].ToString();

                            m.From = new MailAddress(userName, "JMGROVECONSTRUCTION");
                            m.To.Add(new MailAddress(mailId, vendorName));
                            m.Subject = "J.M. Grove " + jobId + " quote request ";
                            m.IsBodyHtml = true;
                            DataSet dsEmailTemplate = fetchVendorCategoryEmailTemplate();

                            if (dsEmailTemplate != null)
                            {
                                string templateHeader = dsEmailTemplate.Tables[0].Rows[0][0].ToString();
                                StringBuilder tHeader = new StringBuilder();
                                tHeader.Append(templateHeader);
                                var replacedHeader = tHeader//.Replace("imgHeader", "<img src=cid:myImageHeader height=10% width=80%>")
                                                               .Replace("src=\"../img/Email art header.png\"", "src=cid:myImageHeader")
                                                            .Replace("lblJobId", jobId.ToString())
                                                            .Replace("lblCustomerId", "C" + customerId.ToString());
                                htmlBody = replacedHeader.ToString();
                                htmlBody += "</br></br></br>";
                                string templateBody = dsEmailTemplate.Tables[0].Rows[0][1].ToString();

                                string materialList = cm.MaterialList;


                                StringBuilder tbody = new StringBuilder();
                                tbody.Append(templateBody);

                                var replacedBody = tbody.Replace("lblMaterialList", materialList);

                                htmlBody += replacedBody.ToString();

                                htmlBody += "</br></br></br>";

                                string templateFooter = dsEmailTemplate.Tables[0].Rows[0][2].ToString();
                                StringBuilder tFooter = new StringBuilder();
                                tFooter.Append(templateFooter);
                                var replacedFooter = tFooter.Replace("src=\"../img/JG-Logo-white.gif\"", "src=cid:myImageLogo")
                                                           .Replace("src=\"../img/Email footer.png\"", "src=cid:myImageFooter");
                                htmlBody += replacedFooter.ToString();
                            }
                            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");

                            string imageSourceHeader = Server.MapPath(@"~\img") + @"\Email art header.png";
                            LinkedResource theEmailImageHeader = new LinkedResource(imageSourceHeader);
                            theEmailImageHeader.ContentId = "myImageHeader";

                            string imageSourceLogo = Server.MapPath(@"~\img") + @"\JG-Logo-white.gif";
                            LinkedResource theEmailImageLogo = new LinkedResource(imageSourceLogo);
                            theEmailImageLogo.ContentId = "myImageLogo";

                            string imageSourceFooter = Server.MapPath(@"~\img") + @"\Email footer.png";
                            LinkedResource theEmailImageFooter = new LinkedResource(imageSourceFooter);
                            theEmailImageFooter.ContentId = "myImageFooter";

                            //Add the Image to the Alternate view
                            htmlView.LinkedResources.Add(theEmailImageHeader);
                            htmlView.LinkedResources.Add(theEmailImageLogo);
                            htmlView.LinkedResources.Add(theEmailImageFooter);

                            m.AlternateViews.Add(htmlView);
                            m.Body = htmlBody;
                            sc.UseDefaultCredentials = false;
                            sc.Host = "jmgrove.fatcow.com";
                            sc.Port = 25;


                            sc.Credentials = new System.Net.NetworkCredential(userName, password);
                            sc.EnableSsl = false; // runtime encrypt the SMTP communications using SSL
                            try
                            {
                                sc.Send(m);
                                emailCounter += 1;
                            }
                            catch (Exception ex)
                            {
                                mailNotSendIds += mailId + " , ";
                                CustomBLL.Instance.UpdateEmailStatusOfCustomMaterialList(jobId, JGConstant.EMAIL_STATUS_NONE);//, productTypeId, estimateId);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('" + ex.Message + "');", true);
            }
            if (mailNotSendIds != string.Empty)
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Failed to send email to : " + mailNotSendIds + "');", true);
            if (emailCounter == 0)
                emailStatus = false;
            else
                emailStatus = true;

            return emailStatus;
        }

        public DataSet fetchVendorEmailTemplate()
        {
            DataSet ds = new DataSet();
            ds = AdminBLL.Instance.FetchContractTemplate(100);
            return ds;
        }

        protected bool sendEmailToVendors(List<CustomMaterialList> cmList)
        {
            bool emailstatus = true;
            string htmlBody = string.Empty;
            string mailNotSendIds = string.Empty;
            int emailCounter = 0;
            try
            {
                //loop for each vendor
                foreach (CustomMaterialList cm in cmList)
                {
                    DataSet dsVendorQuoute = VendorBLL.Instance.GetVendorQuoteByVendorId(jobId, cm.VendorId);
                    string quoteTempName = "", quoteOriginalName = "";
                    if (dsVendorQuoute.Tables[0].Rows.Count > 0)
                    {
                        quoteTempName = dsVendorQuoute.Tables[0].Rows[0]["TempName"].ToString();
                        quoteOriginalName = dsVendorQuoute.Tables[0].Rows[0]["DocName"].ToString();
                    }
                    MailMessage m = new MailMessage();
                    SmtpClient sc = new SmtpClient();

                    string userName = ConfigurationManager.AppSettings["VendorUserName"].ToString();
                    string password = ConfigurationManager.AppSettings["VendorPassword"].ToString();

                    m.From = new MailAddress(userName, "JMGROVECONSTRUCTION");
                    string mailId = cm.VendorEmail;
                    m.To.Add(new MailAddress(mailId, cm.VendorName));
                    m.Subject = "J.M. Grove " + jobId + " quote acceptance ";
                    m.IsBodyHtml = true;
                    DataSet dsEmailTemplate = fetchVendorEmailTemplate();

                    if (dsEmailTemplate != null)
                    {
                        string templateHeader = dsEmailTemplate.Tables[0].Rows[0][0].ToString();
                        StringBuilder tHeader = new StringBuilder();
                        tHeader.Append(templateHeader);

                        var replacedHeader = tHeader//.Replace("imgHeader", "<img src=cid:myImageHeader height=10% width=80%>")
                                                   .Replace("src=\"../img/Email art header.png\"", "src=cid:myImageHeader")
                                                   .Replace("lblJobId", jobId.ToString())
                                                   .Replace("lblCustomerId", "C" + customerId.ToString());
                        htmlBody = replacedHeader.ToString();

                        string templateBody = dsEmailTemplate.Tables[0].Rows[0][1].ToString();

                        StringBuilder tbody = new StringBuilder();
                        tbody.Append(templateBody);

                        var replacedBody = tbody.Replace("lblMaterialList", cm.MaterialList)
                                                .Replace("lblAmount", cm.Amount.ToString());

                        htmlBody += replacedBody.ToString();

                        string templateFooter = dsEmailTemplate.Tables[0].Rows[0][2].ToString();
                        StringBuilder tFooter = new StringBuilder();
                        tFooter.Append(templateFooter);

                        var replacedFooter = tFooter.Replace("src=\"../img/JG-Logo-white.gif\"", "src=cid:myImageLogo")
                                                           .Replace("src=\"../img/Email footer.png\"", "src=cid:myImageFooter");
                        htmlBody += replacedFooter.ToString();
                    }
                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");

                    if (quoteTempName != "")
                    {
                        string sourceDir = Server.MapPath("~/CustomerDocs/VendorQuotes/");
                        Attachment attachment = new Attachment(sourceDir + "\\" + quoteTempName);
                        attachment.Name = quoteOriginalName;
                        m.Attachments.Add(attachment);
                    }
                    string imageSourceHeader = Server.MapPath(@"~\img") + @"\Email art header.png";
                    LinkedResource theEmailImageHeader = new LinkedResource(imageSourceHeader);
                    theEmailImageHeader.ContentId = "myImageHeader";

                    string imageSourceLogo = Server.MapPath(@"~\img") + @"\JG-Logo-white.gif";
                    LinkedResource theEmailImageLogo = new LinkedResource(imageSourceLogo);
                    theEmailImageLogo.ContentId = "myImageLogo";

                    string imageSourceFooter = Server.MapPath(@"~\img") + @"\Email footer.png";
                    LinkedResource theEmailImageFooter = new LinkedResource(imageSourceFooter);
                    theEmailImageFooter.ContentId = "myImageFooter";

                    //Add the Image to the Alternate view
                    htmlView.LinkedResources.Add(theEmailImageHeader);
                    htmlView.LinkedResources.Add(theEmailImageLogo);
                    htmlView.LinkedResources.Add(theEmailImageFooter);

                    m.AlternateViews.Add(htmlView);
                    m.Body = htmlBody;

                    sc.UseDefaultCredentials = false;
                    sc.Host = "jmgrove.fatcow.com";
                    sc.Port = 25;
                    sc.Credentials = new System.Net.NetworkCredential(userName, password);
                    sc.EnableSsl = false; // runtime encrypt the SMTP communications using SSL
                    try
                    {
                        sc.Send(m);
                        emailCounter += 1;
                    }
                    catch (Exception ex)
                    {
                        mailNotSendIds += mailId + " , ";
                        CustomBLL.Instance.UpdateEmailStatusOfCustomMaterialList(jobId, JGConstant.EMAIL_STATUS_VENDORCATEGORIES);//, productTypeId, estimateId);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            if (emailCounter == 0)
                emailstatus = false;
            else
                emailstatus = true;

            if (mailNotSendIds != string.Empty)
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('Failed to send email to : " + mailNotSendIds + "');", true);

            return emailstatus;
        }

        protected void bind()
        {
            DataSet ds = new DataSet();
            ds = AdminBLL.Instance.FetchContractTemplate(0);
            if (ds != null)
            {
                HeaderEditor.Content = ds.Tables[0].Rows[0][0].ToString();
                lblMaterials.Text = ds.Tables[0].Rows[0][1].ToString();
                FooterEditor.Content = ds.Tables[0].Rows[0][2].ToString();
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            string Editor_contentHeader = HeaderEditor.Content;
            string Editor_contentFooter = FooterEditor.Content;
            bool result = AdminBLL.Instance.UpdateEmailVendorCategoryTemplate(Editor_contentHeader, Editor_contentFooter);
            if (result)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('EmailVendor Template Updated Successfully');", true);
            }

        }

        protected void bindVendor()
        {
            DataSet ds = new DataSet();
            ds = AdminBLL.Instance.FetchContractTemplate(100);
            if (ds != null)
            {
                HeaderEditorVendor.Content = ds.Tables[0].Rows[0][0].ToString();
                lblMaterialsVendor.Text = ds.Tables[0].Rows[0][1].ToString();
                FooterEditorVendor.Content = ds.Tables[0].Rows[0][2].ToString();
            }
        }
        protected void btnUpdateVendor_Click(object sender, EventArgs e)
        {
            string Editor_contentHeader = HeaderEditorVendor.Content;
            string Editor_contentFooter = FooterEditorVendor.Content;
            bool result = AdminBLL.Instance.UpdateEmailVendorTemplate(Editor_contentHeader, Editor_contentFooter);
            if (result)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AlertBox", "alert('EmailVendor Template Updated Successfully');", true);
            }
        }
        protected void grdcustom_material_list_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                string fileName = Convert.ToString(e.CommandArgument);
                if (e.CommandName.Equals("View", StringComparison.InvariantCultureIgnoreCase))
                {
                    string domainName = Request.Url.GetLeftPart(UriPartial.Authority);

                    ClientScript.RegisterClientScriptBlock(Page.GetType(), "Myscript", "<script language='javascript'>window.open('" + domainName + "/CustomerDocs/VendorQuotes/" + fileName + "', null, 'width=487px,height=455px,center=1,resize=0,scrolling=1,location=no');</script>");
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}