using Newtonsoft.Json.Linq;
using NICVC.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;




namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScheduleVcPage : ContentPage
    {
        ScheduleVcDatabase scheduleVcDatabase;
        List<SaveUserPreferences> SavedUserPreferList;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        string districtname, studioname;
        List<ScheduleVc> scheduleVclist;
        DateTime startdate, todate;
        public ScheduleVcPage()
        {
            InitializeComponent();
            scheduleVcDatabase = new ScheduleVcDatabase();
            startdate = DateTime.Today.AddDays(1);
            todate = startdate.AddDays(7);
            DatePicker_startdate.Date = startdate;
            DatePicker_enddate.Date = todate;
            DatePicker_startdate.MinimumDate = startdate;
            DatePicker_enddate.MinimumDate = todate;
            Console.WriteLine("From Date : " + startdate + " End date : " + todate);
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

            lbl_user_header.Text = App.GetLabelByKey("ScheduleVC");
            lbl_fromdate.Text = App.GetLabelByKey("from");
            lbl_todate.Text = App.GetLabelByKey("to");
            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");


            /* var current = Connectivity.NetworkAccess;
             if (current == NetworkAccess.Internet)
             {
                 Device.BeginInvokeOnMainThread(async () => await GetScheduleVc(startdate.ToString("dd-MM-yyyy"), todate.ToString("dd-MM-yyyy")));
             }
             else
             { */
            string query = $"SELECT DateofVC, count(*) as NoOfVc from scheduleVc " +
            $"where VCStatus = 'Confirmed' and Startingtime !='All' group by DateofVC order by substr(dateofvc,7)||substr(dateofvc,4,2)||substr(dateofvc,1,2)";
            scheduleVclist = scheduleVcDatabase.GetScheduleVc(query).ToList();
            if (scheduleVclist.Any())
            {
                if (App.Language == 0)
                {
                    listView_schedulevc.IsVisible = true;
                    listView_schedulevclocal.IsVisible = false;
                    listView_schedulevc.ItemsSource = scheduleVclist;
                }
                else
                {
                    listView_schedulevc.IsVisible = false;
                    listView_schedulevclocal.IsVisible = true;
                    listView_schedulevclocal.ItemsSource = scheduleVclist;
                }
            }
            else
            {
                DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("norecords"), App.GetLabelByKey("close"));
            }
            // }

        }



        private async void startDatePicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                await GetScheduleVc(e.NewDate.ToString("dd-MM-yyyy"), todate.ToString("dd-MM-yyyy"));
            }
            else
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
                return;
            }
        }

        private async void endDatePicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            DatePicker_startdate.MaximumDate = e.NewDate;
            todate = e.NewDate;
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {

                await GetScheduleVc(DatePicker_startdate.Date.ToString("dd-MM-yyyy"), todate.ToString("dd-MM-yyyy"));
            }
            else
            {

                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
                return;
            }
        }

        private void listView_schedulevc_ItemTapped(object sender, EventArgs e)
        {
            var tappedEventArgs = e as TappedEventArgs;
            var currentRecord = tappedEventArgs?.Parameter as ScheduleVc;
            if (currentRecord == null)
            {
                // For CollectionView, get the binding context from the sender
                var grid = sender as Microsoft.Maui.Controls.Grid;
                currentRecord = grid?.BindingContext as ScheduleVc;
            }
            
            if (currentRecord != null)
            {
                App.vcdateschedulevc = currentRecord.DateofVC.ToString();
                Navigation.PushAsync(new ScheduleVcDetailsPage());
            }
        }

        private async Task GetScheduleVc(string fromdate, string todate)
        {
            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("loadschedulevc");
            try
            {
                var client = new HttpClient();
                string apiurl = $"{App.TodayVcUrl}" +
                                $"stateid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StateID))}" +
                                $"&districtid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).DistrictID))}" +
                                $"&frdt={HttpUtility.UrlEncode(AESCryptography.EncryptAES(fromdate))}" +
                                $"&todt={HttpUtility.UrlEncode(AESCryptography.EncryptAES(todate))}" +
                                $"&studioid={HttpUtility.UrlEncode(App.SavedUserPreferList.ElementAt(0).StudioID)}";
                var responce = await client.GetAsync(apiurl);

                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);

                    scheduleVcDatabase.DeleteScheduleVc();

                    foreach (var pair in parsed)
                    {
                        if (pair.Key == "NewDecTodayVcList")
                        {
                            var nodes = pair.Value;
                            int countnodes = nodes.Count();
                            if (countnodes != 0)
                            {
                                var item = new ScheduleVc();

                                item.Startingtime = "All";
                                scheduleVcDatabase.AddScheduleVc(item);

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

                                    CultureInfo provider = CultureInfo.InvariantCulture;
                                    string startdatetime = AESCryptography.DecryptAES(node["DateofVC"].ToString()) + " " + AESCryptography.DecryptAES(node["Startingtime"].ToString());
                                    string enddatetime = AESCryptography.DecryptAES(node["DateofVC"].ToString()) + " " + AESCryptography.DecryptAES(node["VCEndTime"].ToString());
                                    DateTime Formattedvcstartdatetime, Formattedvcenddatetime;

                                    try
                                    {
                                        Formattedvcstartdatetime = DateTime.ParseExact(startdatetime, "dd/MM/yyyy HH:mm:ss", provider);
                                        Formattedvcenddatetime = DateTime.ParseExact(enddatetime, "dd/MM/yyyy HH:mm:ss", provider);
                                    }
                                    catch
                                    {
                                        Formattedvcstartdatetime = DateTime.ParseExact(startdatetime, "dd-MM-yyyy HH:mm:ss", provider);
                                        Formattedvcenddatetime = DateTime.ParseExact(enddatetime, "dd-MM-yyyy HH:mm:ss", provider);

                                    }
                                    item.VcStartDateTime = Formattedvcstartdatetime.ToString("yyyy-MM-dd ") + AESCryptography.DecryptAES(node["Startingtime"].ToString());
                                    item.VcEndDateTime = Formattedvcenddatetime.ToString("yyyy-MM-dd ") + AESCryptography.DecryptAES(node["VCEndTime"].ToString());


                                    if (string.IsNullOrEmpty(insertinschedulevc))
                                    {
                                        insertinschedulevc = $"('{item.DateofVC}','{item.Dept_Name}','{item.Important}','{item.LevelName}'," +
                                            $"'{item.Org_Name}','{item.Purpose}','{item.RequestedBy}','{item.Startingtime}'," +
                                            $"'{item.StudioID}','{item.Studio_Name}','{item.VCEndTime}','{item.VCStatus}'," +
                                            $"'{item.VC_ID}','{item.VcCoordEmail}','{item.VcCoordIpPhone}','{item.VcCoordMobile}'," +
                                            $"'{item.VcCoordName}','{item.VccoordLandline}','{item.hoststudio}','{item.mcuip}'," +
                                            $"'{item.mcuname}','{item.participantsExt}','{item.webroom}','{item.VcStartDateTime}','{item.VcEndDateTime}')";
                                    }
                                    else
                                    {
                                        insertinschedulevc += $" ,('{item.DateofVC}','{item.Dept_Name}','{item.Important}','{item.LevelName}'," +
                                            $"'{item.Org_Name}','{item.Purpose}','{item.RequestedBy}','{item.Startingtime}'," +
                                            $"'{item.StudioID}','{item.Studio_Name}','{item.VCEndTime}','{item.VCStatus}'," +
                                            $"'{item.VC_ID}','{item.VcCoordEmail}','{item.VcCoordIpPhone}','{item.VcCoordMobile}'," +
                                            $"'{item.VcCoordName}','{item.VccoordLandline}','{item.hoststudio}','{item.mcuip}'," +
                                            $"'{item.mcuname}','{item.participantsExt}','{item.webroom}','{item.VcStartDateTime}','{item.VcEndDateTime}')";

                                    }

                                    if (i % 500 == 0)
                                    {
                                        scheduleVcDatabase.InsertSchedulevclis(insertinschedulevc);
                                        insertinschedulevc = "";
                                    }
                                }
                                scheduleVcDatabase.InsertSchedulevclis(insertinschedulevc);
                            }

                        }

                    }

                    string query = $"SELECT DateofVC, count(*) as NoOfVc from scheduleVc " +
                    $"where VCStatus = 'Confirmed' and Startingtime !='All' group by DateofVC order by substr(dateofvc,7)||substr(dateofvc,4,2)||substr(dateofvc,1,2)";
                    scheduleVclist = scheduleVcDatabase.GetScheduleVc(query).ToList();
                    if (scheduleVclist.Any())
                    {
                        if (App.Language == 0)
                        {
                            listView_schedulevc.IsVisible = true;
                            listView_schedulevclocal.IsVisible = false;
                            listView_schedulevc.ItemsSource = scheduleVclist;
                        }
                        else
                        {
                            listView_schedulevc.IsVisible = false;
                            listView_schedulevclocal.IsVisible = true;
                            listView_schedulevclocal.ItemsSource = scheduleVclist;
                        }
                    }
                    else
                    {
                        await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("norecords"), App.GetLabelByKey("close"));
                    }

                }
                Loading_activity.IsVisible = false;

            }
            catch (Exception)
            {
                Loading_activity.IsVisible = false;


                // await DisplayAlert(App.GetLabelByKey("Exception"), ey.Message, App.GetLabelByKey("close"));
                return;
            }
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }

    }
}