using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
using NSK_8000S.UIControl.ComboCheckBox;

namespace NSK_8000S
{
    /// <summary>
    /// ComboCheckBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ComboCheckBox : UserControl
    {

        public delegate void OnCheckableChangedEventHandler(ComboCheckBox sender, IEnumerable<Data> changedData);

        private ObservableCollection<Data> datas = new ObservableCollection<Data>();

        public event OnCheckableChangedEventHandler CheckChanged;


        public ComboCheckBox()
        {
            InitializeComponent();

            comboBox.ItemsSource = datas;
        }

        public void AddDataItem(bool defaultChecked, string text, bool isOverallChecked) {
            datas.Add(new Data() {
                Check = defaultChecked,
                Text = text,
                IsOverallChecked = isOverallChecked
            });
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            var chkBox = sender as CheckBox;
            var data = chkBox.DataContext as Data;

            if(data.IsOverallChecked) {
                foreach (var item in datas.Where(x => x != data)) {
                    item.Check = data.Check;
                }

                this.textBlock.Text = data.Check ? "Check All" : string.Empty;
            } else {
                this.textBlock.Text = string.Join(", ", datas.Where(x => x.Check).Select(x => x.Text));
            }

            var handler = this.CheckChanged;
            if (handler != null)
            {
                handler(this, datas);
            }
        }
    }
}
