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
