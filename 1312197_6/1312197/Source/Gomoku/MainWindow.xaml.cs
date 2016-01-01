using System;
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
namespace Gomoku
{
    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Settings() { }

        private string _status = Connect._mes;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public partial class MainWindow : Window
    {
        int[,] caro = new int[n, n];
        int count = 0;
        public int PlayerWin = 0;//biến xác định người nào chơi thắng
        public int player = 0; //player = 1: người chơi thứ nhất, player = 2: người chơi thứ 2
        public int type = 0; //type = 1: người chơi với người, type = 2: người chơi với máy
        public int Player_owner; //người chơi khi chơi với máy
        public int Machine_owner; //máy khi chơi với máy
        public int turn = 0; //biến xét lượt chơi turn chẵn thì thì người thứ nhất đánh, lẻ người thứ 2 đánh
        public int Row, Col, AI_row, AI_col;
        bool check = false;
        bool Check = false;
        string name = "";
        Quobject.SocketIoClientDotNet.Client.Socket socket;
        public MainWindow()
        {
            InitializeComponent();
            //socket = IO.Socket("ws://gomoku-lajosveres.rhcloud.com:8000");
            //Connect.KetNoi(socket, txt_name.Text);
            ban_co.IsEnabled = false;
            worker.DoWork += find_location;
            worker.RunWorkerCompleted += AI_play;
           
        }
        public const int n = 12;
        private readonly BackgroundWorker worker = new BackgroundWorker();

        private void drawEllipse(int row, int col, Brush brush)
        {
            double top, left;
            top = row * (ban_co.ActualHeight / 12);
            left = col * (ban_co.ActualHeight / 12);
            Ellipse elip = new Ellipse();
            elip.Width = elip.Height = ban_co.ActualHeight / 12;
            elip.SetValue(Canvas.TopProperty, top);
            elip.SetValue(Canvas.LeftProperty, left);
            elip.Fill = brush;
            ban_co.Children.Add(elip);
        }
        
        private void btn_send_Click(object sender, RoutedEventArgs e)
        {
            Connect.GoiTinNhan(socket, txt_type.Text, name);
            txt_type.Text = "";
        }
        #region kiem tra dong
        public bool KiemTraDong1(int r, int c, int Player)
        {
            bool kt = false;
            int row = r;
            int col = c;
            if (row >= 0 && col >= 0)
            {
                if (caro[row, col] == Player)
                {
                    count++;
                    if (count == 5)
                    {
                        kt = true;
                        count = 0;
                        return kt;
                    }
                    kt = KiemTraDong1(--row, col, Player);
                }
                if (caro[r, c] == 0 || caro[r, c] == 1 || caro[r, c] == 2 || caro[r, c] == 3)
                {
                    count = 0;
                    return kt;
                }
            }
            return kt;
        }
        public bool KiemTraDong2(int r, int c, int Player)
        {
            bool kt = false;
           
            int row = r;
            int col = c;
            if (row >= 0 && col >= 0)
            {
                if (caro[row, col] == Player)
                {
                    count++;
                    if (count == 5)
                    {
                        kt = true;
                        count = 0;
                        return kt;
                    }
                    if (row < 11)
                        kt = KiemTraDong2(++row, col, Player);
                    else
                    {
                        count = 0;
                        return kt;
                    }
                }
                if (caro[r, c] == 0 || caro[r, c] == 1 || caro[r, c] == 2 || caro[r, c] == 3)
                {
                    count = 0;
                    return kt;
                }
            }
            return kt;
        }

        #endregion
        #region kiem tra cot
        public bool KiemTraCot1(int r, int c, int Player)
        {
            bool kt = false;
            int row = r;
            int col = c;
            if (row >= 0 && col >= 0)
            {
                if (caro[row, col] == Player)
                {
                    count++;
                    if (count == 5)
                    {
                        kt = true;
                        count = 0;
                        return kt;
                    }
                    kt = KiemTraCot1(row, --col, Player);
                }
                if (caro[r, c] == 0 || caro[r, c] == 1 || caro[r, c] == 2 || caro[r, c] == 3)
                {
                    count = 0;
                    return kt;
                }
            }
            return kt;
        }
        public bool KiemTraCot2(int r, int c, int Player)
        {
            bool kt = false;
            int row = r;
            int col = c;
            if (row >= 0 && col >= 0)
            {
                if (caro[row, col] == Player)
                {
                    count++;
                    if (count == 5)
                    {
                        kt = true;
                        count = 0;
                        return kt;
                    }
                    if (col < 11)
                    {
                        kt = KiemTraCot2(row, ++col, Player);
                    }
                    else
                    {
                        count = 0;
                        return kt;
                    }
                }
                if (caro[r, c] == 0 || caro[r, c] == 1 || caro[r, c] == 2 || caro[r, c] == 3)
                {
                    count = 0;
                    return kt;
                }
            }
            return kt;
        }
        #endregion
        #region kiem tra duong cheo xuoi \
        public bool KiemTraCheoXuoi1(int r, int c, int Player)
        {
            bool kt = false;
            int row = r;
            int col = c;
            if (row >= 0 && col >= 0)
            {
                if (caro[row, col] == Player)
                {
                    count++;
                    if (count == 5)
                    {
                        kt = true;
                        count = 0;
                        return kt;
                    }
                    kt = KiemTraCheoXuoi1(--row, --col, Player);
                }
                if (caro[r, c] == 0 || caro[r, c] == 1 || caro[r, c] == 2 || caro[r, c] == 3)
                {
                    count = 0;
                    return kt;
                }
            }
            return kt;
        }
        public bool KiemTraCheoXuoi2(int r, int c, int Player)
        {
            bool kt = false;
            int row = r;
            int col = c;
            if (row >= 0 && col >= 0)
            {
                if (caro[row, col] == Player)
                {
                    count++;
                    if (count == 5)
                    {
                        kt = true;
                        count = 0;
                        return kt;
                    }
                    if (row < 11 && col < 11)
                    {
                        kt = KiemTraCheoXuoi2(++row, ++col, Player);
                    }
                    else
                    {
                        count = 0;
                        return kt;
                    }
                }
                if (caro[r, c] == 0 || caro[r, c] == 1 || caro[r, c] == 2 || caro[r, c] == 3)
                {
                    count = 0;
                    return kt;
                }
            }
            return kt;
        }
        #endregion
        #region kiem tra cheo nguoc /
        public bool KiemTraCheoNguoc1(int r, int c, int Player)
        {
            bool kt = false;
            int row = r;
            int col = c;
            if (row >= 0 && col >= 0)
            {
                if (caro[row, col] == Player)
                {
                    count++;
                    if (count == 5)
                    {
                        kt = true;
                        count = 0;
                        return kt;
                    }
                    if (col < 11)
                    {
                        kt = KiemTraCheoNguoc1(--row, ++col, Player);
                    }
                    else
                    {
                        count = 0;
                        return kt;
                    }
                }
                if (caro[r, c] == 0 || caro[r, c] == 1 || caro[r, c] == 2 || caro[r, c] == 3)
                {
                    count = 0;
                    return kt;
                }
            }
            return kt;
        }
        public bool KiemTraCheoNguoc2(int r, int c, int Player)
        {
            bool kt = false;
            int row = r;
            int col = c;
            if (row >= 0 && col >= 0)
            {
                if (caro[row, col] == Player)
                {
                    count++;
                    if (count == 5)
                    {
                        kt = true;
                        count = 0;
                        return kt;
                    }
                    if (row < 11)
                    {
                        kt = KiemTraCheoNguoc2(++row, --col, Player);
                    }
                    else
                    {
                        count = 0;
                        return kt;
                    }
                }
                if (caro[r, c] == 0 || caro[r, c] == 1 || caro[r, c] == 2 || caro[r, c] == 3)
                {
                    count = 0;
                    return kt;
                }
            }
            return kt;
        }
        #endregion

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
        public void SetTurn(int row, int col)
        {
           
            if (turn % 2 == 0)
            {
                caro[row, col] = 1;
                //drawEllipse(row, col, Brushes.Red);
            }
            else
            {
                caro[row, col] = 2;
                //drawEllipse(row, col, Brushes.Blue);

            }
            
            turn++;
        }
        private void Result(int row, int col)
        {
            player = caro[row, col];
            if (KiemTraDong1(row, col, player) == true)
                PlayerWin = player;
            else if (KiemTraDong2(row, col, player) == true)
                PlayerWin = player;
            else if (KiemTraCot1(row, col, player) == true)
                PlayerWin = player;
            else if (KiemTraCot2(row, col, player) == true)
                PlayerWin = player;
            else if (KiemTraCheoXuoi1(row, col, player) == true)
                PlayerWin = player;
            else if (KiemTraCheoXuoi2(row, col, player) == true)
                PlayerWin = player;
            else if (KiemTraCheoNguoc1(row, col, player) == true)
                PlayerWin = player;
            else if (KiemTraCheoNguoc2(row, col, player) == true)
                PlayerWin = player;
            if(type == 1)
            {
                if (PlayerWin == 1)
                {
                    MessageBox.Show("Người chơi bi màu đỏ thắng!");
                    ban_co.IsEnabled = false;
                }
                    
                else if (PlayerWin == 2)
                {
                    MessageBox.Show("Người chơi bị màu xanh thắng!");
                    ban_co.IsEnabled = false;
                }
                   
            }
            else if(type == 2 || type == 3)
            {
                if (PlayerWin == 1)
                {
                    MessageBox.Show("Người chơi bi màu đỏ thắng!");
                    ban_co.IsEnabled = false;
                }

                else if (PlayerWin == 2)
                {
                    MessageBox.Show("Máy đã thắng!");
                    ban_co.IsEnabled = false;
                }
                   
            }
            return;
                
        }
        
        private void AI_play(object sender, RunWorkerCompletedEventArgs e)
        {
            drawEllipse(AI_row, AI_col, Brushes.Blue);
            caro[AI_row, AI_col] = 2;
            if(type == 3)
            {
                Connect.GuiToaDo(socket, AI_row, AI_col);
            }
            Result(AI_row, AI_col);
            Check = false;
           
        }
        private void find_location(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(500);
            Point temp = ai_FindWay();
            AI_row = (int)temp.X;
            AI_col = (int)temp.Y;
        }

       
        private bool _check(int row, int col)
        {
            if (caro[Row, Col] != 0)
            {
                MessageBox.Show("Vị trí này đã được đánh");
                return true;
                
            }
            return false;
        }
        private void ban_co_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (Check == true)
            //{
            //    MessageBox.Show("Máy đang đánh!");
            //    return;
            //}
            Point p = e.GetPosition((IInputElement)sender);
            Row = ((int)p.Y / (int)(ban_co.ActualHeight / 12));
            Col = ((int)p.X / (int)(ban_co.ActualHeight / 12));
            //string kq = "(X, Y) = " + "(" + Row.ToString() + "," + Col.ToString() + ")";
            //if(caro[Row,Col] == 0)
            //{
            //    MessageBox.Show(kq, "Thông báo", MessageBoxButton.OK);
            //    check = false;
            //}
            //else
            //{
            //    MessageBox.Show("Vị trí này đã được đánh");
            //    check = true;
            //    return;
            //}
            check = _check(Row, Col);
            
            if(type == 1)
            {
                SetTurn(Row, Col);
                if (check == true)
                {
                    turn++;
                    return;
                }
                else
                {
                    if (caro[Row, Col] == 1)
                        drawEllipse(Row, Col, Brushes.Red);
                    else if (caro[Row, Col] == 2)
                        drawEllipse(Row, Col, Brushes.Blue);
                    Result(Row, Col);
                }
            }
            else if(type == 2)
            {
                if(check == false)
                {
                    
                    if(Check == false)
                    {
                        caro[Row, Col] = 1;
                        drawEllipse(Row, Col, Brushes.Red);
                        Result(Row, Col);
                        Check = true;
                        worker.RunWorkerAsync();
                    }
                    else
                    {
                        MessageBox.Show("Chưa đến lượt của bạn!");
                        
                    }
                }
            }
            else if (type == 3)
            {
               
                Connect.GuiToaDo(socket, Row, Col);
                
            }

        }

