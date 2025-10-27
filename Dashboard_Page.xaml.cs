using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using NICVC.Model;
using System.Net.Sockets;
using NICVC.Notification;
using System.Globalization;

namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Dashboard_Page : ContentPage
    {
        ScheduleVcDatabase scheduleVcDatabase;

        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        List<SaveUserPreferences> SavedUserPreferList;

        DashboardDatabase dashboardDatabase;
        List<Dashboard> dashboardlist;

        TodayVcDatabase todayVcDatabase;
        List<TodayVc> todayvclist;
        int TodayVCCount, MonthVCCount, YearVCCount, ImportantVCCount, OngoingVcCount, ALertVCcount;

        AlertableDatabase alertableDatabase;
        List<Alertable> alertablelist;
        public Dashboard_Page()
        {
            InitializeComponent();
            title_dashboard.Title = App.GetLabelByKey("NICVC");

            Lbl_TodayVC.Text = App.GetLabelByKey("todayvc");
            Lbl_ongoingvc.Text = App.GetLabelByKey("Ongoing");
            Lbl_ImportantVC.Text = App.GetLabelByKey("ImportantVc");
            Lbl_alertsvc.Text = App.GetLabelByKey("viewalert");

            //Lbl_TodayVC.Text = App.GetLabelByKey("todayvc");
            Lbl_schedulesvc.Text = App.GetLabelByKey("ScheduleVC");
            Lbl_searchbyvcid.Text = App.GetLabelByKey("SearchVCID");
            lbl_totalvc.Text = App.GetLabelByKey("totalvcs");
            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");
            Lbl_UserDetails.Text = Preferences.Get("DisplayName", "");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            scheduleVcDatabase = new ScheduleVcDatabase();
            saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
            dashboardDatabase = new DashboardDatabase();
            todayVcDatabase = new TodayVcDatabase();
            alertableDatabase = new AlertableDatabase();

            SavedUserPreferList = saveUserPreferencesDatabase.GetSaveUserPreferences("select * from SaveUserPreferences").ToList();
            dashboardlist = dashboardDatabase.GetDashboard("Select * from dashboard").ToList();

            if (SavedUserPreferList.Any())
            {
                string stateid = SavedUserPreferList.ElementAt(0).StateName;
                string district = SavedUserPreferList.ElementAt(0).DistrictName;
                string studio = SavedUserPreferList.ElementAt(0).StudioName;
                string statechanged = SavedUserPreferList.ElementAt(0).StateChanged;

                if (studio != null && studio.Length > 0)
                {
                    Lbl_Header.Text = stateid + " - " + district + " - " + studio;
                }
                else if (district != null && district.Length > 0)
                {
                    Lbl_Header.Text = stateid + " - " + district;
                }
                else
                {
                    Lbl_Header.Text = stateid;
                }

                var current = Connectivity.NetworkAccess;
                if (statechanged.Contains("Y"))
                {
                    if (current == NetworkAccess.Internet)
                    {
                        await GetTodayVc();
                    }
                }
                else
                {
                    if (dashboardlist.Any())
                    {
                        string dashboarddate = dashboardlist.ElementAt(0).Date;
                        if (dashboarddate.Equals(DateTime.Now.ToString("dd/MM/yyyy")))
                        {
                            showdashboard();
                        }
                        else
                        {
                            if (current == NetworkAccess.Internet)
                            {
                                await GetTodayVc();
                            }
                            else
                            {
                                showdashboard();
                            }
                        }
                    }
                    else
                    {
                        if (current == NetworkAccess.Internet)
                        {
                            await GetTodayVc();
                        }
                    }
                }
            }
        }

        void shownotificationpopup()
        {
            try
            {
                if (alertableDatabase == null)
                {
                    alertableDatabase = new AlertableDatabase();
                }

                string query = "Select * from alertable where startingtime <>'All' and vcstatus='Confirmed'";
                alertablelist = alertableDatabase.GetAlertable(query)?.ToList();

                if (alertablelist?.Any() == true)
                {
                    for (int i = 0; i < alertablelist.Count; i++)
                    {
                        var alertItem = alertablelist.ElementAt(i);
                        if (alertItem == null) continue;

                        string vcid = alertItem.VC_ID;
                        string vcdate = alertItem.DateofVC;
                        string vcstarttime = alertItem.Startingtime;
                        string notifystartdate = alertItem.NotificationStartDate;

                        // Validate required fields
                        if (string.IsNullOrEmpty(vcid) || string.IsNullOrEmpty(vcdate) || 
                            string.IsNullOrEmpty(vcstarttime) || string.IsNullOrEmpty(notifystartdate))
                        {
                            continue;
                        }

                        try
                        {
                            string startdatetime = vcdate + " " + vcstarttime;
                            string currentdatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            CultureInfo provider = CultureInfo.InvariantCulture;

                            DateTime vcstartdatetime = DateTime.MinValue;
                            DateTime notificationstartdate = DateTime.MinValue;
                            DateTime currentdttm = DateTime.Now;

                            // Try multiple date formats for parsing
                            bool vcDateParsed = false;
                            bool notifyDateParsed = false;

                            // Try parsing VC start datetime
                            string[] vcDateFormats = { "yyyy/MM/dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss" };
                            foreach (string format in vcDateFormats)
                            {
                                if (DateTime.TryParseExact(startdatetime, format, provider, DateTimeStyles.None, out vcstartdatetime))
                                {
                                    vcDateParsed = true;
                                    break;
                                }
                            }

                            if (!vcDateParsed)
                            {
                                continue; // Skip this item if date parsing fails
                            }

                            // Try parsing notification start date
                            string[] notifyDateFormats = { "yyyy/MM/dd", "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy" };
                            foreach (string format in notifyDateFormats)
                            {
                                if (DateTime.TryParseExact(notifystartdate, format, provider, DateTimeStyles.None, out notificationstartdate))
                                {
                                    notifyDateParsed = true;
                                    break;
                                }
                            }

                            if (!notifyDateParsed)
                            {
                                continue; // Skip this item if date parsing fails
                            }

                            // Check if notification should be shown
                            if (currentdttm.CompareTo(notificationstartdate) >= 0 && currentdttm.CompareTo(vcstartdatetime) <= 0)
                            {
                                string message = "Upcoming VC :  " + vcid + " On : " + vcstartdatetime.ToString("dd-MM-yyyy") + " Starts At : " + vcstarttime + ".";
                                
                                try
                                {
                                    var notificationService = DependencyService.Get<INotification>();
                                    if (notificationService != null)
                                    {
                                        notificationService.CreateNotification("NICVC", message);
                                    }
                                }
                                catch (Exception notifyEx)
                                {
                                    // Log notification error but don't crash the app
                                    System.Diagnostics.Debug.WriteLine($"Notification error: {notifyEx.Message}");
                                }
                            }
                        }
                        catch (Exception parseEx)
                        {
                            // Log parsing error but continue with next item
                            System.Diagnostics.Debug.WriteLine($"Date parsing error for VC {vcid}: {parseEx.Message}");
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the app
                System.Diagnostics.Debug.WriteLine($"Error in shownotificationpopup: {ex.Message}");
            }
        }

        async void showdashboard()
        {
            try
            {
                if (dashboardDatabase == null)
                {
                    dashboardDatabase = new DashboardDatabase();
                }
                if (todayVcDatabase == null)
                {
                    todayVcDatabase = new TodayVcDatabase();
                }

                dashboardlist = dashboardDatabase.GetDashboard($"Select * from dashboard")?.ToList();
                todayvclist = todayVcDatabase.GetTodayVc("Select * from TodayVc where VCStatus = 'Confirmed' and Startingtime <> 'All'")?.ToList();

                if (dashboardlist?.Any() != true)
                {
                    System.Diagnostics.Debug.WriteLine("Dashboard list is empty or null");
                    return;
                }

                string lastupdatedate, lastupdatetime;
                TodayVCCount = todayvclist?.Count() ?? 0;
                
                // Safe parsing with null checks
                if (int.TryParse(dashboardlist.ElementAt(0)?.MonthVCCount, out int monthCount))
                {
                    MonthVCCount = monthCount;
                }
                else
                {
                    MonthVCCount = 0;
                }

                if (int.TryParse(dashboardlist.ElementAt(0)?.YearVCCount, out int yearCount))
                {
                    YearVCCount = yearCount;
                }
                else
                {
                    YearVCCount = 0;
                }


                if (alertableDatabase == null)
                {
                    alertableDatabase = new AlertableDatabase();
                }

                var impcnt = todayVcDatabase.GetTodayVc("Select * from TodayVc where Startingtime <> 'All' and VCStatus = 'Confirmed' and Important='Y' ")?.ToList();
                ImportantVCCount = impcnt?.Count() ?? 0;

                string livevccount = $"Select * from TodayVc where VCStatus = 'Confirmed' and time('" + DateTime.Now.ToString("HH:mm:ss") + "') between time(Startingtime) and time(VCEndTime)";
                var livecount = todayVcDatabase.GetTodayVc(livevccount)?.ToList();
                OngoingVcCount = livecount?.Count() ?? 0;

                // Clean up old alertable records
                try
                {
                    string query = $"delete from Alertable where substr(DateofVC,7)||substr(DateofVC,4,2)||substr(DateofVC,1,2)  < substr('{DateTime.Now.ToString("yyyy-MM-dd")}', 7) || substr('{DateTime.Now.ToString("yyyy-MM-dd")}', 4, 2) || substr('{DateTime.Now.ToString("yyyy-MM-dd")}', 1, 2) ";
                    alertableDatabase.GetAlertable(query);
                }
                catch (Exception cleanupEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error cleaning up alertable records: {cleanupEx.Message}");
                }

                alertablelist = alertableDatabase.GetAlertable("Select * from Alertable  where Startingtime <> 'All' and ifnull(VCStatus,'Confirmed') = 'Confirmed'")?.ToList();
                
                if (alertablelist?.Any() == true)
                {
                    for (int i = 0; i < alertablelist.Count(); i++)
                    {
                        try
                        {
                            string vcidfromalertstable = alertablelist.ElementAt(i)?.VC_ID;
                            if (!string.IsNullOrEmpty(vcidfromalertstable))
                            {
                                var current = Connectivity.NetworkAccess;
                                if (current == NetworkAccess.Internet)
                                {
                                    await getsearchbyvcid(vcidfromalertstable);
                                }
                            }
                        }
                        catch (Exception vcEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing VC ID: {vcEx.Message}");
                            continue;
                        }
                    }
                }

                alertablelist = alertableDatabase.GetAlertable("Select * from Alertable  where Startingtime <> 'All' and VCStatus = 'Confirmed'")?.ToList();
                ALertVCcount = alertablelist?.Count() ?? 0;

                lastupdatedate = dashboardlist.ElementAt(0)?.Date ?? DateTime.Now.ToString("dd/MM/yyyy");
                lastupdatetime = dashboardlist.ElementAt(0)?.Time ?? DateTime.Now.ToString("HH:mm:ss");

                // Update UI labels safely
                Dispatcher.Dispatch(() =>
                {
                    try
                    {
                        Lbl_TodayVC.Text = App.GetLabelByKey("todayvc") + " - " + TodayVCCount;
                        Lbl_ongoingvc.Text = App.GetLabelByKey("Ongoing") + " - " + OngoingVcCount;
                        Lbl_ImportantVC.Text = App.GetLabelByKey("ImportantVc") + " - " + ImportantVCCount;
                        Lbl_alertsvc.Text = App.GetLabelByKey("viewalert") + " - " + ALertVCcount;

                        if (App.Language == 0)
                        {
                            Lbl_vcinmonth.Text = App.GetLabelByKey("vcsessionin") + " " + DateTime.Now.ToString("MMM") + ", " + DateTime.Now.Year + " \n " + MonthVCCount.ToString();
                            Lbl_vcinyear.Text = App.GetLabelByKey("vcsessionin") + " " + DateTime.Now.Year + " \n " + YearVCCount.ToString();
                        }
                        else
                        {
                            Lbl_vcinmonth.Text = DateTime.Now.ToString("MMM") + ", " + DateTime.Now.Year + " " + App.GetLabelByKey("vcsessionin") + " \n " + MonthVCCount.ToString();
                            Lbl_vcinyear.Text = DateTime.Now.Year + ", " + App.GetLabelByKey("vcsessionin") + " \n " + YearVCCount.ToString();
                        }

                        Lbl_LastUpdate.Text = App.GetLabelByKey("lastupdate") + lastupdatedate + " " + App.GetLabelByKey("time") + " : " + lastupdatetime;
                    }
                    catch (Exception uiEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating UI: {uiEx.Message}");
                    }
                });

                shownotificationpopup();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in showdashboard: {ex.Message}");
                // Ensure UI is updated even if there's an error
                Dispatcher.Dispatch(() =>
                {
                    try
                    {
                        Lbl_TodayVC.Text = App.GetLabelByKey("todayvc") + " - 0";
                        Lbl_ongoingvc.Text = App.GetLabelByKey("Ongoing") + " - 0";
                        Lbl_ImportantVC.Text = App.GetLabelByKey("ImportantVc") + " - 0";
                        Lbl_alertsvc.Text = App.GetLabelByKey("viewalert") + " - 0";
                    }
                    catch { }
                });
            }
        }

        private void Todayvc_Tapped(object sender, EventArgs e)
        {
            if (TodayVCCount != 0)
            {
                Navigation.PushAsync(new TodayVcPage());
            }
        }

        private void importantvc_Tapped(object sender, EventArgs e)
        {
            if (ImportantVCCount != 0)
            {
                Navigation.PushAsync(new ImportantVcPage());
            }
        }

        private void ongoingvc_Tapped(object sender, EventArgs e)
        {
            if (OngoingVcCount != 0)
            {
                Navigation.PushAsync(new OngoingVcPage());
            }
        }

        private void alerts_tapped(object sender, EventArgs e)
        {
            if (ALertVCcount != 0)
            {
                Navigation.PushAsync(new ViewSavedAlertsPage());
            }
        }

        private async void vcinmonth_tapped(object sender, EventArgs e)
        {
            if (MonthVCCount != 0)
            {
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    await Navigation.PushAsync(new CurrentMonthVcPage());
                }
                else
                {
                    await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));

                }
            }
        }

        private async void vcinyear_tapped(object sender, EventArgs e)
        {
            if (YearVCCount != 0)
            {
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    await Navigation.PushAsync(new VcInYearPage());
                }
                else
                {
                    await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));

                }
            }
        }

        private void Searchbyvcid_Tapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(new SearchByVcIdPage());
        }

        private async void schedulevc_Tapped(object sender, EventArgs e)
        {
            ScheduleVcDatabase scheduleVcDatabase = new ScheduleVcDatabase();
            List<ScheduleVc> scheduleVclist;
            scheduleVclist = scheduleVcDatabase.GetScheduleVc("Select * from ScheduleVc").ToList();

            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                DateTime startdate = DateTime.Today.AddDays(1);
                DateTime todate = startdate.AddDays(7);
                await GetScheduleVc(startdate.ToString("dd-MM-yyyy"), todate.ToString("dd-MM-yyyy"));
                await Navigation.PushAsync(new ScheduleVcPage());
            }
            else
            {
                if (scheduleVclist.Any())
                {
                    await Navigation.PushAsync(new ScheduleVcPage());
                }
                else
                {
                    await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));

                }
            }


        }

        private void refresh_tapped(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                Dispatcher.Dispatch(async () => await GetTodayVc());
            }
            else
            {
                DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
            }
        }

        async Task GetDashboard()
        {
            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("pleasewait");

            try
            {
                var client = new HttpClient();
                string apiurl = $"{App.Dashboardurl}" +                   
                $"stateid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StateID))}" +
                $"&districtid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).DistrictID))}" +
                $"&studioid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StudioID))}";
                var responce = await client.GetAsync(apiurl);

                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);
                    if (parsed.HasValues)
                    {
                        foreach (var pair in parsed)
                        {
                            if (pair.Key == "Dashboard")
                            {
                                var nodes = pair.Value;

                                if (nodes.Count() > 0)
                                {
                                    var item = new Dashboard();
                                    dashboardDatabase.DeleteDashboard();
                                    foreach (var node in nodes)
                                    {
                                        item.TodayVCCount = AESCryptography.DecryptAES(node["TodayVCCount"].ToString());
                                        item.MonthVCCount = AESCryptography.DecryptAES(node["MonthVCCount"].ToString());
                                        item.YearVCCount = AESCryptography.DecryptAES(node["YearVCCount"].ToString());
                                        item.ImportantVCCount = AESCryptography.DecryptAES(node["ImportantVCCount"].ToString());
                                        item.Date = DateTime.Now.ToString("dd/MM/yyyy");
                                        item.Time = DateTime.Now.ToString("HH:mm:ss");
                                        dashboardDatabase.AddDashboard(item);
                                    }
                                    saveUserPreferencesDatabase.CustomSaveUserPreferences("update saveUserPreferences set StateChanged ='N'");
                                    showdashboard();
                                }
                            }
                        }
                    }
                }
                Loading_activity.IsVisible = false;

            }
            catch (OperationCanceledException)
            {
                await DisplayAlert("Request Timeout", "Could not connect to server. Please try again later", "OK");
                Loading_activity.IsVisible = false;
            }

            catch (TimeoutException)
            {
                await DisplayAlert("Request Timeout", "Could not connect to server. Please try again later", "OK");
                Loading_activity.IsVisible = false;
            }
            catch (SocketException)
            {
                await DisplayAlert("Socket Closed", "Could not connect to server. Please try again later", "OK");
                Loading_activity.IsVisible = false;
            }
            catch (Exception ey)
            {

                await DisplayAlert("Exception", ey.Message, "OK");
                Loading_activity.IsVisible = false;
            }
        }

        async Task GetTodayVc()
        {

            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("pleasewait");

            try
            {

                var client = new HttpClient();
                string apiurl = $"{App.TodayVcUrl}" +
                $"&stateid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StateID))}" +
                $"&districtid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).DistrictID))}" +
                $"&frdt={HttpUtility.UrlEncode(AESCryptography.EncryptAES(DateTime.UtcNow.ToString("dd-MM-yyyy")))}" +
                $"&todt={HttpUtility.UrlEncode(AESCryptography.EncryptAES(DateTime.UtcNow.ToString("dd-MM-yyyy")))}" +
                $"&studioid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StudioID))}";
                var responce = await client.GetAsync(apiurl);

                //string itemJson = await responce.Content.ReadAsStringAsync();

                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);
                    todayVcDatabase = new TodayVcDatabase();
                    todayVcDatabase.DeleteTodayVc();

                    foreach (var pair in parsed)
                    {
                        if (pair.Key == "NewDecTodayVcList")
                        {
                            var nodes = pair.Value;
                            if (nodes.Count() > 0)
                            {
                                var item = new TodayVc();

                                item.Startingtime = "All";
                                todayVcDatabase.AddTodayVc(item);

                                int i = 0;
                                string insertintotodayvc = null;

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


                                    if (string.IsNullOrEmpty(insertintotodayvc))
                                    {
                                        insertintotodayvc = $"('{item.DateofVC}','{item.Dept_Name}','{item.Important}','{item.LevelName}'," +
                                            $"'{item.Org_Name}','{item.Purpose}','{item.RequestedBy}','{item.Startingtime}'," +
                                            $"'{item.StudioID}','{item.Studio_Name}','{item.VCEndTime}','{item.VCStatus}'," +
                                            $"'{item.VC_ID}','{item.VcCoordEmail}','{item.VcCoordIpPhone}','{item.VcCoordMobile}'," +
                                            $"'{item.VcCoordName}','{item.VccoordLandline}','{item.hoststudio}','{item.mcuip}'," +
                                            $"'{item.mcuname}','{item.participantsExt}','{item.webroom}','{item.VcStartDateTime}','{item.VcEndDateTime}')";
                                    }
                                    else
                                    {
                                        insertintotodayvc += $" ,('{item.DateofVC}','{item.Dept_Name}','{item.Important}','{item.LevelName}'," +
                                            $"'{item.Org_Name}','{item.Purpose}','{item.RequestedBy}','{item.Startingtime}'," +
                                            $"'{item.StudioID}','{item.Studio_Name}','{item.VCEndTime}','{item.VCStatus}'," +
                                            $"'{item.VC_ID}','{item.VcCoordEmail}','{item.VcCoordIpPhone}','{item.VcCoordMobile}'," +
                                            $"'{item.VcCoordName}','{item.VccoordLandline}','{item.hoststudio}','{item.mcuip}'," +
                                            $"'{item.mcuname}','{item.participantsExt}','{item.webroom}','{item.VcStartDateTime}','{item.VcEndDateTime}')";

                                    }

                                    if (i % 500 == 0)
                                    {
                                        todayVcDatabase.InsertTodayvclis(insertintotodayvc);
                                        insertintotodayvc = "";
                                    }
                                }
                                todayVcDatabase.InsertTodayvclis(insertintotodayvc);
                            }
                        }

                    }
                    await GetDashboard();
                }
                Loading_activity.IsVisible = false;

            }
            catch (Exception ey)
            {
                Loading_activity.IsVisible = false;
                await DisplayAlert(App.GetLabelByKey("Exception"), ey.Message, "OK");
                return;
            }
        }

        private async Task getsearchbyvcid(string vcid)
        {
            Loading_activity.IsVisible = false;

            try
            {
                var client = new HttpClient();
                string apiurl = $"{App.SearchByVcIdUrl}" + $"vcid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(vcid.Trim()))}";
                var responce = await client.GetAsync(apiurl);

                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);

                    foreach (var pair in parsed)
                    {

                        if (pair.Key == "NewDecSearchVcIdList")
                        {
                            var nodes = pair.Value;
                            if (Convert.ToInt32(nodes.Count()) == 0)
                            {
                                Loading_activity.IsVisible = false;
                                //await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("norecords"), App.GetLabelByKey("close"));
                            }

                            var item = new SearchByVcId();

                            foreach (var node in nodes)
                            {
                                string vcstatus = AESCryptography.DecryptAES(node["VCStatus"].ToString());
                                string query = $"update Alertable set VCStatus ='{vcstatus}' where VC_ID='{vcid}'";
                                alertableDatabase.UpdateCustomqueryAlertable(query);
                            }
                        }
                    }
                }
                Loading_activity.IsVisible = false;


            }
            catch (Exception ey)
            {
                Loading_activity.IsVisible = false;

                await DisplayAlert(App.GetLabelByKey("Exception"), ey.Message, App.GetLabelByKey("close"));
                return;
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
                $"stateid={HttpUtility.UrlEncode(AESCryptography.EncryptAES( App.SavedUserPreferList.ElementAt(0).StateID))}" +
                $"&districtid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).DistrictID))}" +
                $"&frdt={HttpUtility.UrlEncode(AESCryptography.EncryptAES(fromdate))}" +
                $"&todt={HttpUtility.UrlEncode(AESCryptography.EncryptAES(todate))}" +
                $"&studioid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StudioID))}";
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

                }
                Loading_activity.IsVisible = false;

            }
            catch (Exception ey)
            {
                Loading_activity.IsVisible = false;


                await DisplayAlert(App.GetLabelByKey("Exception"), ey.Message, App.GetLabelByKey("close"));
                return;
            }
        }


    }

}