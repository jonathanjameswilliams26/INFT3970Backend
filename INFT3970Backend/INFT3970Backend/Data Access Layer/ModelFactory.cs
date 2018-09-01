using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using INFT3970Backend.Models;

namespace INFT3970Backend.Data_Access_Layer
{
    public class ModelFactory
    {
        private SqlDataReader Reader;

        public ModelFactory(SqlDataReader reader)
        {
            Reader = reader;
        }

        private int GetColIndex(string colName)
        {
            try
            {
                return Reader.GetOrdinal(colName);
            }
            catch
            {
                return -1;
            }
        }

        private int GetInt(string colName)
        {
            return Reader.GetInt32(GetColIndex(colName));
        }


        private bool GetBool(string colName)
        {
            return Reader.GetBoolean(GetColIndex(colName));
        }


        private string SafeGetString(string colName)
        {
            int index = GetColIndex(colName);
            if (!Reader.IsDBNull(index))
                return Reader.GetString(index);
            return string.Empty;
        }





        public Player PlayerFactory()
        {
            try
            {
                Player player = new Player();
                player.PlayerID = GetInt("PlayerID");
                player.Nickname = SafeGetString("Nickname");
                player.Phone = SafeGetString("Phone");
                player.Email = SafeGetString("Email");
                player.SelfieFilePath = SafeGetString("SelfieFilePath");
                player.NumKills = GetInt("NumKills");
                player.NumDeaths = GetInt("NumDeaths");
                player.NumPhotosTaken = GetInt("NumPhotosTaken");
                player.IsHost = GetBool("IsHost");
                player.IsVerified = GetBool("IsVerified");
                player.ConnectionID = SafeGetString("ConnectionID");
                player.IsConnected = GetBool("IsConnected");
                player.IsActive = GetBool("IsActive");
                return player;
            }
            catch
            {
                return null;
            }
        }
    }
}
