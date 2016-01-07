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
        public static bool turn; //xét người đi trước hay máy đi trước khi máy chơi online
        public static string _name;
        public static string _mes;
        public static int dong, cot, nguoichoi;
        public static void KetNoi(Quobject.SocketIoClientDotNet.Client.Socket socket, string name)
        {

            socket.On(Socket.EVENT_CONNECT, () =>
            {
                //MessageBox.Show("Connected", "Thông báo", MessageBoxButton.OK);

            });
            socket.On("ChatMessage", (data) =>
            {
                string s = "You are the first player!";
                turn = data.ToString().Contains(s);
                if (MainWindow.type == 4 && turn == true)
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        MainWindow.txt.Text = data.ToString();
                    }));
                   
                }
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
                AppSetting.a.Mes += ten + "                              " + DateTime.Now.ToString() + "\n" + jobject.Value<String>("message") + "\n" + "....................................................................................\n";
                
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
                if(MainWindow.type == 3) //người đánh online
                {
                    AppSetting.a.Player = jobject.Value<int>("player");
                    AppSetting.a.Row = jobject.Value<int>("row");
                    AppSetting.a.Col = jobject.Value<int>("col");
                }
                else if(MainWindow.type == 4) //máy đánh online
                {
                    AppSetting.a.Ai_player = jobject.Value<int>("player");
                    AppSetting.a.Ai_row = jobject.Value<int>("row");
                    AppSetting.a.Ai_col = jobject.Value<int>("col");
                    
                }
            });
            socket.On("EndGame", (data) =>
            {
                var jobject = data as JToken;
                AppSetting.a.Mes += "Sever                              " + DateTime.Now.ToString() + "\n" + jobject.Value<String>("message") + "\n" + "....................................................................................\n";

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
