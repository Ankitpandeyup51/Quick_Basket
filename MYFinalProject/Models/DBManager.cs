using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace MYFinalProject.Models
{
    public class DBManager
    { //15 Hours
      //add cat , select cat , add product, select product, registration, login , session check , add to cart, cart manage, order , order history , admin- manage user , manage order , status change , my profile
      //call this function to execute Insert , Update, Delete in Database
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["constr"].ConnectionString);
        public int ExecuteSQL(string command)
        {
           
            SqlCommand cmd = new SqlCommand(command,con);
            con.Open();
            int response=cmd.ExecuteNonQuery();
            con.Close();
            return response;
        }
        //Call this function to select table from database
        public DataTable ExecuteSelect(string command) 
        {
            SqlCommand cmd = new SqlCommand(command, con);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

    }
}