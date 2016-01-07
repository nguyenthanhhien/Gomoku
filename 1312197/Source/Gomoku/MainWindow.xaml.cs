﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using System.Configuration;
namespace Gomoku
{
    
    public partial class MainWindow : Window
    {
        public const int n = 12;
        int[,] caro = new int[n, n];
        int count = 0;
        public int PlayerWin = 0;//biến xác định người nào chơi thắng
        public int player = 0; //player = 1: người chơi thứ nhất, player = 2: người chơi thứ 2
        public static int type = 0; //type = 1: người chơi với người, type = 2: người chơi với máy
        public int turn = 0; //biến xét lượt chơi turn chẵn thì thì người thứ nhất đánh, lẻ người thứ 2 đánh
        public int Row, Col, AI_row, AI_col;
        bool check = false; //kiểm tra vị trí đã được đánh chưa
        public bool Check = false; //xét lượt cho máy đánh true: máy đánh, false: người đánh
        string name = "";
        double banco_Width, banco_Height;
        public static TextBox txt = new TextBox(); //dùng bắt sự kiện máy đánh trước khi máy đánh online
        Quobject.SocketIoClientDotNet.Client.Socket socket;
        public MainWindow()
        {
            InitializeComponent();
            ban_co.IsEnabled = false;
            worker.DoWork += find_location;
            worker.RunWorkerCompleted += AI_play;
            txt.TextChanged += new TextChangedEventHandler(txt_change);
        }
        private readonly BackgroundWorker worker = new BackgroundWorker();

        //Khi biến turn ở Connect = true thì gọi sự kiện này
        private void txt_change(object sender, TextChangedEventArgs e)
        {
            Connect.GuiToaDo(socket, 1, 1);
        }
        private void drawEllipse(int row, int col, Brush brush) //Vẽ ellip
        {
            double top = 0, left = 0;
            top = row * banco_Width;
            left = col * banco_Width;
            Ellipse elip = new Ellipse();
            elip.Width = elip.Height = banco_Width;
            elip.SetValue(Canvas.TopProperty, top);
            elip.SetValue(Canvas.LeftProperty, left);
            elip.Fill = brush;
            ban_co.Children.Add(elip);
        }
        
        private void btn_send_Click(object sender, RoutedEventArgs e) //Gởi tin nhắn
        {
            Connect.GoiTinNhan(socket, txt_type.Text, name);
            txt_type.Text = "";
        }

        #region tim diem may danh
        public long[] aScore = new long[7] { 0, 3, 24, 192, 1536, 12288, 98304 };
        public long[] dScore = new long[7] { 0, 1, 9, 81, 729, 6561, 59849 };
        private Point ai_FindWay()
        {
            Point res = new Point();
            long max_Mark = 0; //điểm để xác định nước đi

            for (int i = 1; i < n; i++)
            {
                for (int j = 1; j < n; j++)
                {
                    if (caro[i, j] == 0)
                    {
                        long Attackscore = DiemTC_DuyetDoc(i, j) + DiemTC_DuyetNgang(i, j) + DiemTC_DuyetCheoNguoc(i, j) + DiemTC_DuyetCheoXuoi(i, j);
                        long Defensescore = DiemPN_DuyetDoc(i, j) + DiemPN_DuyetNgang(i, j) + DiemPN_DuyetCheoNguoc(i, j) + DiemPN_DuyetCheoXuoi(i, j); ;
                        long tempMark = Attackscore > Defensescore ? Attackscore : Defensescore;
                        if (max_Mark < tempMark)
                        {
                            max_Mark = tempMark;
                            res.X = i;
                            res.Y = j;
                        }
                    }
                }
            }
            return res;
        }

