using osb.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace osb.Helpers
{
    public class osbDatabaseHelper
    {
        private readonly IConfiguration Configuration;

        public osbDatabaseHelper(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public List<StoryboarderModel> GetCommunityStoryboarder()
        {
            try
            {
                List<StoryboarderModel> listStoryboarder = new List<StoryboarderModel>();
                string spName = Configuration.GetSection("StoredProcedures")["GetCommunityStoryboarder"];
                using (SqlConnection sqlCon = new SqlConnection(Configuration.GetConnectionString("osbDB")))
                {
                    using (SqlCommand sqlCom = new SqlCommand(spName, sqlCon))
                    {
                        sqlCom.CommandType = System.Data.CommandType.StoredProcedure;
                        
                        sqlCon.Open();
                        using (SqlDataReader sqlDataReader = sqlCom.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            if (sqlDataReader.HasRows)
                            {
                                while (sqlDataReader.Read())
                                {
                                    int currentUserID = int.Parse(sqlDataReader["UserID"].ToString());
                                    int index = listStoryboarder.FindIndex(x => x.UserID == currentUserID);
                                    if (index == -1)
                                    {
                                        listStoryboarder.Add
                                        (
                                            new StoryboarderModel
                                            (
                                                currentUserID,
                                                sqlDataReader["UserName"].ToString().Trim(),
                                                string.Empty,
                                                new List<DiscordRoleModel>()
                                            )
                                        );
                                        index = listStoryboarder.FindIndex(x => x.UserID == currentUserID);
                                    }
                                    int roleID = int.Parse(sqlDataReader["RoleID"].ToString());
                                    string roleName = sqlDataReader["RoleName"].ToString();
                                    string roleHexColour = sqlDataReader["RoleHexColour"].ToString();
                                    // listStoryboarder[index].Roles.Add(new DiscordRoleModel(roleID, roleName, roleHexColour));
                                    // todo: remove direct access to private class data
                                }
                            }
                        }
                    }
                    sqlCon.Close();
                }

                return listStoryboarder;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
