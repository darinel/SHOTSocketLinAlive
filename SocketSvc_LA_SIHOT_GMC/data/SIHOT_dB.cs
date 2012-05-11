using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Microsoft.ApplicationBlocks.Data;


    class SIHOT_dB
    {
        public static void InsertSIHOT_ResStart(SIHOT value)
        {
            //Hotel Poperty
            SqlParameter[] param = new SqlParameter[] {
		new SqlParameter("@TN", value.TN),
		new SqlParameter("@OC", value.OC),
		new SqlParameter("@HN", value.HN),
		new SqlParameter("@RC", value.RC)
	};
            SqlHelper.ExecuteNonQuery(Helper.ConnectionString, CommandType.StoredProcedure, "insert_tblComm_SIHOT", param);
        }
        //public static SIHOT_Reservation Read(SqlDataReader reader)
        //{
        //    SIHOT_Reservation retval = new SIHOT_Reservation();

        //    for (int i = 0; i < reader.FieldCount; i++)
        //    {
        //        switch (reader.GetName(i))
        //        {
        //            case "TN":
        //                retval.TN = Helper.ToInt32(reader[i]);
        //                break;
        //        }
        //    }
        //}
    }

