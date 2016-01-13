using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku
{
    public class Inotify: INotifyPropertyChanged
    {
         public event PropertyChangedEventHandler PropertyChanged;

         public Inotify() { }

         private string _mes;
         private int row;
         private int col;
         private int player;

         private int ai_row;

         public int Ai_row
         {
             get { return ai_row; }
             set
             {
                 ai_row = value;
                 OnPropertyChanged("Ai_row");
             }
         }
         private int ai_col;

         public int Ai_col
         {
             get { return ai_col; }
             set
             {
                 ai_col = value;
                 OnPropertyChanged("Ai_col");
             }
         }
         private int ai_player;

         public int Ai_player
         {
             get { return ai_player; }
             set
             {
                 ai_player = value;
                 OnPropertyChanged("Ai_player");
             }
         }

         public int Col
         {
             get { return col; }
             set
             { 
                 col = value;
                 OnPropertyChanged("Col");
             }
         }
         

         public int Player
         {
             get { return player; }
             set 
             {
                 player = value;
                 OnPropertyChanged("Player");
             }
         }

         public int Row
         {
             get { return row; }
             set 
             { 
                 row = value;
                 OnPropertyChanged("Row");
             }
         }
        public string Mes
        {
            get { return _mes; }
            set
            {
                _mes = value;
                OnPropertyChanged("Mes");
            }
        }

        private string first;
        public string First
        {
            get { return first; }
            set
            {
                first = value;
                OnPropertyChanged("First");
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
}
