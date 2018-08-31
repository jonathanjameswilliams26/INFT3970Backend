using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using INFT3970Backend.Models;
using System.Collections;

namespace INFT3970Backend.Hubs
{
    public class NotificationHub : Hub
    {
        private static Hashtable clientsTable = new Hashtable(30);
        


        // represents a client that exists in the database, for now its just my name
        Player newPlayer = new Player { Nickname = "test"};

        string ClientFromDb = "mathew";


        // when any player connects, update all players lobby list with the new player
        public void UpdateLobbyList(string playerID) //connID of new player
        {

            Clients.All.recievePlayerList(newPlayer); // return player of new player
        }

        // call to add new player to hub register
        public void RegisterConID(string playerID)
        {
            clientsTable.Add(playerID, Context.ConnectionId);

            UpdateLobbyList(playerID);
        }

        public void SendNotifications(string name)
        {
            // this will check if the client is in the database
            if (ClientFromDb == name)
            {

                Clients.All.receiveNotification(name);
                Clients.All.setList();
            }
            else if (ClientFromDb != name)
            {
                //Probably should set other setting off like addin the client
                // notify all users
                Clients.All.receiveNotification(name + "not from the db");



            }
        }


        // when user disconnects message will be sent
        public void SendDisconnect()
        {
            // sends disconnection message to all users
            Clients.All.receiveNotification("Disconnection");
        }

        // when a user disconnects from the hub
        public override Task OnDisconnected(bool stopCalled)
        {
            SendDisconnect();
            return base.OnDisconnected(stopCalled);
        }
    }
}
