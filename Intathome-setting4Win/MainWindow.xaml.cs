using Codeplex.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Intathome_setting4Win
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public string beginhour = "00";
        public string beginmin = "00";
        public string endhour = "00";
        public string endmin = "00";
        public string function = "有効";
        public string ontime = "0";
        public string selectpath = "";
        public string myDocument = "";

        public MainWindow()
        {
            InitializeComponent();
            myDocument = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/intathome/";
            getrequest();
            checkchr();
            getvisitor();
        }

        private async Task getrequest()
        {
            string content = getjson("http://intathome.azurewebsites.net/api/light");
            content = content.Replace("\\", "");
            content = content.Substring(1, content.Length-2);
            dynamic json = DynamicJson.Parse(content);
            string tmp = json.begin;
            beginhour = tmp.Substring(0, 2);
            beginmin = tmp.Substring(2,2);
            tmp = json.end;
            endhour = tmp.Substring(0, 2);
            endmin = tmp.Substring(2, 2);
            if (json.function)
            {
                func.IsChecked = true;
                falHid.Visibility = Visibility.Visible;
            }
            else
            {
                func.IsChecked = false;
                falHid.Visibility = Visibility.Hidden;
            }
            ontime = json.ontime;
            content = getjson("http://intathome.azurewebsites.net/api/response");
            content = content.Replace("\\", "");
            content = content.Substring(1, content.Length - 2);
            json = DynamicJson.Parse(content);
            if(json.response)
            {
                autoreschk.IsChecked = true;
                select.Visibility = Visibility.Visible;
            }
            else
            {
                autoreschk.IsChecked = false;
                select.Visibility = Visibility.Hidden;
            }
        }

        public string[] date;
        public string[] time;
        public string[] img;
        public string[] fpath;

        public List<CBItem> dataList { get; set; }

        public void getvisitor()
        {
            dataList = new List<CBItem>();
            
            string content = getjson("http://intathome.azurewebsites.net/api/visitor");
            content = content.Replace("\\", "");
            content = content.Substring(1, content.Length - 2);
            dynamic json = DynamicJson.Parse(content);
            date = json.date;
            time = json.time;
            img = json.image;
            int len = date.Length;
            fpath = new string[len];
            Array.Copy(date, fpath, date.Length);
            for (int i = len-1; i != -1; i--)
            {
                CBItem item = new CBItem();
                string filename = date[i] + time[i];
                byte[] bs = Convert.FromBase64String(img[i]);
                FileStream fs = new FileStream(
                    @myDocument + filename + ".jpg",
                    FileMode.Create,
                    FileAccess.Write);
                fs.Write(bs, 0, bs.Length);
                fs.Close();
                fpath[i] = myDocument + filename + ".jpg";
                item.picture = fpath[i];
                string bind = date[i].Substring(0, 4) + "年" + date[i].Substring(4, 2) + "月" + date[i].Substring(6,2) +"日";
                bind += " " + time[i].Substring(0, 2) + "時" + time[i].Substring(2, 2) + "分" + time[i].Substring(4, 2) + "秒";
                item.date =　bind;
                dataList.Add(item);
            }
            ListBoxConverter.ItemsSource = dataList;
            ListBoxConverter.DataContext = this;
        }

        public void checkchr()
        {
            string tmp = beginhour.Substring(0, 1);
            if (tmp == "0")
            {
                startH.Text = beginhour.Substring(1, 1);
            }
            else
            {
                startH.Text = tmp;
            }
            tmp = beginmin.Substring(0, 1);
            if (tmp == "0")
            {
                startM.Text = beginmin.Substring(1, 1);
            }
            else
            {
                startM.Text = tmp;
            }
            tmp = endhour.Substring(0, 1);
            if (tmp == "0")
            {
                endH.Text = endhour.Substring(1, 1);
            }
            else
            {
                endH.Text = tmp;
            }
            tmp = endmin.Substring(0, 1);
            if (tmp == "0")
            {
                endM.Text = endmin.Substring(1, 1);
            }
            else
            {
                endM.Text = tmp;
            }
            onTim.Text = ontime;
        }

        public static string getjson(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Headers.Add("Auth", "intathome");
            req.Method = "GET";
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            Stream s = res.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            return sr.ReadToEnd();
        }

        public static async Task postjson(string url,dynamic json)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string beg = "";
            string en = "";
            if (func.IsChecked == true)
            {
                if(int.Parse(startH.Text) <= 9)
                {
                    beg += "0" + startH.Text;
                }
                else
                {
                    beg += startH.Text;
                }
                if(int.Parse(startM.Text) <= 9)
                {
                    beg += "0" + startM.Text;
                }
                else
                {
                    beg += startM.Text;
                }
                if(int.Parse(endH.Text) <= 9)
                {
                    en += "0" + endH.Text;
                }
                else
                {
                    en += endH.Text;
                }
                if(int.Parse(endM.Text) <= 9)
                {
                    en += "0" + endM.Text;
                }
                else
                {
                    en += endM.Text;
                }
                var obj = new
                {
                    begin = beg,
                    end = en,
                    function = func.IsChecked,
                    ontime = onTim.Text
                };
                dynamic json = DynamicJson.Serialize(obj);
                postjson("http://intathome.azurewebsites.net/api/light", json);
            }
            else
            {
                var obj = new
                {
                    begin = "0000",
                    end = "0000",
                    function = false,
                    ontime = "30"
                };
                dynamic json = DynamicJson.Serialize(obj);
                postjson("http://intathome.azurewebsites.net/api/light", json);
            }
        }

        private void func_Unchecked(object sender, RoutedEventArgs e)
        {
            falHid.Visibility = Visibility.Hidden;
        }

        private void func_Checked(object sender, RoutedEventArgs e)
        {
            falHid.Visibility = Visibility.Visible;
        }

        private void selectfile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "ファイルを開く";
            dialog.Filter = "音声ファイル(*.mp3,*.mp4)|*.mp3;*.mp4";
            if (dialog.ShowDialog() == true)
            {
                textBlockFileName.Text = dialog.FileName;
                selectpath = dialog.FileName;
            }
        }

        private void autoreschk_Checked(object sender, RoutedEventArgs e)
        {
            select.Visibility = Visibility.Visible;
        }

        private void autoreschk_Unchecked(object sender, RoutedEventArgs e)
        {
            select.Visibility = Visibility.Hidden;
        }

        private void absButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectpath != "")
            {
                warn.Visibility = Visibility.Hidden;
                //send api true
                var obj = new
                {
                    response = autoreschk.IsChecked
                };
                dynamic json = DynamicJson.Serialize(obj);
                postjson("http://intathome.azurewebsites.net/api/response", json);

                FileStream fs = new FileStream(
                    @selectpath,
                    FileMode.Open,
                    FileAccess.Read);
                byte[] bs = new byte[fs.Length];
                fs.Read(bs, 0, bs.Length);
                fs.Close();
                dynamic base64 = Convert.ToBase64String(bs);
                postjson("http://intathome.azurewebsites.net/api/blob", base64);
            }
            else
            {
                warn.Visibility = Visibility.Visible;
            }
        }
    }
}
