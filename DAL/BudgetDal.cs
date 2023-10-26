using Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Configuration;

namespace DAL
{
    public class BudgetDal
    {
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

        string keytest = "d7a50e0f2f9546d35ce700eebfb0c911";
        // string ivkeytest = GenerateRandomIV(16);
        string ivkeytest = "lw-hv-ThGioHnTAi";

        string connStr = ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString;

        string connection = "";

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

        public BudgetDal()

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

            connection = connStr;// connStr decrypt(connStr, keytest, ivkeytest);
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

        private static string MD5Hash(string text)

        {

            MD5 md5 = new MD5CryptoServiceProvider();




            //compute hash from the bytes of text

            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));




            //get hash result after compute it

            byte[] result = md5.Hash;




            StringBuilder strBuilder = new StringBuilder();

            for (int i = 0; i < result.Length; i++)

            {

                //change it into 2 hexadecimal digits

                //for each byte

                strBuilder.Append(result[i].ToString("x2"));

            }

            Console.WriteLine("md5 hash of they key=" + strBuilder.ToString());

            return strBuilder.ToString();

        }

        public DataSet GetYear()
        {
            DataSet ds_Data = new DataSet("getData");
            string storeProcedure = "PROC_GET_BUDGETYEAR";
            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand(storeProcedure, conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return ds_Data;
        }

        public DataSet GetLocationMaster1()
        {
            DataSet ds_Data = new DataSet("getData");
            string storeProcedure = "PROC_GET_LOCATIONMASTERDETAILS";
            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand(storeProcedure, conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return ds_Data;
        }

        public DataSet BranchDetils()
        {
            DataSet ds_Data = new DataSet("getData");
            string storeProcedure = "PROC_GET_BRANCHDETAILS";
            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand(storeProcedure, conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return ds_Data;
        }

        public DataSet GetBusinesslevel(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("getData");
            string storeProcedure = "PROC_GET_BUSINESS_LEVEL";


            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand(storeProcedure, conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    
                    sqlComm.Parameters.Add("@Main_Group", SqlDbType.VarChar).Value = obj.Main_Group;
                    sqlComm.Parameters.Add("@Subgroup", SqlDbType.VarChar).Value = obj.Subgroup;
                    sqlComm.Parameters.Add("@Department", SqlDbType.VarChar).Value = obj.Department;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return ds_Data;
        }

        public DataSet GetLocation(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("getData");
            string storeProcedure = "PROC_GET_ZONEMAPPING";
            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand(storeProcedure, conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    
                    sqlComm.Parameters.Add("@Mega_Zone", SqlDbType.VarChar).Value = obj.Mega_Zone;
                    sqlComm.Parameters.Add("@Zone", SqlDbType.VarChar).Value = obj.Zone;
                    sqlComm.Parameters.Add("@Region", SqlDbType.VarChar).Value = obj.Region;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return ds_Data;
        }

        public DataSet UserLogin(string action, BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");
            string storeProcedure = "";
            if (action == "PROC_API_USERLOGIN")
            {
                storeProcedure = "PROC_API_USERLOGIN";
            }
            if (action == "Token")
            {
                storeProcedure = "PROC_API_USERLOGIN";
            }
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand(storeProcedure, conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    
                    sqlComm.Parameters.Add("@User_Id", SqlDbType.VarChar).Value = obj.UserId;
                    sqlComm.Parameters.Add("@Token", SqlDbType.VarChar).Value = obj.Tokens;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet InsertBudget(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_INSERTBUDGETING", conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    
                    sqlComm.Parameters.Add("@maingrp", SqlDbType.VarChar).Value = obj.Main_Group;
                    sqlComm.Parameters.Add("@subgrp", SqlDbType.VarChar).Value = obj.Subgroup;
                    sqlComm.Parameters.Add("@deptgrp", SqlDbType.VarChar).Value = obj.Department;
                    sqlComm.Parameters.Add("@rolegrp", SqlDbType.VarChar).Value = obj.Role;
                    sqlComm.Parameters.Add("@region", SqlDbType.VarChar).Value = obj.Region;
                    sqlComm.Parameters.Add("@branch", SqlDbType.VarChar).Value = obj.Branch;
                    sqlComm.Parameters.Add("@zone", SqlDbType.VarChar).Value = obj.Zone;
                    sqlComm.Parameters.Add("@megazone", SqlDbType.VarChar).Value = obj.Mega_Zone;
                    sqlComm.Parameters.Add("@budget", SqlDbType.VarChar).Value = obj.Budget;
                    sqlComm.Parameters.Add("@budgetyear", SqlDbType.VarChar).Value = obj.Budgetyear;
                    sqlComm.Parameters.Add("@File", SqlDbType.VarChar).Value = obj.file;
                    sqlComm.Parameters.Add("@Buffer", SqlDbType.VarChar).Value = obj.Buffer;
                    sqlComm.Parameters.Add("@Remarks", SqlDbType.VarChar).Value = obj.Remark;
                    sqlComm.Parameters.Add("@GradeID", SqlDbType.VarChar).Value = obj.Grade;
                    sqlComm.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = obj.CreatedBy;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetBudgetDetails(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("Proc_GET_BUDGETDETAILS", conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    
                    sqlComm.Parameters.Add("@maingrp", SqlDbType.VarChar).Value = obj.Main_Group;
                    sqlComm.Parameters.Add("@subgrp", SqlDbType.VarChar).Value = obj.Subgroup;
                    sqlComm.Parameters.Add("@deptgrp", SqlDbType.VarChar).Value = obj.Department;
                    sqlComm.Parameters.Add("@rolegrp", SqlDbType.VarChar).Value = obj.Role;
                    sqlComm.Parameters.Add("@region", SqlDbType.VarChar).Value = obj.Region;
                    sqlComm.Parameters.Add("@branch", SqlDbType.VarChar).Value = obj.Branch;
                    sqlComm.Parameters.Add("@zone", SqlDbType.VarChar).Value = obj.Zone;
                    sqlComm.Parameters.Add("@megazone", SqlDbType.VarChar).Value = obj.Mega_Zone;
                    sqlComm.Parameters.Add("@type", SqlDbType.VarChar).Value = obj.Type;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet InsertZone(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_INSERT_ZONEMASTERDETAILS", conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    

                    sqlComm.Parameters.Add("@Region", SqlDbType.VarChar).Value = obj.Region;
                    sqlComm.Parameters.Add("@BranchName", SqlDbType.VarChar).Value = obj.Branch;
                    sqlComm.Parameters.Add("@Zone", SqlDbType.VarChar).Value = obj.Zone;
                    sqlComm.Parameters.Add("@Megazone", SqlDbType.VarChar).Value = obj.Mega_Zone;
                    sqlComm.Parameters.Add("@LocationId", SqlDbType.VarChar).Value = obj.LocationId;

                    //  sqlComm.Parameters.Add("@Buffer", SqlDbType.VarChar).Value = obj.Buffer;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetBudgetStaggering(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_BudgetStaggering", conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                                       
                    sqlComm.Parameters.Add("@YEAR", SqlDbType.VarChar).Value = obj.Year;
                    sqlComm.Parameters.Add("@PID", SqlDbType.VarChar).Value = obj.ID;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet addBudgetStaggering(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_INSERT_BudgetStaggering", conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    

                    sqlComm.Parameters.Add("@Date", SqlDbType.VarChar).Value = obj.Date;
                    sqlComm.Parameters.Add("@YEAR", SqlDbType.VarChar).Value = obj.Year;
                    sqlComm.Parameters.Add("@ACTIVATE", SqlDbType.VarChar).Value = obj.Budget;
                    sqlComm.Parameters.Add("@PID", SqlDbType.VarChar).Value = obj.ID;
                    sqlComm.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = obj.CreatedBy;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);
                }
            }

            return ds_Data;
        }

        public DataSet ChangeDefaultBudgetYear(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("Proc_UPDATE_DefaultBudgetYear", conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    

                    sqlComm.Parameters.Add("@CURRENT_YEAR", SqlDbType.VarChar).Value = obj.CurrentYear;
                    sqlComm.Parameters.Add("@NEXT_YEAR", SqlDbType.VarChar).Value = obj.NextYear;
                    sqlComm.Parameters.Add("@maingrp", SqlDbType.VarChar).Value = obj.Main_Group;
                    sqlComm.Parameters.Add("@subgrp", SqlDbType.VarChar).Value = obj.Subgroup;
                    sqlComm.Parameters.Add("@deptgrp", SqlDbType.VarChar).Value = obj.Department;
                    sqlComm.Parameters.Add("@rolegrp", SqlDbType.VarChar).Value = obj.Role;
                    sqlComm.Parameters.Add("@region", SqlDbType.VarChar).Value = obj.Region;
                    sqlComm.Parameters.Add("@branch", SqlDbType.VarChar).Value = obj.Branch;
                    sqlComm.Parameters.Add("@zone", SqlDbType.VarChar).Value = obj.Zone;
                    sqlComm.Parameters.Add("@megazone", SqlDbType.VarChar).Value = obj.Mega_Zone;
                    sqlComm.Parameters.Add("@ModifyBy", SqlDbType.VarChar).Value = obj.ModifyBy;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet UpdateBudget(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_UPDATEBUDGETING", conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    
                    sqlComm.Parameters.Add("@RefID", SqlDbType.VarChar).Value = obj.ID;
                    sqlComm.Parameters.Add("@budget", SqlDbType.VarChar).Value = obj.Budget;
                    sqlComm.Parameters.Add("@budgetyear", SqlDbType.VarChar).Value = obj.Budgetyear;
                    sqlComm.Parameters.Add("@File", SqlDbType.VarChar).Value = obj.file;
                    sqlComm.Parameters.Add("@Buffer", SqlDbType.VarChar).Value = obj.Buffer;
                    sqlComm.Parameters.Add("@Remarks", SqlDbType.VarChar).Value = obj.Remark;
                    sqlComm.Parameters.Add("@Grade", SqlDbType.VarChar).Value = obj.Grade;
                    sqlComm.Parameters.Add("@ModifyBy", SqlDbType.VarChar).Value = obj.ModifyBy;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet UpadateStaggering(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_UPDATE_BudgetStaggering", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    
                    sqlComm.Parameters.Add("@STAGGEREDID", SqlDbType.VarChar).Value = obj.ID;
                    sqlComm.Parameters.Add("@ActiveBudget", SqlDbType.VarChar).Value = obj.Budget;
                    sqlComm.Parameters.Add("@ModifyBy", SqlDbType.VarChar).Value = obj.ModifyBy;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet DeleteStaggering(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_DELETE_BudgetStaggering", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    
                    sqlComm.Parameters.Add("@STAGGEREDID", SqlDbType.VarChar).Value = obj.ID;
                    sqlComm.Parameters.Add("@DeletedBy", SqlDbType.VarChar).Value = obj.DeletedBy;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetRemarks(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_REMARK", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    

                    sqlComm.Parameters.Add("@RemarkId", SqlDbType.VarChar).Value = obj.ID;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetBudgetAttachment(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_BudgetAttachment", conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    


                    sqlComm.Parameters.Add("@PId", SqlDbType.VarChar).Value = obj.ID;




                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetGradeDetails(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_GRADEDETAILS", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    
                    sqlComm.Parameters.Add("@OFFSET", SqlDbType.VarChar).Value = obj.OFFSET;
                    sqlComm.Parameters.Add("@LIMIT", SqlDbType.VarChar).Value = obj.LIMIT;


                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetRAId(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_RAID", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    
                    sqlComm.Parameters.Add("@OFFSET", SqlDbType.VarChar).Value = obj.OFFSET;
                    sqlComm.Parameters.Add("@LIMIT", SqlDbType.VarChar).Value = obj.LIMIT;
                    sqlComm.Parameters.Add("@EMPName", SqlDbType.VarChar).Value = obj.EmpName;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet InsertIndent(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_INSERT_INDENT", conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    

                    sqlComm.Parameters.Add("@Ref_BM_ID", SqlDbType.VarChar).Value = obj.RefBMId;
                    sqlComm.Parameters.Add("@Ref_emp_Id", SqlDbType.VarChar).Value = obj.RefEMPID;
                    sqlComm.Parameters.Add("@Emp_count", SqlDbType.VarChar).Value = obj.Emp_Count;
                    sqlComm.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = obj.CreatedBy;
                    sqlComm.Parameters.Add("@Reason", SqlDbType.VarChar).Value = obj.Reason;
                    sqlComm.Parameters.Add("@IndentType", SqlDbType.VarChar).Value = obj.IndentType;
                    sqlComm.Parameters.Add("@Grade", SqlDbType.VarChar).Value = obj.Grade;
                    sqlComm.Parameters.Add("@Remark", SqlDbType.VarChar).Value = obj.Remark;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetIndentDetails(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_INDENTDETAILS", conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    

                    sqlComm.Parameters.Add("@OFFSET", SqlDbType.VarChar).Value = obj.OFFSET;
                    sqlComm.Parameters.Add("@LIMIT", SqlDbType.VarChar).Value = obj.LIMIT;


                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetBudgetMappingDetails(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_BUDGETMAPPINGDETAILS", conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    
                    sqlComm.Parameters.Add("@RefBMId", SqlDbType.VarChar).Value = obj.RefBMId;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetIndentTypeMaster(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_IndentTypeMaster", conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataTable ImportBusinesslevelMapping(DataTable csvFileData, BudgetEntity obj)
        {
            DataTable ds_Data = new DataTable();

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("PROC_INSERT_IMPORTBUSINESSLEVELEXCEL", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = obj.CreatedBy;
                    SqlParameter tblvaluetype = cmd.Parameters.AddWithValue("@ImportBusinesslevelExcel", csvFileData);  //Passing table value parameter
                    tblvaluetype.SqlDbType = SqlDbType.Structured; // This one is used to tell ADO.NET we are passing Table value Parameter
                    int result = cmd.ExecuteNonQuery();

                }
            }


            return ds_Data;


        }

        public DataTable ImportLocationlevelMapping(DataTable csvFileData, BudgetEntity obj)
        {
            DataTable ds_Data = new DataTable();

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("PROC_INSERT_IMPORTLOCATIONLEVELEXCEL", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0; //2 min (120 secs)                    
                    cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = obj.CreatedBy;
                    SqlParameter tblvaluetype = cmd.Parameters.AddWithValue("@ImportLocationLevelCSV", csvFileData);  //Passing table value parameter
                    tblvaluetype.SqlDbType = SqlDbType.Structured; // This one is used to tell ADO.NET we are passing Table value Parameter
                    cmd.ExecuteNonQuery();

                }
            }


            return ds_Data;
        }

        public DataTable ExportCsvFile(BudgetEntity obj)
        {
            DataTable ds_Data = new DataTable();
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_EXPORT_CSVFILE", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                    
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }
            return ds_Data;
        }

        public DataTable ImportResponseCsvFile(DataTable csvFileData)
        {
            DataTable ds_Data = new DataTable();

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("PROC_Update_StudentDetails", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlParameter tblvaluetype = cmd.Parameters.AddWithValue("@TVP", csvFileData);  //Passing table value parameter
                    tblvaluetype.SqlDbType = SqlDbType.Structured; // This one is used to tell ADO.NET we are passing Table value Parameter
                    int result = cmd.ExecuteNonQuery();

                }
            }


            return ds_Data;
        }

        public DataSet GetLocationMaster(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_LOCATIONMASTERDETAILS", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    
                    sqlComm.Parameters.Add("@OFFSET", SqlDbType.VarChar).Value = obj.OFFSET;
                    sqlComm.Parameters.Add("@LIMIT", SqlDbType.VarChar).Value = obj.LIMIT;


                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetBusinessMaster(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_BUSINESSMASTERDETAILS", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    
                    sqlComm.Parameters.Add("@OFFSET", SqlDbType.VarChar).Value = obj.OFFSET;
                    sqlComm.Parameters.Add("@LIMIT", SqlDbType.VarChar).Value = obj.LIMIT;


                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet UpadateIndent(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_UPDATE_INDENT", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    
                    sqlComm.Parameters.Add("@RefIndentId", SqlDbType.VarChar).Value = obj.RefIndentId;
                    sqlComm.Parameters.Add("@Jobcode", SqlDbType.VarChar).Value = obj.Jobcode;
                    sqlComm.Parameters.Add("@Location", SqlDbType.VarChar).Value = obj.Location;
                    sqlComm.Parameters.Add("@Grade", SqlDbType.VarChar).Value = obj.Grade;
                    sqlComm.Parameters.Add("@RA", SqlDbType.VarChar).Value = obj.RA;
                    sqlComm.Parameters.Add("@Reason", SqlDbType.VarChar).Value = obj.Reason;
                    sqlComm.Parameters.Add("@Remark", SqlDbType.VarChar).Value = obj.Remark;
                    sqlComm.Parameters.Add("@ModifyBy", SqlDbType.VarChar).Value = obj.ModifyBy;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet InsertLocationMapping(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_INSERT_LocationMapping", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    
                    sqlComm.Parameters.Add("@Data", SqlDbType.VarChar).Value = obj.Data;
                    sqlComm.Parameters.Add("@Type", SqlDbType.VarChar).Value = obj.Type;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet UpdateLocationMapping(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_UPDATE_LocationMapping", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)
                    sqlComm.Parameters.Add("@RefId", SqlDbType.VarChar).Value = obj.RefId;
                    sqlComm.Parameters.Add("@Data", SqlDbType.VarChar).Value = obj.Data;
                    sqlComm.Parameters.Add("@Type", SqlDbType.VarChar).Value = obj.Type;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetLocationMapping(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_LocationMapping", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)
                    sqlComm.Parameters.Add("@Type", SqlDbType.VarChar).Value = obj.Type;
                    sqlComm.Parameters.Add("@OFFSET", SqlDbType.VarChar).Value = obj.OFFSET;
                    sqlComm.Parameters.Add("@LIMIT", SqlDbType.VarChar).Value = obj.LIMIT;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet InsertZoneMasterMappingData(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_INSERT_ZONEMASTERMAPPINGDATA", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)
                    sqlComm.Parameters.Add("@Megazone", SqlDbType.VarChar).Value = obj.Mega_Zone;
                    sqlComm.Parameters.Add("@Zone", SqlDbType.VarChar).Value = obj.Zone;
                    sqlComm.Parameters.Add("@Region", SqlDbType.VarChar).Value = obj.Region;
                    sqlComm.Parameters.Add("@Branch", SqlDbType.VarChar).Value = obj.Branch;
                    sqlComm.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = obj.CreatedBy;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet InsertManualIndent(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_INSERT_MANUALINDENT", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)
                    sqlComm.Parameters.Add("@maingrp", SqlDbType.VarChar).Value = obj.Main_Group;
                    sqlComm.Parameters.Add("@subgrp", SqlDbType.VarChar).Value = obj.Subgroup;
                    sqlComm.Parameters.Add("@deptgrp", SqlDbType.VarChar).Value = obj.Department;
                    sqlComm.Parameters.Add("@rolegrp", SqlDbType.VarChar).Value = obj.Role;
                    sqlComm.Parameters.Add("@megazone", SqlDbType.VarChar).Value = obj.Mega_Zone;
                    sqlComm.Parameters.Add("@zone", SqlDbType.VarChar).Value = obj.Zone;
                    sqlComm.Parameters.Add("@region", SqlDbType.VarChar).Value = obj.Region;
                    sqlComm.Parameters.Add("@branch", SqlDbType.VarChar).Value = obj.Branch;
                    sqlComm.Parameters.Add("@Grade", SqlDbType.VarChar).Value = obj.Grade;
                    sqlComm.Parameters.Add("@RA", SqlDbType.VarChar).Value = obj.RA;
                    sqlComm.Parameters.Add("@Year", SqlDbType.VarChar).Value = obj.Year;
                    sqlComm.Parameters.Add("@Reason", SqlDbType.VarChar).Value = obj.Reason;
                    sqlComm.Parameters.Add("@Remark", SqlDbType.VarChar).Value = obj.Remark;
                    sqlComm.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = obj.CreatedBy;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetRole(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_Role", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    
                    sqlComm.Parameters.Add("@OFFSET", SqlDbType.VarChar).Value = obj.OFFSET;
                    sqlComm.Parameters.Add("@LIMIT", SqlDbType.VarChar).Value = obj.LIMIT;


                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetMaingroupSubgropDepartment(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_MAINGROUP_SUBGROUP_DEPARTMENT", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    
                    sqlComm.Parameters.Add("@Main_Group", SqlDbType.VarChar).Value = obj.Main_Group;
                    sqlComm.Parameters.Add("@Subgroup", SqlDbType.VarChar).Value = obj.Subgroup;


                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet InsertBusinessMappingData(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_INSERT_Business_Mapping_Data", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    
                    sqlComm.Parameters.Add("@maingrp", SqlDbType.VarChar).Value = obj.Main_Group;
                    sqlComm.Parameters.Add("@subgrp", SqlDbType.VarChar).Value = obj.Subgroup;
                    sqlComm.Parameters.Add("@deptgrp", SqlDbType.VarChar).Value = obj.Department;
                    sqlComm.Parameters.Add("@Jobcode", SqlDbType.VarChar).Value = obj.Jobcode;
                    sqlComm.Parameters.Add("@Description", SqlDbType.VarChar).Value = obj.Description;
                    sqlComm.Parameters.Add("@Descrshort", SqlDbType.VarChar).Value = obj.DescrShort;
                    sqlComm.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = obj.CreatedBy;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet UpdateJobCodeInBusinessMapping(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_UPDATE_JOBCODEINBUSINESSMAPPING", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)  
                    sqlComm.Parameters.Add("@RefId", SqlDbType.VarChar).Value = obj.RefId;
                    sqlComm.Parameters.Add("@Jobcode", SqlDbType.VarChar).Value = obj.Jobcode;
                    sqlComm.Parameters.Add("@Description", SqlDbType.VarChar).Value = obj.Description;
                    sqlComm.Parameters.Add("@descShort", SqlDbType.VarChar).Value = obj.DescrShort;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataTable IndentReport(BudgetEntity obj)
        {
            DataTable ds_Data = new DataTable();
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_INDENT_REPORT", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs) 
                    sqlComm.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = obj.FromDate;
                    sqlComm.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = obj.ToDate;
                    sqlComm.Parameters.Add("@Location", SqlDbType.VarChar).Value = obj.Location;
                    sqlComm.Parameters.Add("@IndentType", SqlDbType.VarChar).Value = obj.IndentType;
                    sqlComm.Parameters.Add("@RequestorName", SqlDbType.VarChar).Value = obj.RequestorName;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }
            return ds_Data;
        }

        public DataTable BudgetingReport(BudgetEntity obj)
        {
            DataTable ds_Data = new DataTable();
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_BUDGETING_REPORT", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs) 
                    sqlComm.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = obj.FromDate;
                    sqlComm.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = obj.ToDate;
                    sqlComm.Parameters.Add("@Location", SqlDbType.VarChar).Value = obj.Location;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }
            return ds_Data;
        }

        public DataTable GradeReport(BudgetEntity obj)
        {
            DataTable ds_Data = new DataTable();
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_GRADE_REPORT", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs) 
                    sqlComm.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = obj.FromDate;
                    sqlComm.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = obj.ToDate;
                    sqlComm.Parameters.Add("@Grade", SqlDbType.VarChar).Value = obj.Grade;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }
            return ds_Data;
        }

        public DataTable LocationReport(BudgetEntity obj)
        {
            DataTable ds_Data = new DataTable();
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_LOCATION_REPORT", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs) 
                    sqlComm.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = obj.FromDate;
                    sqlComm.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = obj.ToDate;
                    sqlComm.Parameters.Add("@Location", SqlDbType.VarChar).Value = obj.Location;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }
            return ds_Data;
        }

        public DataTable JobcodeReport(BudgetEntity obj)
        {
            DataTable ds_Data = new DataTable();
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_JOBCODE_REPORT", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs) 
                    sqlComm.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = obj.FromDate;
                    sqlComm.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = obj.ToDate;
                    sqlComm.Parameters.Add("@Jobcode", SqlDbType.VarChar).Value = obj.Jobcode;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }
            return ds_Data;
        }

        public DataTable BusinessLevelReport(BudgetEntity obj)
        {
            DataTable ds_Data = new DataTable();
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_DEPT_TREE_REPORT", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs) 
                    sqlComm.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = obj.FromDate;
                    sqlComm.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = obj.ToDate;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }
            return ds_Data;
        }

        public DataTable ErrorLogFilter(BudgetEntity obj)
        {
            DataTable ds_Data = new DataTable();
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_ERROR_FILTER", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs) 
                    sqlComm.Parameters.Add("@Type", SqlDbType.VarChar).Value = obj.Type;
                    sqlComm.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = obj.FromDate;
                    sqlComm.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = obj.ToDate;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }
            return ds_Data;
        }

        public DataTable SenderEmail(BudgetEntity obj)
        {
            DataTable ds_Data = new DataTable();
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_MailUserDetails", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs) 
                    sqlComm.Parameters.Add("@Action", SqlDbType.VarChar).Value = obj.Action;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);
                }
            }
            return ds_Data;
        }

        public DataSet IndentGetRemarks(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_Indent_REMARK", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    

                    sqlComm.Parameters.Add("@RemarkId", SqlDbType.VarChar).Value = obj.ID;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetMZRMapping(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_MZR", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    

                    sqlComm.Parameters.Add("@Megazone", SqlDbType.VarChar).Value = obj.Mega_Zone;
                    sqlComm.Parameters.Add("@Zone", SqlDbType.VarChar).Value = obj.Zone;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet InsertMZRZoneMaster(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_INSERT_MZPMapping", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)
                    sqlComm.Parameters.Add("@Megazone", SqlDbType.VarChar).Value = obj.Mega_Zone;
                    sqlComm.Parameters.Add("@Zone", SqlDbType.VarChar).Value = obj.Zone;
                    sqlComm.Parameters.Add("@Region", SqlDbType.VarChar).Value = obj.Region;
                    sqlComm.Parameters.Add("@Location", SqlDbType.VarChar).Value = obj.Branch;
                    sqlComm.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = obj.CreatedBy;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet UpdateZoneMaster(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_UPDATE_ZONEMASTER", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)
                    sqlComm.Parameters.Add("@RefId", SqlDbType.VarChar).Value = obj.RefId;
                    sqlComm.Parameters.Add("@Megazone", SqlDbType.VarChar).Value = obj.Mega_Zone;
                    sqlComm.Parameters.Add("@Zone", SqlDbType.VarChar).Value = obj.Zone;
                    sqlComm.Parameters.Add("@Region", SqlDbType.VarChar).Value = obj.Region;
                    sqlComm.Parameters.Add("@ModifyBy", SqlDbType.VarChar).Value = obj.ModifyBy;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet SearchBusinesslevel(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_SEARCH_BUSINESSLEVEL", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)
                    sqlComm.Parameters.Add("@Search", SqlDbType.VarChar).Value = obj.Search;
                    sqlComm.Parameters.Add("@SearchOn", SqlDbType.VarChar).Value = obj.SearchOn;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet SearchLocationlevel(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_SEARCH_LOCATIONLEVEL", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)
                    sqlComm.Parameters.Add("@Search", SqlDbType.VarChar).Value = obj.Search;
                    sqlComm.Parameters.Add("@SearchOn", SqlDbType.VarChar).Value = obj.SearchOn;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet SearchIndent(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_SEARCH_INDENT", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)
                    sqlComm.Parameters.Add("@Search", SqlDbType.VarChar).Value = obj.Search;
                    sqlComm.Parameters.Add("@SearchOn", SqlDbType.VarChar).Value = obj.SearchOn;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet SearchBudgetDetails(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_SEARCH_BUDGETDETAILS", conn);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)  
                    sqlComm.Parameters.Add("@Search", SqlDbType.VarChar).Value = obj.Search;
                    sqlComm.Parameters.Add("@maingrp", SqlDbType.VarChar).Value = obj.Main_Group;
                    sqlComm.Parameters.Add("@subgrp", SqlDbType.VarChar).Value = obj.Subgroup;
                    sqlComm.Parameters.Add("@deptgrp", SqlDbType.VarChar).Value = obj.Department;
                    sqlComm.Parameters.Add("@rolegrp", SqlDbType.VarChar).Value = obj.Role;
                    sqlComm.Parameters.Add("@megazone", SqlDbType.VarChar).Value = obj.Mega_Zone;
                    sqlComm.Parameters.Add("@zone", SqlDbType.VarChar).Value = obj.Zone;
                    sqlComm.Parameters.Add("@region", SqlDbType.VarChar).Value = obj.Region;
                    sqlComm.Parameters.Add("@branch", SqlDbType.VarChar).Value = obj.Branch;
                    sqlComm.Parameters.Add("@SearchOn", SqlDbType.VarChar).Value = obj.SearchOn;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetUserDetails(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_UserDetails", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    
                    sqlComm.Parameters.Add("@OFFSET", SqlDbType.VarChar).Value = obj.OFFSET;
                    sqlComm.Parameters.Add("@LIMIT", SqlDbType.VarChar).Value = obj.LIMIT;
                    sqlComm.Parameters.Add("@UserId", SqlDbType.VarChar).Value = obj.UserId;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet GetCurrentFY(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_CURRENT_YEAR", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)    
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet InsertIndividualMZRMapping(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_INSERT_Individual_MZPMapping", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)
                    sqlComm.Parameters.Add("@Megazone", SqlDbType.VarChar).Value = obj.Mega_Zone;
                    sqlComm.Parameters.Add("@Zone", SqlDbType.VarChar).Value = obj.Zone;
                    sqlComm.Parameters.Add("@Region", SqlDbType.VarChar).Value = obj.Region;
                    sqlComm.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = obj.CreatedBy;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataTable ImportBudget(DataTable csvFileData, BudgetEntity obj)
        {
            DataTable ds_Data = new DataTable();

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("PROC_IMPORT_BUDGET", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = obj.CreatedBy;
                    SqlParameter tblvaluetype = cmd.Parameters.AddWithValue("@ImportBudget", csvFileData);  //Passing table value parameter
                    tblvaluetype.SqlDbType = SqlDbType.Structured; // This one is used to tell ADO.NET we are passing Table value Parameter
                    int result = cmd.ExecuteNonQuery();
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(ds_Data);
                }
            }


            return ds_Data;


        }

        public DataSet InsertBookmarkBudget(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_INSERT_BookmarkBudget", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)
                    sqlComm.Parameters.Add("@maingrp", SqlDbType.VarChar).Value = obj.Main_Group;
                    sqlComm.Parameters.Add("@subgrp", SqlDbType.VarChar).Value = obj.Subgroup;
                    sqlComm.Parameters.Add("@deptgrp", SqlDbType.VarChar).Value = obj.Department;
                    sqlComm.Parameters.Add("@rolegrp", SqlDbType.VarChar).Value = obj.Role;
                    sqlComm.Parameters.Add("@megazone", SqlDbType.VarChar).Value = obj.Mega_Zone;
                    sqlComm.Parameters.Add("@zone", SqlDbType.VarChar).Value = obj.Zone;
                    sqlComm.Parameters.Add("@region", SqlDbType.VarChar).Value = obj.Region;
                    sqlComm.Parameters.Add("@branch", SqlDbType.VarChar).Value = obj.Branch;
                    sqlComm.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = obj.CreatedBy;
                    sqlComm.Parameters.Add("@BookmarkName", SqlDbType.VarChar).Value = obj.BookmarkName;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }


        public DataSet GetBookmarkBudget(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_BookmarkBudget", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                  
                    sqlComm.Parameters.Add("@CreatedBy", SqlDbType.VarChar).Value = obj.CreatedBy;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }
        public DataSet DeleteBookmarkBudget(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_DELETE_BookmarkBudget", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                  
                    sqlComm.Parameters.Add("@PId", SqlDbType.VarChar).Value = obj.PId;
                    sqlComm.Parameters.Add("@DeletedBy", SqlDbType.VarChar).Value = obj.DeletedBy;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }

        public DataSet Logout(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");

            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_API_USER_Logout", conn);
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)                  
                    sqlComm.Parameters.Add("@User_Id", SqlDbType.VarChar).Value = obj.UserId;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);

                }
            }

            return ds_Data;
        }


        public DataSet GetTokenByUserId(BudgetEntity obj)
        {
            DataSet ds_Data = new DataSet("DS");
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    SqlCommand sqlComm = new SqlCommand("PROC_GET_TokenByUserId", conn);
                    sqlComm.Parameters.Add("@UserId", SqlDbType.VarChar).Value = obj.UserId;
                    sqlComm.CommandType = CommandType.StoredProcedure;
                    sqlComm.CommandTimeout = 300; //2 min (120 secs)
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds_Data);
                }
            }
            return ds_Data;
        }


    }
}