        private long DiemTC_DuyetDoc(int currRow, int currCol)
        {
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            long Sum = 0;

            //duyệt từ dưới lên 
            for (int count = 1; count < 6 && currRow - count >= 0; count++)
            {
                if (caro[currRow - count, currCol] == 2)
                    SoQuanTa++;
                else if (caro[currRow - count, currCol] == 1)
                {
                    SoQuanDich++;
                    break;
                }
                else
                    break;
            }

            //duyet từ trên xuống
            for (int count = 1; count < 6 && currRow + count < n; count++)
            {
                if (caro[currRow + count, currCol] == 2)
                    SoQuanTa++;
                else if (caro[currRow + count, currCol] == 1)
                {
                    SoQuanDich++;
                    break;
                }
                else
                    break;
            }
            if (SoQuanDich == 2)
                return 0;
            Sum -= dScore[SoQuanDich + 1];
            Sum += aScore[SoQuanTa];
            return Sum;
        }
        private long DiemTC_DuyetNgang(int currRow, int currCol)
        {
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            long Sum = 0;

            //duyệt từ phải sang trái
            for (int count = 1; count < 6 && currCol - count >= 0; count++)
            {
                if (caro[currRow, currCol - count] == 2)
                    SoQuanTa++;
                else if (caro[currRow, currCol - count] == 1)
                {
                    SoQuanDich++;
                    break;
                }
                else
                    break;
            }

            //duyet từ trái sang phải
            for (int count = 1; count < 6 && currCol + count < n; count++)
            {
                if (caro[currRow, currCol + count] == 2)
                    SoQuanTa++;
                else if (caro[currRow, currCol + count] == 1)
                {
                    SoQuanDich++;
                    break;
                }
                else
                    break;
            }
            if (SoQuanDich == 2)
                return 0;
            Sum -= dScore[SoQuanDich + 1];
            Sum += aScore[SoQuanTa];
            return Sum;
        }
        private long DiemTC_DuyetCheoXuoi(int currRow, int currCol)
        {
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            long Sum = 0;

            //duyệt góc phải trên
            for (int count = 1; count < 6 && currRow - count >= 0 && currCol + count < n; count++)
            {
                if (caro[currRow - count, currCol + count] == 2)
                    SoQuanTa++;
                else if (caro[currRow - count, currCol + count] == 1)
                {
                    SoQuanDich++;
                    break;
                }
                else
                    break;
            }

            //duyet goc trái dưới
            for (int count = 1; count < 6 && currRow + count < n && currCol - count >= 0; count++)
            {
                if (caro[currRow + count, currCol - count] == 2)
                    SoQuanTa++;
                else if (caro[currRow + count, currCol - count] == 1)
                {
                    SoQuanDich++;
                    break;
                }
                else
                    break;
            }
            if (SoQuanDich == 2)
                return 0;
            Sum -= dScore[SoQuanDich + 1];
            Sum += aScore[SoQuanTa];
            return Sum;
        }
        private long DiemTC_DuyetCheoNguoc(int currRow, int currCol)
        {
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            long Sum = 0;

            //duyệt góc trái trên
            for (int count = 1; count < 6 && currRow - count >= 0 && currCol - count >= 0; count++)
            {
                if (caro[currRow - count, currCol - count] == 2)
                    SoQuanTa++;
                else if (caro[currRow - count, currCol - count] == 1)
                {
                    SoQuanDich++;
                    break;
                }
                else
                    break;
            }

            //duyet goc phải dưới
            for (int count = 1; count < 6 && currRow + count < n && currCol + count < n; count++)
            {
                if (caro[currRow + count, currCol + count] == 2)
                    SoQuanTa++;
                else if (caro[currRow + count, currCol + count] == 1)
                {
                    SoQuanDich++;
                    break;
                }
                else
                    break;
            }
            if (SoQuanDich == 2)
                return 0;
            Sum -= dScore[SoQuanDich + 1];
            Sum += aScore[SoQuanTa];
            return Sum;
        }
        private long DiemPN_DuyetDoc(int currRow, int currCol)
        {
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            long Sum = 0;

            //duyệt từ dưới lên 
            for (int count = 1; count < 6 && currRow - count >= 0; count++)
            {
                if (caro[currRow - count, currCol] == 2)
                {
                    SoQuanTa++;
                    break;
                }
                else if (caro[currRow - count, currCol] == 1)
                    SoQuanDich++;
                else
                    break;
            }

            //duyet từ trên xuống
            for (int count = 1; count < 6 && currRow + count < n; count++)
            {
                if (caro[currRow + count, currCol] == 2)
                {
                    SoQuanTa++;
                    break;
                }
                else if (caro[currRow + count, currCol] == 1)
                    SoQuanDich++;
                else
                    break;
            }
            if (SoQuanTa == 2)
                return 0;
            Sum += dScore[SoQuanDich];
            return Sum;
        }
        private long DiemPN_DuyetNgang(int currRow, int currCol)
        {
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            long Sum = 0;

            //duyệt từ phải sang trái
            for (int count = 1; count < 6 && currCol - count >= 0; count++)
            {
                if (caro[currRow, currCol - count] == 2)
                {
                    SoQuanTa++;
                    break;
                }
                else if (caro[currRow, currCol - count] == 1)
                    SoQuanDich++;
                else
                    break;
            }

            //duyet từ trái sang phải
            for (int count = 1; count < 6 && currCol + count < n; count++)
            {
                if (caro[currRow, currCol + count] == 2)
                {
                    SoQuanTa++;
                    break;
                }
                else if (caro[currRow, currCol + count] == 1)
                    SoQuanDich++;
                else
                    break;
            }
            if (SoQuanTa == 2)
                return 0;
            Sum += dScore[SoQuanDich];
            return Sum;
        }
        private long DiemPN_DuyetCheoXuoi(int currRow, int currCol)
        {
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            long Sum = 0;

            //duyet góc phải trên
            for (int count = 1; count < 6 && currRow - count >= 0 && currCol + count < n; count++)
            {
                if (caro[currRow - count, currCol + count] == 2)
                {
                    SoQuanTa++;
                    break;
                }
                else if (caro[currRow - count, currCol + count] == 1)
                    SoQuanDich++;
                else
                    break;
            }

            //duyet góc trái dưới
            for (int count = 1; count < 6 && currRow + count < n && currCol - count >= 0; count++)
            {
                if (caro[currRow + count, currCol - count] == 2)
                {
                    SoQuanTa++;
                    break;
                }
                else if (caro[currRow + count, currCol - count] == 1)
                    SoQuanDich++;
                else
                    break;
            }
            if (SoQuanTa == 2)
                return 0;
            Sum += dScore[SoQuanDich];
            return Sum;
        }
        private long DiemPN_DuyetCheoNguoc(int currRow, int currCol)
        {
            int SoQuanTa = 0;
            int SoQuanDich = 0;
            long Sum = 0;

            //duyet góc trái trên
            for (int count = 1; count < 6 && currRow - count >= 0 && currCol - count >= 0; count++)
            {
                if (caro[currRow - count, currCol - count] == 2)
                {
                    SoQuanTa++;
                    break;
                }
                else if (caro[currRow - count, currCol - count] == 1)
                    SoQuanDich++;
                else
                    break;
            }

            //duyet góc phải dưới
            for (int count = 1; count < 6 && currRow + count < n && currCol + count < n; count++)
            {
                if (caro[currRow + count, currCol + count] == 2)
                {
                    SoQuanTa++;
                    break;
                }
                else if (caro[currRow + count, currCol + count] == 1)
                    SoQuanDich++;
                else
                    break;
            }
            if (SoQuanTa == 2)
                return 0;
            Sum += dScore[SoQuanDich];
            return Sum;
        }
        #endregion


