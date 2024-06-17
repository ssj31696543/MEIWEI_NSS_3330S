using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace NSK_8000S.UIControl.ComboCheckBox
{
    public class Data : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool check;

        public bool Check
        {
            get
            {
                return check;
            }
            set {
                check = value;
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Check)));
                var handler = this.PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs("Check"));
                }
            }
        }

        public string Text { get; set; }

        public bool IsOverallChecked { get; set; }


    }
}
