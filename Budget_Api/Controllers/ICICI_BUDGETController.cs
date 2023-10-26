using BAL;
using ClosedXML.Excel;
using CsvHelper;
using Entity;
using ExcelDataReader;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
//using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using System.Xml;

namespace Budget_Api.Controllers
{

    //[EnableCors("*", "*", "*")]
    public class ICICI_BUDGETController : ApiController
    {

        BudgetEntity objEntity = new BudgetEntity();
        BudgetBal objBAL = new BudgetBal();
        string JsonStr = string.Empty;
        //     string key = System.Configuration.ConfigurationManager.AppSettings["linkpreviewkey"].ToString();

        //  List<dynamic> objJobData = null;
        DataSet res = new DataSet();

        [EnableCors("*", "*", "*")]
        [HttpPost]
        [Route("Api/Login")]
        public IHttpActionResult Login(JObject objData)
        {
            try
            {
                dynamic item = objData;
                string token = string.Empty;
                string data = item["data"];
                objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                string json = objEntity.Data;
                var jss = new JavaScriptSerializer();
                var table = jss.Deserialize<dynamic>(json);
                objEntity.UserId = table["UserId"];
                UtilityClass.ActivityLog(DateTime.Now.ToString()+"UserId :" + objEntity.UserId );

                objEntity.Tokens = token;
            
                DataSet resultDS = new DataSet();
                DataSet resultDS1 = new DataSet();
                Dictionary<string, object> row = new Dictionary<string, object>();

                resultDS = objBAL.UserLogin("PROC_API_USERLOGIN", objEntity); //External User
                if ((resultDS != null) && (resultDS.Tables.Count > 0))
                {
                    token = GetToken(resultDS.Tables[0].Rows[0]["User_Code"].ToString(),
                        resultDS.Tables[0].Rows[0]["role"].ToString(), resultDS.Tables[0].Rows[0]["CurrentFY"].ToString(), 
                        resultDS.Tables[0].Rows[0]["LastloginDate"].ToString());
                }
                else
                {
                    row.Add("Login", "User access role not found");
                }
                objEntity.Tokens = token;
                UtilityClass.ActivityLog("token :" + objEntity.Tokens);

                resultDS1 = objBAL.UserLogin("Token", objEntity);
                objEntity.stauscode = Convert.ToInt32(resultDS.Tables[0].Rows[0]["Code"]);
                objEntity.message = resultDS.Tables[0].Rows[0]["message"].ToString();
                var Token1 = resultDS1.Tables[1].Rows[0]["Token"].ToString();
                UtilityClass.ActivityLog(DateTime.Now.ToString() + "Token1 :" + Token1);

                string token2 = Token1.ToString();
                UtilityClass.ActivityLog(DateTime.Now.ToString()+ "token2 :" + token2);
                var response = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,
                    Token = token2,
                };

                var jsonString = JsonConvert.SerializeObject(response);
                var response1 = new
                {
                    data = AESEncrytDecry.Encryptstring(jsonString)
                };

                return Ok(response1);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [EnableCors("*", "*", "*")]
        [HttpGet]
        [Route("Api/outho_Token")]
        public string GetToken(string UserId, string Role, string CurrentFY, string LastloginDate)
        {
            try
            {
                string key = "my_secret_key_12345";
                var issuer = WebConfigurationManager.AppSettings["baseurl"];

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var permClaims = new List<Claim>();
                permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                permClaims.Add(new Claim("UserId", UserId));
                permClaims.Add(new Claim("role", Role));
                permClaims.Add(new Claim("CurrentFY", CurrentFY));
                permClaims.Add(new Claim("LastloginDate", LastloginDate));

                var token = new JwtSecurityToken(issuer,
                            issuer,
                            permClaims,
                            expires: DateTime.Now.AddMinutes(15),
                            signingCredentials: credentials);
                var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
                return jwt_token;
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpGet]
        // [Authorize]
        //[Route("Api/GetBudgetYear")]
        public IHttpActionResult GetYear()
        {

            if (User.Identity.IsAuthenticated)
            {
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    res = objBAL.GetYear();

                    var response = new
                    {
                        statuscode = "100",
                        message = "Data Found",
                        data = res.Tables[0]

                    };
                    return Ok(response);
                }
            }
            else
            {
                return BadRequest();
            }
            return null;
        }

        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [Route("Api/GetBusinesslevel")]
        public IHttpActionResult GetBusinesslevel(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        DataSet res = new DataSet();
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Main_Group = table["Main_Group"];
                        objEntity.Subgroup = table["Subgroup"];
                        objEntity.Department = table["Department"];
                        //objEntity.Main_Group = Main_Group;
                        //objEntity.Subgroup = Subgroup;
                        //objEntity.Department = Department;


                        res = objBAL.GetBusinesslevel(objEntity);

                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = new
                            {
                                Main_Group = res.Tables[1],
                                Sub_Group = res.Tables[2],
                                Department = res.Tables[3],
                                Role = res.Tables[4],
                            },
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };

                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [Route("Api/GetLocation")]
        public IHttpActionResult GetLocation(JObject objData)
        {
            try
            {
                var req = Request;
                var headers = req.Headers;
                string token = headers.GetValues("Authorization").First();
                var handler = new JwtSecurityTokenHandler();
                string authHeader = token;
                authHeader = authHeader.Replace("Bearer ", "");
                authHeader = authHeader.Replace("\"", "");
                var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                objEntity.UserId = User_Id;
                res = objBAL.GetTokenByUserId(objEntity);
                string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                string token_ = token.Replace("Bearer", "");
                string Header_Token = token_.Trim();
                if (Header_Token == DB_Token)
                {
                    dynamic item = objData;
                    string data = item["data"];
                    objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                    string json = objEntity.Data;
                    var jss = new JavaScriptSerializer();
                    var table = jss.Deserialize<dynamic>(json);
                    objEntity.Mega_Zone = table["Mega_Zone"];
                    objEntity.Zone = table["Zone"];
                    objEntity.Region = table["Region"];
                    res = objBAL.GetLocation(objEntity);

                    objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                    objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                    var response = new
                    {
                        statuscode = objEntity.stauscode,
                        message = objEntity.message,
                        data = new
                        {
                            Mega_Zone = res.Tables[1],
                            Zone = res.Tables[2],
                            Region = res.Tables[3],
                            Branch = res.Tables[4],
                        },
                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };

                    return Ok(response1);
                }
                else
                {
                    var response = new
                    {
                        statuscode = 404,
                        message = "Invalid Token",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.NotAcceptable, response1);
                }
            }

            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }


        [HttpGet]
        //[Authorize]
        //[AllowAnonymous]
        //[Route("Api/GetLocationMaster1")]
        public IHttpActionResult GetLocationMaster1()
        {

            if (User.Identity.IsAuthenticated)
            {
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    res = objBAL.GetLocationMaster1();
                    objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                    objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                    var response = new
                    {
                        statuscode = objEntity.stauscode,
                        message = objEntity.message,
                        data = res.Tables[1]

                    };
                    return Ok(response);
                }
            }
            else
            {
                return BadRequest();
            }
            return null;
        }


