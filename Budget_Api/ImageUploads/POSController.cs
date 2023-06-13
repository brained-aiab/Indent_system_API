using BAL;
using DAL;
using ENTITY;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Script.Serialization;
using Razorpay.Api;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation.Results;
using FluentValidation;

namespace POS_API.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*", exposedHeaders: "X-Custom-Header")]
    public class POSController : ApiController
    {
        // GET: POS
        POSEntity objEntity = new POSEntity();
        POSBAL objBAL = new POSBAL();
        POSDAL objDAL = new POSDAL();
        DataSet res = new DataSet();

        public class PathImage
        {
            public string url { get; set; }
        }



        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [Route("AddCategory")]
        public IHttpActionResult AddCategory(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];


            objEntity.Category = Convert.ToString(item["Category"]);
            objEntity.CategoryType = Convert.ToString(item["CategoryType"]);
            objEntity.Sub_Category = Convert.ToString(item["Sub_Category"]);

            if (item["IsParent"].ToString().ToUpper() == "YES")
            {
                objEntity.IsParent = "true";
                objEntity.Category = Convert.ToString(item["Category"]);
                objEntity.CategoryType = Convert.ToString(item["CategoryType"]);
                objEntity.ReferenceID = "0";
            }
            if (item["IsParent"].ToString().ToUpper() == "NO")
            {
                objEntity.IsParent = "false";
                objEntity.Category = Convert.ToString(item["Category"]);
                objEntity.Sub_Category = Convert.ToString(item["Sub_Category"]);
                objEntity.CategoryType = Convert.ToString(item["CategoryType"]);
            }
            //objEntity.CommunityName = Convert.ToString(item["CommunityName"]);
            //objEntity.AiabClient = Convert.ToString(item["AiabClient"]);

            res = objBAL.AddCategory("CREATE", objEntity);
            // objEntity.stauscode ="100";
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {

                data = new
                {

                    message = objEntity.message
                },

            };

            return Ok(response);

        }




        [HttpPost]
        [Authorize]
        //  [AllowAnonymous]
        [Route("EditCategory")]
        public IHttpActionResult EditCategory(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];

            objEntity.CategoryID = Convert.ToString(item["CategoryID"]);
            objEntity.Category = Convert.ToString(item["Category"]);
            objEntity.CategoryType = Convert.ToString(item["CategoryType"]);
            objEntity.Sub_Category = Convert.ToString(item["Sub_Category"]);
            objEntity.IsParent = Convert.ToString(item["IsParent"]);

            if (item["IsParent"].ToString().ToUpper() == "YES")
            {
                objEntity.IsParent = "true";
                objEntity.CategoryID = Convert.ToString(item["CategoryID"]);
                objEntity.Category = Convert.ToString(item["Category"]);
                objEntity.CategoryType = Convert.ToString(item["CategoryType"]);
                objEntity.Sub_Category = Convert.ToString(item["Sub_Category"]);
                objEntity.ReferenceID = "0";
            }
            if (item["IsParent"].ToString().ToUpper() == "NO")
            {
                objEntity.IsParent = "false";
                objEntity.CategoryID = Convert.ToString(item["CategoryID"]);
                objEntity.Category = Convert.ToString(item["Category"]);
                objEntity.CategoryType = Convert.ToString(item["CategoryType"]);
                objEntity.Sub_Category = Convert.ToString(item["Sub_Category"]);
                objEntity.ReferenceID = Convert.ToString(item["ReferenceID"]);
            }
            //objEntity.CommunityName = Convert.ToString(item["CommunityName"]);
            //objEntity.AiabClient = Convert.ToString(item["AiabClient"]);

            res = objBAL.EditCategory("UPDATE", objEntity);

            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {

                data = new
                {
                    messaage = objEntity.message
                },

            };

            return Ok(response);

        }

        [HttpGet]
        [Authorize]
        [AllowAnonymous]
        [Route("GetCategory")]
        public IHttpActionResult Get_Category(string Category_ID, string CategoryType)
        {
            // if ((objEntity.Category_ID == null) &&(objEntity.Category_ID =="") && )
            if (ModelState.IsValid == false)
            {
                var response = "Please Fill All Details";
                return Ok(response);
            }
            else
            {
                DataSet res = new DataSet();
                objEntity.Category_ID = Category_ID;
                objEntity.CategoryType = CategoryType;
                res = objBAL.Get_Category("Get_Category", objEntity);

                objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                if (objEntity.message == "Record Found")
                {
                    var response = new
                    {
                        statuscode = objEntity.stauscode,
                        message = objEntity.message,
                        data = new
                        {

                            Category_Details = res.Tables[1],

                        },
                    };
                    return Ok(response);
                }
                else
                {


                    var response = new
                    {
                        statuscode = objEntity.stauscode,
                        message = objEntity.message,

                    };

                    return Ok(response);
                }


            }

        }

        [HttpGet]
        //  [Authorize]
        [AllowAnonymous]
        [Route("GetAllCategory")]
        public IHttpActionResult Get_All_Category()
        {

            res = objBAL.Get_ALL_Category("Get_All_Category", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    Category_Details = res.Tables[1],

                },
            };
            return Ok(response);
        }



        [HttpGet]
        //[Authorize]
        [AllowAnonymous]
        [Route("GetSubCategory_ByID")]
        public IHttpActionResult GetSubCategory_ByID(string Category_ID)
        {
            DataSet res = new DataSet();
            objEntity.Category_ID = Category_ID;
            //objEntity.AiabClient = AiabClient;

            res = objBAL.Get_SubCategory("GetSubCategory_ByID", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {
                    SubCategory_Details = res.Tables[1],

                },
            };
            return Ok(response);
        }


        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [Route("DeleteCategory")]
        public IHttpActionResult DeleteCategory(JObject objData)
        {
            dynamic item = objData;
           
           try
            {
               

                string data = item["data"];

                objEntity.Category_ID = Convert.ToString(item["CategoryID"]);
                BAL.Utility.ActivityLog("Inside DeleteCategory Category_ID:" + objEntity.Category_ID);
               
                    res = objBAL.DeleteCategory("CREATE", objEntity);
                    BAL.Utility.ActivityLog("Inside DeleteCategory SP:DeleteCategory :" + res);
                    // objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                    BAL.Utility.ActivityLog("Inside DeleteCategory stauscode:" + objEntity.stauscode);
                    objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                    BAL.Utility.ActivityLog("Inside DeleteCategory message:" + objEntity.message);
                    var response = new
                    {

                        data = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message
                        },

                    };

                    return Ok(response);

                
            }
            catch
            {
                return InternalServerError();
            }
        }



        [HttpPost]
        [Authorize]
      [AllowAnonymous]
        [Route("DeleteSubCategory")]
        public IHttpActionResult DeleteSubCategory(JObject objData)
        {
            dynamic item = objData;

            if (objData == null)
            {
                var response = "Parameters Missing";
                return Ok(response);
            }
            else
            {
                string data = item["data"];


                objEntity.Sub_Category_ID = Convert.ToString(item["SUBCategoryID"]);

                res = objBAL.DeleteSubCategory("CREATE", objEntity);
                objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                var response = new
                {

                    data = new
                    {
                        statuscode = objEntity.stauscode,
                        message = objEntity.message
                    },

                };

                return Ok(response);
            }
        }

        [HttpPost]
        [Authorize]
         [AllowAnonymous]
        [Route("AddTags")]
        public IHttpActionResult AddTags(JObject objData)
        {
            dynamic item = objData;
            if (objData == null)
            {
                var response = "Parameters Missing";
                return BadRequest(response);
            }

            if (item["Tag"] == "")
            {
                var response = "Please Fill All Details";
                return Ok(response);
            }
            else
            {
                string data = item["data"];

                objEntity.Tag = Convert.ToString(item["Tag"]);

                res = objBAL.AddTag("CREATE", objEntity);
                objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                var response = new
                {

                    data = new
                    {
                        statuscode = objEntity.stauscode,
                        message = objEntity.message
                    },

                };

                return Ok(response);
            }
        }

        [HttpGet]
        //[Authorize]
         [AllowAnonymous]
        [Route("GetAllTag")]
        public IHttpActionResult Get_All_Tag()
        {
            DataSet res = new DataSet();

            res = objBAL.get_all_tag("get_all_tag", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    Tag_Details = res.Tables[1],

                },
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize]
            [AllowAnonymous]
        [Route("EditTag")]
        public IHttpActionResult EditTag(JObject objData)
        {
           
            dynamic item = objData;
            string data= item["data"];
            if (item["TagId"] == "" || (item["Tag"]) == "")
            {
                var response = "Parameters Missing";
                return BadRequest(response);
            }
          
            if (objData == null)
            {
                var response = "Parameters Missing";
                return BadRequest(response);
            }
           
            else
            {
             
                objEntity.TagID = Convert.ToString(item["TagId"]);
                objEntity.Tag = Convert.ToString(item["Tag"]);

                res = objBAL.EditTag("CREATE", objEntity);
                objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                var response = new
                {

                    data = new
                    {
                        statuscode = objEntity.stauscode,
                        message = objEntity.message
                    },

                };

                return Ok(response);
            }
           
        }

        [HttpPost]
           [AllowAnonymous]
        [Route("DeleteTag")]
        public IHttpActionResult DeleteTag(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];


            objEntity.TagID = Convert.ToString(item["TagId"]);
            BAL.Utility.ActivityLog("Inside DeleteTag TagID:" + objEntity.TagID);

            res = objBAL.DeleteTag("CREATE", objEntity);
            BAL.Utility.ActivityLog("Inside DeleteTag SP:DeleteTag :" + res);
            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            BAL.Utility.ActivityLog("Inside DeleteTag stauscode:" + objEntity.stauscode);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            BAL.Utility.ActivityLog("Inside DeleteTag message:" + objEntity.message);
            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message
                },

            };

            return Ok(response);

        }

        [HttpPost]
        [Authorize]
            [AllowAnonymous]
        [Route("AddDesigner")]
        public async Task<HttpResponseMessage> AddDesigner()
        {

            Dictionary<string, object> dict = new Dictionary<string, object>();


            int i = 0;
            var message1 = "";
            var message2 = "";
            string fileName = string.Empty;
            string savePath = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
            string savePathLocation = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
            string savePathLocationDB = @"~/ImageUploads/";

            var httpRequest = HttpContext.Current.Request;
            objEntity.Brand = httpRequest.Form["Brand"];
            objEntity.BrandAddress = httpRequest.Form["BrandAddress"];
            objEntity.Description = httpRequest.Form["Description"];

            objEntity.Pincode = httpRequest.Form["Pincode"];

            foreach (string file in httpRequest.Files)
            {
                HttpResponseMessage response2 = Request.CreateResponse(HttpStatusCode.Created);


                var postedFile = httpRequest.Files[file];
                //  var postedFile = httpRequest.Files[file]
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    // var postedFile1 = httpRequest.Files.AllKeys[file][1];
                    int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB  

                    IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png", ".jpeg" };
                    var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                    var extension = ext.ToLower();
                    if (!AllowedFileExtensions.Contains(extension))
                    {

                        var message = string.Format("Please Upload image of type .jpg,.gif,.png,.jpeg");

                        dict.Add("error", message);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                    }
                    else if (postedFile.ContentLength > MaxContentLength)
                    {

                        var message = string.Format("Please Upload a file upto 1 mb.");

                        dict.Add("error", message);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                    }
                    else
                    {
                        var filePath = HttpContext.Current.Server.MapPath("~/ImageUploads/" + postedFile.FileName);

                        switch (i)
                        {

                            case 0:
                                var postedFile1 = httpRequest.Files.AllKeys[i];
                                var postedFile2 = httpRequest.Files[postedFile1];
                                objEntity.CoverPicture = (savePathLocationDB + postedFile2.FileName.Replace(" ", "_").Replace("%", "_percentage_").Replace("&", "_and_"));
                                res = objBAL.AddDesigner("CREATE", objEntity);
                                objEntity.Brand = res.Tables[0].Rows[0]["DesignerId"].ToString();
                                postedFile2.SaveAs(filePath);
                                break;


                            case 1:
                                var postedFile3 = httpRequest.Files.AllKeys[i];
                                var postedFile4 = httpRequest.Files[postedFile3];
                                objEntity.ProfilePicture = (savePathLocationDB + postedFile4.FileName.Replace(" ", "_").Replace("%", "_percentage_").Replace("&", "_and_"));
                                // objEntity.Brand = httpRequest.Form["Brand"];
                                res = objBAL.AddDesigner("GET_UPDATEPROFILEPIC", objEntity);
                                fileName = string.Empty;
                                postedFile4.SaveAs(filePath);
                                break;


                        }
                        i++;

                    }
                }
                else
                {
                    var res = string.Format("Please Upload a image.");
                    dict.Add("error", res);
                    return Request.CreateResponse(HttpStatusCode.NotFound, dict);
                }

            }
            DataSet res_designer = new DataSet();
            res_designer = objBAL.AddDesigner("GET_DESIGNERDATA", objEntity);
            //Dictionary<string, object> AddOrderEntity = new Dictionary<string, object>();
            //Dictionary<string, object> addressDict = new Dictionary<string, object>();
            //addressDict.Add("ID (phone number)", res_designer.Tables[0].Rows[0]["ID"].ToString());
            //BAL.Utility.ActivityLog("ID (phone number):" + objEntity.message);
            //addressDict.Add("Name", Convert.ToString(res_designer.Tables[0].Rows[0]["Name"].ToString()));
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  message:" + objEntity.message);

            //List<PathImage> Imageurl = new List<PathImage>();

            //if (res_designer != null && res_designer.Tables.Count > 0 && res_designer.Tables[0].Rows.Count > 0)
            //{

            //    foreach (DataRow item1 in res_designer.Tables[0].Rows)
            //    {
            //        Imageurl.Add(new PathImage
            //        {
            //            url = item1["url"].ToString()
            //        });
            //    }
            //}
            //addressDict.Add("Pic", new ArrayList(Imageurl));
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  message:" + new ArrayList(Imageurl));
            //addressDict.Add("Address", res_designer.Tables[0].Rows[0]["Address"].ToString());
            //addressDict.Add("pincode", res_designer.Tables[0].Rows[0]["pincode"].ToString());
            //ArrayList myArrayList = new ArrayList();
            //myArrayList.Add(AddOrderEntity);
            //AddOrderEntity.Add("fields", addressDict);
            //Dictionary<string, ArrayList> MainJson = new Dictionary<string, ArrayList>();
            //MainJson["records"] = myArrayList;
            //var json = new JavaScriptSerializer().Serialize(MainJson);
            //BAL.Utility.ActivityLog("Inside AddProductSKU Api  message:" + json);
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //var client = new RestSharp.RestClient("https://api.airtable.com/v0/appem7EqUkOGDItmS/Designers");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.POST);
            //BAL.Utility.ActivityLog("Airtable Request:" + request);
            //request.AddHeader("Content-Type", "application/json");
            //BAL.Utility.ActivityLog("Airtable Request: request1");
            //request.AddParameter("application/json", json, ParameterType.RequestBody);
            //BAL.Utility.ActivityLog("Airtable Request: request2");
            //request.AddHeader("Authorization", "Bearer keycvDlbZJvfafDnj");
            //BAL.Utility.ActivityLog("Airtable Request: request3");
            //IRestResponse response1 = client.Execute(request);
            //var content = JObject.Parse(response1.Content);
            //message2 = content["records"][0]["id"].ToString();
            //BAL.Utility.ActivityLog("Airtable Request: message" + message2);

            //objEntity.Description = message2;
            //res = objBAL.AddDesigner("INSERT_UPDATEDESIGNERID", objEntity);
            //BAL.Utility.ActivityLog("Airtable Designer table response:" + content);
            message1 = string.Format("Image Updated Successfully.");

            return Request.CreateErrorResponse(HttpStatusCode.Created, message1);


        }

        [HttpGet]
         [AllowAnonymous]
        [Route("GetDesigner")]
        public IHttpActionResult GetDesigner()
        {


            res = objBAL.AddDesigner("GET_DESIGNER", objEntity);
            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"].ToString());
            objEntity.message = res.Tables[0].Rows[0]["Message"].ToString();
            var response = new
            {

                data = new
                {
                    stauscode = objEntity.stauscode,
                    message = objEntity.message,
                    Designer = res.Tables[1],
                },

            };
            return Ok(response);
        }



        [HttpPost]
          [Route("EditDesigner")]
        [Authorize]
             [AllowAnonymous]

        public async Task<HttpResponseMessage> EditDesigner()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {
                DataSet dres = new DataSet();

                //dynamic item = objData;
                //string data = item["data"];

                var httpRequest = HttpContext.Current.Request;
                objEntity.BrandID = httpRequest.Form["BrandID"];

                objEntity.Brand = httpRequest.Form["Brand"];
                objEntity.BrandAddress = httpRequest.Form["BrandAddress"];
                objEntity.Description = httpRequest.Form["Description"];
                objEntity.Pincode = httpRequest.Form["BrandPincode"];
                foreach (string file in httpRequest.Files)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {

                        int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB  

                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png", ".jpeg" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();
                        if (!AllowedFileExtensions.Contains(extension))
                        {

                            var message = string.Format("Please Upload image of type .jpg,.gif,.png.");

                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else if (postedFile.ContentLength > MaxContentLength)
                        {

                            var message = string.Format("Please Upload a file upto 1 mb.");

                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else
                        {
                            string savePath = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
                            string savePathLocation = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
                            string savePathLocationDB = @"~/ImageUploads/";
                            var filePath = HttpContext.Current.Server.MapPath("/ImageUploads/" + postedFile.FileName);
                            objEntity.CoverPicture = (savePathLocationDB + postedFile.FileName.Replace(" ", "_").Replace("%", "_percentage_").Replace("&", "_and_"));
                            dres = objBAL.EditDesigner("CREATE", objEntity);

                            postedFile.SaveAs(filePath);

                        }
                    }

                    var message1 = string.Format("Image Updated Successfully.");
                    return Request.CreateErrorResponse(HttpStatusCode.Created, message1); ;
                }
                var res = string.Format("Please Upload a image.");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        [HttpPost]
        [Authorize]
         [AllowAnonymous]
        [Route("DeleteDesigner")]
        public IHttpActionResult DeleteDesigner(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];


            objEntity.BrandID = Convert.ToString(item["DesignerID"]);


            res = objBAL.DeleteDesigner("CREATE", objEntity);
            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message
                },

            };

            return Ok(response);

        }

        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [Route("GetDetailsByBrandId")]
        public IHttpActionResult GetDetailsByBrandId(string BRAND)
        {
            if (BRAND == null)
            {
                var response = "Parameters Missing";
                return BadRequest(response);
            }
            else
            {
                objEntity.Brand = BRAND;
                res = objBAL.AddDesigner("GET_DESIGNERBYID", objEntity);
                objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"].ToString());
                objEntity.message = res.Tables[0].Rows[0]["Message"].ToString();
                var response = new
                {

                    data = new
                    {
                        stauscode = objEntity.stauscode,
                        message = objEntity.message,
                        Designer = res.Tables[1],
                        Product = res.Tables[2],
                    },

                };
                return Ok(response);
            }
        }


        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [Route("AddProduct")]
        public IHttpActionResult AddProduct(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];
            BAL.Utility.ActivityLog("Inside AddProduct api : ");
            objEntity.Product_ID = Convert.ToString(item["Product_ID"]);
            BAL.Utility.ActivityLog("ProductID:" + objEntity.Product_ID);
            objEntity.Name = Convert.ToString(item["Name"]);
            BAL.Utility.ActivityLog("Name : " + objEntity.Name);
            objEntity.Category = Convert.ToString(item["Category"]);
            BAL.Utility.ActivityLog("Category : " + objEntity.Category);
            objEntity.Sub_Category = Convert.ToString(item["Sub_Category"]);
            BAL.Utility.ActivityLog("Sub_Category : " + objEntity.Sub_Category);
            objEntity.Tag = Convert.ToString(item["Tag"]);
            BAL.Utility.ActivityLog("Tag : " + objEntity.Tag);
            objEntity.Brand = Convert.ToString(item["Brand"]);
            BAL.Utility.ActivityLog("Brand : " + objEntity.Brand);
            objEntity.ProductDescription = Convert.ToString(item["ProductDescription"]);
            BAL.Utility.ActivityLog("ProductDescription : " + objEntity.ProductDescription);
            objEntity.ThirdTextDescription = Convert.ToString(item["ThirdTextDescription"]);
            BAL.Utility.ActivityLog("ThirdTextDescription : " + objEntity.ThirdTextDescription);
            objEntity.ingredients = Convert.ToString(item["ingredients"]);
            BAL.Utility.ActivityLog("ingredients : " + objEntity.ingredients);
            objEntity.Name_Reviews = Convert.ToString(item["Name_Reviews"]);
            BAL.Utility.ActivityLog("Name_Reviews : " + objEntity.Name_Reviews);
            objEntity.Your_Reviews = Convert.ToString(item["Your_Reviews"]);
            BAL.Utility.ActivityLog("Your_Reviews : " + objEntity.Your_Reviews);
            objEntity.Rating = Convert.ToString(item["Rating"]);
            BAL.Utility.ActivityLog("Rating : " + objEntity.Rating);
            objEntity.VerificationCode_Reviews = Convert.ToString(item["VerificationCode_Reviews"]);
            BAL.Utility.ActivityLog("VerificationCode_Reviews : " + objEntity.VerificationCode_Reviews);
            objEntity.MetaTital = Convert.ToString(item["MetaTital"]);
            BAL.Utility.ActivityLog("MetaTital : " + objEntity.Product_ID);
            objEntity.MetaKeyword = Convert.ToString(item["MetaKeyword"]);
            BAL.Utility.ActivityLog("MetaKeyword : " + objEntity.MetaKeyword);
            objEntity.MetaDescription = Convert.ToString(item["MetaDescription"]);
            BAL.Utility.ActivityLog("MetaDescription : " + objEntity.MetaDescription);
            objEntity.RelatedProduct = Convert.ToString(item["RelatedProduct"]);
            BAL.Utility.ActivityLog("RelatedProduct : " + objEntity.RelatedProduct);
            objEntity.feedingGuide = Convert.ToString(item["feedingGuide"]);
            BAL.Utility.ActivityLog("feedingGuide : " + objEntity.feedingGuide);
            objEntity.Analysis = Convert.ToString(item["Analysis"]);
            BAL.Utility.ActivityLog("Analysis : " + objEntity.Analysis);
            objEntity.material = Convert.ToString(item["material"]);
            BAL.Utility.ActivityLog("material : " + objEntity.material);
            objEntity.usage = Convert.ToString(item["usage"]);
            BAL.Utility.ActivityLog("usage : " + objEntity.usage);
            objEntity.HsnCode = Convert.ToString(item["HsnCode"]);
            BAL.Utility.ActivityLog("HsnCode : " + objEntity.HsnCode);
            objEntity.Country_Of_Origin = Convert.ToString(item["Country_Of_Origin"]);
            BAL.Utility.ActivityLog("Country_Of_Origin : " + objEntity.Country_Of_Origin);
            objEntity.YouTubeUrl = Convert.ToString(item["YouTubeUrl"]);
            BAL.Utility.ActivityLog("YouTubeUrl : " + objEntity.YouTubeUrl);
            objEntity.GSTValue = Convert.ToString(item["GSTValue"]);
            BAL.Utility.ActivityLog("GSTValue : " + objEntity.GSTValue);
            res = objBAL.AddProduct("CREATE", objEntity);


            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

            if (objEntity.stauscode == 102)
            {
                var response = new
                {

                    data = new
                    {
                        statuscode = objEntity.stauscode,
                        message = res.Tables[0].Rows[0]["message"].ToString(),
            },

                };

                return Ok(response);
            }
            else
            {
                var response = new
                {

                    data = new
                    {
                        statuscode = objEntity.stauscode,
                        message = "Please Insert All the Details"
                    },

                };

                return Ok(response);
            }

        }

        [HttpPost]
        [Authorize]
            [AllowAnonymous]
        [Route("Editproduct")]
        public IHttpActionResult Editproduct(JObject objData)
        {

            dynamic item = objData;
            string data = item["data"];
            BAL.Utility.ActivityLog("Inside Edit api : ");
            objEntity.Product_ID = Convert.ToString(item["Product_ID"]);
            BAL.Utility.ActivityLog("Product_ID:" + objEntity.Product_ID);
            objEntity.Name = Convert.ToString(item["Name"]);
            BAL.Utility.ActivityLog("Name : " + objEntity.Name);
            objEntity.Category = Convert.ToString(item["Category"]);
            BAL.Utility.ActivityLog("Category : " + objEntity.Category);
            objEntity.Sub_Category = Convert.ToString(item["Sub_Category"]);
            BAL.Utility.ActivityLog("Sub_Category : " + objEntity.Sub_Category);
            objEntity.Tag = Convert.ToString(item["Tag"]);
            BAL.Utility.ActivityLog("Tag : " + objEntity.Tag);
            objEntity.Brand = Convert.ToString(item["Brand"]);
            BAL.Utility.ActivityLog("Brand : " + objEntity.Brand);
            objEntity.ProductDescription = Convert.ToString(item["ProductDescription"]);
            BAL.Utility.ActivityLog("ProductDescription : " + objEntity.ProductDescription);
            objEntity.ThirdTextDescription = Convert.ToString(item["ThirdTextDescription"]);
            BAL.Utility.ActivityLog("ThirdTextDescription : " + objEntity.ThirdTextDescription);
            objEntity.ingredients = Convert.ToString(item["ingredients"]);
            BAL.Utility.ActivityLog("ingredients : " + objEntity.ingredients);
            objEntity.Name_Reviews = Convert.ToString(item["Name_Reviews"]);
            BAL.Utility.ActivityLog("Name_Reviews : " + objEntity.Name_Reviews);
            objEntity.Your_Reviews = Convert.ToString(item["Your_Reviews"]);
            BAL.Utility.ActivityLog("Your_Reviews : " + objEntity.Your_Reviews);
            objEntity.Rating = Convert.ToString(item["Rating"]);
            BAL.Utility.ActivityLog("Rating : " + objEntity.Rating);
            objEntity.VerificationCode_Reviews = Convert.ToString(item["VerificationCode_Reviews"]);
            BAL.Utility.ActivityLog("VerificationCode_Reviews : " + objEntity.VerificationCode_Reviews);
            objEntity.MetaTital = Convert.ToString(item["MetaTital"]);
            BAL.Utility.ActivityLog("MetaTital : " + objEntity.Product_ID);
            objEntity.MetaKeyword = Convert.ToString(item["MetaKeyword"]);
            BAL.Utility.ActivityLog("MetaKeyword : " + objEntity.MetaKeyword);
            objEntity.MetaDescription = Convert.ToString(item["MetaDescription"]);
            BAL.Utility.ActivityLog("MetaDescription : " + objEntity.MetaDescription);
            objEntity.RelatedProduct = Convert.ToString(item["RelatedProduct"]);
            BAL.Utility.ActivityLog("RelatedProduct : " + objEntity.RelatedProduct);
            objEntity.feedingGuide = Convert.ToString(item["feedingGuide"]);
            BAL.Utility.ActivityLog("feedingGuide : " + objEntity.feedingGuide);
            objEntity.Analysis = Convert.ToString(item["Analysis"]);
            BAL.Utility.ActivityLog("Analysis : " + objEntity.Analysis);
            objEntity.material = Convert.ToString(item["material"]);
            BAL.Utility.ActivityLog("material : " + objEntity.material);
            objEntity.usage = Convert.ToString(item["usage"]);
            BAL.Utility.ActivityLog("usage : " + objEntity.usage);
            objEntity.HsnCode = Convert.ToString(item["HsnCode"]);
            BAL.Utility.ActivityLog("HsnCode : " + objEntity.HsnCode);
            objEntity.Country_Of_Origin = Convert.ToString(item["Country_Of_Origin"]);
            BAL.Utility.ActivityLog("Country_Of_Origin : " + objEntity.Country_Of_Origin);
            objEntity.YouTubeUrl = Convert.ToString(item["YouTubeUrl"]);
            BAL.Utility.ActivityLog("YouTubeUrl : " + objEntity.YouTubeUrl);
            objEntity.GSTValue = Convert.ToString(item["GSTValue"]);
            BAL.Utility.ActivityLog("GSTValue : " + objEntity.GSTValue);
            //   objEntity.Tagname = Convert.ToString(item["Tagname"]);

            res = objBAL.Editproduct("Update", objEntity);

            //////Dictionary<string, object> AddOrderEntity = new Dictionary<string, object>();


            //////Dictionary<string, object> addressDict = new Dictionary<string, object>();
            //////addressDict.Add("Product ID", Convert.ToString(objEntity.Product_ID));
            //////addressDict.Add("Product description", Convert.ToString(objEntity.ProductDescription));
            ////////addressDict.Add("Attachments", new string[] { " " });
            //////// addressDict.Add("Youtube link", "");
            ////////addressDict.Add("Designer", new string[] { " " });
            //////// addressDict.Add("Price", "");
            //////addressDict.Add("Live", "Live");
            //////// addressDict.Add("Category", objEntity.Category);
            //////addressDict.Add("Product name", objEntity.Name);
            //////addressDict.Add("Country of origin", objEntity.Country_Of_Origin);
            //////addressDict.Add("Stock", "Instock");
            ////////addressDict.Add("Product is in the cart of users", new string[] { " " });
            ////////addressDict.Add("Orders for this product", new string[] { " " });
            //////addressDict.Add("Approval status", "Approved");
            //////addressDict.Add("Product completion percent", 0.01);
            //////// addressDict.Add("Tags", new string[] { " " });

            //////ArrayList myArrayList = new ArrayList();
            //////myArrayList.Add(AddOrderEntity);
            //////AddOrderEntity.Add("fields", addressDict);
            //////Dictionary<string, ArrayList> MainJson = new Dictionary<string, ArrayList>();
            //////MainJson["records"] = myArrayList;
            //////var json = new JavaScriptSerializer().Serialize(MainJson);

            //////ServicePointManager.Expect100Continue = true;
            //////ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //////var client = new RestClient("https://api.airtable.com/v0/appem7EqUkOGDItmS/Product");
            //////client.Timeout = -1;
            //////var request = new RestRequest(Method.POST);
            //////request.AddHeader("Content-Type", "application/json");
            //////request.AddParameter("application/json", json, ParameterType.RequestBody);
            //////request.AddHeader("Authorization", "Bearer keycvDlbZJvfafDnj");
            //////IRestResponse response1 = client.Execute(request);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {

                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {
                    OrderDetails = res.Tables[1],
                },
            };
            return Ok(response);
        }




        [HttpPost]
        [Route("AddProductAttribute")]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> PostUserImage()
        {

            BAL.Utility.ActivityLog("ActivityLog : Log created on - Inside AddProductAttribute : " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"));
           BAL.Utility.ActivityLog("Inside AddProductAttribute : Exception " );
      
            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {
                DataSet dres = new DataSet();
                int i = 0;
                int fileCount;
                //  var postedFile;
                string fileName = string.Empty;
                string savePath = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
                string savePathLocation = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
                string savePathLocationDB = @"~/ImageUploads/";

                var httpRequest = HttpContext.Current.Request;
                objEntity.Product_ID = httpRequest.Form["product_id"];
                objEntity.Color = httpRequest.Form["Color"];
                if (objEntity.Color != null && objEntity.Color != "")
                {
                    objEntity.Color = httpRequest.Form["Color"];
                    BAL.Utility.ActivityLog("Inside AddProductAttribute : Color " + objEntity.Color);
                }
                else
                {
                    objEntity.Color = "Standard";
                    BAL.Utility.ActivityLog("Inside AddProductAttribute : Color "+ objEntity.Color);
                }
                foreach (string file in httpRequest.Files)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);


                    var postedFile = httpRequest.Files[file];
                    //  var postedFile = httpRequest.Files[file]
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        // var postedFile1 = httpRequest.Files.AllKeys[file][1];
                        int MaxContentLength = 1024 * 1024 * 5; //Size = 5 MB  

                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png", ".jpeg" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();
                        if (!AllowedFileExtensions.Contains(extension))
                        {

                            var message = string.Format("Please Upload image of type .jpg,.gif,.png,.jpeg");
                            BAL.Utility.ActivityLog("Inside AddProductAttribute : Color " + message);
                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else if (postedFile.ContentLength > MaxContentLength)
                        {

                            var message = string.Format("Please Upload a file upto 5 mb.");
                            BAL.Utility.ActivityLog("Inside AddProductAttribute : max length" + message);
                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else
                        {
                            var filePath = HttpContext.Current.Server.MapPath("~/ImageUploads/" + postedFile.FileName);

                            switch (i)
                            {

                                case 0:
                                    var postedFile1 = httpRequest.Files.AllKeys[i];
                                    var postedFile2 = httpRequest.Files[postedFile1];
                                    objEntity.ProductImage = (savePathLocationDB + postedFile2.FileName.Replace(" ", " ").Replace("%", "%").Replace("&", "&"));
                                    dres = objBAL.ProductColor("CREATE_TEST", objEntity);
                                    postedFile2.SaveAs(filePath);
                                    break;
                                case 1:
                                    var postedFile3 = httpRequest.Files.AllKeys[i];
                                    var postedFile4 = httpRequest.Files[postedFile3];
                                    objEntity.PathImage1 = (savePathLocationDB + postedFile4.FileName.Replace(" ", " ").Replace("%", "%").Replace("&", "&"));
                                    BAL.Utility.ActivityLog("Inside AddProductAttribute : case 1 brfore " );
                                    res = objBAL.ProductColorImage("GET_ProductColorView", objEntity);
                                    BAL.Utility.ActivityLog("Inside AddProductAttribute : case 1 After res" + res);
                                    objEntity.ID = res.Tables[0].Rows[0]["ID"].ToString();
                                    objEntity.Product_ID = res.Tables[0].Rows[0]["ProductID"].ToString();
                                    res = objBAL.ProductColorImage("UPDATE", objEntity);
                                    BAL.Utility.ActivityLog("Inside AddProductAttribute : 1109 " + res);
                                    fileName = string.Empty;
                                    postedFile4.SaveAs(filePath);
                                    break;
                                case 2:
                                    var postedFile5 = httpRequest.Files.AllKeys[i];
                                    var postedFile6 = httpRequest.Files[postedFile5];
                                    objEntity.PathImage2 = (savePathLocationDB + postedFile6.FileName.Replace(" ", " ").Replace("%", "%").Replace("&", "&"));
                                    BAL.Utility.ActivityLog("Inside AddProductAttribute case 2 1117:");
                                    res = objBAL.ProductColorImage("GET_ProductColorView", objEntity);
                                    BAL.Utility.ActivityLog("Inside AddProductAttribute :1119"+res);
                                    objEntity.ID = res.Tables[0].Rows[0]["ID"].ToString();
                                    objEntity.Product_ID = res.Tables[0].Rows[0]["ProductID"].ToString();
                                    BAL.Utility.ActivityLog("Inside AddProductAttribute 1122:");
                                    res = objBAL.ProductColorImage("UPDATE", objEntity);
                                    BAL.Utility.ActivityLog("Inside AddProductAttribute : 1124" + res);
                                    fileName = string.Empty;
                                    postedFile6.SaveAs(filePath);
                                    break;
                                case 3:
                                    BAL.Utility.ActivityLog("Inside AddProductAttribute case 3 start line 1131:");
                                    var postedFile7 = httpRequest.Files.AllKeys[i];
                                    var postedFile8 = httpRequest.Files[postedFile7];
                                    objEntity.PathImage3 = (savePathLocationDB + postedFile8.FileName.Replace(" ", " ").Replace("%", "%").Replace("&", "&"));
                                    res = objBAL.ProductColorImage("GET_ProductColorView", objEntity);
                                    objEntity.ID = res.Tables[0].Rows[0]["ID"].ToString();
                                    objEntity.Product_ID = res.Tables[0].Rows[0]["ProductID"].ToString();
                                    res = objBAL.ProductColorImage("UPDATE", objEntity);
                                    fileName = string.Empty;
                                    postedFile8.SaveAs(filePath);
                                    BAL.Utility.ActivityLog("Inside AddProductAttribute case 3 end line 1131:");
                                    break;
                                case 4:
                                    BAL.Utility.ActivityLog("Inside AddProductAttribute case 4 start line 1131:");
                                    var postedFile9 = httpRequest.Files.AllKeys[i];
                                    var postedFile10 = httpRequest.Files[postedFile9];
                                    objEntity.PathImage4 = (savePathLocationDB + postedFile10.FileName.Replace(" ", "").Replace("%", "%").Replace("&", "&"));
                                    res = objBAL.ProductColorImage("GET_ProductColorView", objEntity);
                                    objEntity.ID = res.Tables[0].Rows[0]["ID"].ToString();
                                    objEntity.Product_ID = res.Tables[0].Rows[0]["ProductID"].ToString();
                                    res = objBAL.ProductColorImage("UPDATE", objEntity);
                                    fileName = string.Empty;
                                    postedFile10.SaveAs(filePath);
                                    BAL.Utility.ActivityLog("Inside AddProductAttribute case 4 end line 1131:");
                                    break;

                            }
                            i++;

                        }
                    }
                    else
                    {
                        var res = string.Format("Please Upload a image.");
                        BAL.Utility.ActivityLog("Inside AddProductAttribute:"+res);
                        dict.Add("error", res);
                        return Request.CreateResponse(HttpStatusCode.NotFound, dict);
                    }

                }

                var message1 = "";
                if (dres != null && dres.Tables.Count > 0 && dres.Tables[0].Rows.Count > 0)
                {
                    message1 = dres.Tables[0].Rows[0]["message"].ToString();
                    BAL.Utility.ActivityLog("Inside AddProductAttribute : Color " + message1);
                }
                else
                {
                    message1 = string.Format("Image Updated Successfully.");
                    BAL.Utility.ActivityLog("Inside AddProductAttribute : Color " + message1);
                }
                return Request.CreateErrorResponse(HttpStatusCode.Created, message1);
            }
            catch (Exception ex)
            {
                //var res = string.Format("some Message");
                //dict.Add("error", res);
                //return Request.CreateResponse(HttpStatusCode.NotFound, dict);
                throw ex;
            }
        }


        [HttpPost]
        [Route("EditProductAttribute")]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> EditProductAttribute()
        {
            BAL.Utility.ActivityLog("ActivityLog : Log created on - Inside EditProductAttribute : " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"));
            BAL.Utility.ActivityLog("Inside EditProductAttribute : Exception ");

            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {
                DataSet dres = new DataSet();

                var httpRequest = HttpContext.Current.Request;
                objEntity.AttributeID = httpRequest.Form["Attributeid"];
                objEntity.Color = httpRequest.Form["Color"];
                foreach (string file in httpRequest.Files)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {

                        int MaxContentLength = 1024 * 1024 * 5; //Size = 5 MB  

                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png", ".jpeg" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();
                        if (!AllowedFileExtensions.Contains(extension))
                        {

                            var message = string.Format("Please Upload image of type .jpg,.gif,.png.");

                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else if (postedFile.ContentLength > MaxContentLength)
                        {

                            var message = string.Format("Please Upload a file upto 5 mb.");

                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else
                        {
                            string savePath = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
                            string savePathLocation = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
                            string savePathLocationDB = @"~/ImageUploads/";
                            var filePath = HttpContext.Current.Server.MapPath("/ImageUploads/" + postedFile.FileName);
                            objEntity.ProductImage = (savePathLocationDB + postedFile.FileName.Replace(" ", " ").Replace("%", "%").Replace("&", "&"));
                            dres = objBAL.ProductColor("PROC_EDIT_PRODUCTIMAGE", objEntity);

                            postedFile.SaveAs(filePath);

                        }
                    }

                    var message1 = string.Format("Image Updated Successfully.");
                    return Request.CreateErrorResponse(HttpStatusCode.Created, message1); ;
                }
                var res = string.Format("Please Upload a image.");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
            catch (Exception ex)
            {
                //  var res = 
                throw ex;
                //     dict.Add("error", res);
                //     return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
        }



        [HttpPost]
        [Authorize]
           [AllowAnonymous]
        [Route("DeleteAttribute")]
        public IHttpActionResult DeleteAttribute(JObject objData)
        {
            BAL.Utility.ActivityLog("Inside DeleteAttribute:");
            dynamic item = objData;
            string data = item["data"];
            objEntity.ID = Convert.ToString(item["ID"]);
            BAL.Utility.ActivityLog("Inside DeleteAttribute ID:" + objEntity.ID);
            res = objBAL.DeleteAttribute("CREATE", objEntity);
            BAL.Utility.ActivityLog("Inside DeleteAttribute SP:DeleteAttribute" + res);
            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            BAL.Utility.ActivityLog("Inside DeleteAttribute stauscode:" + objEntity.stauscode);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            BAL.Utility.ActivityLog("Inside DeleteAttribute message:" + objEntity.message);

            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message
                },

            };

            return Ok(response);

        }

        [HttpPost]
        [Authorize]
       [AllowAnonymous]
        [Route("AddProductSKU")]
        public IHttpActionResult AddProductSKU(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];
            var message = "";
            objEntity.Product_ID = Convert.ToString(item["Product_ID"]);
            BAL.Utility.ActivityLog("Inside AddProductSKU Api Product_ID:" + objEntity.Product_ID);
            objEntity.SKU = Convert.ToString(item["SKU"]);
            BAL.Utility.ActivityLog("Inside AddProductSKU Api SKU:" + objEntity.SKU);
            //objEntity.Amazon_SKU = Convert.ToString(item["Amazon_SKU"]);
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  Amazon_SKU:" + objEntity.Amazon_SKU);
            //objEntity.SKUCODE = Convert.ToString(item["SKUCODE"]);
            //BAL.Utility.ActivityLog("Inside Orderhistory Api SKUCODE:" + objEntity.SKUCODE);
            objEntity.Quantity = Convert.ToString(item["Quantity"]);
            BAL.Utility.ActivityLog("nside AddProductSKU Api  Quantity:" + objEntity.Quantity);
            objEntity.Price = Convert.ToString(item["Price"]);
            BAL.Utility.ActivityLog("nside AddProductSKU Api  Price:" + objEntity.Price);
            //objEntity.StrikePrice = Convert.ToString(item["StrikePrice"]);
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  StrikePrice:" + objEntity.StrikePrice);
            objEntity.AttributeID = Convert.ToString(item["AttributeID"]);
            BAL.Utility.ActivityLog("nside AddProductSKU Api  AttributeID:" + objEntity.AttributeID);
            objEntity.Size = Convert.ToString(item["Size"]);
            BAL.Utility.ActivityLog("nside AddProductSKU Api Size:" + objEntity.Size);
            //objEntity.StockStatus = Convert.ToString(item["Stockstatus"]);
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  StockStatus:" + objEntity.StockStatus);
            res = objBAL.AddProductSKU("CREATE", objEntity);
            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            BAL.Utility.ActivityLog("nside AddProductSKU Api  stauscode:" + objEntity.stauscode);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            BAL.Utility.ActivityLog("nside AddProductSKU Api  message:" + objEntity.message);

            //Dictionary<string, object> AddOrderEntity = new Dictionary<string, object>();
            //Dictionary<string, object> addressDict = new Dictionary<string, object>();
            //addressDict.Add("Product ID", Convert.ToString(res.Tables[1].Rows[0]["Product_ID"].ToString()));
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  message:" + objEntity.message);
            //addressDict.Add("Product description", Convert.ToString(res.Tables[1].Rows[0]["ProductDescription"].ToString()));
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  message:" + objEntity.message);

            //List<PathImage> Imageurl = new List<PathImage>();

            //if (res != null && res.Tables.Count > 0 && res.Tables[1].Rows.Count > 0)
            //{

            //    foreach (DataRow item1 in res.Tables[1].Rows)
            //    {
            //        Imageurl.Add(new PathImage
            //        {
            //            url = item1["PathImage"].ToString()
            //        });
            //    }
            //}
            //addressDict.Add("Attachments", new ArrayList(Imageurl));
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  message:" + new ArrayList(Imageurl));
            //addressDict.Add("Price", Convert.ToInt32(res.Tables[1].Rows[0]["Price"].ToString()));
            //addressDict.Add("Strikeout Price", Convert.ToInt32(res.Tables[1].Rows[0]["Strikeprice"].ToString()));
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  message:" + objEntity.message);
            //addressDict.Add("Designer", Convert.ToString(res.Tables[1].Rows[0]["Brand"].ToString()));
            //addressDict.Add("Live", Convert.ToString(res.Tables[1].Rows[0]["IsPublished"].ToString()));
            //addressDict.Add("Product name", res.Tables[1].Rows[0]["Name"].ToString());
            //addressDict.Add("Youtube link", Convert.ToString(res.Tables[1].Rows[0]["YoutubeUrl"].ToString()));
            //addressDict.Add("Country of origin", res.Tables[1].Rows[0]["COUNTRY"].ToString());
            //addressDict.Add("Stock_Status", Convert.ToString(res.Tables[1].Rows[0]["StockStatus"].ToString()));
            //addressDict.Add("Category", res.Tables[1].Rows[0]["Category"].ToString());
            //addressDict.Add("SubCategory", Convert.ToString(res.Tables[1].Rows[0]["Sub_Category"].ToString()));
            //addressDict.Add("Tags", Convert.ToString(res.Tables[1].Rows[0]["Tags"].ToString()));
            //addressDict.Add("Approval status", "Approved");


            //ArrayList myArrayList = new ArrayList();
            //myArrayList.Add(AddOrderEntity);
            //AddOrderEntity.Add("fields", addressDict);
            //Dictionary<string, ArrayList> MainJson = new Dictionary<string, ArrayList>();
            //MainJson["records"] = myArrayList;
            //var json = new JavaScriptSerializer().Serialize(MainJson);
            //BAL.Utility.ActivityLog("Inside AddProductSKU Api  message:" + json);
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //var client = new RestSharp.RestClient("https://api.airtable.com/v0/appem7EqUkOGDItmS/Product");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.POST);
            //BAL.Utility.ActivityLog("Airtable Request:" + request);
            //request.AddHeader("Content-Type", "application/json");
            //BAL.Utility.ActivityLog("Airtable Request: request1");
            //request.AddParameter("application/json", json, ParameterType.RequestBody);
            //BAL.Utility.ActivityLog("Airtable Request: request2");
            //request.AddHeader("Authorization", "Bearer keycvDlbZJvfafDnj");
            //BAL.Utility.ActivityLog("Airtable Request: request3");
            //IRestResponse response1 = client.Execute(request);
            //var content = JObject.Parse(response1.Content);
            //BAL.Utility.ActivityLog("Airtable Request: request4");
            ////message = content["records"][0]["id"].ToString();
            //BAL.Utility.ActivityLog("Airtable Request: request5");
            ////BAL.Utility.ActivityLog("Airtable Responseabcde:" + message);
            //BAL.Utility.ActivityLog("Inside AddProductSKU Api Airtable message:" + content);
            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message
                },

            };

            return Ok(response);

        }

        [HttpPost]
        [Authorize]
         [AllowAnonymous]
        [Route("EditProductSKU")]
        public IHttpActionResult EditProductSKU(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];
            var message = "";
            objEntity.Product_ID = Convert.ToString(item["Product_ID"]);
            BAL.Utility.ActivityLog("Inside EditProductSKU Api Product_ID:" + objEntity.Product_ID);
            objEntity.SKUID = Convert.ToString(item["SKUID"]);
            BAL.Utility.ActivityLog("Inside EditProductSKU Api SKUID:" + objEntity.SKUID);
            objEntity.SKU = Convert.ToString(item["SKU"]);
            BAL.Utility.ActivityLog("Inside EditProductSKU Api SKU:" + objEntity.SKU);
            objEntity.Amazon_SKU = Convert.ToString(item["Amazon_SKU"]);
            BAL.Utility.ActivityLog("nside EditProductSKU Api  Amazon_SKU:" + objEntity.Amazon_SKU);
            objEntity.SKUCODE = Convert.ToString(item["SKUCODE"]);
            BAL.Utility.ActivityLog("Inside EditProductSKU Api SKUCODE:" + objEntity.SKUCODE);
            objEntity.Quantity = Convert.ToString(item["Quantity"]);
            BAL.Utility.ActivityLog("nside EditProductSKU Api  Quantity:" + objEntity.Quantity);
            objEntity.Price = Convert.ToString(item["Price"]);
            BAL.Utility.ActivityLog("nside EditProductSKU Api  Price:" + objEntity.Price);
            //objEntity.StrikePrice = Convert.ToString(item["StrikePrice"]);
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  StrikePrice:" + objEntity.StrikePrice);
            objEntity.AttributeID = Convert.ToString(item["AttributeID"]);
            BAL.Utility.ActivityLog("nside EditProductSKU Api  AttributeID:" + objEntity.AttributeID);
            objEntity.Size = Convert.ToString(item["Size"]);
            BAL.Utility.ActivityLog("nside EditProductSKU Api Size:" + objEntity.Size);
           // objEntity.StockStatus = Convert.ToString(item["Stockstatus"]);
           // BAL.Utility.ActivityLog("nside EditProductSKU Api  StockStatus:" + objEntity.StockStatus);
            res = objBAL.EditProductSKU("UPDATE", objEntity);
            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            BAL.Utility.ActivityLog("nside EditProductSKU Api  stauscode:" + objEntity.stauscode);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            BAL.Utility.ActivityLog("nside EditProductSKU Api  message:" + objEntity.message);

            //Dictionary<string, object> AddOrderEntity = new Dictionary<string, object>();
            //Dictionary<string, object> addressDict = new Dictionary<string, object>();
            //addressDict.Add("Product ID", Convert.ToString(res.Tables[1].Rows[0]["Product_ID"].ToString()));
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  message:" + objEntity.message);
            //addressDict.Add("Product description", Convert.ToString(res.Tables[1].Rows[0]["ProductDescription"].ToString()));
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  message:" + objEntity.message);

            //List<PathImage> Imageurl = new List<PathImage>();

            //if (res != null && res.Tables.Count > 0 && res.Tables[1].Rows.Count > 0)
            //{

            //    foreach (DataRow item1 in res.Tables[1].Rows)
            //    {
            //        Imageurl.Add(new PathImage
            //        {
            //            url = item1["PathImage"].ToString()
            //        });
            //    }
            //}
            //addressDict.Add("Attachments", new ArrayList(Imageurl));
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  message:" + new ArrayList(Imageurl));
            //addressDict.Add("Price", Convert.ToInt32(res.Tables[1].Rows[0]["Price"].ToString()));
            //addressDict.Add("Strikeout Price", Convert.ToInt32(res.Tables[1].Rows[0]["Strikeprice"].ToString()));
            //BAL.Utility.ActivityLog("nside AddProductSKU Api  message:" + objEntity.message);
            //addressDict.Add("Designer", Convert.ToString(res.Tables[1].Rows[0]["Brand"].ToString()));
            //addressDict.Add("Live", Convert.ToString(res.Tables[1].Rows[0]["IsPublished"].ToString()));
            //addressDict.Add("Product name", res.Tables[1].Rows[0]["Name"].ToString());
            //addressDict.Add("Youtube link", Convert.ToString(res.Tables[1].Rows[0]["YoutubeUrl"].ToString()));
            //addressDict.Add("Country of origin", res.Tables[1].Rows[0]["COUNTRY"].ToString());
            //addressDict.Add("Stock_Status", Convert.ToString(res.Tables[1].Rows[0]["StockStatus"].ToString()));
            //addressDict.Add("Category", res.Tables[1].Rows[0]["Category"].ToString());
            //addressDict.Add("SubCategory", Convert.ToString(res.Tables[1].Rows[0]["Sub_Category"].ToString()));
            //addressDict.Add("Tags", Convert.ToString(res.Tables[1].Rows[0]["Tags"].ToString()));
            //addressDict.Add("Approval status", "Approved");


            //ArrayList myArrayList = new ArrayList();
            //myArrayList.Add(AddOrderEntity);
            //AddOrderEntity.Add("fields", addressDict);
            //Dictionary<string, ArrayList> MainJson = new Dictionary<string, ArrayList>();
            //MainJson["records"] = myArrayList;
            //var json = new JavaScriptSerializer().Serialize(MainJson);
            //BAL.Utility.ActivityLog("Inside AddProductSKU Api  message:" + json);
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //var client = new RestSharp.RestClient("https://api.airtable.com/v0/appem7EqUkOGDItmS/Product");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.POST);
            //BAL.Utility.ActivityLog("Airtable Request:" + request);
            //request.AddHeader("Content-Type", "application/json");
            //BAL.Utility.ActivityLog("Airtable Request: request1");
            //request.AddParameter("application/json", json, ParameterType.RequestBody);
            //BAL.Utility.ActivityLog("Airtable Request: request2");
            //request.AddHeader("Authorization", "Bearer keycvDlbZJvfafDnj");
            //BAL.Utility.ActivityLog("Airtable Request: request3");
            //IRestResponse response1 = client.Execute(request);
            //var content = JObject.Parse(response1.Content);
            //BAL.Utility.ActivityLog("Airtable Request: request4");
            ////message = content["records"][0]["id"].ToString();
            //BAL.Utility.ActivityLog("Airtable Request: request5");
            ////BAL.Utility.ActivityLog("Airtable Responseabcde:" + message);
            //BAL.Utility.ActivityLog("Inside AddProductSKU Api Airtable message:" + content);
            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message
                },

            };

            return Ok(response);


        }




        [HttpGet]
        [AllowAnonymous]
        [Route("GetProductByID")]
        public IHttpActionResult GetProductByID(string Flag)
        {
            DataSet res = new DataSet();

            //------start Authorization-------
            var re = Request;
            var headers = re.Headers;

            if (headers.Contains("Authorization"))
            {

                string token = headers.GetValues("Authorization").First();
                var handler = new JwtSecurityTokenHandler();
                string authHeader = token;
                authHeader = authHeader.Replace("Bearer ", "");
                authHeader = authHeader.Replace("\"", "");
                var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                objEntity.CustomerID = tokenS.Claims.First(claim => claim.Type == "Customer_ID").Value;
                BAL.Utility.ActivityLog(" Inside GetProductByID CustomerID :" + objEntity.CustomerID);
            }
            else
            {
                objEntity.CustomerID = "0";
                BAL.Utility.ActivityLog(" Inside GetProductByID CustomerID :" + objEntity.CustomerID);
            }

            objEntity.Flag = Flag;
            BAL.Utility.ActivityLog(" Inside GetProductByID Flag:" + objEntity.Flag);
            //objEntity.CustomerID = "";
            //BAL.Utility.ActivityLog(" Inside GetProductByID  :" + objEntity.CustomerID);

            res = objBAL.Get_ProductDetails_By_Id("get_Product", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            BAL.Utility.ActivityLog(" Inside GetProductByID  stauscode:" + objEntity.stauscode);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            BAL.Utility.ActivityLog(" Inside GetProductByID  message:" + objEntity.message);
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    Product_Details = res.Tables[1],

                },
            };
            return Ok(response);
        }



       // [HttpGet]
       // [Authorize]
       //[AllowAnonymous]
       // [Route("GetProductByID")]
       // public IHttpActionResult GetProductByID(JObject objData)
       // {

       //     dynamic item = objData;
       //     string data = item["data"];
       //     DataSet res = new DataSet();

       //     objEntity.Flag =   Convert.ToString(item["Flag"]); 
       //     BAL.Utility.ActivityLog(" Inside GetProductByID:" + objEntity.Flag);
           
          
       //     //------start Authorization-------
       //     var re = Request;
       //     var headers = re.Headers;

       //     if (headers.Contains("Authorization"))
       //     {

       //         string token = headers.GetValues("Authorization").First();
       //         var handler = new JwtSecurityTokenHandler();
       //         string authHeader = token;
       //         authHeader = authHeader.Replace("Bearer ", "");
       //         authHeader = authHeader.Replace("\"", "");
       //         var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
       //         objEntity.CustomerID = tokenS.Claims.First(claim => claim.Type == "Customer_ID").Value;
       //         BAL.Utility.ActivityLog(" Inside GetProductByID  :" + objEntity.CustomerID);
       //     }else
       //     {
       //          objEntity.CustomerID = Convert.ToString(item["UserID"]);  
       //         BAL.Utility.ActivityLog(" Inside GetProductByID  :" + objEntity.CustomerID);
       //     }


       //     // objEntity.CustomerID = UserID;
       //     BAL.Utility.ActivityLog(" Inside GetProductByID  :" + objEntity.CustomerID);
       //     res = objBAL.Get_ProductDetails_By_Id("get_Product", objEntity);

       //     objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
       //     BAL.Utility.ActivityLog(" Inside GetProductByID  :" + objEntity.stauscode);
       //     objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
       //     BAL.Utility.ActivityLog(" Inside GetProductByID  :" + objEntity.message);
       //     var response = new
       //     {
       //         statuscode = objEntity.stauscode,
       //         message = objEntity.message,
       //         data = new
       //         {

       //             Product_Details = res.Tables[1],

       //         },
       //     };
       //     return Ok(response);
       // }


        [HttpPost]
        [Authorize]
         [AllowAnonymous]
        [Route("DeleteProduct")]
        public IHttpActionResult DeleteProduct(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];


            objEntity.SKUID = Convert.ToString(item["SkuId"]);
            objEntity.Product_ID = Convert.ToString(item["ProductId"]);
            objEntity.AttributeID = Convert.ToString(item["ProductAttributId"]);

            res = objBAL.DeleteProduct("CREATE", objEntity);
            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message
                },

            };

            return Ok(response);

        }


        [HttpPost]
        [Authorize]
         [AllowAnonymous]
        [Route("DeleteSKU")]
        public IHttpActionResult DeleteSKU(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];


            objEntity.SKUID = Convert.ToString(item["SkuID"]);
            BAL.Utility.ActivityLog("Inside DeleteSKU skuId:" + objEntity.SKUID);
            res = objBAL.DeleteSKU("CREATE", objEntity);
            BAL.Utility.ActivityLog("Inside DeleteSKU SP:DeleteSKU :" + res);
            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            BAL.Utility.ActivityLog("Inside DeleteSKU stauscode :" + objEntity.stauscode);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            BAL.Utility.ActivityLog("Inside DeleteSKU message:" + objEntity.message);
            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message
                },

            };

            return Ok(response);

        }

        [HttpPost]
        [Authorize]
         [AllowAnonymous]
        [Route("Publish")]
        public IHttpActionResult Publish(JObject objData)
        {
            DataSet res = new DataSet();
            dynamic item = objData;
            string data = item["data"];

            objEntity.Product_ID = Convert.ToString(item["Product_ID"]);
            BAL.Utility.ActivityLog(" Inside Razor pay  :" + objEntity.Product_ID);

            objEntity.AttributeID = Convert.ToString(item["AttributeID"]);
            BAL.Utility.ActivityLog(" Inside Razor pay  :" + objEntity.AttributeID);

            objEntity.SKUID = Convert.ToString(item["SKUID"]);
            BAL.Utility.ActivityLog(" Inside Razor pay  :" + objEntity.SKUID);

            objEntity.PublishID = Convert.ToString(item["PublishID"]);
            BAL.Utility.ActivityLog(" Inside Razor pay  :" + objEntity.PublishID);


            //  objEntity.Tag = Convert.ToString(item["isVisible"]);
            res = objBAL.Publish("CREATE", objEntity);
            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message
                },

            };

            return Ok(response);

        }


        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [Route("StockStatus")]
        public IHttpActionResult StockStatus(JObject objData)
        {
            DataSet res = new DataSet();
            dynamic item = objData;
            string data = item["data"];

            objEntity.Product_ID = Convert.ToString(item["Product_ID"]);
            BAL.Utility.ActivityLog(" Inside Razor pay  :" + objEntity.Product_ID);

            objEntity.Stock = Convert.ToString(item["StockStatus"]);
            BAL.Utility.ActivityLog(" Inside Razor pay  :" + objEntity.Stock);

            res = objBAL.StockStatus("CREATE", objEntity);
            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            if (objEntity.stauscode==102) { 
            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = res.Tables[0].Rows[0]["message"].ToString(),
            },

            };

            return Ok(response);
            }
            else
            {
                var response = new
                {

                    data = new
                    {
                        statuscode = objEntity.stauscode,
                        message = "Please Insert All the Details"
                    },

                };
                return Ok(response);
            }
        }



        [HttpGet]
        [Authorize]
          [AllowAnonymous]
        [Route("Category_Wise_Product")]
        public IHttpActionResult Category_Wise_Product(string CategoryType, string Category_ID, string Sub_Category_ID, string Brand_ID)
        {
            DataSet res = new DataSet();
            objEntity.Category_ID = Category_ID;
            objEntity.Brand = Brand_ID;
            objEntity.Sub_Category_ID = Sub_Category_ID;
            objEntity.CategoryType = CategoryType;
            res = objBAL.GetBrandProduct_ByCategories("Get_Brands_Category", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    Products_Details = res.Tables[1],
                    Type_Details = res.Tables[2],

                },
            };
            return Ok(response);
        }


        [HttpGet]
        [Authorize]
        [AllowAnonymous]
        [Route("CategoryWiseProduct")]
        public IHttpActionResult CategoryWiseProduct(string Category_ID)
        {
            DataSet res = new DataSet();
            objEntity.Category_ID = Category_ID;

            res = objBAL.GetBrandProduct_ByCategories("Get_Brands_Category", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    Products_Details = res.Tables[1],


                },
            };
            return Ok(response);
        }

        [HttpGet]
        [Authorize]
         [AllowAnonymous]
        [Route("GETGSTPERCENTAGE")]
        public IHttpActionResult GETGSTPERCENTAGE()
        {
            DataSet res = new DataSet();

            res = objBAL.GETGSTPERCENTAGE("Get_GSTPERCENTAGE", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    GSTPERCENTAGE = res.Tables[1],


                },
            };
            return Ok(response);
        }

        [HttpGet]
          [AllowAnonymous]
        [Route("GETSTATE")]
        public IHttpActionResult GETSTATE()
        {
            DataSet res = new DataSet();

            res = objBAL.GETSTATE("GETSTATE", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    STATE = res.Tables[1],


                },
            };
            return Ok(response);
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("Product_Wise_Attribute")]
        public IHttpActionResult Product_Wise_Attribute(string ProductId)
        {
            DataSet res = new DataSet();
            objEntity.Product_ID = ProductId;
            //objEntity.AiabClient = AiabClient;

            res = objBAL.Product_Wise_Attribute("Get_Attribute", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    Attribute_Details = res.Tables[1],


                },
            };
            return Ok(response);
        }


        [HttpGet]
        [Authorize]
        [AllowAnonymous]
        [Route("SubCategory_Wise_Product")]
        public IHttpActionResult SubCategory_Wise_Product(string SubCategoryId)
        {
            DataSet res = new DataSet();
            objEntity.Sub_Category_ID = SubCategoryId;
            //objEntity.AiabClient = AiabClient;

            res = objBAL.SubCategory_Wise_Product("Get_Products", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    Product_Details = res.Tables[1],


                },
            };
            return Ok(response);
        }


        [HttpPost]
        [Authorize]
         [AllowAnonymous]
        [Route("AddBoosterOrder")]
        public IHttpActionResult AddOrder(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];

            objEntity.AiabClient = Convert.ToString(item["AiabClient"]);
            objEntity.BusinessName = Convert.ToString(item["BusinessName"]);
            objEntity.MobileNo = Convert.ToString(item["MobileNo"]);
            objEntity.StateName = Convert.ToString(item["StateName"]);
            objEntity.StateCode = Convert.ToString(item["StateCode"]);
            objEntity.Address = Convert.ToString(item["Address"]);
            objEntity.GSTIN = Convert.ToString(item["GSTIN"]);
            objEntity.BoosterQty = Convert.ToString(item["BoosterQty"]);
            res = objBAL.AddOrder("CREATE", objEntity);
            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();


            //Dictionary<string, object> AddOrderEntity = new Dictionary<string, object>();

            //Dictionary<string, object> addressDict = new Dictionary<string, object>();
            //addressDict.Add("Order ID", Convert.ToString(objEntity.Product_ID));
            //addressDict.Add("Order made by user", Convert.ToString(objEntity.ProductDescription));

            //addressDict.Add("Products that user ordered", "Live");
            //addressDict.Add("Quantity", objEntity.Name);
            //addressDict.Add("Order status", objEntity.Country_Of_Origin);

            //ArrayList myArrayList = new ArrayList();
            //myArrayList.Add(AddOrderEntity);
            //AddOrderEntity.Add("fields", addressDict);
            //Dictionary<string, ArrayList> MainJson = new Dictionary<string, ArrayList>();
            //MainJson["records"] = myArrayList;
            //var json = new JavaScriptSerializer().Serialize(MainJson);

            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //var client = new RestClient("https://api.airtable.com/v0/appem7EqUkOGDItmS/Product");
            //client.Timeout = -1;
            //var request = new RestRequest(Method.POST);
            //request.AddHeader("Content-Type", "application/json");
            //request.AddParameter("application/json", json, ParameterType.RequestBody);
            //request.AddHeader("Authorization", "Bearer keycvDlbZJvfafDnj");
            //IRestResponse response1 = client.Execute(request);


            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message
                },

            };
            return Ok(response);

        }


        [HttpPost]
     //   [AllowAnonymous]
        [Authorize]
        [Route("AddToCart")]
        public IHttpActionResult AddToCart(JObject Cartdata)
        {
            DataSet res = new DataSet();
            string result = string.Empty;   
            dynamic item = Cartdata;
            string data = item["data"];
            
            var re = Request;
            var headers = re.Headers;
            string token = headers.GetValues("Authorization").First();
            var handler = new JwtSecurityTokenHandler();
            string authHeader =token;
            authHeader = authHeader.Replace("Bearer ", "");
            authHeader = authHeader.Replace("\"", "");
            var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
            objEntity.CustomerID = tokenS.Claims.First(claim => claim.Type == "Customer_ID").Value;

            //objEntity.CustomerID = Convert.ToString(item["User_ID"]);
            objEntity.AttributeID = Convert.ToString(item["Attribute_ID"]);
            objEntity.Product_ID = Convert.ToString(item["Product_Id"]);
            objEntity.SKUID = Convert.ToString(item["SKU_ID"]);
            objEntity.Quantity = Convert.ToString(item["quantity"]);
            //objEntity.Price = Convert.ToString(item["Price"]);
            //objEntity.BrandID = Convert.ToInt32(item["Brand_ID"]);


            res = objBAL.Add_Cart_ByBrandWebsite("Add_Cart_ByBrandWebsite", objEntity);

            if (res != null && res.Tables.Count > 0)
            {
                if (res.Tables[0].Rows[0]["Result"].ToString() == "0")
                {
                    result = "0";

                }
                else
                {
                    result = "1";
                    objEntity.stauscode = 100;
                    objEntity.message = "Record Inserted Successfully";
                }
            }
            //objEntity.stauscode = 100;
            //objEntity.message = "Record Inserted Successfully";
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {
                    Result = result,
                    Cart_Count = res.Tables[1],
                    Cart_Details = res.Tables[2],
                },
            };
            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        // [AllowAnonymous]
        [Route("ViewCart")]
        public IHttpActionResult Get_ViewCartDetails()
        {
            //sirf jwt token dalna--
            DataSet res = new DataSet();
            string result = string.Empty;


            var re = Request;
            var headers = re.Headers;
            string token = headers.GetValues("Authorization").First();
            var handler = new JwtSecurityTokenHandler();
            string authHeader = token;
            authHeader = authHeader.Replace("Bearer ", "");
            authHeader = authHeader.Replace("\"", "");
            var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
            objEntity.CustomerID = tokenS.Claims.First(claim => claim.Type == "Customer_ID").Value;

           // objEntity.CustomerID = User_ID;

            res = objBAL.Get_Cart_ByBrandWebsite("Get_Cart_ByBrandWebsite", objEntity);

            objEntity.stauscode = 100;
            objEntity.message = "Record Found";
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    Cart_Details = res.Tables[0],
                },
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        // [AllowAnonymous]
        [Route("RemoveCart")]
        public IHttpActionResult RemoveFromCart(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];
            var re = Request;
            var headers = re.Headers;
            string token = headers.GetValues("Authorization").First();
            var handler = new JwtSecurityTokenHandler();
            string authHeader = token;
            authHeader = authHeader.Replace("Bearer ", "");
            authHeader = authHeader.Replace("\"", "");
            var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
            objEntity.CustomerID = tokenS.Claims.First(claim => claim.Type == "Customer_ID").Value;

           // objEntity.CustomerID = Convert.ToString(item["User_ID"]);
            objEntity.Cart_ID = Convert.ToString(item["CartID"]);

            res = objBAL.RemoveFromCart(objEntity);
            objEntity.stauscode = 100;
            objEntity.message = "Item Removed Successfully";
            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message

                },

            };

            return Ok(response);

        }


        [HttpPost]
        [Authorize]
        //  [AllowAnonymous]
        [Route("Orderhistory")]
        public IHttpActionResult Orderhistory(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];
            BAL.Utility.ActivityLog("Inside Orderhistory Api:");

            var re = Request;
            var headers = re.Headers;
            string token = headers.GetValues("Authorization").First();
            var handler = new JwtSecurityTokenHandler();
            string authHeader = token;
            authHeader = authHeader.Replace("Bearer ", "");
            authHeader = authHeader.Replace("\"", "");
            var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
            objEntity.CustomerID = tokenS.Claims.First(claim => claim.Type == "Customer_ID").Value;

           // objEntity.CustomerID = Convert.ToString(item["CustomerID"]);
            BAL.Utility.ActivityLog("CustomerID:" + objEntity.CustomerID);

            res = objBAL.GetOrderhistory("GET_ORDERHISTORY", objEntity);
            BAL.Utility.ActivityLog("GetOrderhistory SP:");

            objEntity.stauscode = 140;
            BAL.Utility.ActivityLog("Statuscode:" + objEntity.stauscode);
            objEntity.message = "Record Found";
            BAL.Utility.ActivityLog("message:" + objEntity.message);

            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {
                    Orderhistory_Details = res.Tables[1],

                },
            };
            return Ok(response);
            BAL.Utility.ActivityLog("response" + response);
        }


        [HttpPost]
        [Authorize]
         [AllowAnonymous]
        [Route("DynamicTable")]
        public IHttpActionResult CreateDynamicTable(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];
            objEntity.AiabClient = Convert.ToString(item["AiabClientId"]);
            objEntity.BusinessName = Convert.ToString(item["BussinessName"]);
            objEntity.Category = Convert.ToString(item["Category"]);

            res = objBAL.CreateDynamicTable("Create_DynamicTable", objEntity);
            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {
                    DynamicTable = res.Tables[1],

                },
            };
            return Ok(response);

        }

        [HttpPost]
       
        [AllowAnonymous]
        [Route("AddCustomer")]
        public IHttpActionResult AddCustomer(JObject objData)
        {
            DataSet res = new DataSet();
            string result = string.Empty;

            dynamic item = objData;
            string data = item["data"];

            objEntity.Brained_UserID = Convert.ToString(item["Brained_UserID"]);
            objEntity.FirstName = Convert.ToString(item["FirstName"]);
            objEntity.LastName = Convert.ToString(item["LastName"]);
            objEntity.Mobile = Convert.ToString(item["Mobile"]);
            objEntity.Email = Convert.ToString(item["Email"]);

            objEntity.City = Convert.ToString(item["City"]);
            objEntity.State = Convert.ToString(item["State_code"]);
            objEntity.Pincode = Convert.ToString(item["Pincode"]);
            objEntity.Address = Convert.ToString(item["Address"]);
            res = objBAL.CRUD_Customer("CREATE", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

           
            /////
            if (objEntity.stauscode == 100)
            {

                //This For JWT Token 
                string key = "my_secret_key_Brained";
                //localhost for testing 
                var issuer = "http://ecommaspapi.aiab.in/";
                //this for new Deployed Instance
                //var issuer2 = "http://wellsapi.aiab.in/";
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var permClaims = new List<Claim>();
                permClaims.Add(new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                permClaims.Add(new Claim("Customer_ID", objEntity.Brained_UserID));

                var token = new JwtSecurityToken(issuer,
                                issuer,
                                permClaims,
                                expires: DateTime.Now.AddDays(1),
                                signingCredentials: credentials);
                var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);

                var response = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,
                    data = new
                    {

                        Customer_Id = res.Tables[1],
                        Token = jwt_token,
                    },

                };
                return Ok(response);
            }
            else
            {
                var response = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,

                };
                return Ok(response);
            }
            //////

           
        }

        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [Route("AddInventory")]
        public IHttpActionResult AddInventory(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];


            objEntity.SKUID = Convert.ToString(item["ID"]);
            objEntity.Total_Quantity = Convert.ToString(item["Total_Quantity"]);
            objEntity.Remaining_Quantity = Convert.ToString(item["Total_Quantity"]);
            objEntity.Expiry_Date = Convert.ToString(item["Expiry_Date"]);
            objEntity.ImportDate = Convert.ToString(item["ImportDate"]);
            objEntity.LocationID = Convert.ToString(item["LocationID"]);

            res = objBAL.AddInventory("CREATE", objEntity);
            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message
                },

            };

            return Ok(response);

        }

        [HttpGet]
        [Authorize]
         [AllowAnonymous]
        [Route("GetInventoryDetails")]
        public IHttpActionResult GetInventoryDetails()
        {
            DataSet res = new DataSet();

            res = objBAL.GetInventoryDetails("GetInventoryDetails", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    Inventory_Details = res.Tables[1],


                },
            };
            return Ok(response);
        }

  

        //[HttpPost]
        //[AllowAnonymous]
        //[Route("AddCategories")]
        //public IHttpActionResult AddCategories(JObject objData)
        //{

        //    DataSet res = new DataSet();
        //    string result = string.Empty;

        //    dynamic item = objData;
        //    string data = item["data"];
        //    objEntity.AiabClient= Convert.ToString(item["AiabClient"]);
        //    objEntity.Category_ID = Convert.ToString(item["Category_ID"]);

        //    objEntity.Type = Convert.ToString(item["Type"]);

        //    objEntity.Sub_Category = Convert.ToString(item["Sub_Category"]);

        //    if (item["IsParent"].ToString().ToUpper() == "YES")
        //        {
        //        objEntity.IsParent = "true";
        //        objEntity.Category = Convert.ToString(item["Category"]);
        //        objEntity.Sub_Category = Convert.ToString(item["Category"]);
        //        objEntity.ReferenceID = "0";
        //        }
        //        if (item["IsParent"].ToString().ToUpper() == "NO")
        //        {
        //            objEntity.IsParent = "false";
        //        objEntity.Category = Convert.ToString(item["Category"]);
        //        objEntity.Sub_Category = Convert.ToString(item["Sub_Category"]);
        //        objEntity.ReferenceID = Convert.ToString(item["Category_ID"]);
        //    }
        //        res = objBAL.AddCategories("CREATE", objEntity);



        //    objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
        //    objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
        //    var response = new
        //    {
        //        statuscode = objEntity.stauscode,
        //        message = objEntity.message,
        //        data = new
        //        {

        //          category_Details = res.Tables[1],
        //        },
        //    };
        //    return Ok(response);

        //}


        //[HttpPost]
        //[AllowAnonymous]
        //[Route("AddTag")]
        //public IHttpActionResult AddTag(JObject objData)
        //{

        //    DataSet res = new DataSet();
        //    string result = string.Empty;

        //    dynamic item = objData;
        //    string data = item["data"];
        //    objEntity.AiabClient = Convert.ToString(item["AiabClient"]);
        //    objEntity.Tagname = Convert.ToString(item["Tag_Name"]);

        //    res = objBAL.AddTag("CREATE", objEntity);

        //    objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
        //    objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
        //    var response = new
        //    {
        //        statuscode = objEntity.stauscode,
        //        message = objEntity.message,
        //        data = new
        //        {

        //            category_Details = res.Tables[1],
        //        },
        //    };
        //    return Ok(response);

        //}



        //[HttpGet]
        //[AllowAnonymous]
        //[Route("GetCategory")]


        //public IHttpActionResult get_categories(string AiabClient)
        //{
        //    DataSet res = new DataSet();
        //  //  objEntity.Category_ID = Category_ID;
        //    objEntity.AiabClient = AiabClient;

        //    res = objBAL.get_categories("get_categories", objEntity);

        //    objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
        //    objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
        //    var response = new
        //    {
        //        statuscode = objEntity.stauscode,
        //        message = objEntity.message,
        //        data = new
        //        {

        //            Category_Details = res.Tables[1],

        //        },
        //    };
        //    return Ok(response);
        //}



        [HttpGet]
        [Authorize]
       [AllowAnonymous]
        [Route("Product_Filter")]
        public IHttpActionResult Product_Filter(string Subcategories, string Category_ID, string Tag_ID)
        {
            DataSet res = new DataSet();
            objEntity.Category_ID = Category_ID;
            objEntity.Tagname = Tag_ID;
            objEntity.Sub_Category_ID = Subcategories;
            res = objBAL.GetBrandProduct_Filter("ProductFilter", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {
                    Products_Details = res.Tables[1]
                },
            };
            return Ok(response);
        }

        [HttpGet]
        [Authorize]
       [AllowAnonymous]
        [Route("Get_ProductDetails")]
        public IHttpActionResult ProductDetails(string AttributeID, string ProductID, string SkuID)
        {
            objEntity.AttributeID = AttributeID;
            BAL.Utility.ActivityLog(" Inside ProductDetails:" + objEntity.AttributeID);
            objEntity.Product_ID = ProductID;
            BAL.Utility.ActivityLog(" Inside ProductDetails:" + objEntity.Product_ID);
            objEntity.SKUID = SkuID;
            BAL.Utility.ActivityLog(" Inside ProductDetails:" + objEntity.SKUID);


            res = objBAL.getProductdetails("GET_PRODUCT", objEntity);
            BAL.Utility.ActivityLog(" Inside ProductDetails:" + res);
            objEntity.stauscode = 140;
            BAL.Utility.ActivityLog(" Inside ProductDetails:" + objEntity.stauscode);
            objEntity.message = "Record Found";
            BAL.Utility.ActivityLog(" Inside ProductDetails:" + objEntity.message);
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,

                data = new
                {
                    MenuSEO_Details = res.Tables[0],
                    Products_Details = res.Tables[1],
                    Sku_Details = res.Tables[2],
                    Image_details = res.Tables[3],
                    Related_Product = res.Tables[4],
                },
            };
            return Ok(response);

        }

        [HttpPost]
        [Authorize]
        //   [AllowAnonymous]
        [Route("CheckOut")]
        public IHttpActionResult ConfirmOrder(JObject Cartdata)
        {
            DataSet ds = new DataSet();
            string result = string.Empty;
            dynamic item = Cartdata;
            string data = item["data"];
            var re = Request;
            var headers = re.Headers;
            string token = headers.GetValues("Authorization").First();
            var handler = new JwtSecurityTokenHandler();
            string authHeader = token;
            authHeader = authHeader.Replace("Bearer ", "");
            authHeader = authHeader.Replace("\"", "");
            var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
            objEntity.CustomerID = tokenS.Claims.First(claim => claim.Type == "Customer_ID").Value;

          //  objEntity.CustomerID = Convert.ToString(item["User_ID"]);
            BAL.Utility.ActivityLog(" CustomerID :" + objEntity.CustomerID);
            objEntity.payment_method = Convert.ToString(item["payment_method"]);
            BAL.Utility.ActivityLog(" payment_method :" + objEntity.payment_method);
            if (objEntity.payment_method == "COD")
            {
                objEntity.Transaction_Status = "COD";
            }


            if (objEntity.payment_method == "RazorPay")
            {

                objEntity.Transaction_Status = "Transaction Completed";
            }

            res = objBAL.InsertOrderFromWeb(objEntity);
            string order_ID = res.Tables[1].Rows[0]["OrderID"].ToString();
            BAL.Utility.ActivityLog(" OrderID :" + order_ID);
            string TransationStatus = res.Tables[1].Rows[0]["TransationStatus"].ToString();
            BAL.Utility.ActivityLog("TransationStatus :" + TransationStatus);
            //string guest_Id = res.Tables[2].Rows[0]["GUESTID"].ToString();
            objEntity.OrderId = res.Tables[1].Rows[0]["OrderID"].ToString();
            BAL.Utility.ActivityLog("AddOrderDetails :OrderID :" + objEntity.OrderId);
            objEntity.PaymentType = objEntity.payment_method;
            //objEntity.Amount = Total;
            objEntity.Comment = Convert.ToString(item["Comment"]);
            objEntity.CoupanCode = Convert.ToString(item["CoupanCode"]);
            objEntity.B_firstName = Convert.ToString(item["B_firstName"]);
            objEntity.B_lastName = Convert.ToString(item["B_lastName"]);
            objEntity.B_email = Convert.ToString(item["B_email"]);
            objEntity.B_mobile = Convert.ToString(item["B_mobile"]);
            objEntity.B_company = Convert.ToString(item["B_company"]);
            objEntity.B_address1 = Convert.ToString(item["B_address1"]);
            objEntity.B_address2 = Convert.ToString(item["B_address2"]);
            objEntity.B_city = Convert.ToString(item["B_city"]);
            objEntity.B_postalCode = Convert.ToString(item["B_postalCode"]);
            objEntity.B_country = Convert.ToString(item["B_country"]);
            objEntity.B_State = Convert.ToString(item["B_State"]);
            BAL.Utility.ActivityLog("middle with all parameter :");
            objEntity.D_firstName = Convert.ToString(item["D_firstName"]);  //firstname
            objEntity.D_lastName = Convert.ToString(item["D_lastName"]);//lastname
            objEntity.D_email = Convert.ToString(item["D_email"]); //email
            objEntity.D_mobile = Convert.ToString(item["D_mobile"]);//mobile
            objEntity.D_company = Convert.ToString(item["D_company"]);  //company
            objEntity.D_address1 = Convert.ToString(item["D_address1"]);  //address1
            objEntity.D_address2 = Convert.ToString(item["D_address2"]); //address2
            objEntity.D_city = Convert.ToString(item["D_city"]); //city
            objEntity.D_postalCode = Convert.ToString(item["D_postalCode"]); //postal code
            objEntity.D_country = Convert.ToString(item["D_country"]);//country
            objEntity.D_State = Convert.ToString(item["D_State"]); //state
            BAL.Utility.ActivityLog("Done with all parameter :");
            DataSet ds_Data = new DataSet();
            if (objEntity.OrderId != "" && objEntity.PaymentType != "" && objEntity.Transaction_Status != ""
               && objEntity.B_firstName != "" && objEntity.D_postalCode != ""
               && objEntity.D_mobile != "" && objEntity.CustomerID != "" && objEntity.AddressLine1 != "" && objEntity.AddressLine2 != ""
               && objEntity.B_email != "" && objEntity.D_country != "" && objEntity.D_State != "")
            {
                ds_Data = objBAL.InsertInvoiceDetails(objEntity);
                BAL.Utility.ActivityLog("Inserted in invoice table:");
            }

            if (order_ID != "" || order_ID != null)
            {
                objEntity.stauscode = 100;
                objEntity.message = "Record Inserted Successfully";
                BAL.Utility.ActivityLog("finish:");
            }
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    order_ID = objEntity.OrderId
                },
            };

            return Ok(response);
        }


        [HttpPost]
        [Authorize]
      [AllowAnonymous]
        [Route("InvoiceDetails")]
        public IHttpActionResult InvoiceDetails(JObject Cartdata)
        {
            DataSet res = new DataSet();
            string result = string.Empty;

            dynamic item = Cartdata;
            string data = item["data"];

            objEntity.OrderId = Convert.ToString(item["Order_ID"]);
            res = objBAL.InvoiceDetails(objEntity);
            objEntity.OrderId = res.Tables[0].Rows[0]["MailOrderID"].ToString();

            objEntity.stauscode = 100;
            objEntity.message = "Record Inserted Successfully";
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {
                    InvoiceDetails = res.Tables[0],
                    ProductDetails = res.Tables[1],
                    Quantity = res.Tables[2],
                    TaxDetails = res.Tables[3]

                },
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        // [AllowAnonymous]
        [Route("Wishlist")]
        public IHttpActionResult AddToWishlist(JObject Cartdata)
        {
            DataSet res = new DataSet();
            string result = string.Empty;

            dynamic item = Cartdata;
            string data = item["data"];
            var re = Request;
            var headers = re.Headers;
            string token = headers.GetValues("Authorization").First();
            var handler = new JwtSecurityTokenHandler();
            string authHeader = token;
            authHeader = authHeader.Replace("Bearer ", "");
            authHeader = authHeader.Replace("\"", "");
            var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
            objEntity.CustomerID = tokenS.Claims.First(claim => claim.Type == "Customer_ID").Value;

           // objEntity.CustomerID = Convert.ToString(item["User_ID"]);
            objEntity.SKUID = Convert.ToString(item["SKU_ID"]);
            objEntity.AttributeID = Convert.ToString(item["Attribute_ID"]);
            objEntity.Product_ID = Convert.ToString(item["Product_Id"]);
            //objEntity.Quantity = Convert.ToString(item["quantity"]);
            //objEntity.Price = Convert.ToString(item["Price"]);
            //objEntity.BrandID = Convert.ToInt32(item["Brand_ID"]);


            res = objBAL.Add_Wishlist_By_BrandWebsite("Add_Wishlist_By_BrandWebsite", objEntity);


            objEntity.stauscode = 100;
            objEntity.message = "Record Inserted Successfully";
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    Wishlist_Details = res.Tables[0]
                },
            };
            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        // [AllowAnonymous]
        [Route("View_Wishlist")]
        public IHttpActionResult Get_WishlistDetails()
        {//sirf jwt dalna

            DataSet res = new DataSet();
            string result = string.Empty;
            var re = Request;
            var headers = re.Headers;
            string token = headers.GetValues("Authorization").First();
            var handler = new JwtSecurityTokenHandler();
            string authHeader = token;
            authHeader = authHeader.Replace("Bearer ", "");
            authHeader = authHeader.Replace("\"", "");
            var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
            objEntity.CustomerID = tokenS.Claims.First(claim => claim.Type == "Customer_ID").Value;

           // objEntity.CustomerID = User_ID;

            res = objBAL.Get_Wishlist(objEntity);

            objEntity.stauscode = 100;
            objEntity.message = "Record Found";
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    Wishlist_Details = res.Tables[0],
                },
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        // [AllowAnonymous]
        [Route("RemoveWishlisht")]
        public IHttpActionResult RemoveItemFromWishlisht(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];
            var re = Request;
            var headers = re.Headers;
            string token = headers.GetValues("Authorization").First();
            var handler = new JwtSecurityTokenHandler();
            string authHeader = token;
            authHeader = authHeader.Replace("Bearer ", "");
            authHeader = authHeader.Replace("\"", "");
            var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
            objEntity.CustomerID = tokenS.Claims.First(claim => claim.Type == "Customer_ID").Value;

           // objEntity.CustomerID = Convert.ToString(item["User_ID"]);
            objEntity.ID = Convert.ToString(item["ID"]);

            res = objBAL.RemoveFromWishlist(objEntity);
            objEntity.stauscode = 100;
            objEntity.message = "Item Removed Successfully";
            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message

                },

            };

            return Ok(response);

        }

        

        [HttpPost]
        [Authorize]
          [AllowAnonymous]
        [Route("AddSubcategories")]
        public IHttpActionResult AddSubcategories(JObject objData)
        {
            BAL.Utility.ActivityLog("Inside EditSubcategories:");
            dynamic item = objData;
            string data = item["data"];


            objEntity.Category_ID = Convert.ToString(item["Category_ID"]);
            BAL.Utility.ActivityLog("Inside EditSubcategories:Category_ID:" + objEntity.Category_ID);

            objEntity.Sub_Category = Convert.ToString(item["Sub_Category"]);
            BAL.Utility.ActivityLog("Inside EditSubcategories:Sub_Category:" + objEntity.Sub_Category);

            //objEntity.AiabClient = Convert.ToString(item["AiabClient"]);


            res = objBAL.AddSubCategory("CREATE", objEntity);
            BAL.Utility.ActivityLog("Inside EditSubcategories:" + res);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            BAL.Utility.ActivityLog("Inside EditSubcategories:stauscode:" + objEntity.stauscode);

            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            BAL.Utility.ActivityLog("Inside EditSubcategories:");

            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message
                },

            };

            return Ok(response);

        }

        [HttpPost]
        [Authorize]
      [AllowAnonymous]
        [Route("EditSubcategories")]
        public IHttpActionResult EditSubcategories(JObject objData)
        {
            BAL.Utility.ActivityLog("Inside EditSubcategories:");
            dynamic item = objData;
            string data = item["data"];

            objEntity.ReferenceID = Convert.ToString(item["ReferenceID"]);
            BAL.Utility.ActivityLog("Inside EditSubcategories:ReferenceID:" + objEntity.ReferenceID);

            
            objEntity.Category_ID = Convert.ToString(item["Category_ID"]);
            BAL.Utility.ActivityLog("Inside EditSubcategories:Category_ID:" + objEntity.Category_ID);

            objEntity.Sub_Category = Convert.ToString(item["Sub_Category"]);
            BAL.Utility.ActivityLog("Inside EditSubcategories:Sub_Category:" + objEntity.Sub_Category);

            //objEntity.AiabClient = Convert.ToString(item["AiabClient"]);


            res = objBAL.EditSubCategory("Update", objEntity);
            BAL.Utility.ActivityLog("Inside EditSubcategories:" + res);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            BAL.Utility.ActivityLog("Inside EditSubcategories:stauscode:" + objEntity.stauscode);

            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            BAL.Utility.ActivityLog("Inside EditSubcategories:");

            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message
                },

            };

            return Ok(response);

        }


        [HttpPost]
        [Authorize]
        // [AllowAnonymous]
        [Route("AddRazorPayOrder")]
        public IHttpActionResult RazorPayOrder()
        {
            BAL.Utility.ActivityLog("start Inside RazorPayOrder  :");
            DataSet ds = new DataSet();
            string result = string.Empty;
         
            RazorpayClient client = new RazorpayClient("rzp_test_EgzhzuXldAqQXK", "L8crvF97yyXKTbzCUKOs3Mal");
            Random randomObj = new Random();
            string transactionId = randomObj.Next(10000000, 100000000).ToString();
            BAL.Utility.ActivityLog(" Inside AddRazorPayOrder  :" + transactionId);
            var re = Request;
            var headers = re.Headers;
            string token = headers.GetValues("Authorization").First();
            var handler = new JwtSecurityTokenHandler();
            string authHeader = token;
            authHeader = authHeader.Replace("Bearer ", "");
            authHeader = authHeader.Replace("\"", "");
            var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
            objEntity.CustomerID = tokenS.Claims.First(claim => claim.Type == "Customer_ID").Value;

            //objEntity.CustomerID = User_ID;
            BAL.Utility.ActivityLog(" Inside AddRazorPayOrder  :" + objEntity.CustomerID);
            ds = objBAL.RazorPayOrder(objEntity);
            Dictionary<string, object> obj = new Dictionary<string, object>();
            int amount = Convert.ToInt32(ds.Tables[0].Rows[0]["amount"].ToString());
            BAL.Utility.ActivityLog(" Inside AddRazorPayOrder  :" + amount);
            obj["currency"] = ds.Tables[0].Rows[0]["currency"].ToString();
            BAL.Utility.ActivityLog(" Inside AddRazorPayOrder  :" + obj["currency"]);
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", amount * 100);  // Amount will in paise
            options.Add("receipt", transactionId);
            options.Add("currency", "INR");
            options.Add("payment_capture", "0"); // 1 - automatic  , 2 - manual
                                                 //options.Add("notes", "-- You can put any notes here --");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Razorpay.Api.Order orderResponse = client.Order.Create(options);
            string orderId = orderResponse["id"].ToString();
            BAL.Utility.ActivityLog(" Inside AddRazorPayOrder  :" + orderId);
            objEntity.stauscode = 100;
            BAL.Utility.ActivityLog(" Inside AddRazorPayOrder  :" + objEntity.stauscode);
            objEntity.message = "OrderId Created at RazorPay";
            BAL.Utility.ActivityLog(" Inside AddRazorPayOrder  :" + objEntity.message);
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    order_ID = orderId

                },
            };
            return Ok(response);
        }


        [HttpPost]
        [Authorize]
      //  [AllowAnonymous]
        [Route("RazorPaymentStatus")]
        public IHttpActionResult RazorPaymentStatus(string rzp_paymentid, string rzp_orderid)
        {
            BAL.Utility.ActivityLog("start Inside RazorPaymentStatus  :");
            RazorpayClient client = new RazorpayClient("rzp_test_EgzhzuXldAqQXK", "L8crvF97yyXKTbzCUKOs3Mal");
            Random randomObj = new Random();
            string paymentId = rzp_paymentid;
            BAL.Utility.ActivityLog(" Inside RazorPaymentStatus  :" + paymentId);
            // This is orderId
            string orderId = rzp_orderid;
            BAL.Utility.ActivityLog(" Inside RazorPaymentStatus  :" + orderId);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Razorpay.Api.Payment payment = client.Payment.Fetch(paymentId);
            // This code is for capture the payment 
            //Dictionary<string, object> options = new Dictionary<string, object>();
            //options.Add("amount", payment.Attributes["amount"]);
            //Razorpay.Api.Payment paymentCaptured = payment.Capture(options);
            //string amt = paymentCaptured.Attributes["amount"];
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //Razorpay.Api.Order orderResponse = client.Order.Create(options);
            // BAL.Utility.ActivityLog(" Inside RazorPaymentStatus  :" + orderResponse);
            if (payment.Attributes["status"] == "captured")
            {
                // Create these action method
                objEntity.stauscode = 100;
                objEntity.message = "Success";
            }
            else if (payment.Attributes["status"] == "authorized")
            {
                objEntity.stauscode = 100;
                objEntity.message = "Success";
            }
            else
            {
                objEntity.stauscode = 100;
                objEntity.message = "Failure";
            }

            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    order_ID = orderId

                },
            };
            return Ok(response);
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("Get_UserToken")]
        public IHttpActionResult Get_UserToken(string Brained_UserID)
        {
            BAL.Utility.ActivityLog("start Inside Get_UserToken  :");
            objEntity.Brained_UserID = Brained_UserID;
            BAL.Utility.ActivityLog("start Inside Get_UserToken    objEntity.Brained_UserID  :" + objEntity.Brained_UserID);
            res = objBAL.Get_UserToken("Get_UserToken", objEntity);

                objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            BAL.Utility.ActivityLog("start Inside Get_UserToken    objEntity.stauscode  :" + objEntity.stauscode);
            BAL.Utility.ActivityLog("start Inside Get_UserToken    objEntity.message  :" + objEntity.message);

            objEntity.CustomerID = res.Tables[1].Rows[0]["ID"].ToString() ;
            BAL.Utility.ActivityLog("start Inside Get_UserToken    objEntity.CustomerID  :" + objEntity.CustomerID);
            if (objEntity.stauscode == 100)
                {

                string key = "my_secret_key_Brained";
                // localhost for testing 
                //  var issuer = "http://localhost:51601/";
                // this for new Deployed Instance
                var issuer = "http://ecommaspapi.aiab.in/";
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var permClaims = new List<Claim>();
                permClaims.Add(new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                permClaims.Add(new Claim("Customer_ID", objEntity.CustomerID));

                var token = new JwtSecurityToken(issuer,
                                issuer,
                                permClaims,
                                expires: DateTime.Now.AddDays(1),
                                signingCredentials: credentials);

                var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
                // return jwt_token;
                BAL.Utility.ActivityLog("start Inside Get_UserToken  jwt_token  :" + jwt_token);

                var response = new
                    {
                        statuscode = objEntity.stauscode,
                        message = objEntity.message,
                        data = new
                        {

                            User_Details = res.Tables[1],
                            Token = jwt_token,
                        },

                    };
                    return Ok(response);
                }
                else
                {
                    var response = new
                    {
                        statuscode = objEntity.stauscode,
                        message = objEntity.message,

                    };
                     return Ok(response);
                }
   
            }
        


        [HttpGet]
        [Authorize]
       // [AllowAnonymous]
        [Route("OrderProcessed_ByAdmin")]
        public IHttpActionResult OrderProcessed_ByAdmin()
        {
          
           
            BAL.Utility.ActivityLog("Inside Orderhistory Api:");
           
            res = objBAL.OrderProcessed_ByAdmin("Create", objEntity);
            BAL.Utility.ActivityLog("GetOrderhistory SP:");

            objEntity.stauscode = 140;
            BAL.Utility.ActivityLog("Statuscode:" + objEntity.stauscode);
            objEntity.message = "Record Found";
            BAL.Utility.ActivityLog("message:" + objEntity.message);

            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {
                    Orderhistory_Details = res.Tables[1],

                },
            };
            return Ok(response);
            BAL.Utility.ActivityLog("response" + response);
        }

        [HttpGet]
        [Authorize]
        [AllowAnonymous]
        [Route("GetAllSubCategory")]
        public IHttpActionResult Get_All_SubCategory()
        {

            res = objBAL.Get_ALL_SubCategory("Get_ALL_SubCategory", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    SubCategory_Details = res.Tables[1],

                },
            };
            return Ok(response);
        }






        [HttpPost]
        [AllowAnonymous]
        [Route("UpdateStatus")]
        public IHttpActionResult UpdateStatus(JObject objData)
        {
            BAL.Utility.ActivityLog("Inside UpdateStatus:");
            dynamic item = objData;
            string data = item["data"];


            objEntity.OrderId = Convert.ToString(item["OrderID"]);
            BAL.Utility.ActivityLog("Inside EditSubcategories:OrderID:" + objEntity.OrderId);

            objEntity.Status = Convert.ToString(item["Status"]);
            BAL.Utility.ActivityLog("Inside UpdateStatus:Status:" + objEntity.Status);

            //objEntity.AiabClient = Convert.ToString(item["AiabClient"]);


            res = objBAL.UpdateStatus("Update", objEntity);
            BAL.Utility.ActivityLog("Inside UpdateStatus:" + res);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            BAL.Utility.ActivityLog("Inside UpdateStatus:stauscode:" + objEntity.stauscode);

            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            BAL.Utility.ActivityLog("Inside UpdateStatus:");

            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message
                },

            };

            return Ok(response);

        }


        [HttpGet]
        [AllowAnonymous]
        [Route("GET_ProductsByCategory")]
        public IHttpActionResult GET_ProductsByCategory(string Category_ID)
        {
            DataSet res = new DataSet();
            objEntity.Category_ID = Category_ID;

            res = objBAL.GET_ProductsByCategory("ProductsByCategory", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    Products_Details = res.Tables[1],


                },
            };
            return Ok(response);
        }




        [HttpGet]
        [AllowAnonymous]
        [Route("GetAllStatusCount")]
        public IHttpActionResult GetAllStatusCount()
        {
            DataSet res = new DataSet();

            res = objBAL.GetAllStatusCount(objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {
                    ALL_Status_Count = res.Tables[1]
                },
            };
            return Ok(response);
        }






        [HttpGet]
        [AllowAnonymous]
        [Route("FilterOrderStatus")]
        public IHttpActionResult FilterOrderStatus(string Status)
        {
            DataSet res = new DataSet();
            objEntity.Status = Status;

            res = objBAL.FilterOrderStatus("FilterOrderStatus", objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
            var response = new
            {
                statuscode = objEntity.stauscode,
                message = objEntity.message,
                data = new
                {

                    Orders = res.Tables[1],


                },
            };
            return Ok(response);
        }



        //[HttpPost]
        //[AllowAnonymous]
        //[Route("getDiscountapplyCoupenCode")]
        //public IHttpActionResult getDiscountapplyCoupenCode(JObject objData)
        //{
        //    DataSet res = new DataSet();
        //    DataSet res1 = new DataSet();
        //    dynamic item = objData;
        //    string data = item["data"];

        //    objEntity.CoupanCode = Convert.ToString(item["CoupanCode"]);

        //    objEntity.CustomerID = Convert.ToString(item["CustomerID"]);
        //    string result = "";
        //    DataTable dt = new DataTable();
        //    dt.Clear();
        //    dt.Columns.Add("number");
        //    dt.Columns.Add("SkuId");
        //    dt.Columns.Add("Quantity");
        //    ///////
        //    res1 = objBAL.Get_Cart_ByBrandWebsite("Get_Cart_ByBrandWebsite", objEntity);
        //    BAL.Utility.ActivityLog("Inside ADD_PO_ORDER_DETAILS OrderID:" + res1);
        //    int number = 1;
        //    while (number == res1.Tables[0].Rows.Count)
        //    {



        //        dt.Rows.Add(number, res1.Tables[0].Rows[0]["SkuId"].ToString(), res1.Tables[0].Rows[0]["Quantity"].ToString());
        //        number++;



        //    }


        //    DataTable finaleOrderDT = dt;
        //    objEntity.orderDTweb = finaleOrderDT;




        //    res = objBAL.getDiscApplyCode("GET_DISCOUNT", objEntity);


        //    if (objEntity.CustomerID != null)
        //    {
        //        result = res.Tables[1].Rows[0]["Discount"].ToString();
        //        if (result != "0")
        //        {
        //            var response = new
        //            {
        //                data = new
        //                {
        //                    SubTotal = res.Tables[2].Rows[0]["SubTotal"].ToString(),
        //                    Discount = res.Tables[2].Rows[0]["Discount"].ToString(),
        //                    Tax = res.Tables[2].Rows[0]["Tax"].ToString(),
        //                    Total = res.Tables[2].Rows[0]["Total"].ToString(),
        //                },
        //            };
        //            return Ok(response);
        //        }
        //        else
        //        {
        //            var response = new
        //            {
        //                data = new
        //                {
        //                    Discount = res.Tables[1].Rows[0]["Discount"].ToString(),
        //                },
        //            };
        //            return Ok(response);
        //        }
        //    }
        //    else
        //    {
        //        result = res.Tables[0].Rows[0]["Discount"].ToString();
        //        if (result != "0")
        //        {
        //            var response = new
        //            {
        //                data = new
        //                {
        //                    SubTotal = res.Tables[1].Rows[0]["SubTotal"].ToString(),
        //                    Discount = res.Tables[1].Rows[0]["Discount"].ToString(),
        //                    Tax = res.Tables[1].Rows[0]["Tax"].ToString(),
        //                    Total = res.Tables[1].Rows[0]["Total"].ToString(),
        //                },
        //            };
        //            return Ok(response);
        //        }
        //        else
        //        {
        //            var response = new
        //            {
        //                data = new
        //                {
        //                    Discount = res.Tables[0].Rows[0]["Discount"].ToString(),
        //                },
        //            };
        //            return Ok(response);
        //        }
        //    }


        //}



    }
}