        #region kiểm tra chéo xuôi \
        public bool KiemTraCheoXuoi(int row, int col, int Player)
        {
            bool kt = false;
            count = 1;
            for (int i = row; i < n - 1; i++)
            {
                if(col <11)
                {
                    if (caro[i, col] == Player && caro[i + 1, col + 1] == Player)
                    {
                        count++;
                        col += 1;
                        if (count == 5)
                        {
                            kt = true;
                            count = 0;
                            return kt;
                        }
                    }
                }
                    
            }
            col = col - count + 1;
            for (int i = row; i > 0; i--)
            {
                if (col > 0) 
                {
                    if (caro[i, col] == Player && caro[i - 1, col - 1] == Player)
                    {
                        count++;
                        col -= 1;
                        if (count == 5)
                        {
                            kt = true;
                            count = 0;
                            return kt;
                        }
                    }
                }

            }
            
            return kt;
        }
        #endregion

        #region kiểm tra chéo ngược /
        public bool KiemTraCheoNguoc(int row, int col, int Player)
        {
            bool kt = false;
            count = 1;
            for (int i = row; i > 0; i--)
            {
                if (col < 11)
                {
                    if (caro[i, col] == Player && caro[i - 1, col + 1] == Player)
                    {
                        count++;
                        col += 1;
                        if (count == 5)
                        {
                            kt = true;
                            count = 0;
                            return kt;
                        }
                    }
                }
            }
            col = col - count + 1;
           
            for (int i = row; i < n - 1; i++)
            {
                if(col > 0)
                if (caro[i, col] == Player && caro[i + 1, col - 1] == Player)
                {
                    count++;
                    col -= 1;
                    if (count == 5)
                    {
                        kt = true;
                        count = 0;
                        return kt;
                    }
                }

            }
            
         
            
            return kt;
        }

