using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FlowerMaster.Helpers
{
    class DESHelper
    {
        string key = "vW2-#$0&*G./3Kp;";
        string iv = "^yA._@rSnB%)D+~F!9l";

        public DESHelper()
        {
        }

        public DESHelper(string key, string iv)
        {
            this.key = key;
            this.iv = iv;
        }

        private string getMD5(string data)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] rdata = md5.ComputeHash(Encoding.ASCII.GetBytes(data));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rdata.Length; i++)
            {
                sb.AppendFormat("{0:x2}", rdata[i]);
            }
            return sb.ToString();
        }

        public string Encrypt(string data)
        {
            if (data == null || data.Length == 0)
                return String.Empty;
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.GetEncoding("UTF-8").GetBytes(data);

            MemoryStream ms = null;
            CryptoStream encStream = null;
            string result = String.Empty;
            try
            {
                des.Key = Encoding.ASCII.GetBytes(getMD5(this.key).Substring(8, 8));
                des.IV = Encoding.ASCII.GetBytes(getMD5(this.iv).Substring(19, 8));
                ms = new MemoryStream();
                encStream = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                encStream.Write(inputByteArray, 0, inputByteArray.Length);
                encStream.FlushFinalBlock();

                StringBuilder ret = new StringBuilder();
                foreach (byte b in ms.ToArray())
                {
                    ret.AppendFormat("{0:X2}", b);
                }
                result = ret.ToString();
            }
            finally
            {
                if (encStream != null)
                    encStream.Close();
                if (ms != null)
                    ms.Close();
            }
            return result;
        }

        public string Decrypt(string data)
        {
            if (data == null || data.Length == 0)
                return String.Empty;
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            byte[] inputByteArray = new byte[data.Length / 2];
            for (int x = 0; x < data.Length / 2; x++)
            {
                int i = (Convert.ToInt32(data.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            MemoryStream ms = null;
            CryptoStream encStream = null;
            string result = String.Empty;

            try
            {
                des.Key = Encoding.ASCII.GetBytes(getMD5(this.key).Substring(8, 8));
                des.IV = Encoding.ASCII.GetBytes(getMD5(this.iv).Substring(19, 8));
                ms = new MemoryStream();
                encStream = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
                encStream.Write(inputByteArray, 0, inputByteArray.Length);
                encStream.FlushFinalBlock();
                StringBuilder ret = new StringBuilder();
                result = Encoding.UTF8.GetString(ms.ToArray());
            }
            finally
            {
                if (encStream != null)
                    encStream.Close();
                if (ms != null)
                    ms.Close();
            }
            return result;
        }
    }
}
