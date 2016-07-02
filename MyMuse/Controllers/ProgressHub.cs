using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;
using MyMuse.Models;

namespace MyMuse
{
    public class ProgressHub : Hub
    {
        public void GetStarted()
        {
            Clients.Caller.sendMessage(string.Format("Reading Media Library."));
        }

        public static void Disable()
        {
            var PH = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            PH.Clients.All.disableProgress();
        }

        public static void ReportMessage(string workfile, int x, int y, double pct)
        {
            pct = pct * 100;
            var PH = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            if (x < y)
                PH.Clients.All.reportProgress(workfile, string.Format("Processing: {2:P1}." + "  {0} of {1} files processed.", x, y, pct), pct);
            else
                PH.Clients.All.reportProgress(" ", string.Format("Finished processing {0} files.", y), pct);
        }
    }

    public class Chat2Hub : Hub
    {
        public void Send(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.addNewMessageToPage(name, message);
        }
    }
    public class ChatHub : Hub
    {
        #region Data Members

        static List<UserDetail> ConnectedUsers = new List<UserDetail>();
        static List<MessageDetail> CurrentMessage = new List<MessageDetail>();

        #endregion

        #region Methods

        public void Connect(string userName)
        {
            var id = Context.ConnectionId;


            if (ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
            {
                ConnectedUsers.Add(new UserDetail { ConnectionId = id, ChatName = userName });

                // send to caller
                Clients.Caller.onConnected(id, userName, ConnectedUsers, CurrentMessage);

                // send to all except caller client
                Clients.AllExcept(id).onNewUserConnected(id, userName);

            }

        }

        public void SendMessageToAll(string userName, string message)
        {
            // store last 100 messages in cache
            AddMessageinCache(userName, message);

            // Broad cast message
            Clients.All.messageReceived(userName, message);
        }

        public void SendPrivateMessage(string toUserId, string message)
        {

            string fromUserId = Context.ConnectionId;

            var toUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == toUserId);
            var fromUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == fromUserId);

            if (toUser != null && fromUser != null)
            {
                // send to 
                Clients.Client(toUserId).sendPrivateMessage(fromUserId, fromUser.ChatName, message);

                // send to caller user
                Clients.Caller.sendPrivateMessage(toUserId, fromUser.ChatName, message);
            }

        }

        public override Task OnDisconnected(bool stoppit)
        {
            var item = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                ConnectedUsers.Remove(item);

                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, item.ChatName);

            }

            return base.OnDisconnected(stoppit);
        }


        #endregion

        #region private Messages

        private void AddMessageinCache(string userName, string message)
        {
            CurrentMessage.Add(new MessageDetail { ChatName = userName, Message = message });

            if (CurrentMessage.Count > 100)
                CurrentMessage.RemoveAt(0);
        }

        #endregion
    }
}