        #endregion
        #region kiểm tra dòng
        public bool KiemTraDong(int row, int col, int Player)
        {
            bool kt = false;
            count = 1;
            for (int i = row; i < n - 1;i++)
            {
                if (caro[i, col] == Player && caro[i+1, col] == Player)
                {
                    count++;
                    if(count == 5)
                    {
                        kt = true;
                        count = 0;
                        return kt;
                    }
                }
            }
            for (int i = row; i > 0;i--)
            {
                if (caro[i, col] == Player && caro[i - 1, col] == Player)
                {
                    count++;
                    if (count == 5)
                    {
                        kt = true;
                        count = 0;
                        return kt;
                    }
                }
            }

            return kt;
        }
        #endregion
        #region kiểm tra cột
        public bool KiemTraCot(int row, int col, int Player)
        {
            bool kt = false;
          
            count = 1;
            for (int i = col; i < n - 1; i++)
            {
                if (caro[row, i] == Player && caro[row, i+1] == Player)
                {
                    count++;
                    if (count == 5)
                    {
                        kt = true;
                        count = 0;
                        return kt;
                    }
                }
            }
            for (int i = col; i > 0; i--)
            {
                if (caro[row, i] == Player && caro[row, i-1] == Player)
                {
                    count++;
                    if (count == 5)
                    {
                        kt = true;
                        count = 0;
                        return kt;
                    }
                }
            }

            return kt;
       
        }
        #endregion
        
        public void SetTurn(int row, int col) //Xét lượt chơi cho người
        {
            if (turn % 2 == 0)
            {
                caro[row, col] = 1;
                drawEllipse(row, col, Brushes.Red);
            }
            else
            {
                caro[row, col] = 2;
                drawEllipse(row, col, Brushes.Blue);

            }
            
            turn++;
        }
        private void Result(int row, int col)  //Kiểm tra kết quả chơi
        {
            player = caro[row, col];
            if (KiemTraDong(row, col, player) == true)
                PlayerWin = player;
            else if (KiemTraCot(row, col, player) == true)
                PlayerWin = player;
            else if (KiemTraCheoXuoi(row, col, player) == true)
                PlayerWin = player;
            else if (KiemTraCheoNguoc(row, col, player) == true)
                PlayerWin = player;
            if(type == 1)
            {
                if (PlayerWin == 1)
                {
                    MessageBox.Show("Người chơi bi màu đỏ thắng!");
                    ban_co.IsEnabled = false;
                    return;
                }
                    
                else if (PlayerWin == 2)
                {
                    MessageBox.Show("Người chơi bị màu xanh thắng!");
                    ban_co.IsEnabled = false;
                    return;
                }
                   
            }
            else if(type == 2)
            {
                if (PlayerWin == 1)
                {
                    MessageBox.Show("Người chơi bi màu đỏ thắng!");
                    ban_co.IsEnabled = false;
                    return;
                }

                else if (PlayerWin == 2)
                {
                    MessageBox.Show("Máy đã thắng!");
                    ban_co.IsEnabled = false;
                    return;
                }
                   
            }
            
                
        } 

        private void AI_play(object sender, RunWorkerCompletedEventArgs e) //Máy đánh
        {
            caro[AI_row, AI_col] = 2;
            drawEllipse(AI_row, AI_col, Brushes.Blue);
            Result(AI_row, AI_col);
            Check = false;
        }   
        private void find_location(object sender, DoWorkEventArgs e)  //Tìm vị trí cho máy đánh
        {
            Thread.Sleep(500);
            Point temp = ai_FindWay();
            AI_row = (int)temp.X;
            AI_col = (int)temp.Y;
        } 
        private void ban_co_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition((IInputElement)sender);
            Row = (int)(p.Y / banco_Width);
            Col = (int)(p.X / banco_Width);
            
