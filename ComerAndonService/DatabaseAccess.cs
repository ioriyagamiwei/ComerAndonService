using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComerAndonService
{
    class DatabaseAccess
    {
        internal static List<string> GetAllPlants()
        {
            List<string> plants = new List<string>();
            SqlConnection conn = ConnectionManager.GetConnection();
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            string query = @"select LineID from Line_Information";
            try
            {
                cmd = new SqlCommand(query, conn);
                cmd.CommandTimeout = 300;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    plants.Add(reader["LineID"].ToString());
                }
            }
            catch(Exception ex)
            {
                Logger.WriteErrorLog("Exception while fetching All Plants : " + ex.Message);
            }
            finally
            {
                if (reader != null) reader.Close();
                if (conn != null) conn.Close();
            }
            return plants;
        }

        internal static bool SaveLineStationStatus(string plant, string machine)
        {
            bool isUpdated = false;
            SqlConnection conn = ConnectionManager.GetConnection();
            SqlCommand cmd = null;
            string proc = @"S_Get_LineStationStatusSaveView";
            try
            {
                cmd = new SqlCommand(proc, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LineId", plant);
                //cmd.Parameters.AddWithValue("@machine", machine);
                cmd.Parameters.AddWithValue("@Param", "");
                cmd.CommandTimeout = 300;
                int affected=cmd.ExecuteNonQuery();

                if (affected >= 0)
                    isUpdated = true;
            }
            catch(Exception ex)
            {
                Logger.WriteErrorLog("Exception in Executing Proc :" + ex.Message);
                isUpdated = false;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
            return isUpdated;
        }

        internal static List<AllPlantsAndMachinesDTO> GetAllPlantsAndMachines()
        {
            List<AllPlantsAndMachinesDTO> allPlantsNMachines = new List<AllPlantsAndMachinesDTO>();
            SqlConnection conn = ConnectionManager.GetConnection();
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            string query = @"select PlantID,MachineID from PlantMachine";
            try
            {
                cmd = new SqlCommand(query, conn);
                cmd.CommandTimeout = 300;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    AllPlantsAndMachinesDTO allPlantsAndMachines = new AllPlantsAndMachinesDTO();
                    allPlantsAndMachines.Plant = reader["PlantID"].ToString();
                    allPlantsAndMachines.Machine = reader["MachineID"].ToString();
                    allPlantsNMachines.Add(allPlantsAndMachines);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog("Exception while fetching All Machines and Plants : " + ex.Message);
            }
            finally
            {
                if (reader != null) reader.Close();
                if (conn != null) conn.Close();
            }
            return allPlantsNMachines;
        }
    }
}
