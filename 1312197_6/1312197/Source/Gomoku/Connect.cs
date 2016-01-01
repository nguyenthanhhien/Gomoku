using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using System.Windows;
namespace Gomoku
{
    public class Connect
    {
        public static bool turn;
        public static string _name;
        public static int _row, _col;
        public static string _mes;
        public static void KetNoi(Quobject.SocketIoClientDotNet.Client.Socket socket, string name)
        {

            socket.On(Socket.EVENT_CONNECT, () =>
            {
                MessageBox.Show("Connected", "Thông báo", MessageBoxButton.OK);

            });
            //socket.On(Socket.EVENT_MESSAGE, (data) =>
            //{
            //    MessageBox.Show(data.ToString());
            //    _mes = data.ToString();
            //});
           
            socket.On("ChatMessage", (data) =>
            {
                //string s = "You are the first player!";
                //turn = data.ToString().Contains(s);
                string ten = "";
                var o = JObject.Parse(data.ToString());
                if ((string)o["from"] != null)
                {
                    ten = (string)o["from"];
                }
                else
                {
                    ten = "Server";
                }
                var jobject = data as JToken;
                AppSetting.a.Mes += ten +"\n" + jobject.Value<String>("message") + "  " + DateTime.Now.ToString() + "\n\n";
                
                if (((Newtonsoft.Json.Linq.JObject) data)["message"].ToString() == "Welcome!")
                {
                    socket.Emit("MyNameIs", name);
                    socket.Emit("ConnectToOtherPlayer");
                    _name = name;
                   
                }
            });
            socket.On(Socket.EVENT_ERROR, (data) =>
            {
                MessageBox.Show(data.ToString());
            });
            socket.On("NextStepIs", (data) =>
            {
               
                var jobject = data as JToken;
                AppSetting.a.Player += jobject.Value<int>("player");
                AppSetting.a.Row += jobject.Value<int>("row");
                AppSetting.a.Col += jobject.Value<int>("col");
               
            });
            socket.On("EndGame", (data) =>
            {
                var jobject = data as JToken;
                AppSetting.a.Mes += jobject.Value<String>("message") + "  " + DateTime.Now.ToString() + "\n\n";

            });
           
        }
        public static void GuiToaDo(Quobject.SocketIoClientDotNet.Client.Socket socket, int row, int col)
        {
            socket.On(Socket.EVENT_ERROR, (data) =>
            {
                MessageBox.Show(data.ToString());
            });
            socket.Emit("MyStepIs", JObject.FromObject(new { row = row, col = col }));
        }
        public static void DoiTen(Quobject.SocketIoClientDotNet.Client.Socket socket, string name)
        {
            socket.Emit("MyNameIs", name);
            socket.Emit("message:", _name + "is now called" + name);
            _name = name;
        }
        public static void GoiTinNhan(Quobject.SocketIoClientDotNet.Client.Socket socket, string mes, string name)
        {
            socket.Emit("ChatMessage", mes);
            socket.Emit("message:" + mes, "from:" + name);
        }
        

    }
}