            if(caro[Row, Col] != 0)
            {
                MessageBox.Show("Vị trí này đã được đánh");
                check = true;
                return;
            }
            if(type == 1) //người đánh với người
            {
                if(check == false)
                {
                    SetTurn(Row, Col);
                    Result(Row, Col);
                }
            }
            else if(type == 2) //người đánh với máy
            {
                if(check == false) //kiểm tra đánh trùng
                {
                    if (Check == false) //người đánh trước
                    {
                        caro[Row, Col] = 1;
                        drawEllipse(Row, Col, Brushes.Red);
                        Result(Row, Col);
                        if (PlayerWin == 1)
                            return;
                        Check = true;
                        worker.RunWorkerAsync();
                        
                    }
                    else
                    {
                        MessageBox.Show("Chưa đến lượt của bạn!");

                    }
                   
                }
            }
            else if (type == 3)// người đánh online
            {
                Connect.GuiToaDo(socket, Row, Col);
                
            }

        }
        private void ban_co_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            drawBoard();
        }
        void drawBoard()//Vẽ bàn cờ
        {
            for (int x = 0; x < n; x++)
                for (int y = 0; y < n; y++)
                    caro[x, y] = 0;
            banco_Width = banco_Height = ban_co.ActualWidth / 12;
            ban_co.Children.Clear();
            
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double top = 0, left = 0;
                    top = i * banco_Width;
                    left = j * banco_Width;
                    Rectangle rect = new Rectangle();
                    rect.Width = rect.Height = banco_Width;
                    rect.SetValue(Canvas.TopProperty, top);
                    rect.SetValue(Canvas.LeftProperty, left);
                    rect.Fill = Brushes.White;
                    if ((i + j) % 2 == 0)
                    {
                        rect.Fill = Brushes.Gray;
                    }
                    ban_co.Children.Add(rect);

                }
            }
            
        }
        private void btn_player_Click(object sender, RoutedEventArgs e)//Người đánh với người
        {
            ban_co.IsEnabled = true;
            type = 1;
            drawBoard();
            PlayerWin = 0;
        }

        private void btn_ai_Click(object sender, RoutedEventArgs e)//Người đánh với máy
        {
            ban_co.IsEnabled = true;
            type = 2;
            drawBoard();
            PlayerWin = 0;
        }
        private void btn_change_Click(object sender, RoutedEventArgs e) //Đổi tên
        {
            Connect.DoiTen(socket, txt_name.Text);
        } 
        private void txt_name_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_name.Text = "";
        }
        #region Người đánh online
        private void txt_row_TextChanged(object sender, TextChangedEventArgs e)
        {
            int p = AppSetting.a.Player;
            int r = AppSetting.a.Row;
            int c = AppSetting.a.Col;
            if (p == 0)
            {
                if (caro[r, c] != 0)
                {
                    return;
                }
                else
                    drawEllipse(r, c, Brushes.Red);
            }

            else if (p == 1)
            {
                if (caro[r, c] != 0)
                {
                    return;
                }
                else
                    drawEllipse(r, c, Brushes.Blue);

            }
        }
      
        private void txt_col_TextChanged(object sender, TextChangedEventArgs e)
        {
            int p = AppSetting.a.Player;
            int r = AppSetting.a.Row;
            int c = AppSetting.a.Col;
            if (p == 0)
            {
                if(caro[r,c] != 0)
                {
                    return;
                }
                else
                    drawEllipse(r, c, Brushes.Red);
            }

            else if (p == 1)
            {
                if (caro[r, c] != 0)
                {
                    return;
                }
                else
                    drawEllipse(r, c, Brushes.Blue);
            }
           
        }
        
        private void btn_player_online_Click_1(object sender, RoutedEventArgs e)
        {
            type = 3;
            string connect = ConfigurationManager.ConnectionStrings["LINK_GOMOKU"].ConnectionString;
            socket = IO.Socket(connect);
            Connect.KetNoi(socket, txt_name.Text);
            drawBoard();
            ban_co.IsEnabled = true;
            PlayerWin = 0;
        }
        #endregion
        #region Máy đánh online
        private void btn_machine_online_Click(object sender, RoutedEventArgs e)
        {
            type = 4;
            string connect = ConfigurationManager.ConnectionStrings["LINK_GOMOKU"].ConnectionString;
            socket = IO.Socket(connect);
            Connect.KetNoi(socket, txt_name.Text);
            drawBoard();
            ban_co.IsEnabled = true;
            PlayerWin = 0;
        }

        private void btn_ai_row_TextChanged(object sender, TextChangedEventArgs e)
        {
            int p = AppSetting.a.Ai_player;
            int r = AppSetting.a.Ai_row;
            int c = AppSetting.a.Ai_col;
            if (caro[r, c] != 0)
                return;
            if(p == 1)
            {
                drawEllipse(r, c, Brushes.Blue);
                caro[r, c] = 1;
                Point temp = ai_FindWay();
                AI_row = (int)temp.X;
                AI_col = (int)temp.Y;
                Connect.GuiToaDo(socket, AI_row, AI_col);
                
            }
            if(p == 0)
            {
                drawEllipse(r, c, Brushes.Red);
                caro[r, c] = 2;
            }
           
        }

        private void btn_ai_col_TextChanged(object sender, TextChangedEventArgs e)
        {
            int p = AppSetting.a.Ai_player;
            int r = AppSetting.a.Ai_row;
            int c = AppSetting.a.Ai_col;
            
            if (caro[r, c] != 0)
                return;
            if (p == 1)
            {
                drawEllipse(r, c, Brushes.Blue);
                caro[r, c] = 1;
                Point temp = ai_FindWay();
                AI_row = (int)temp.X;
                AI_col = (int)temp.Y;
                Connect.GuiToaDo(socket, AI_row, AI_col);
                
            }
            if (p == 0)
            {
                drawEllipse(r, c, Brushes.Red);
                caro[r, c] = 2;
            }

        }
        #endregion

        private void txt_mes_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
