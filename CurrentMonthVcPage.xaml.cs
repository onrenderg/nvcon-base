using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;



using NICVC.Model;

namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrentMonthVcPage : ContentPage
    {
        List<SaveUserPreferences> SavedUserPreferList;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        string districtname, studioname;
        CurrentMonthVcDatabase currentMonthVcDatabase;


        public CurrentMonthVcPage()
        {
            InitializeComponent();
            currentMonthVcDatabase = new CurrentMonthVcDatabase();

            lbl_user_header.Text = App.GetLabelByKey("currentmonth");

            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");

            DateTime date = DateTime.Now;
            var firstDateOfMonth = new DateTime(date.Year, date.Month, 1).ToString("dd-MM-yyyy");
            //var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var currentDateOfMonth = DateTime.Now.ToString("dd-MM-yyyy");

            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                Dispatcher.Dispatch(async () => await GetCurrentMonthVc(firstDateOfMonth, currentDateOfMonth));
            }
            else
            {
                listView_schedulevc.IsVisible = true;
                string query = $"SELECT DateofVC, count(*) as NoOfVc from currentMonthVc where VCStatus = 'Confirmed' and Startingtime !='All' group by DateofVC";
                if (App.Language == 0)
                {
                    listView_schedulevc.IsVisible = true;
                    listView_schedulevclocal.IsVisible = false;
                    listView_schedulevc.ItemsSource = currentMonthVcDatabase.GetCurrentMonthVc(query);
                }
                else
                {
                    listView_schedulevc.IsVisible = false;
                    listView_schedulevclocal.IsVisible = true;
                    listView_schedulevclocal.ItemsSource = currentMonthVcDatabase.GetCurrentMonthVc(query);
                }


                DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
                return;
            }

            if (App.Language == 0)
            {
                listView_schedulevc.IsVisible = true;
                listView_schedulevclocal.IsVisible = false;
            }
            else
            {
                listView_schedulevc.IsVisible = false;
                listView_schedulevclocal.IsVisible = true;
            }



            saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
            SavedUserPreferList = saveUserPreferencesDatabase.GetSaveUserPreferences("select * from saveUserPreferences").ToList();
            string statename = SavedUserPreferList.ElementAt(0).StateName.ToString();

            try
            {
                districtname = SavedUserPreferList.ElementAt(0).DistrictName.ToString();
            }
            catch
            {
                districtname = "";
            }
            try
            {
                studioname = SavedUserPreferList.ElementAt(0).StudioName.ToString();
            }
            catch
            {
                studioname = "";
            }


            if (!string.IsNullOrEmpty(studioname))
            {
                lbl_user_header1.Text = statename + " - " + districtname + " - " + studioname;
            }
            else if (!string.IsNullOrEmpty(districtname))
            {
                lbl_user_header1.Text = statename + " - " + districtname;
            }
            else
            {
                lbl_user_header1.Text = statename;
            }
        }
        private void listView_schedulevc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;
                
            var currentRecord = e.CurrentSelection[0] as CurrentMonthVc;
            App.vcdateschedulevc = currentRecord.DateofVC.ToString();
            
            // Clear selection
            ((CollectionView)sender).SelectedItem = null;
            
            Navigation.PushAsync(new CurrentMonthVcDetailsPage());
        }

        private async Task GetCurrentMonthVc(string fromdate, string todate)
        {
            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("pleasewait");
            try
            {

                var client = new HttpClient();
                string apiurl = $"{App.currentmonthvcurl}" +
                  
                $"stateid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StateID))}" +
                $"&districtid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).DistrictID))}" +
                $"&frdt={HttpUtility.UrlEncode(AESCryptography.EncryptAES(fromdate))}" +
                $"&todt={HttpUtility.UrlEncode(AESCryptography.EncryptAES(todate))}" +
                $"&studioid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StudioID))}";
                var responce = await client.GetAsync(apiurl);

                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);

                    currentMonthVcDatabase.DeleteCurrentMonthVc();
                    foreach (var pair in parsed)
                    {
                        if (pair.Key == "NewDecTodayVcList")
                        {
                            var nodes = pair.Value;
                            if (nodes.Count() > 0)
                            {
                                var item = new CurrentMonthVc();

                                item.Startingtime = "All";
                                currentMonthVcDatabase.AddCurrentMonthVc(item);

                                int i = 0;
                                string insertinschedulevc = null;

                                foreach (var node in nodes)
                                {
                                    i += 1;
                                    item.DateofVC = AESCryptography.DecryptAES(node["DateofVC"].ToString());
                                    item.Dept_Name = AESCryptography.DecryptAES(node["Dept_Name"].ToString());
                                    item.Important = AESCryptography.DecryptAES(node["Important"].ToString());
                                    item.LevelName = AESCryptography.DecryptAES(node["LevelName"].ToString());
                                    item.Org_Name = AESCryptography.DecryptAES(node["Org_Name"].ToString());
                                    item.Purpose = AESCryptography.DecryptAES(node["Purpose"].ToString());
                                    item.RequestedBy = AESCryptography.DecryptAES(node["RequestedBy"].ToString());
                                    item.Startingtime = AESCryptography.DecryptAES(node["Startingtime"].ToString());
                                    item.StudioID = AESCryptography.DecryptAES(node["StudioID"].ToString());
                                    item.Studio_Name = AESCryptography.DecryptAES(node["Studio_Name"].ToString());
                                    item.VCEndTime = AESCryptography.DecryptAES(node["VCEndTime"].ToString());
                                    item.VCStatus = AESCryptography.DecryptAES(node["VCStatus"].ToString());
                                    item.VC_ID = AESCryptography.DecryptAES(node["VC_ID"].ToString());
                                    item.VcCoordEmail = AESCryptography.DecryptAES(node["VcCoordEmail"].ToString());
                                    item.VcCoordIpPhone = AESCryptography.DecryptAES(node["VcCoordIpPhone"].ToString());
                                    item.VcCoordMobile = AESCryptography.DecryptAES(node["VcCoordMobile"].ToString());
                                    item.VcCoordName = AESCryptography.DecryptAES(node["VcCoordName"].ToString());
                                    item.VccoordLandline = AESCryptography.DecryptAES(node["VccoordLandline"].ToString());
                                    item.hoststudio = AESCryptography.DecryptAES(node["hoststudio"].ToString());
                                    item.mcuip = AESCryptography.DecryptAES(node["mcuip"].ToString());
                                    item.mcuname = AESCryptography.DecryptAES(node["mcuname"].ToString());
                                    item.participantsExt = AESCryptography.DecryptAES(node["participantsExt"].ToString());
                                    item.webroom = AESCryptography.DecryptAES(node["webroom"].ToString());                                   

                                    if (string.IsNullOrEmpty(insertinschedulevc))
                                    {
                                        insertinschedulevc = $"('{item.DateofVC}','{item.Dept_Name}','{item.Important}','{item.LevelName}'," +
                                                                $"'{item.Org_Name}','{item.Purpose}','{item.RequestedBy}','{item.Startingtime}'," +
                                                                $"'{item.StudioID}','{item.Studio_Name}','{item.VCEndTime}','{item.VCStatus}'," +
                                                                $"'{item.VC_ID}','{item.VcCoordEmail}','{item.VcCoordIpPhone}','{item.VcCoordMobile}'," +
                                                                $"'{item.VcCoordName}','{item.VccoordLandline}','{item.hoststudio}','{item.mcuip}'," +
                                                                $"'{item.mcuname}','{item.participantsExt}','{item.webroom}')";
                                    }
                                    else
                                    {
                                        insertinschedulevc += $" ,('{item.DateofVC}','{item.Dept_Name}','{item.Important}','{item.LevelName}'," +
                                                                $"'{item.Org_Name}','{item.Purpose}','{item.RequestedBy}','{item.Startingtime}'," +
                                                                $"'{item.StudioID}','{item.Studio_Name}','{item.VCEndTime}','{item.VCStatus}'," +
                                                                $"'{item.VC_ID}','{item.VcCoordEmail}','{item.VcCoordIpPhone}','{item.VcCoordMobile}'," +
                                                                $"'{item.VcCoordName}','{item.VccoordLandline}','{item.hoststudio}','{item.mcuip}'," +
                                                                $"'{item.mcuname}','{item.participantsExt}','{item.webroom}')";

                                    }

                                    if (i % 500 == 0)
                                    {
                                        currentMonthVcDatabase.InsertCurrentMonthvclist(insertinschedulevc);
                                        insertinschedulevc = "";
                                    }
                                }
                                currentMonthVcDatabase.InsertCurrentMonthvclist(insertinschedulevc);
                            }
                        }
                    }
                }
                Loading_activity.IsVisible = false;
                listView_schedulevc.IsVisible = true;
                string query = $"SELECT DateofVC, count(*) as NoOfVc from CurrentMonthVc where VCStatus = 'Confirmed' and Startingtime !='All' group by DateofVC";
                if (App.Language == 0)
                {
                    listView_schedulevc.IsVisible = true;
                    listView_schedulevclocal.IsVisible = false;
                    listView_schedulevc.ItemsSource = currentMonthVcDatabase.GetCurrentMonthVc(query).ToList();
                }
                else
                {
                    listView_schedulevc.IsVisible = false;
                    listView_schedulevclocal.IsVisible = true;
                    listView_schedulevclocal.ItemsSource = currentMonthVcDatabase.GetCurrentMonthVc(query).ToList();
                }
            }
            catch (Exception ey)
            {
                Loading_activity.IsVisible = false;
                listView_schedulevc.IsVisible = false;
                await DisplayAlert(App.GetLabelByKey("Exception"), ey.Message, App.GetLabelByKey("close"));
                return;
            }
        }
        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}