        [HttpPost]
        //[Authorize]
        //[AllowAnonymous]
        //[Route("Api/InsertBudget1")]
        public IHttpActionResult InsertBudget1(JObject objData)
        {
            dynamic item = objData;
            string data = item["data"];
            objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
            string json = objEntity.Data;
            var jss = new JavaScriptSerializer();
            var table = jss.Deserialize<dynamic>(json);
            objEntity.Main_Group = table["maingrp"];
            objEntity.Subgroup = table["subgrp"];
            objEntity.Department = table["deptgrp"];
            objEntity.Role = table["rolegrp"];
            objEntity.Region = table["region"];
            objEntity.Branch = table["branch"];
            objEntity.Zone = table["zone"];
            objEntity.Mega_Zone = table["megazone"];
            objEntity.Budget = table["budget"];
            objEntity.Budgetyear = table["budgetyear"];
            objEntity.Buffer = table["buffer"];

            res = objBAL.InsertBudget(objEntity);

            objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
            objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

            var response = new
            {

                data = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,
                },
            };
            return Ok(response);
        }


        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [Route("Api/GetBudgetDetails")]
        public IHttpActionResult BudgetDetails(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {

                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Main_Group = table["maingrp"];
                        objEntity.Subgroup = table["subgrp"];
                        objEntity.Department = table["deptgrp"];
                        objEntity.Role = table["rolegrp"];
                        objEntity.Region = table["region"];
                        objEntity.Branch = table["branch"];
                        objEntity.Zone = table["zone"];
                        objEntity.Mega_Zone = table["megazone"];
                        objEntity.Type = table["type"];

                        res = objBAL.GetBudgetDetails(objEntity);
                        var response = new
                        {
                            statuscode = "100",
                            message = "Data Found",
                            data = res.Tables[1],
                            Details = res.Tables[2]
                        };

                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };

                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }

            }

            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }


        [HttpGet]
        //[Authorize]
        //[Route("Api/GetBranchDetails")]
        public IHttpActionResult BranchDetils()
        {

            if (User.Identity.IsAuthenticated)
            {
                var identity = User.Identity as ClaimsIdentity;

                if (identity != null)
                {
                    res = objBAL.BranchDetils();
                    objEntity.stauscode = Convert.ToInt32(res.Tables[1].Rows[0]["Code"]);
                    objEntity.message = res.Tables[1].Rows[0]["message"].ToString();
                    var response = new
                    {

                        data = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            BranchDetails = res.Tables[0]
                        },
                    };
                    return Ok(response);
                }
            }
            else
            {
                return BadRequest();
            }
            return null;
        }

        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [Route("Api/InsertZoneMaster")]
        public IHttpActionResult InsertZone(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {

                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];

                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Region = Convert.ToString(table["region"]);
                        objEntity.Branch = Convert.ToString(table["branchname"]);
                        objEntity.Zone = Convert.ToString(table["zone"]);
                        objEntity.Mega_Zone = Convert.ToString(table["megazone"]);
                        objEntity.LocationId = Convert.ToString(table["LocationId"]);

                        res = objBAL.InsertZone(objEntity);

                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {

                            data = new
                            {
                                statuscode = objEntity.stauscode,
                                message = objEntity.message,
                            },
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };

                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }

            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/InsertBudget")]
        [Authorize]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> InsertBudget()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        DataSet dres = new DataSet();
                        var httpRequest = HttpContext.Current.Request;
                        string data = httpRequest.Form["data"];
                        objEntity.CreatedBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);

                        objEntity.Main_Group = table["maingrp"];
                        objEntity.Subgroup = table["subgrp"];
                        objEntity.Department = table["deptgrp"];
                        objEntity.Role = table["rolegrp"];
                        objEntity.Region = table["region"];
                        objEntity.Branch = table["branch"];
                        objEntity.Zone = table["zone"];
                        objEntity.Mega_Zone = table["megazone"];
                        objEntity.Budget = table["budget"];
                        objEntity.Budgetyear = table["budgetyear"];
                        objEntity.Buffer = table["buffer"];
                        objEntity.Grade = table["Grade"];
                        objEntity.Remark = table["Remark"];
                        objEntity.file = httpRequest.Form["Files"];
                        //  objEntity.CreatedBy = table["dBy"];
                        DataSet ds = new DataSet();
                        if (objEntity.file != "")
                        {
                            foreach (string file in httpRequest.Files)
                            {
                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                                var postedFile = httpRequest.Files[file];

                                if (postedFile != null && postedFile.ContentLength > 0)
                                {
                                    int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB  
                                    string[] supportedTypes = { "txt", "doc", "docx", "pdf", "xls", "xlsx", "jpeg", "jpg", "odt", "png" };
                                    var fileExt = System.IO.Path.GetExtension(postedFile.FileName).Substring(1);
                                    int count = postedFile.FileName.Split('.').Length - 1;
                                    if (count > 1)
                                    {
                                        var message = string.Format("Double Extension File not Allowed");
                                        data = AESEncrytDecry.Encryptstring(message);
                                        return Request.CreateResponse(HttpStatusCode.Created, data);
                                    }
                                    else if (!Array.Exists(supportedTypes, Element => Element == fileExt))
                                    {
                                        var message = string.Format("File extension is invalid");
                                        dict.Add("error", message);
                                        data = AESEncrytDecry.Encryptstring(message);
                                        return Request.CreateResponse(HttpStatusCode.Created, data);
                                    }
                                    else if (postedFile.ContentLength > MaxContentLength)
                                    {
                                        var message = string.Format("Please Upload a file upto 1 mb.");
                                        dict.Add("error", message);
                                        data = AESEncrytDecry.Encryptstring(message);
                                        return Request.CreateResponse(HttpStatusCode.Created, data);
                                    }
                                    else
                                    {
                                        string savePath = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
                                        string savePathLocation = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
                                        //string savePathLocationDB = @"/ImageUploads/";
                                        var filePath = HttpContext.Current.Server.MapPath("~/ImageUploads/" + postedFile.FileName);
                                        //objEntity.file = (savePathLocationDB + postedFile.FileName.Replace(" ", "_").Replace("%", "_percentage_").Replace("&", "_and_"));
                                        postedFile.SaveAs(filePath);
                                        objEntity.file = "/ImageUploads/" + postedFile.FileName;
                                        ds = objBAL.InsertBudget(objEntity);
                                    }

                                }
                                else
                                {
                                    objEntity.file = "";
                                    ds = objBAL.InsertBudget(objEntity);
                                }

                            }

                        }
                        else
                        {
                            objEntity.file = "";
                            ds = objBAL.InsertBudget(objEntity);
                        }

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(ds.GetXml());

                        var message1 = JsonConvert.SerializeXmlNode(doc);
                        dict.Add("data", message1);
                        //return Request.CreateResponse(HttpStatusCode.Created, dict);
                        var jsonString = JsonConvert.SerializeObject(message1);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };

                        return Request.CreateResponse(HttpStatusCode.Created, response1);

                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };

                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, response1);
                }
            }

            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/GetBudgetStaggering")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetBudgetStaggering(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Year = Convert.ToString(table["Year"]);
                        objEntity.ID = Convert.ToString(table["PID"]);

                        res = objBAL.GetBudgetStaggering(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1]
                        };

                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };

                        return Ok(response1);
                    }

                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/addBudgetStaggering")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult addBudgetStaggering(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.CreatedBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Date = Convert.ToString(table["Date"]);
                        objEntity.Year = Convert.ToString(table["Year"]);
                        objEntity.Budget = Convert.ToString(table["ActivateBudget"]);
                        objEntity.ID = Convert.ToString(table["PID"]);
                        //   objEntity.CreatedBy = Convert.ToString(table["CreatedBy"]);


                        res = objBAL.addBudgetStaggering(objEntity);

                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        objEntity.ID = res.Tables[0].Rows[0]["PId"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            PId = objEntity.ID
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };

                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/ChangeDefaultBudgetYear")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult ChangeDefaultBudgetYear(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {

                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.ModifyBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.CurrentYear = Convert.ToString(table["CurrentYear"]);
                        objEntity.NextYear = Convert.ToString(table["NextYear"]);
                        objEntity.Main_Group = Convert.ToString(table["maingrp"]);
                        objEntity.Subgroup = Convert.ToString(table["subgrp"]);
                        objEntity.Department = Convert.ToString(table["deptgrp"]);
                        objEntity.Role = Convert.ToString(table["rolegrp"]);
                        objEntity.Region = Convert.ToString(table["region"]);
                        objEntity.Branch = Convert.ToString(table["branch"]);
                        objEntity.Zone = Convert.ToString(table["zone"]);
                        objEntity.Mega_Zone = Convert.ToString(table["megazone"]);
                        // objEntity.ModifyBy = Convert.ToString(table["ModifyBy"]);
                        res = objBAL.ChangeDefaultBudgetYear(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message
                        };

                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };

                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/UpdateBudget")]
        [Authorize]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> UpdateBudget()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + "Inside Try ");
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        DataSet dres = new DataSet();
                        var httpRequest = HttpContext.Current.Request;
                        string data = httpRequest.Form["data"];
                        objEntity.ModifyBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.ID = table["PId"];
                        objEntity.Budget = table["budget"];
                        objEntity.Budgetyear = table["budgetyear"];
                        objEntity.Buffer = table["buffer"];
                        objEntity.Remark = table["Remark"];
                        //  objEntity.Grade = table["Grade"];
                        objEntity.file = httpRequest.Form["Files"];

                        //  objEntity.ModifyBy = Convert.ToString(table["ModifyBy"]);
                        DataSet ds = new DataSet();
                        if (objEntity.file != "")
                        {
                            foreach (string file in httpRequest.Files)
                            {
                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                                var postedFile = httpRequest.Files[file];
                                string Post_File = postedFile.ToString();
                                if (postedFile != null && postedFile.ContentLength > 0)
                                {
                                    int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB  
                                    string[] supportedTypes = { "txt", "doc", "docx", "pdf", "xls", "xlsx", "jpeg", "jpg", "odt", "png" };
                                    string savePath_ = HttpContext.Current.Server.MapPath(@"~/ImageUploads/" + postedFile.FileName);
                                    var fileExt = System.IO.Path.GetExtension(postedFile.FileName).Substring(1);
                                    int count = postedFile.FileName.Split('.').Length - 1;

                                    //char ch = '.';
                                    //int freq = postedFile.FileName.Where(x => (x == ch)).Count();
                                    if (count > 1)
                                    {
                                        var message = string.Format("Double Extension File not Allowed");
                                        data = AESEncrytDecry.Encryptstring(message);
                                        return Request.CreateResponse(HttpStatusCode.Created, data);
                                    }
                                    else if (!Array.Exists(supportedTypes, Element => Element == fileExt))
                                    {

                                        var message = string.Format("File extension is invalid");
                                        data = AESEncrytDecry.Encryptstring(message);
                                        return Request.CreateResponse(HttpStatusCode.Created, data);
                                    }
                                    else if (postedFile.ContentLength > MaxContentLength)
                                    {

                                        var message = string.Format("Please Upload a file upto 1 mb.");
                                        data = AESEncrytDecry.Encryptstring(message);
                                        return Request.CreateResponse(HttpStatusCode.Created, data);
                                    }
                                    else
                                    {
                                        string savePath = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
                                        string savePathLocation = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
                                        //string savePathLocationDB = @"~/ImageUploads/";
                                        var filePath = HttpContext.Current.Server.MapPath("~/ImageUploads/" + postedFile.FileName);
                                        //objEntity.file = (savePathLocationDB + postedFile.FileName.Replace(" ", "_").Replace("%", "_percentage_").Replace("&", "_and_"));

                                        postedFile.SaveAs(filePath);
                                        objEntity.file = "/ImageUploads/" + postedFile.FileName;
                                        ds = objBAL.UpdateBudget(objEntity);
                                    }


                                }

                                else
                                {
                                    objEntity.file = "";
                                    ds = objBAL.UpdateBudget(objEntity);
                                }


                            }

                        }
                        else
                        {
                            objEntity.file = "";
                            ds = objBAL.UpdateBudget(objEntity);
                        }

                        objEntity.file = "";
                        ds = objBAL.UpdateBudget(objEntity);

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(ds.GetXml());
                        var message1 = JsonConvert.SerializeXmlNode(doc);
                        dict.Add("data", message1);
                        var jsonString = JsonConvert.SerializeObject(dict);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Request.CreateResponse(HttpStatusCode.Created, response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
                // return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
        }

        [HttpPost]
        [Route("Api/UpadateStaggering")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult UpadateStaggering(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.ModifyBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.ID = Convert.ToString(table["PId"]);
                        objEntity.Budget = Convert.ToString(table["ActiveBudget"]);
                        //  objEntity.ModifyBy = Convert.ToString(table["ModifyBy"]);
                        res = objBAL.UpadateStaggering(objEntity);

                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",
                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/DeleteStaggering")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult DeleteStaggering(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.ID = Convert.ToString(table["PId"]);
                        objEntity.DeletedBy = Convert.ToString(table["DeletedBy"]);
                        res = objBAL.DeleteStaggering(objEntity);

                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }


            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/GetRemarks")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetRemarks(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.ID = Convert.ToString(table["RemarkId"]);
                        res = objBAL.GetRemarks(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1]
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/GetBudgetAttachment")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetBudgetAttachment(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.ID = Convert.ToString(table["PId"]);
                        res = objBAL.GetBudgetAttachment(objEntity);
                        string AttachmentURL = WebConfigurationManager.AppSettings["AttachmentURL"];
                        Byte[] bytes = File.ReadAllBytes(AttachmentURL + res.Tables[1].Rows[0]["File"].ToString());
                        String file = Convert.ToBase64String(bytes);
                        string Filename = (res.Tables[1].Rows[0]["File"].ToString());
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = file,
                            Filename = Filename
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/GetGradeDetails")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetGradeDetails(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.OFFSET = table["OFFSET"];
                        objEntity.LIMIT = table["LIMIT"];
                        res = objBAL.GetGradeDetails(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1]
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/GetRAId")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetRAId(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);

                        objEntity.OFFSET = table["OFFSET"];
                        objEntity.LIMIT = table["LIMIT"];
                        objEntity.EmpName = table["EmpName"];
                        res = objBAL.GetRAId(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1],
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }

            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/InsertIndent")]
        [AllowAnonymous]
        public IHttpActionResult InsertIndent(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.CreatedBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.RefBMId = table["RefBMID"];
                        objEntity.RefEMPID = table["RefEMPID"];
                        objEntity.Emp_Count = table["EmpCount"];
                        //   objEntity.CreatedBy = table["CreatedBy"];
                        objEntity.Reason = table["Reason"];
                        objEntity.IndentType = table["IndentType"];
                        objEntity.Grade = table["Grade"];
                        objEntity.Remark = table["Remark"];
                        res = objBAL.InsertIndent(objEntity);

                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/GetIndentDetails")]
        [AllowAnonymous]
        public IHttpActionResult GetIndentDetails(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {

                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.OFFSET = table["OFFSET"];
                        objEntity.LIMIT = table["LIMIT"];
                        res = objBAL.GetIndentDetails(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1],
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/GetBudgetMappingDetails")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetBudgetMappingDetails(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.RefBMId = table["RefBMId"];
                        res = objBAL.GetBudgetMappingDetails(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1],

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/GetIndentTypeMaster")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetIndentTypeMaster(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        res = objBAL.GetIndentTypeMaster(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1],
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/ImportBusinesslevelMapping")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult ImportBusinesslevelMapping()
        {
            var httpRequest = HttpContext.Current.Request;
            //string data = httpRequest.Form["data"];
            //objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
            //string json = objEntity.Data;
            //var jss = new JavaScriptSerializer();
            //var table = jss.Deserialize<dynamic>(json);
            objEntity.CreatedBy = httpRequest.Form["CreatedBy"];
            DataTable csvData = new DataTable();
            string filePath = "";
            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    filePath = HttpContext.Current.Server.MapPath("~/MappingFiles/" + "BussinessLevelMapping.csv");
                    postedFile.SaveAs(filePath);
                    docfiles.Add(filePath);
                }

            }
            else
            {

            }

            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(filePath))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();

                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                    DataTable db = objBAL.ImportBusinesslevelMapping(csvData, objEntity);

                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
            var response = new
            {

                data = csvData
            };
            var jsonString = JsonConvert.SerializeObject(response);
            var response1 = new
            {
                data = AESEncrytDecry.Encryptstring(jsonString)
            };
            return Ok(response1);


        }

        [HttpPost]
        [Route("Api/ImportLocationlevelMapping")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult ImportLocationlevelMapping()
        {

            DataTable csvData = new DataTable();
            var httpRequest = HttpContext.Current.Request;
            //string data = httpRequest.Form["data"];
            //objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
            //string json = objEntity.Data;
            //var jss = new JavaScriptSerializer();
            //var table = jss.Deserialize<dynamic>(json);
            objEntity.CreatedBy = httpRequest.Form["CreatedBy"];

            string filePath = "";
            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    filePath = HttpContext.Current.Server.MapPath("~/MappingFiles/" + "Locationlevelmapping.csv");
                    postedFile.SaveAs(filePath);
                    docfiles.Add(filePath);
                }

            }
            else
            {

            }

            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(filePath))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();

                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                    DataTable db = objBAL.ImportLocationlevelMapping(csvData, objEntity);

                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
            var response = new
            {

                data = csvData
            };
            var jsonString = JsonConvert.SerializeObject(response);
            var response1 = new
            {
                data = AESEncrytDecry.Encryptstring(jsonString)
            };
            return Ok(response1);

        }

        [HttpGet]
        [Route("Api/ExportCsvFile")]
        [Authorize]
        [AllowAnonymous]
        public void ExportCsvFile(JObject objData)
        {

            try
            {
                DataTable dt = objBAL.ExportCsvFile(objEntity);

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Buffer = true;
                HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=Exportfile.csv");
                HttpContext.Current.Response.ContentType = "text/csv";


                StringBuilder sb = new StringBuilder();
                for (int k = 0; k < dt.Columns.Count; k++)
                {
                    //add separator
                    sb.Append(dt.Columns[k].ColumnName + ',');
                }
                //append new line
                sb.Append("\r\n");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int k = 0; k < dt.Columns.Count; k++)
                    {
                        //add separator
                        sb.Append(dt.Rows[i][k].ToString().Replace(",", ";") + ',');
                    }
                    //append new line
                    sb.Append("\r\n");
                }
                HttpContext.Current.Response.Output.Write(sb.ToString());
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpPost]
        [Route("Api/ImportResponseCsvFile")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult ImportResponseCsvFile(JObject objData)
        {
            DataTable csvData = new DataTable();
            string csv_file_path = HttpContext.Current.Server.MapPath("~/MappingFiles/" + "StudentDetails.csv");

            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();

                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                    DataTable db = objBAL.ImportResponseCsvFile(csvData);

                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
            var response = new
            {

                data = csvData
            };
            var jsonString = JsonConvert.SerializeObject(response);
            var response1 = new
            {
                data = AESEncrytDecry.Encryptstring(jsonString)
            };
            return Ok(response1);
        }

        [HttpPost]
        [Route("Api/GetLocationMaster")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetLocationMaster(JObject objData)
        {

            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);

                        objEntity.OFFSET = table["OFFSET"];
                        objEntity.LIMIT = table["LIMIT"];
                        res = objBAL.GetLocationMaster(objEntity);

                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1]
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/GetBusinessMaster")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetBusinessMaster(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.OFFSET = table["OFFSET"];
                        objEntity.LIMIT = table["LIMIT"];
                        res = objBAL.GetBusinessMaster(objEntity);

                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1]
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/UpdateIndent")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult UpadateIndent(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.ModifyBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.RefIndentId = table["RefIndentId"];
                        objEntity.Jobcode = table["Jobcode"];
                        objEntity.Location = table["Location"];
                        objEntity.Grade = table["Grade"];
                        objEntity.RA = table["RA"];
                        objEntity.Reason = table["Reason"];
                        objEntity.Remark = table["Remark"];
                        //objEntity.ModifyBy = table["ModifyBy"];
                        res = objBAL.UpadateIndent(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/InsertLocationMapping")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult InsertLocationMapping(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Data = table["Data"];
                        objEntity.Type = table["Type"];
                        res = objBAL.InsertLocationMapping(objEntity);
                        var response = res.Tables[0];
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/UpdateLocationMapping")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult UpdateLocationMapping(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.RefId = table["RefId"];
                        objEntity.Data = table["Data"];
                        objEntity.Type = table["Type"];
                        res = objBAL.UpdateLocationMapping(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/GetLocationMapping")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetLocationMapping(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Type = table["Type"];
                        objEntity.OFFSET = table["OFFSET"];
                        objEntity.LIMIT = table["LIMIT"];
                        res = objBAL.GetLocationMapping(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1]

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/InsertZoneMasterMappingData")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult InsertZoneMasterMappingData(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        objEntity.CreatedBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Mega_Zone = table["Megazone"];
                        objEntity.Zone = table["Zone"];
                        objEntity.Region = table["Region"];
                        objEntity.Branch = table["Branch"];
                        //  objEntity.CreatedBy = table["CreatedBy"];
                        res = objBAL.InsertZoneMasterMappingData(objEntity);

                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpPost]
        [Route("Api/InsertManualIndent")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult InsertManualIndent(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.CreatedBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Main_Group = Convert.ToString(table["Maingroup"]);
                        objEntity.Subgroup = Convert.ToString(table["Subgroup"]);
                        objEntity.Department = Convert.ToString(table["Department"]);
                        objEntity.Role = Convert.ToString(table["Role"]);
                        objEntity.Mega_Zone = Convert.ToString(table["Megazone"]);
                        objEntity.Zone = Convert.ToString(table["Zone"]);
                        objEntity.Region = Convert.ToString(table["Region"]);
                        objEntity.Branch = Convert.ToString(table["Branch"]);
                        objEntity.Grade = Convert.ToString(table["Grade"]);
                        objEntity.RA = Convert.ToString(table["RA"]);
                        objEntity.Year = Convert.ToString(table["Year"]);
                        objEntity.Reason = Convert.ToString(table["Reason"]);
                        objEntity.Remark = Convert.ToString(table["Remark"]);
                        //  objEntity.CreatedBy = Convert.ToString(table["CreatedBy"]);

                        res = objBAL.InsertManualIndent(objEntity);
                        var response = res.Tables[0];

                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);

                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }


        [HttpPost]
        [Route("Api/GetRole")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetRole(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.OFFSET = table["OFFSET"];
                        objEntity.LIMIT = table["LIMIT"];
                        res = objBAL.GetRole(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1]
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }


        [HttpPost]
        [Route("Api/GetMaingroupSubgropDepartment")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetMaingroupSubgropDepartment(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Main_Group = table["Maingroup"];
                        objEntity.Subgroup = table["Subgroup"];
                        res = objBAL.GetMaingroupSubgropDepartment(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1]
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }


        [HttpPost]
        [Route("Api/InsertBusinessMappingData")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult InsertBusinessMappingData(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.CreatedBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Main_Group = table["Maingroup"];
                        objEntity.Subgroup = table["Subgroup"];
                        objEntity.Department = table["Dept"];
                        objEntity.Jobcode = table["Jobcode"];
                        objEntity.Description = table["Description"];
                        objEntity.DescrShort = table["Descshort"];
                        // objEntity.CreatedBy = table["CreatedBy"];
                        res = objBAL.InsertBusinessMappingData(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = res.Tables[0];

                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }


        [HttpPost]
        [Route("Api/UpdateJobCodeInBusinessMapping")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult UpdateJobCodeInBusinessMapping(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);

                        objEntity.RefId = table["RefId"];
                        objEntity.Jobcode = table["Jobcode"];
                        objEntity.Description = table["Description"];
                        objEntity.DescrShort = table["Descrshort"];
                        res = objBAL.UpdateJobCodeInBusinessMapping(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpGet]
        [Route("Api/IndentReport")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult IndentReport(string FromDate, string ToDate, string Location = "", string IndentType = "", string RequestorName = "")
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        string AttachmentURL = WebConfigurationManager.AppSettings["AttachmentURL"];
                        string Filename = "MappingFiles\\Indent_Report_File.csv";
                        var Filepath = string.Concat(AttachmentURL, Filename);

                        string strFilePath = WebConfigurationManager.AppSettings["WriteIndentCsvFile"];
                        objEntity.FromDate = FromDate;
                        objEntity.ToDate = ToDate;
                        objEntity.Location = Location;
                        objEntity.IndentType = IndentType;
                        objEntity.RequestorName = RequestorName;
                        DataTable dtDataTable = objBAL.IndentReport(objEntity);
                        StreamWriter sw = new StreamWriter(strFilePath, false);
                        for (int i = 0; i < dtDataTable.Columns.Count; i++)
                        {
                            sw.Write(dtDataTable.Columns[i]);
                            if (i < dtDataTable.Columns.Count - 1)
                            {
                                sw.Write(",");
                            }
                        }
                        sw.Write(sw.NewLine);
                        foreach (DataRow dr in dtDataTable.Rows)
                        {
                            for (int i = 0; i < dtDataTable.Columns.Count; i++)
                            {
                                if (!Convert.IsDBNull(dr[i]))
                                {
                                    string value = dr[i].ToString();

                                    if (value.Contains(','))
                                    {
                                        value = String.Format("\"{0}\"", value);
                                        sw.Write(value);
                                    }
                                    else
                                    {
                                        sw.Write(dr[i].ToString());
                                    }
                                }
                                if (i < dtDataTable.Columns.Count - 1)
                                {
                                    sw.Write(",");
                                }
                            }
                            sw.Write(sw.NewLine);
                        }
                        sw.Close();
                        objEntity.stauscode = 100;
                        objEntity.message = "Record Found";
                        Byte[] bytes = File.ReadAllBytes(Filepath);
                        String File_path = Convert.ToBase64String(bytes);
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            Filepath = File_path,
                            Filename = Filename
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpGet]
        [Route("Api/BudgetingReport")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult BudgetingReport(string FromDate, string ToDate, string Location = "")
        {
            try
            {
                string AttachmentURL = WebConfigurationManager.AppSettings["AttachmentURL"];
                string Filename = "MappingFiles\\Budget_Report_File.csv";
                var Filepath = string.Concat(AttachmentURL, Filename);

                string strFilePath = WebConfigurationManager.AppSettings["WriteBudgetCsvFile"];
                objEntity.FromDate = FromDate;
                objEntity.ToDate = ToDate;
                objEntity.Location = Location;
                DataTable dtDataTable = objBAL.BudgetingReport(objEntity);
                StreamWriter sw = new StreamWriter(strFilePath, false);
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    sw.Write(dtDataTable.Columns[i]);
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
                foreach (DataRow dr in dtDataTable.Rows)
                {
                    for (int i = 0; i < dtDataTable.Columns.Count; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            string value = dr[i].ToString();

                            if (value.Contains(','))
                            {
                                value = String.Format("\"{0}\"", value);
                                sw.Write(value);
                            }
                            else
                            {
                                sw.Write(dr[i].ToString());
                            }
                        }
                        if (i < dtDataTable.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);
                }
                sw.Close();
                objEntity.stauscode = 100;
                objEntity.message = "Record Found";
                Byte[] bytes = File.ReadAllBytes(Filepath);
                String File_path = Convert.ToBase64String(bytes);
                var response = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,
                    Filepath = File_path,
                    Filename = Filename
                };
                var jsonString = JsonConvert.SerializeObject(response);
                var response1 = new
                {
                    data = AESEncrytDecry.Encryptstring(jsonString)
                };
                return Ok(response1);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpGet]
        [Route("Api/GradeReport")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GradeReport(string FromDate, string ToDate, string Grade = "")
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {

                    string AttachmentURL = WebConfigurationManager.AppSettings["AttachmentURL"];
                    string Filename = "MappingFiles\\Grade_Report_File.csv";
                    var Filepath = string.Concat(AttachmentURL, Filename);

                    string strFilePath = WebConfigurationManager.AppSettings["WriteGradeCsvFile"];
                    objEntity.FromDate = FromDate;
                    objEntity.ToDate = ToDate;
                    objEntity.Grade = Grade;
                    DataTable dtDataTable = objBAL.GradeReport(objEntity);
                    StreamWriter sw = new StreamWriter(strFilePath, false);
                    for (int i = 0; i < dtDataTable.Columns.Count; i++)
                    {
                        sw.Write(dtDataTable.Columns[i]);
                        if (i < dtDataTable.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);
                    foreach (DataRow dr in dtDataTable.Rows)
                    {
                        for (int i = 0; i < dtDataTable.Columns.Count; i++)
                        {
                            if (!Convert.IsDBNull(dr[i]))
                            {
                                string value = dr[i].ToString();

                                if (value.Contains(','))
                                {
                                    value = String.Format("\"{0}\"", value);
                                    sw.Write(value);
                                }
                                else
                                {
                                    sw.Write(dr[i].ToString());
                                }
                            }
                            if (i < dtDataTable.Columns.Count - 1)
                            {
                                sw.Write(",");
                            }
                        }
                        sw.Write(sw.NewLine);
                    }
                    sw.Close();
                    objEntity.stauscode = 100;
                    objEntity.message = "Record Found";
                    Byte[] bytes = File.ReadAllBytes(Filepath);
                    String File_path = Convert.ToBase64String(bytes);
                    var response = new
                    {
                        statuscode = objEntity.stauscode,
                        message = objEntity.message,
                        Filepath = File_path,
                        Filename = Filename
                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Ok(response1);
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpGet]
        [Route("Api/LocationReport")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult LocationReport(string FromDate, string ToDate, string Location = "")
        {
            try
            {
                string AttachmentURL = WebConfigurationManager.AppSettings["AttachmentURL"];
                string Filename = "MappingFiles\\Location_Report_File.csv";
                var Filepath = string.Concat(AttachmentURL, Filename);

                string strFilePath = WebConfigurationManager.AppSettings["WriteLocationCsvFile"];
                objEntity.FromDate = FromDate;
                objEntity.ToDate = ToDate;
                objEntity.Location = Location;
                DataTable dtDataTable = objBAL.LocationReport(objEntity);
                StreamWriter sw = new StreamWriter(strFilePath, false);
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    sw.Write(dtDataTable.Columns[i]);
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
                foreach (DataRow dr in dtDataTable.Rows)
                {
                    for (int i = 0; i < dtDataTable.Columns.Count; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            string value = dr[i].ToString();

                            if (value.Contains(','))
                            {
                                value = String.Format("\"{0}\"", value);
                                sw.Write(value);
                            }
                            else
                            {
                                sw.Write(dr[i].ToString());
                            }
                        }
                        if (i < dtDataTable.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);
                }
                sw.Close();
                objEntity.stauscode = 100;
                objEntity.message = "Record Found";

                Byte[] bytes = File.ReadAllBytes(Filepath);
                String File_path = Convert.ToBase64String(bytes);
                var response = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,
                    Filepath = File_path,
                    Filename = Filename
                };
                var jsonString = JsonConvert.SerializeObject(response);
                var response1 = new
                {
                    data = AESEncrytDecry.Encryptstring(jsonString)
                };
                return Ok(response1);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpGet]
        [Route("Api/JobcodeReport")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult JobcodeReport(string FromDate, string ToDate, string Jobcode = "")
        {
            try
            {
                string AttachmentURL = WebConfigurationManager.AppSettings["AttachmentURL"];
                string Filename = "MappingFiles\\Jobcode_Report_File.csv";
                var Filepath = string.Concat(AttachmentURL, Filename);

                string strFilePath = WebConfigurationManager.AppSettings["WriteJobcodeCsvFile"];
                objEntity.FromDate = FromDate;
                objEntity.ToDate = ToDate;
                objEntity.Jobcode = Jobcode;
                DataTable dtDataTable = objBAL.JobcodeReport(objEntity);
                StreamWriter sw = new StreamWriter(strFilePath, false);
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    sw.Write(dtDataTable.Columns[i]);
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
                foreach (DataRow dr in dtDataTable.Rows)
                {
                    for (int i = 0; i < dtDataTable.Columns.Count; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            string value = dr[i].ToString();

                            if (value.Contains(','))
                            {
                                value = String.Format("\"{0}\"", value);
                                sw.Write(value);
                            }
                            else
                            {
                                sw.Write(dr[i].ToString());
                            }
                        }
                        if (i < dtDataTable.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);
                }
                sw.Close();
                objEntity.stauscode = 100;
                objEntity.message = "Record Found";

                Byte[] bytes = File.ReadAllBytes(Filepath);
                String File_path = Convert.ToBase64String(bytes);
                var response = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,
                    Filepath = File_path,
                    Filename = Filename
                };
                var jsonString = JsonConvert.SerializeObject(response);
                var response1 = new
                {
                    data = AESEncrytDecry.Encryptstring(jsonString)
                };
                return Ok(response1);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpGet]
        [Route("Api/BusinessLevelReport")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult BusinessLevelReport(string FromDate, string ToDate)
        {
            try
            {
                string AttachmentURL = WebConfigurationManager.AppSettings["AttachmentURL"];
                string Filename = "MappingFiles\\Businesslevel_Report_File.csv";
                var Filepath = string.Concat(AttachmentURL, Filename);

                string strFilePath = WebConfigurationManager.AppSettings["WriteBusinesslevelCsvFile"];
                objEntity.FromDate = FromDate;
                objEntity.ToDate = ToDate;
                DataTable dtDataTable = objBAL.BusinessLevelReport(objEntity);
                StreamWriter sw = new StreamWriter(strFilePath, false);
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    sw.Write(dtDataTable.Columns[i]);
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
                foreach (DataRow dr in dtDataTable.Rows)
                {
                    for (int i = 0; i < dtDataTable.Columns.Count; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            string value = dr[i].ToString();

                            if (value.Contains(','))
                            {
                                value = String.Format("\"{0}\"", value);
                                sw.Write(value);
                            }
                            else
                            {
                                sw.Write(dr[i].ToString());
                            }
                        }
                        if (i < dtDataTable.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);
                }
                sw.Close();
                objEntity.stauscode = 100;
                objEntity.message = "Record Found";
                Byte[] bytes = File.ReadAllBytes(Filepath);
                String File_path = Convert.ToBase64String(bytes);
                var response = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,
                    Filepath = File_path,
                    Filename = Filename
                };
                var jsonString = JsonConvert.SerializeObject(response);
                var response1 = new
                {
                    data = AESEncrytDecry.Encryptstring(jsonString)
                };
                return Ok(response1);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpGet]
        [Route("Api/ErrorLogFilter")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult ErrorLogFilter(string FromDate, string ToDate, string Type)
        {
            try
            {
                string AttachmentURL = WebConfigurationManager.AppSettings["AttachmentURL"];
                string Filename = "MappingFiles\\Businesslevel_Report_File.csv";
                var Filepath = string.Concat(AttachmentURL, Filename);

                string strFilePath = WebConfigurationManager.AppSettings["WriteBusinesslevelCsvFile"];
                objEntity.Type = Type;

                objEntity.FromDate = FromDate;
                objEntity.ToDate = ToDate;
                DataTable dtDataTable = objBAL.ErrorLogFilter(objEntity);
                StreamWriter sw = new StreamWriter(strFilePath, false);
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    sw.Write(dtDataTable.Columns[i]);
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
                foreach (DataRow dr in dtDataTable.Rows)
                {
                    for (int i = 0; i < dtDataTable.Columns.Count; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            string value = dr[i].ToString();

                            if (value.Contains(','))
                            {
                                value = String.Format("\"{0}\"", value);
                                sw.Write(value);
                            }
                            else
                            {
                                sw.Write(dr[i].ToString());
                            }
                        }
                        if (i < dtDataTable.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);
                }
                sw.Close();
                objEntity.stauscode = 100;
                objEntity.message = "Record Found";

                var response = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,
                    Filepath = Filepath
                };
                var jsonString = JsonConvert.SerializeObject(response);
                var response1 = new
                {
                    data = AESEncrytDecry.Encryptstring(jsonString)
                };
                return Ok(response1);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpPost]
        [Route("Api/LDAPLOGIN")]
        public IHttpActionResult LDAPLOGIN(JObject objData)
        {
            dynamic item = objData;
            string User_Id = string.Empty;
            string Encrypt_password = string.Empty;
            string data = item["data"];
            objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
            string json = objEntity.Data;
            var jss = new JavaScriptSerializer();
            var table = jss.Deserialize<dynamic>(json);
            User_Id = table["UserId"];
            Encrypt_password = table["Password"];
            string UId = User_Id.Replace('"', ' ');
            string Pswd = Encrypt_password.Replace('"', ' ');
            string U_Id = UId.Trim();
            string P_swd = Pswd.Trim();
            if (LDAP_Authentication((U_Id), (P_swd)))
            {
                UtilityClass.ActivityLog("LDAP_Authentication :Sucess");

                string[] result = Bind_LDAP_For_IPRU(Convert.ToString(U_Id));
                UtilityClass.ActivityLog("LDAP_Authentication :Sucess2");

                var response = new
                {
                    statuscode = 200,
                    message = "LDAP Login Success",
                    Result = new
                    {
                        FullName = result[0],
                        EmpId = result[1],
                        EmailId = result[2],
                        MobileNo = result[3],
                        FirstName = result[4],
                        LastName = result[5]
                    }
                };
                var jsonString = JsonConvert.SerializeObject(response);
                var response1 = new
                {
                    data = AESEncrytDecry.Encryptstring(jsonString)
                };
                return Ok(response1);
            }
            else
            {
                var response = new
                {
                    statuscode = 400,
                    message = "Invalid Credentials"
                };
                var jsonString = JsonConvert.SerializeObject(response);
                var response1 = new
                {
                    data = AESEncrytDecry.Encryptstring(jsonString)
                };
                return Ok(response1);

            }
        }

        [HttpPost]
        [Route("Api/LDAP_Authentication")]
        [Authorize]
        [AllowAnonymous]
        public bool LDAP_Authentication(string User_Code, string Password)
        {
            string IsValidationLDAP = String.Empty;
            string LDAPURL = String.Empty;
            IsValidationLDAP = ConfigurationManager.AppSettings["IsValidationLDAP"].ToString();
            LDAPURL = ConfigurationManager.AppSettings["LDAPURL"].ToString();
            if (IsValidationLDAP != null && IsValidationLDAP != "" && IsValidationLDAP.ToLower() == "true")
            {
                UtilityClass.ActivityLog("LDAP_Authentication : LDAP IF PART IsValidationLDAP IS = " + IsValidationLDAP);
                if (LDAPURL != null && LDAPURL != "")
                {
                    UtilityClass.ActivityLog("LDAP_Authentication : LDAP INSIDE IF - IF PART URL IS = " + LDAPURL);
                    //string LdapPath = "LDAP://icicibankltd.com :3268/DC=icicibankltd,DC=com"; //"LDAP://hydaddc37.icicibankltd.com:636";
                    string LdapPath = LDAPURL; //"LDAP://hydaddc37.icicibankltd.com:636";
                    string domain = "icicibankltd";
                    string Errmsg = "True";
                    string domainAndUsername = domain + @"\" + User_Code;
                    DirectoryEntry entry = new DirectoryEntry(LdapPath, domainAndUsername, Password);
                    UtilityClass.ActivityLog("LDAP_Authentication : LDAP PARAMETERS LdapPath = " + LdapPath + " domain = " + domain + " Errmsg = " + Errmsg + " domainAndUsername = " + domainAndUsername + " UserId = " + User_Code);
                    UtilityClass.ActivityLog("LDAP_Authentication : LDAP PARAMETERS DirectoryEntry = " + entry);
                    try
                    {
                        UtilityClass.ActivityLog("LDAP_Authentication : LDAP inside try function");
                        //Bind to the native AdsObject to force authentication.
                        Object obj = entry.NativeObject;
                        DirectorySearcher search = new DirectorySearcher(entry);
                        search.Filter = "(SAMAccountName=" + User_Code + ")";
                        search.PropertiesToLoad.Add("cn");
                        SearchResult result = search.FindOne();
                        UtilityClass.ActivityLog("LDAP_Authentication : LDAP PARAMETERS search.Filter = " + search.Filter);
                        UtilityClass.ActivityLog("LDAP_Authentication : LDAP PARAMETERS search.PropertiesToLoad.Add = " + search.PropertiesToLoad);
                        UtilityClass.ActivityLog("LDAP_Authentication : LDAP PARAMETERS SearchResult result = " + result);
                        if (null == result)
                        {
                            //Response.Write("none");
                            UtilityClass.ActivityLog("LDAP_Authentication : LDAP if null == result return false");
                            return false;
                        }
                        // Update the new path to the user in the directory
                        LdapPath = result.Path;
                        string _filterAttribute = (String)result.Properties["cn"][0];
                        UtilityClass.ActivityLog("LDAP_Authentication : LDAP PARAMETERS LdapPath = " + LdapPath);
                        UtilityClass.ActivityLog("LDAP_Authentication : LDAP PARAMETERS _filterAttribute = " + _filterAttribute);
                    }
                    catch (Exception ex)
                    {
                        UtilityClass.ActivityLog("LDAP_Authentication : LDAP inside catch function");
                        //Error_log_writting(ex.StackTrace.ToString());
                        //sendMailUAT_icici_Error("", "ERROR:: AuthenticateUser", "EX: Username:: " + username + " //// Exception:::::: " + ex, "", "");
                        Errmsg = ex.Message;
                        UtilityClass.ActivityLog("LDAP_Authentication : LDAP inside catch function PARAMETERS Errmsg " + Errmsg);
                        UtilityClass.ActivityLog("LDAP_Authentication : LDAP inside catch function return false");
                        // Response.Write(ex.Message + "\n" + ex.StackTrace + "\n");
                        return false;
                        throw new Exception("Error authenticating user." + ex.Message);
                    }
                    return true;
                }
                else
                {
                    //Response.Write("none");
                    UtilityClass.ActivityLog("LDAP_Authentication : LDAP INSIDE IF - ELSE PART IsValidationLDAP IS = " + IsValidationLDAP);
                    UtilityClass.ActivityLog("LDAP_Authentication : LDAP INSIDE IF - ELSE PART URL IS = " + LDAPURL);
                    return false;
                }
            }
            else if (IsValidationLDAP != null && IsValidationLDAP != "" && IsValidationLDAP.ToLower() == "false")
            {
                UtilityClass.ActivityLog("LDAP_Authentication : LDAP ELSE IF PART IsValidationLDAP IS = " + IsValidationLDAP);
                UtilityClass.ActivityLog("LDAP_Authentication : LDAP ELSE IF PART URL IS = " + LDAPURL);
                if (Password == "dyna@3$")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                UtilityClass.ActivityLog("LDAP_Authentication : LDAP ELSE PART IsValidationLDAP IS = " + IsValidationLDAP);
                UtilityClass.ActivityLog("LDAP_Authentication : LDAP ELSE PART URL IS = " + LDAPURL);
                return false;
            }
        }

        [HttpPost]
        [Route("Api/Get_LDAP_UserInfo")]
        [Authorize]
        [AllowAnonymous]
        public string[] Bind_LDAP_For_IPRU(string userid)
        {
            string[] empdetails;
            try
            {
                UtilityClass.ActivityLog("Bind_LDAP_For_IPRU:Inside");
                string strLDAP = ConfigurationManager.AppSettings["LDAPURL"].ToString();
                DirectoryEntry m_obDirEntry = new DirectoryEntry(strLDAP, "", "");
                string strDept = string.Empty;
                empdetails = new string[1000];
                DirectorySearcher srch = new DirectorySearcher(m_obDirEntry);
                UtilityClass.ActivityLog("DirectorySearcher:" + srch);

                srch.Filter = "(samAccountName=" + userid + ")";
                SearchResultCollection results;
                results = srch.FindAll();
                UtilityClass.ActivityLog("Results of Ldap" + results);
                ResultPropertyCollection propColl;
                if (results != null)
                {
                    foreach (SearchResult result in results)
                    {
                        propColl = result.Properties;
                        string str;
                        foreach (string strKey in propColl.PropertyNames)
                        {
                            foreach (object obProp in propColl[strKey])
                            {
                                str = obProp.ToString().Trim();
                                if ((strKey.Trim().ToUpper() == "name".ToUpper()))
                                {
                                    if ((obProp.ToString() != ""))
                                    {
                                        if (Convert.ToString(obProp) == "System.Byte[]")
                                        {
                                            byte[] myByteArray = (byte[])result.Properties["name"][0];
                                            string name = Encoding.Default.GetString(myByteArray);
                                            empdetails[0] = name.ToString().Substring(0, ((name.ToString().IndexOf("/") + 1) - 2)).Trim();
                                            UtilityClass.ActivityLog("LDAP NAME :" + empdetails[0]);
                                            //strDept = name.ToString().Substring((name.ToString().IndexOf("/") + 1));
                                            //empdetails[1] = strDept.Substring(0, ((strDept.IndexOf("/") + 1)
                                            // - 1));
                                        }
                                        else
                                        {
                                            empdetails[0] = obProp.ToString().Substring(0, ((obProp.ToString().IndexOf("/") + 1) - 2)).Trim();
                                            UtilityClass.ActivityLog("LDAP NAME :" + empdetails[0]);
                                            //strDept = obProp.ToString().Substring((obProp.ToString().IndexOf("/") + 1));
                                            //empdetails[1] = strDept.Substring(0, ((strDept.IndexOf("/") + 1)
                                            // - 1));
                                        }
                                    }
                                }
                                if ((strKey.Trim().ToUpper() == "samaccountname".ToUpper()))
                                {
                                    if ((obProp.ToString() != ""))
                                    {
                                        if (Convert.ToString(obProp) == "System.Byte[]")
                                        {
                                            byte[] myByteArray = (byte[])result.Properties["samaccountname"][0];
                                            string samaccountname = Encoding.Default.GetString(myByteArray);
                                            //empdetails[4] = samaccountname;
                                            empdetails[1] = samaccountname;
                                            UtilityClass.ActivityLog("samaccountname :" + empdetails[2]);
                                        }
                                        else
                                        {
                                            //empdetails[4] = Convert.ToString(obProp);
                                            empdetails[1] = Convert.ToString(obProp);
                                            UtilityClass.ActivityLog("samaccountname :" + empdetails[2]);
                                        }
                                    }
                                }
                                if ((strKey.Trim().ToUpper() == "mail".ToUpper()))
                                {
                                    if ((obProp.ToString() != ""))
                                    {
                                        //empdetails[9] = Convert.ToString(obProp);
                                        empdetails[2] = Convert.ToString(obProp);
                                        UtilityClass.ActivityLog("Mail ID :" + empdetails[3]);
                                    }
                                }
                                if ((strKey.Trim().ToUpper() == "mobile".ToUpper()))
                                {
                                    if ((obProp.ToString() != ""))
                                    {
                                        //empdetails[11] = Convert.ToString(obProp);
                                        empdetails[3] = Convert.ToString(obProp);
                                        UtilityClass.ActivityLog("Mobile NO :" + empdetails[4]);
                                    }
                                }
                                if ((strKey.Trim().ToUpper() == "GIVENNAME".ToUpper())) //first name
                                {
                                    if ((obProp.ToString() != ""))
                                    {
                                        empdetails[4] = Convert.ToString(obProp);
                                        UtilityClass.ActivityLog("GIVENNAME :" + empdetails[5]);
                                    }
                                }
                                if ((strKey.Trim().ToUpper() == "SN".ToUpper()))//last name
                                {
                                    if ((obProp.ToString() != ""))
                                    {
                                        empdetails[5] = Convert.ToString(obProp);
                                        UtilityClass.ActivityLog("SN :" + empdetails[6]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;

            }
            return empdetails;
        }


        [HttpPost]
        [Route("Api/SenderEmail")]
        [Authorize]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> SenderEmail()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            try
            {
                bool emailStatus = false;
                var httpRequest = HttpContext.Current.Request;

                objEntity.Action = httpRequest.Form["Action"];
                objEntity.file = httpRequest.Form["Files"];

                DataTable dsMailDetails = objBAL.SenderEmail(objEntity);
                string MailTo = dsMailDetails.Rows[0]["EmailId"].ToString();
                string Subject = dsMailDetails.Rows[0]["Subject"].ToString();
                string MsgBody = dsMailDetails.Rows[0]["Body"].ToString();
                string MsgAction = dsMailDetails.Rows[0]["Action"].ToString();
                string cc = dsMailDetails.Rows[0]["Cc"].ToString();
                if (objEntity.file != "")
                {
                    foreach (string file in httpRequest.Files)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                        var postedFile = httpRequest.Files[file];
                        if (postedFile != null && postedFile.ContentLength > 0)
                        {
                            string savePath = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
                            string savePathLocation = HttpContext.Current.Server.MapPath(@"~/ImageUploads/");
                            //string savePathLocationDB = @"/ImageUploads/";
                            var filePath = HttpContext.Current.Server.MapPath("~/ImageUploads/" + postedFile.FileName);
                            //objEntity.file = (savePathLocationDB + postedFile.FileName.Replace(" ", "_").Replace("%", "_percentage_").Replace("&", "_and_"));
                            postedFile.SaveAs(filePath);
                            objEntity.file = "/ImageUploads/" + postedFile.FileName;

                        }
                        else
                        {
                            objEntity.file = "";
                        }

                    }

                }
                string files = objEntity.file;
                if (dsMailDetails != null && dsMailDetails.Rows.Count > 0)
                {
                    UtilityClass.ActivityLog(DateTime.Now.ToString() + "============================================================== Start : Daily mail ============================================================== ");
                    for (int i = 0; i < dsMailDetails.Rows.Count; i++)
                    {
                        string EmailTo = dsMailDetails.Rows[i]["EmailId"].ToString();
                        emailStatus = SendMailViaGmailSMTP(null, EmailTo, Subject, MsgBody, MsgAction, cc, files);
                    }
                    UtilityClass.ActivityLog(DateTime.Now.ToString() + "============================================================== End : Daily mail ============================================================== ");
                }
                return Request.CreateResponse(HttpStatusCode.Created, dict);

            }

            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpPost]
        [Route("Api/SendMailViaGmailSMTP")]
        [Authorize]
        [AllowAnonymous]
        public static bool SendMailViaGmailSMTP(string from, string To, string Subj, string MsgBody, string subType, string cc, string files)
        {
            string result = "";

            try
            {
                UtilityClass.ActivityLog("from : " + from + ", To:" + To + ", Subj" + Subj + ", subType" + subType + ", cc:" + cc);
                string host = WebConfigurationManager.AppSettings["emailHost"].ToString();
                string port = WebConfigurationManager.AppSettings["emailPort"].ToString();
                string uName = WebConfigurationManager.AppSettings["emailUname"].ToString();
                string pwd = WebConfigurationManager.AppSettings["emailPwd"].ToString();
                string mailFileDirectory = WebConfigurationManager.AppSettings["mailFileDirectory"].ToString();

                bool enablessl = WebConfigurationManager.AppSettings["emailenablessl"].ToString().ToLower() == "true" ? true : false;
                bool usedefaultcredentials = WebConfigurationManager.AppSettings["emailusedefaultcredentials"].ToString().ToLower() == "true" ? true : false;
                bool isbodyhtml = WebConfigurationManager.AppSettings["emailIsBodyHtml"].ToString().ToLower() == "true" ? true : false;
                string ccmailid = WebConfigurationManager.AppSettings["emailcc"].ToString();


                string[] emailIdArr = cc.Split(',');
                string bccmailid = WebConfigurationManager.AppSettings["emailcc"].ToString();
                string emailBcc = Convert.ToString(WebConfigurationManager.AppSettings["emailBcc"]);


                if (string.IsNullOrEmpty(from))
                    from = uName;
                if (string.IsNullOrEmpty(cc))
                    ccmailid = cc;

                if (!string.IsNullOrEmpty(To))
                {
                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(from);
                    message.To.Add(new MailAddress(To));
                    //if (subType == "mapped")  //for invigilator
                    //{
                    //    if (!string.IsNullOrEmpty(Convert.ToString(ccmailid)))
                    //        message.CC.Add(Convert.ToString(ccmailid));
                    //}
                    //if (subType == "1")  //for testTaker  --Er Manager
                    //{
                    //    if (!string.IsNullOrEmpty(Convert.ToString(emailIdArr[0])))
                    //    {
                    //        message.CC.Add(Convert.ToString(emailIdArr[0]));
                    //    }
                    //}
                    //if (subType == "2")  //for testTaker  --Reporting Authority
                    //{
                    //    if (!string.IsNullOrEmpty(Convert.ToString(emailIdArr[1])))
                    //    {
                    //        message.CC.Add(Convert.ToString(emailIdArr[1]));
                    //    }
                    //}
                    //if (subType == "3")  //for testTaker  --Er Manager and Reporting Authority
                    //{
                    //    if (!string.IsNullOrEmpty(Convert.ToString(cc)))
                    //    {
                    //        message.CC.Add(Convert.ToString(cc));
                    //    }
                    //}
                    //if (emailBcc != "")
                    //{
                    //    if (!string.IsNullOrEmpty(Convert.ToString(emailBcc)))
                    //    {
                    //        message.Bcc.Add(Convert.ToString(emailBcc));
                    //    }
                    //}

                    MsgBody = "<html><body>" + MsgBody;
                    MsgBody = MsgBody + "<style>body{font-family: Zurich BT; font-style:Zurich BT;font-size: 11px;} p{font-family: Zurich BT; font-style:Zurich BT;font-size: 11px;} </style>";
                    MsgBody = MsgBody + "</body></html>";

                    message.Subject = Convert.ToString(Subj); //test
                    message.IsBodyHtml = isbodyhtml; //true
                    message.Body = MsgBody; // Html Body
                    System.Net.Mail.Attachment attachment;
                    attachment = new System.Net.Mail.Attachment(files); //Attaching File to Mail  
                    message.Attachments.Add(attachment);

                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(); //SMTP Client
                    smtp.Port = int.Parse(port); // 587;
                    smtp.Host = host;// "smtp.gmail.com";
                    smtp.EnableSsl = enablessl; //true or false (For Gmail Client True)
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential(uName, pwd); // Check Credential
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;


                    //smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;   // SSL must not be enabled for pickup-directory delivery methods. -- Exception in GMail SMTP
                    smtp.PickupDirectoryLocation = Convert.ToString(mailFileDirectory); // Email Directory
                    smtp.DeliveryFormat = SmtpDeliveryFormat.SevenBit;
                    UtilityClass.ActivityLog("from : " + from + ", To:" + To + ", Subj :" + Subj + ", subType:" + subType + ", cc:" + cc);

                    try
                    {
                        smtp.Send(message);
                        result = "Email sent sucessfully";
                    }

                    catch (SmtpFailedRecipientsException ex) // To get SMTP Error Code if fail to send.
                    {
                        for (int i = 0; i < ex.InnerExceptions.Length; i++)
                        {
                            SmtpStatusCode status = ex.InnerExceptions[i].StatusCode;
                            if (status == SmtpStatusCode.MailboxBusy ||
                                status == SmtpStatusCode.MailboxUnavailable)
                            {
                                UtilityClass.ActivityLog("Delivery failed - retrying in 5 seconds.");
                                System.Threading.Thread.Sleep(5000);
                                smtp.Send(message);
                            }
                            else
                            {
                                UtilityClass.ActivityLog("Failed to deliver message : " + ex.InnerExceptions[i].FailedRecipient);
                                result = "Failed to deliver Email :" + status;
                            }
                        }
                    }
                }
            }


            catch (Exception exc)
            {
                UtilityClass.ActivityLog("======================================== ActivityLog ========================================");
                UtilityClass.ActivityLog("ActivityLog : Log created on - " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"));
                UtilityClass.ActivityLog("EmailProcess : Exception " + exc);
                result = exc.Message;
            }
            return true;
        }


        [HttpPost]
        [Route("Api/IndentGetRemarks")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult IndentGetRemarks(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.ID = Convert.ToString(table["RemarkId"]);

                        res = objBAL.IndentGetRemarks(objEntity);

                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1]
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }

            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpPost]
        [Route("Api/GetMZRMapping")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetMZRMapping(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Mega_Zone = Convert.ToString(table["Megazone"]);
                        objEntity.Zone = Convert.ToString(table["Zone"]);
                        res = objBAL.GetMZRMapping(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1]
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpPost]
        [Route("Api/InsertMZRZoneMaster")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult InsertMZRZoneMaster(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.CreatedBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Mega_Zone = Convert.ToString(table["Megazone"]);
                        objEntity.Zone = Convert.ToString(table["Zone"]);
                        objEntity.Region = Convert.ToString(table["Region"]);
                        objEntity.Branch = Convert.ToString(table["Branch"]);
                        ///  objEntity.CreatedBy = Convert.ToString(item["CreatedBy"]);
                        res = objBAL.InsertMZRZoneMaster(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpPost]
        [Route("Api/UpdateZoneMaster")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult UpdateZoneMaster(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.ModifyBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.RefId = table["RefId"];
                        objEntity.Mega_Zone = table["Megazone"];
                        objEntity.Zone = table["Zone"];
                        objEntity.Region = table["Region"];
                        // objEntity.ModifyBy = table["ModifyBy"];
                        res = objBAL.UpdateZoneMaster(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpPost]
        [Route("Api/SearchBusinesslevel")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult SearchBusinesslevel(JObject objData)
        {
            try
            {

                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {

                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Search = table["Search"];
                        objEntity.SearchOn = table["SearchOn"];
                        res = objBAL.SearchBusinesslevel(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1]
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpPost]
        [Route("Api/SearchLocationlevel")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult SearchLocationlevel(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {

                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Search = table["Search"];
                        objEntity.SearchOn = table["SearchOn"];
                        res = objBAL.SearchLocationlevel(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1]
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpPost]
        [Route("Api/SearchIndent")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult SearchIndent(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))

                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Search = table["Search"];
                        objEntity.SearchOn = table["SearchOn"];
                        res = objBAL.SearchIndent(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1]
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpPost]
        [Route("Api/SearchBudgetDetails")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult SearchBudgetDetails(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Search = table["Search"];
                        objEntity.Main_Group = table["maingrp"];
                        objEntity.Subgroup = table["subgrp"];
                        objEntity.Department = table["deptgrp"];
                        objEntity.Role = table["rolegrp"];
                        objEntity.Mega_Zone = table["megazone"];
                        objEntity.Zone = table["zone"];
                        objEntity.Region = table["region"];
                        objEntity.Branch = table["branch"];
                        objEntity.SearchOn = table["SearchOn"];
                        res = objBAL.SearchBudgetDetails(objEntity);
                        var response = new
                        {
                            statuscode = "100",
                            message = "Data Found",
                            data = res.Tables[1],
                            Details = res.Tables[2]
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpPost]
        [Route("Api/GetUserDetails")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetUserDetails(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {

                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.OFFSET = table["OFFSET"];
                        objEntity.LIMIT = table["LIMIT"];
                        objEntity.UserId = table["UserId"];
                        res = objBAL.GetUserDetails(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1],
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpPost]
        [Route("Api/GetCurrentFY")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetCurrentFY(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        res = objBAL.GetCurrentFY(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            data = res.Tables[1],
                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpPost]
        [Route("Api/InsertIndividualMZRMapping")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult InsertIndividualMZRMapping(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.CreatedBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Mega_Zone = Convert.ToString(table["Megazone"]);
                        objEntity.Zone = Convert.ToString(table["Zone"]);
                        objEntity.Region = Convert.ToString(table["Region"]);
                        // objEntity.CreatedBy = Convert.ToString(table["CreatedBy"]);
                        res = objBAL.InsertIndividualMZRMapping(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();

                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        UTF8Encoding _enc;

        RijndaelManaged _rcipher;

        byte[] _key, _pwd, _ivBytes, _iv;

        /***

         * Encryption mode enumeration

         */

        private enum EncryptMode { ENCRYPT, DECRYPT };
        static readonly char[] CharacterMatrixForRandomIVStringGeneration = {

                     'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',

                     'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',

                     'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',

                     'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',

                     '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_'

              };
        /**

         * This function generates random string of the given input length.

         *

         * @param _plainText

         *            Plain text to be encrypted

         * @param _key

         *            Encryption Key. You'll have to use the same key for decryption

         * @return returns encrypted (cipher) text

         */

        internal static string GenerateRandomIV(int length)

        {

            char[] _iv = new char[length];
            byte[] randomBytes = new byte[length];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes); //Fills an array of bytes with a cryptographically strong sequence of random values.
            }
            for (int i = 0; i < _iv.Length; i++)
            {
                int ptr = randomBytes[i] % CharacterMatrixForRandomIVStringGeneration.Length;
                _iv[i] = CharacterMatrixForRandomIVStringGeneration[ptr];
            }
            return new string(_iv);
        }


        public ICICI_BUDGETController()

        {

            _enc = new UTF8Encoding();

            _rcipher = new RijndaelManaged();

            _rcipher.Mode = CipherMode.CBC;

            _rcipher.Padding = PaddingMode.PKCS7;

            _rcipher.KeySize = 256;

            _rcipher.BlockSize = 128;

            _key = new byte[32];

            _iv = new byte[_rcipher.BlockSize / 8]; //128 bit / 8 = 16 bytes

            _ivBytes = new byte[16];

        }

        /**

         *

         * @param _inputText

         *            Text to be encrypted or decrypted

         * @param _encryptionKey

         *            Encryption key to used for encryption / decryption

         * @param _mode

         *            specify the mode encryption / decryption

         * @param _initVector

         *                      initialization vector

         * @return encrypted or decrypted string based on the mode

*/

        private String encryptDecrypt(string _inputText, string _encryptionKey, EncryptMode _mode, string _initVector)

        {


            //Task.Delay(2000);

            string _out = "";// output string

            //_encryptionKey = MD5Hash (_encryptionKey);

            _pwd = Encoding.UTF8.GetBytes(_encryptionKey);

            _ivBytes = Encoding.UTF8.GetBytes(_initVector);




            int len = _pwd.Length;

            if (len > _key.Length)

            {

                len = _key.Length;

            }

            int ivLenth = _ivBytes.Length;

            if (ivLenth > _iv.Length)

            {

                ivLenth = _iv.Length;

            }




            Array.Copy(_pwd, _key, len);

            Array.Copy(_ivBytes, _iv, ivLenth);

            _rcipher.Key = _key;

            _rcipher.IV = _iv;




            if (_mode.Equals(EncryptMode.ENCRYPT))

            {

                //encrypt

                byte[] plainText = _rcipher.CreateEncryptor().TransformFinalBlock(_enc.GetBytes(_inputText), 0, _inputText.Length);

                _out = Convert.ToBase64String(plainText);

            }

            if (_mode.Equals(EncryptMode.DECRYPT))

            {

                //decrypt

                byte[] plainText = _rcipher.CreateDecryptor().TransformFinalBlock(Convert.FromBase64String(_inputText), 0, Convert.FromBase64String(_inputText).Length);

                _out = _enc.GetString(plainText);

            }

            _rcipher.Dispose();

            return _out;// return encrypted/decrypted string

        }

        /**

         * This function encrypts the plain text to cipher text using the key

         * provided. You'll have to use the same key for decryption

         *

         * @param _plainText

         *            Plain text to be encrypted

         * @param _key

         *            Encryption Key. You'll have to use the same key for decryption

         * @return returns encrypted (cipher) text

         */

        public string encrypt(string _plainText, string _key, string _initVector)

        {

            return encryptDecrypt(_plainText, _key, EncryptMode.ENCRYPT, _initVector);

        }

        /***

         * This funtion decrypts the encrypted text to plain text using the key

         * provided. You'll have to use the same key which you used during

         * encryprtion

         *

         * @param _encryptedText

         *            Encrypted/Cipher text to be decrypted

         * @param _key

         *            Encryption key which you used during encryption

         * @return encrypted value

         */
        public string decrypt(string _encryptedText, string _key, string _initVector)

        {

            return encryptDecrypt(_encryptedText, _key, EncryptMode.DECRYPT, _initVector);

        }

        /***

         * This function decrypts the encrypted text to plain text using the key

         * provided. You'll have to use the same key which you used during

         * encryption

         *

         * @param _encryptedText

         *            Encrypted/Cipher text to be decrypted

         * @param _key

         *            Encryption key which you used during encryption

         */

        public static string getHashSha256(string text, int length)

        {

            byte[] bytes = Encoding.UTF8.GetBytes(text);

            SHA256Managed hashstring = new SHA256Managed();

            byte[] hash = hashstring.ComputeHash(bytes);

            string hashString = string.Empty;

            foreach (byte x in hash)

            {

                hashString += String.Format("{0:x2}", x); //covert to hex string

            }

            if (length > hashString.Length)

                return hashString;

            else

                return hashString.Substring(0, length);

        }
        //this function is no longer used.

        [HttpPost]
        [Route("Api/ConnectionString")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult ConnectionString(JObject objData)
        {
            try
            {
                dynamic item = objData;
                string data = item["data"];
                objEntity.file = (item["file"]);
                string keytest = "d7a50e0f2f9546d35ce700eebfb0c911";
                //string ivkeytest = GenerateRandomIV(16);
                string ivkeytest = "lw-hv-ThGioHnTAi";
                //string userIDEncrypted = encrypt(objEntity.file, keytest, ivkeytest);
                string userIDDecrypted = decrypt(objEntity.file, keytest, ivkeytest);


                return Ok(userIDDecrypted);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpPost]
        [Route("Api/EncryptConnectionString")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult EncryptConnectionString(JObject objData)
        {
            try
            {
                dynamic item = objData;
                string data = item["data"];
                objEntity.file = (item["file"]);
                string keytest = "d7a50e0f2f9546d35ce700eebfb0c911";
                //string ivkeytest = GenerateRandomIV(16);
                string ivkeytest = "lw-hv-ThGioHnTAi";
                string userIDEncrypted = encrypt(objEntity.file, keytest, ivkeytest);
                return Ok(userIDEncrypted);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpPost]
        [Route("Api/DecryptConnectionString")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult DecryptConnectionString(JObject objData)
        {
            try
            {
                dynamic item = objData;
                string data = item["data"];
                objEntity.file = (item["file"]);
                string keytest = "d7a50e0f2f9546d35ce700eebfb0c911";
                string ivkeytest = GenerateRandomIV(16);
                //  string ivkeytest = "lw-hv-ThGioHnTAi";
                string userIDDecrypted = decrypt(objEntity.file, keytest, ivkeytest);
                return Ok(userIDDecrypted);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpPost]
        [Route("Api/ImportBudget")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult ImportBudget()
        {

            var httpRequest = HttpContext.Current.Request;
            objEntity.CreatedBy = httpRequest.Form["CreatedBy"];
            DataTable csvData = new DataTable();
            string filePath = "";
            DataTable dtble;
            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    filePath = HttpContext.Current.Server.MapPath("~/MappingFiles/" + "Budget.csv");
                    postedFile.SaveAs(filePath);
                    docfiles.Add(filePath);
                }

            }
            else
            {
            }

            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(filePath))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();

                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                    DataTable db = objBAL.ImportBudget(csvData, objEntity);
                    objEntity.datatable = db;
                    dtble = objEntity.datatable;
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

            var jsonString = JsonConvert.SerializeObject(dtble);
            var response1 = new
            {
                data = AESEncrytDecry.Encryptstring(jsonString)
            };
            return Ok(jsonString);

        }


        [HttpPost]
        [Route("Api/InsertBookmarkBudget")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult InsertBookmarkBudget(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.CreatedBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.Main_Group = table["Maingroup"];
                        objEntity.Subgroup = table["Subgroup"];
                        objEntity.Department = table["Department"];
                        objEntity.Role = table["Role"];
                        objEntity.Mega_Zone = table["Megazone"];
                        objEntity.Zone = table["Zone"];
                        objEntity.Region = table["Region"];
                        objEntity.Branch = table["Branch"];
                        //  objEntity.CreatedBy = table["CreatedBy"];
                        objEntity.BookmarkName = table["BookmarkName"];

                        res = objBAL.InsertBookmarkBudget(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);

                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }

                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpPost]
        [Route("Api/GetBookmarkBudget")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetBookmarkBudget(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.CreatedBy = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        //  objEntity.CreatedBy = table["CreatedBy"];

                        res = objBAL.GetBookmarkBudget(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            Data = res.Tables[1]

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpPost]
        [Route("Api/DeleteBookmarkBudget")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult DeleteBookmarkBudget(JObject objData)
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {
                    var req = Request;
                    var headers = req.Headers;
                    string token = headers.GetValues("Authorization").First();
                    var handler = new JwtSecurityTokenHandler();
                    string authHeader = token;
                    authHeader = authHeader.Replace("Bearer ", "");
                    authHeader = authHeader.Replace("\"", "");
                    var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                    string User_Id = tokenS.Claims.First(claim => claim.Type == "UserId").Value;
                    objEntity.UserId = User_Id;
                    res = objBAL.GetTokenByUserId(objEntity);
                    string DB_Token = res.Tables[0].Rows[0]["Token"].ToString();
                    string token_ = token.Replace("Bearer", "");
                    string Header_Token = token_.Trim();
                    if (Header_Token == DB_Token)
                    {
                        dynamic item = objData;
                        string data = item["data"];
                        objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                        string json = objEntity.Data;
                        var jss = new JavaScriptSerializer();
                        var table = jss.Deserialize<dynamic>(json);
                        objEntity.PId = table["PId"];
                        objEntity.DeletedBy = table["DeletedBy"];
                        res = objBAL.DeleteBookmarkBudget(objEntity);
                        objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                        objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                        var response = new
                        {
                            statuscode = objEntity.stauscode,
                            message = objEntity.message,
                            Data = res.Tables[1]

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Ok(response1);
                    }
                    else
                    {
                        var response = new
                        {
                            statuscode = 404,
                            message = "Invalid Token",

                        };
                        var jsonString = JsonConvert.SerializeObject(response);
                        var response1 = new
                        {
                            data = AESEncrytDecry.Encryptstring(jsonString)
                        };
                        return Content(HttpStatusCode.NotAcceptable, response1);
                    }
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpPost]
        [Route("Api/Logout")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult Logout(JObject objData)
        {
            try
            {
                dynamic item = objData;
                string data = item["data"];
                objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                string json = objEntity.Data;
                var jss = new JavaScriptSerializer();
                var table = jss.Deserialize<dynamic>(json);
                objEntity.UserId = table["UserId"];
                res = objBAL.Logout(objEntity);
                objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                var response = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,
                };
                var jsonString = JsonConvert.SerializeObject(response);
                var response1 = new
                {
                    data = AESEncrytDecry.Encryptstring(jsonString)
                };
                return Ok(response1);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpPost]
        [Route("Api/GetCurrentFY1")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetCurrentFY1(JObject objData)
        {
            try
            {
                res = objBAL.GetCurrentFY(objEntity);
                objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                UtilityClass.ActivityLog(DateTime.Now.ToString() + "stauscode" + objEntity.stauscode);
                objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                UtilityClass.ActivityLog(DateTime.Now.ToString() + "stauscode" + objEntity.message);
                var response = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,
                    data = res.Tables[1],
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }

        [HttpGet]
        [Route("Api/IndentReport1")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult IndentReport1(string FromDate, string ToDate, string Location = "", string IndentType = "", string RequestorName = "")
        {
            try
            {
                string AttachmentURL = WebConfigurationManager.AppSettings["AttachmentURL"];
                //string Filename = @"Indent_Report_File.csv";
                string Filename = "MappingFiles\\Indent_Report_File.csv";

                var Filepath = string.Concat(AttachmentURL, Filename);

                string strFilePath = WebConfigurationManager.AppSettings["WriteIndentCsvFile"];
                objEntity.FromDate = FromDate;
                objEntity.ToDate = ToDate;
                objEntity.Location = Location;
                objEntity.IndentType = IndentType;
                objEntity.RequestorName = RequestorName;
                DataTable dtDataTable = objBAL.IndentReport(objEntity);
                StreamWriter sw = new StreamWriter(Filename, false);
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    sw.Write(dtDataTable.Columns[i]);
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
                foreach (DataRow dr in dtDataTable.Rows)
                {
                    for (int i = 0; i < dtDataTable.Columns.Count; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            string value = dr[i].ToString();

                            if (value.Contains(','))
                            {
                                value = String.Format("\"{0}\"", value);
                                sw.Write(value);
                            }
                            else
                            {
                                sw.Write(dr[i].ToString());
                            }
                        }
                        if (i < dtDataTable.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);
                }
                sw.Close();
                objEntity.stauscode = 100;
                objEntity.message = "Record Found";
                Byte[] bytes = File.ReadAllBytes(Filepath);
                String File_path = Convert.ToBase64String(bytes);
                var response = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,
                    Filepath = File_path
                };
                var jsonString = JsonConvert.SerializeObject(response);
                var response1 = new
                {
                    data = AESEncrytDecry.Encryptstring(jsonString)
                };
                return Ok(response1);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }


        [HttpPost]
        [Route("Api/GetBudgetAttachment100")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GetBudgetAttachment100(JObject objData)
        {
            try
            {
                dynamic item = objData;
                //string data = item["data"];
                //objEntity.Data = AESEncrytDecry.DecryptStringAES(data);
                //string json = objEntity.Data;
                //var jss = new JavaScriptSerializer();
                //var table = jss.Deserialize<dynamic>(json);
                objEntity.ID = item["PId"];
                res = objBAL.GetBudgetAttachment(objEntity);
                string AttachmentBaseURL = WebConfigurationManager.AppSettings["AttachmentURL"];
                Byte[] bytes = File.ReadAllBytes(AttachmentBaseURL + res.Tables[1].Rows[0]["File"].ToString());
                String file = Convert.ToBase64String(bytes);
                string Filename = (res.Tables[1].Rows[0]["File"].ToString());
                objEntity.stauscode = Convert.ToInt32(res.Tables[0].Rows[0]["Code"]);
                objEntity.message = res.Tables[0].Rows[0]["message"].ToString();
                var response = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,
                    data = file,
                    Filename = Filename
                };

                //var jsonString = JsonConvert.SerializeObject(response);
                //var response1 = new
                //{
                //    data = AESEncrytDecry.Encryptstring(jsonString)
                //};
                return Ok(response);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }
        }

        [HttpGet]
        [Route("Api/LocationReport1")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult LocationReport1(string FromDate, string ToDate, string Location = "")
        {
            try
            {
                string AttachmentURL = WebConfigurationManager.AppSettings["AttachmentURL"];
                string Filename = "MappingFiles\\Location_Report_File.csv";
                var Filepath = string.Concat(AttachmentURL, Filename);

                string strFilePath = WebConfigurationManager.AppSettings["WriteLocationCsvFile"];
                objEntity.FromDate = FromDate;
                objEntity.ToDate = ToDate;
                objEntity.Location = Location;
                DataTable dtDataTable = objBAL.LocationReport(objEntity);
                StreamWriter sw = new StreamWriter(strFilePath, false);
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    sw.Write(dtDataTable.Columns[i]);
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
                foreach (DataRow dr in dtDataTable.Rows)
                {
                    for (int i = 0; i < dtDataTable.Columns.Count; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            string value = dr[i].ToString();

                            if (value.Contains(','))
                            {
                                value = String.Format("\"{0}\"", value);
                                sw.Write(value);
                            }
                            else
                            {
                                sw.Write(dr[i].ToString());
                            }
                        }
                        if (i < dtDataTable.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);
                }
                sw.Close();
                objEntity.stauscode = 100;
                objEntity.message = "Record Found";
                Byte[] bytes = File.ReadAllBytes(Filepath);
                String File_path = Convert.ToBase64String(bytes);
                var response = new
                {
                    statuscode = objEntity.stauscode,
                    message = objEntity.message,
                    Filepath = File_path,
                    Filename = Filename
                };
                var jsonString = JsonConvert.SerializeObject(response);
                var response1 = new
                {
                    data = AESEncrytDecry.Encryptstring(jsonString)
                };
                return Ok(response1);
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }



        [HttpGet]
        [Route("Api/GradeReport1")]
        [Authorize]
        [AllowAnonymous]
        public IHttpActionResult GradeReport1(string FromDate, string ToDate, string Grade = "")
        {
            try
            {
                if (Request.Headers.Contains("Authorization") && (Request.Headers.GetValues("Authorization").FirstOrDefault() != null || Request.Headers.GetValues("Authorization").FirstOrDefault() != "null"))
                {

                    string BaseUrl = WebConfigurationManager.AppSettings["baseurl"];
                    string Filename = "MappingFiles\\Grade_Report_File.csv";
                    var Filepath = string.Concat(BaseUrl, Filename);

                    string strFilePath = WebConfigurationManager.AppSettings["WriteGradeCsvFile"];
                    objEntity.FromDate = FromDate;
                    objEntity.ToDate = ToDate;
                    objEntity.Grade = Grade;
                    DataTable dtDataTable = objBAL.GradeReport(objEntity);
                    StreamWriter sw = new StreamWriter(strFilePath, false);
                    for (int i = 0; i < dtDataTable.Columns.Count; i++)
                    {
                        sw.Write(dtDataTable.Columns[i]);
                        if (i < dtDataTable.Columns.Count - 1)
                        {
                            sw.Write(",");
                        }
                    }
                    sw.Write(sw.NewLine);
                    foreach (DataRow dr in dtDataTable.Rows)
                    {
                        for (int i = 0; i < dtDataTable.Columns.Count; i++)
                        {
                            if (!Convert.IsDBNull(dr[i]))
                            {
                                string value = dr[i].ToString();

                                if (value.Contains(','))
                                {
                                    value = String.Format("\"{0}\"", value);
                                    sw.Write(value);
                                }
                                else
                                {
                                    sw.Write(dr[i].ToString());
                                }
                            }
                            if (i < dtDataTable.Columns.Count - 1)
                            {
                                sw.Write(",");
                            }
                        }
                        sw.Write(sw.NewLine);
                    }
                    sw.Close();
                    objEntity.stauscode = 100;
                    objEntity.message = "Record Found";
                    Byte[] bytes = File.ReadAllBytes(Filepath);
                    String File_path = Convert.ToBase64String(bytes);
                    var response = new
                    {
                        statuscode = objEntity.stauscode,
                        message = objEntity.message,
                        Filepath = File_path
                    };
                   
                    return Ok(response);
                }
                else
                {
                    var response = new
                    {
                        statuscode = 401,
                        message = "Token not Found",

                    };
                    var jsonString = JsonConvert.SerializeObject(response);
                    var response1 = new
                    {
                        data = AESEncrytDecry.Encryptstring(jsonString)
                    };
                    return Content(HttpStatusCode.Unauthorized, response1);
                }
            }
            catch (Exception ex)
            {
                UtilityClass.ActivityLog(DateTime.Now.ToString() + ex);
                throw ex;
            }

        }



    }
}