        private void ban_co_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
            drawBoard();
            
            
        }
        void drawBoard()
        {
            for (int x = 0; x < n; x++)
                for (int y = 0; y < n; y++)
                    caro[x, y] = 0;
            ban_co.Children.Clear();
            double wi = ban_co.ActualHeight / 12;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double top, left;
                    top = j * wi;
                    left = i * wi;
                    Rectangle rect = new Rectangle();
                    rect.Width = rect.Height = wi;
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
        private void btn_player_Click(object sender, RoutedEventArgs e)
        {
            ban_co.IsEnabled = true;
            type = 1;
            drawBoard();
            PlayerWin = 0;
        }

        private void btn_ai_Click(object sender, RoutedEventArgs e)
        {
            ban_co.IsEnabled = true;
            type = 2;
            drawBoard();
            PlayerWin = 0;
        }

        private void btn_newgame_Click(object sender, RoutedEventArgs e)
        {
            type = 3;
            socket = IO.Socket("ws://gomoku-lajosveres.rhcloud.com:8000");
            Connect.KetNoi(socket, txt_name.Text);
            drawBoard();
            ban_co.IsEnabled = true;
            PlayerWin = 0;
        }

        private void txt_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            //txt_name.Text = "";
        }

        private void btn_change_Click(object sender, RoutedEventArgs e)
        {
            Connect.DoiTen(socket, txt_name.Text);
        }

        private void txt_name_GotFocus(object sender, RoutedEventArgs e)
        {
            txt_name.Text = "";
        }

        private void btn_player_online_Click(object sender, RoutedEventArgs e)
        {
            ban_co.IsEnabled = true;
            type = 3;
            drawBoard();
            PlayerWin = 0;
            worker.RunWorkerAsync();
            
        }

        private void txt_row_TextChanged(object sender, TextChangedEventArgs e)
        {
            int p = int.Parse(txt_player.Text);
            int r = int.Parse(txt_row.Text);
            int c = int.Parse(txt_col.Text);
            int temp = 0;
           
            if (p == 0)
            {
                drawEllipse(r, c, Brushes.Red);
                caro[r, c] = p + 1;

              
            }
                
            else if (p == 1)
            {
                drawEllipse(r, c, Brushes.Blue);
                caro[r, c] = p + 1;
             
            }
            txt_player.Text = temp.ToString() ;
            txt_row.Text = temp.ToString();
            txt_col.Text = temp.ToString();
                
        }
     
    }
}
