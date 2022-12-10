using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V106.Overlay;
using OpenQA.Selenium.Support.UI;

namespace VerifyTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            string response = string.Empty;
            using(HttpClient httpClient = new())
            {
                response = httpClient.GetStringAsync("https://raw.githubusercontent.com/toramcalculator/crysta/main/optionlist").Result;
            }
            string TempOption = string.Empty;
            for(int i = 0; i < response.Length; i++)
            {
                if (response[i] != '\n') TempOption += response[i];
                else
                {
                    CrystaOptionBox.Items.Add(TempOption);
                    TempOption = string.Empty;
                }
            }
            CrystaOptionBox.SelectedIndex = 0;
            OptionEquipmentReq.SelectedIndex = 0;
            ResultWindow.Text = "{\n\"name\":\"\"\n\"option\":\"\"\n}";
        }

        public static string CmdExecute(string Commandval)
        {
            System.Diagnostics.ProcessStartInfo info = new();
            System.Diagnostics.Process proces = new();
            info.FileName = "cmd.exe";
            info.CreateNoWindow = false;
            info.UseShellExecute = false;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            proces.StartInfo = info;
            proces.Start();
            proces.StandardInput.Write(Commandval+Environment.NewLine);
            proces.StandardInput.Close();
            string result = proces.StandardOutput.ReadToEnd();
            proces.WaitForExit();
            proces.Close();
            return result;
        }

        private void AddOption(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(OptionValue.Text, out _) || Convert.ToDecimal(OptionValue.Text) == 0)
            {
                MessageBox.Show("유효한 수를 입력해주세요");
                return;
            }
            string OptionValueString = OptionValue.Text;
            if (Convert.ToDecimal(OptionValueString) > 0)
            {
                OptionValueString = '+' + OptionValueString;
            }
            string TempText = ResultWindow.Text;
            int t = TempText.IndexOf("\"\n\"option\":\"") + 12;
            string OptionalText = string.Empty;
            if (TempText[t] != '\"') OptionalText += '&';
#pragma warning disable CS8602 // null 가능 참조에 대한 역참조입니다.
            if (OptionEquipmentReq.SelectedIndex != 0) OptionalText = OptionalText + '(' + OptionEquipmentReq.SelectedItem.ToString().Remove(0, OptionEquipmentReq.SelectedItem.ToString().IndexOf(": ") + 2) + ')';
#pragma warning restore CS8602 // null 가능 참조에 대한 역참조입니다.
            TempText = TempText.Insert(TempText.IndexOf("\"\n}"), OptionalText + CrystaOptionBox.SelectedItem.ToString() + OptionValueString);
            ResultWindow.Text = TempText;
        }

        private void NameChanged(object sender, TextChangedEventArgs e)
        {
            int t = ResultWindow.Text.IndexOf("\n\"name\":\"") + 9;
            string TempText = ResultWindow.Text;
            TempText = TempText.Remove(t, TempText.IndexOf("\"\n\"") - t);
            TempText = TempText.Insert(t, CrystaName.Text);
            ResultWindow.Text = TempText;
            CommitMessage.Text = "신규 크리스타(" + CrystaName.Text + ')';
        }

        private void FromChanged(object sender, TextChangedEventArgs e)
        {
            string TempText = ResultWindow.Text;
            if (String.IsNullOrEmpty(CrystaEnhanceFrom.Text) && TempText.IndexOf("\"\n\"enhance\":\"") != -1)
            {
                ResultWindow.Text = TempText.Remove(TempText.IndexOf("\"\n\"enhance\":\"") + 2, TempText.IndexOf("\"\n\"option\"") - TempText.IndexOf("\"\n\"enhance\":\""));
                return;
            }
            if(TempText.IndexOf("\"\n\"enhance\":\"") == -1)
            {
                TempText = TempText.Insert(TempText.IndexOf("\"\n\"") + 2, "\"enhance\":\"\"\n");
            }
            int t = TempText.IndexOf("\"\n\"enhance\":\"") + 13;
            TempText = TempText.Remove(t, TempText.IndexOf("\"\n\"option\"") - t);
            TempText = TempText.Insert(t, CrystaEnhanceFrom.Text);
            ResultWindow.Text = TempText;
        }

        private void Copy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ResultWindow.Text);
            MessageBox.Show("내용이 클립보드에 복사되었습니다!");
        }

        private void Git_Commit(object sender, RoutedEventArgs e)
        {
            if (CrystaType.SelectedIndex == -1)
            {
                MessageBox.Show("크리스타 종류를 선택하세요");
                return;
            }
            string CrystaPath;
            if (CrystaType.SelectedIndex == 0)
            {
                CrystaPath = "normal";
            }
            else if (CrystaType.SelectedIndex == 1)
            {
                CrystaPath = "enhance_normal";
            }
            else if (CrystaType.SelectedIndex == 2)
            {
                CrystaPath = "weapon";
            }
            else if (CrystaType.SelectedIndex == 3)
            {
                CrystaPath = "enhance_weapon";
            }
            else if (CrystaType.SelectedIndex == 4)
            {
                CrystaPath = "main/armor";
            }
            else if (CrystaType.SelectedIndex == 5)
            {
                CrystaPath = "enhance_armor";
            }
            else if (CrystaType.SelectedIndex == 6)
            {
                CrystaPath = "additional";
            }
            else if (CrystaType.SelectedIndex == 7)
            {
                CrystaPath = "enhance_additional";
            }
            else if (CrystaType.SelectedIndex == 8)
            {
                CrystaPath = "special";
            }
            else
            {
                CrystaPath = "enhance_special";
            }
            using (IWebDriver webDriver = new ChromeDriver())
            {
                webDriver.Url = "https://github.com/login";
                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                webDriver.FindElement(By.XPath("//*[@id=\"login_field\"]")).SendKeys("toramcalculator");
                webDriver.FindElement(By.XPath("//*[@id=\"password\"]")).SendKeys("3669jwok");
                webDriver.FindElement(By.XPath("//*[@id=\"login\"]/div[4]/form/div/input[11]")).Click();
                using (HttpClient client = new())
                {
                    File.Create(CrystaPath).Close();
                    File.AppendAllTextAsync(CrystaPath, client.GetStringAsync("https://raw.githubusercontent.com/toramcalculator/crysta/main/" + CrystaPath).Result + ResultWindow.Text + '\n', Encoding.UTF8);
                }
                webDriver.Navigate().GoToUrl("https://github.com/toramcalculator/crysta/upload/main");
                webDriver.FindElement(By.XPath("//*[@id=\"upload-manifest-files-input\"]")).SendKeys(Directory.GetCurrentDirectory() + '\\' + CrystaPath);
                {
                    WebDriverWait webDriverWait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
                    webDriverWait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath("//*[@id=\"repo-content-pjax-container\"]/div/div[4]/div/div/div[2]")));
                }
                File.Delete(CrystaPath);
                webDriver.FindElement(By.XPath("//*[@id=\"commit-summary-input\"]")).SendKeys(CommitMessage.Text);
                webDriver.FindElement(By.XPath("//*[@id=\"repo-content-pjax-container\"]/div/form/button")).Click();
                while(webDriver.Url!= "https://github.com/toramcalculator/crysta")
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
