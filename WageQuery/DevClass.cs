using libzkfpcsharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WageQuery
{
    class DevClass
    {

        static IntPtr mDevHandle = IntPtr.Zero;
        static IntPtr mDBHandle = IntPtr.Zero;


        static byte[] FPBuffer = new byte[204800];
        public static byte[] CapTmp = new byte[2048];
        static byte[] RegTmp = new byte[2048];
        static int cbCapTmp = 2048;


        static public void Init()
        {
            if (zkfp2.Init() == zkfperrdef.ZKFP_ERR_OK)
            {
                int nCount = zkfp2.GetDeviceCount();
                if (nCount < 0)
                {
                    zkfp2.Terminate();
                    Console.WriteLine("No device connected!");
                }
            }

            if (IntPtr.Zero == (mDevHandle = zkfp2.OpenDevice(0)))
            {
                Console.WriteLine("OpenDevice fail");
                return;
            }

            if (IntPtr.Zero == (mDBHandle = zkfp2.DBInit()))
            {
                Console.WriteLine("Init DB fail");
                zkfp2.CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
                return;
            }
        }

        static public byte[] Capture() //失败返回null
        {
            cbCapTmp = 2048;
            while (true)
            {
                int ret = zkfp2.AcquireFingerprint(mDevHandle, FPBuffer, CapTmp, ref cbCapTmp);
                if (ret == zkfp.ZKFP_ERR_OK)
                {
                    return CapTmp;
                }
                else if (ret == zkfp.ZKFP_ERR_FAIL)
                    return null;
            }
        }

        static public int Verify() // 返回0 匹配失败，否则返回userid
        {
            string sql = @"select user_id, Template_Data  FROM Biotemplate where nOldType > 3";
            DataTable dt = SQLRepository.ExecuteQueryToDataTable(SQLRepository.hsbs_strConn, sql);


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                object o = dt.Rows[i]["Template_Data"];
                if (!Convert.IsDBNull(o) && o != null)
                {
                    RegTmp = zkfp2.Base64ToBlob((string)o);
                    int ret = zkfp2.DBMatch(mDBHandle, CapTmp, RegTmp);

                    if (0 < ret)//匹配成功
                        return Convert.ToInt32(dt.Rows[i]["user_id"]);
                }
            }
            return 0;
        }


        static public void Free()
        {
            if (IntPtr.Zero != mDBHandle)
            {
                zkfp2.DBFree(mDBHandle);
                mDBHandle = IntPtr.Zero;
            }
            if (mDevHandle != IntPtr.Zero)
            {
                zkfp2.CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
            }
            zkfp2.Terminate();
        }

    }
}
