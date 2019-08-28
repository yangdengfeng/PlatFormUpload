using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PlatFormUpload.Common
{
    public class CommonUtil
    {
        public static string Base64Decode(Encoding encodeType, string result)
        {
            string decode = string.Empty;
            byte[] bytes = Convert.FromBase64String(result);
            try
            {
                decode = encodeType.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }


        public static void WriteFileBytes(string filePath, byte[] b)
        {
            if (File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(b);
                        bw.Close();
                    }
                    fs.Close();
                }
            }
            else
            {
                File.WriteAllBytes(filePath, b);
            }
        }


    }
}
