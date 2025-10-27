using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Web;
using NICVC.Model;
using NICVC.ReserveNic;
using System.Globalization;

namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReserveNicDashboardPage : ContentPage
    {
        MultipointStateDatabase multipointStateDatabase;
        List<MultipointState> multipointStatelist;

        MultipointNICDatabase multipointNICDatabase;

        PointToPointDatabase pointToPointDatabase;
        List<PointToPoint> pointToPointlist;

        CicVcDatabase cicVcDatabase;
        List<CicVc> cicVclist;

        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        List<SaveUserPreferences> SavedUserPreferList;

        string districtname, studioname, selecteddate;
        int responsecode;
        int ImportantVCCount, OngoingVcCount;

        public ReserveNicDashboardPage()
        {
            InitializeComponent();
            title_reservepage.Title = App.GetLabelByKey("reserve");
            cicVcDatabase = new CicVcDatabase();
            pointToPointDatabase = new PointToPointDatabase();
            multipointStateDatabase = new MultipointStateDatabase();
            multipointNICDatabase = new MultipointNICDatabase();
            Lbl_UserDetails.Text = Preferences.Get("DisplayName", "");

        }

        private async void DatePicker_startdate_DateSelected(object sender, DateChangedEventArgs e)
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {                
                selecteddate = e.NewDate.ToString("dd-MM-yyyy");                
            }
            else
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
                return;
            }
        }

        protected override void OnAppearing()
        {
            title_reservepage.Title = App.GetLabelByKey("reserve");
            DatePicker_startdate.MinimumDate = DateTime.Now;
            selecteddate = DatePicker_startdate.Date.ToString("dd-MM-yyyy");

            saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");
            lbl_fromdate.Text = App.GetLabelByKey("selectdate");
            Lbl_multipointbystate.Text = App.GetLabelByKey("multistate");
            Lbl_multipointbynic.Text = App.GetLabelByKey("multinichq");
            Lbl_pointopoint.Text = App.GetLabelByKey("ptp");
            Lbl_cic.Text = App.GetLabelByKey("cic");
            lbl_ongoingconferences.Text = App.GetLabelByKey("OngoingConfat");
            //Lbl_Ongoing.Text = App.GetLabelByKey("Ongoing");
            // Lbl_Important.Text = App.GetLabelByKey("ImportantVc");
            TodayVcDatabase todayVcDatabase = new TodayVcDatabase();

            string livevccount = $"Select * from TodayVc where VCStatus = 'Confirmed' and time('" + DateTime.Now.ToString("HH:mm:ss") + "') between time(Startingtime) and time(VCEndTime)";
            var livecount = todayVcDatabase.GetTodayVc(livevccount).ToList();
            OngoingVcCount = livecount.Count();

           
            var impcnt = todayVcDatabase.GetTodayVc("Select * from TodayVc where Startingtime <> 'All' and VCStatus = 'Confirmed' and Important='Y' ").ToList();
            ImportantVCCount = impcnt.Count();


            Lbl_Ongoing.Text = App.GetLabelByKey("Ongoing") + " - " + OngoingVcCount;
            Lbl_Important.Text = App.GetLabelByKey("ImportantVc") + " - " + ImportantVCCount;

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
                Lbl_Header.Text = statename + " - " + districtname + " - " + studioname;
            }
            else if (!string.IsNullOrEmpty(districtname))
            {
                Lbl_Header.Text = statename + " - " + districtname;
            }
            else
            {
                Lbl_Header.Text = statename;
            }
        }

        private async void multistate_Tapped(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
               int responsecode =  await GetMultipointStateVc(selecteddate, "S");
                if (responsecode == 200)
                {
                    await Navigation.PushAsync(new MultipointVcStatePage(selecteddate, "S"));
                }
                else if (responsecode == 300)
                {
                    await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("novcfnd"), App.GetLabelByKey("close"));
                }
                else
                {
                    multipointStatelist = multipointStateDatabase.GetMultipointState($"Select * from multipointState " +
                     $"where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{selecteddate}', 7) || substr('{selecteddate}', 4, 2) || substr('{selecteddate}', 1, 2) ").ToList();
                    if (multipointStatelist.Any())
                    {
                        await Navigation.PushAsync(new MultipointVcStatePage(selecteddate, "S"));
                    }

                    else
                    {
                        await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("novcfnd"), App.GetLabelByKey("close"));
                    }
                }
            }
            else
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
                multipointStatelist = multipointStateDatabase.GetMultipointState($"Select * from multipointState " +
                    $"where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{selecteddate}', 7) || substr('{selecteddate}', 4, 2) || substr('{selecteddate}', 1, 2) ").ToList();
                if (multipointStatelist.Any())
                {
                    await Navigation.PushAsync(new MultipointVcStatePage(selecteddate, "S"));
                }
                
                else
                {
                    return;
                }


            }
        }
    
        private async void multinichq_Tapped(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
               int responsecode =  await GetMultipointNicHqVc(selecteddate, "N");
                if (responsecode == 200)
                {
                    await Navigation.PushAsync(new MultipointNicHqPage(selecteddate, "N"));
                }else if (responsecode == 300)
                {
                    await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("novcfnd"), App.GetLabelByKey("close"));
                }
                else
                {
                    List<MultipointNIC> multipointNIClist;
                    multipointNIClist = multipointNICDatabase.GetMultipointNIC($"Select * from MultipointNIC " +
                         $"where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{selecteddate}', 7) || substr('{selecteddate}', 4, 2) || substr('{selecteddate}', 1, 2) ").ToList();
                    if (multipointNIClist.Any())
                    {
                        await Navigation.PushAsync(new MultipointNicHqPage(selecteddate, "N"));
                    }
                    else
                    {
                        await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("novcfnd"), App.GetLabelByKey("close"));
                    }
                }

            }
            else
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
                List<MultipointNIC> multipointNIClist;
                multipointNIClist = multipointNICDatabase.GetMultipointNIC($"Select * from MultipointNIC " +
                     $"where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{selecteddate}', 7) || substr('{selecteddate}', 4, 2) || substr('{selecteddate}', 1, 2) ").ToList();
                if (multipointNIClist.Any())
                {
                    await Navigation.PushAsync(new MultipointNicHqPage(selecteddate, "N"));
                }
                else
                {
                    return;
                }
            }
        }

        private async void pointopoint_Tapped(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                await GetPointToPointVc(selecteddate);
                if (responsecode == 200)
                {
                    await Navigation.PushAsync(new PointToPointVcPage(selecteddate));
                }
                else if (responsecode == 300)
                {
                    await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("novcfnd"), App.GetLabelByKey("close"));
                }

            }
            else
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
                
                pointToPointlist = pointToPointDatabase.GetPointToPoint($"Select * from PointToPoint " +
                     $"where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{selecteddate}', 7) || substr('{selecteddate}', 4, 2) || substr('{selecteddate}', 1, 2) ").ToList();
                if (pointToPointlist.Any())
                {
                    await Navigation.PushAsync(new PointToPointVcPage(selecteddate));
                }
                else
                {
                    return;
                }
            }
        }

        private async void cicvc_Tapped(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                int responsecode = await GetCICVc(selecteddate);
                if (responsecode == 200)
                {
                    await Navigation.PushAsync(new CicVcPage(selecteddate));
                }
                else if (responsecode == 300)
                {
                    await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("novcfnd"), App.GetLabelByKey("close"));
                }

            }
            else
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));

                cicVclist = cicVcDatabase.GetCicVc($"Select * from CicVc " +
                     $"where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{selecteddate}', 7) || substr('{selecteddate}', 4, 2) || substr('{selecteddate}', 1, 2) ").ToList();
                if (cicVclist.Any())
                {
                    await Navigation.PushAsync(new CicVcPage(selecteddate));
                }
                else
                {
                    return;
                }
            }
        }

        private async void ongoingvc_Tapped(object sender, EventArgs e)
        {
            if (OngoingVcCount != 0)
            {
                await Navigation.PushAsync(new OngoingVcPage());
            }
        
        }

        private async void importantvc_Tapped(object sender, EventArgs e)
        {          

            if (ImportantVCCount != 0)
            {
                await Navigation.PushAsync(new ImportantVcPage());
            }
        }

        async Task<int> GetMultipointStateVc(string selecteddate, string MPType)
        {

            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("pleasewait");

            try
            {

                var client = new HttpClient();
                string apiurl = $"{App.newdec_multipointvcnichq_url}" +
                $"stateid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StateID))}" +
                $"&districtid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).DistrictID))}" +
                $"&frdt={HttpUtility.UrlEncode(AESCryptography.EncryptAES(selecteddate))}" +
                $"&MPType={HttpUtility.UrlEncode(AESCryptography.EncryptAES(MPType))}" +
                $"&studioid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StudioID))}";
                var responce = await client.GetAsync(apiurl);


                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);

                    multipointStateDatabase.DeleteMultipointState();

                    foreach (var pair in parsed)
                    {

                        if (pair.Key == "MultipointSept19List")
                        {
                            var nodes = pair.Value;
                            if(nodes.Count() > 0)
                            {
                                var item = new MultipointState();

                                item.Startingtime = "All";
                                multipointStateDatabase.AddMultipointState(item);

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
                                    item.VCStatus = "Confirmed";
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
                                    string startdatetime = AESCryptography.DecryptAES(node["DateofVC"].ToString())+ " " + AESCryptography.DecryptAES(node["Startingtime"].ToString());
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
                                        multipointStateDatabase.InsertMultipointStatelist(insertintotodayvc);
                                        insertintotodayvc = "";
                                    }
                                }
                                multipointStateDatabase.InsertMultipointStatelist(insertintotodayvc);
                                responsecode = 200;
                            }
                            else
                            {
                                responsecode = 300;
                            }
                            
                          
                        }

                    }
                    

                }
                Loading_activity.IsVisible = false;
                return responsecode;

            }
            catch (Exception ey)
            {
                responsecode = 400;
                Loading_activity.IsVisible = false;
                await DisplayAlert("Exception", ey.Message, "OK");
                return responsecode;
            }
        }

        async Task<int> GetMultipointNicHqVc(string selecteddate, string MPType)
        {
            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("pleasewait");

            try
            {
                var client = new HttpClient();
                string apiurl = $"{App.newdec_multipointvcnichq_url}" +
                $"stateid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StateID))}" +
                $"&districtid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).DistrictID))}" +
                $"&frdt={HttpUtility.UrlEncode(AESCryptography.EncryptAES(selecteddate))}" +
                $"&MPType={HttpUtility.UrlEncode(AESCryptography.EncryptAES(MPType))}" +
                $"&studioid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StudioID))}";
                var responce = await client.GetAsync(apiurl);

                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);
                  
                    multipointNICDatabase.DeleteMultipointNIC();

                    foreach (var pair in parsed)
                    {
                        if (pair.Key == "MultipointSept19List")
                        {
                            var nodes = pair.Value;
                            if (nodes.Count() >0)
                            {
                                var item = new MultipointNIC();

                                item.Startingtime = "All";
                                multipointNICDatabase.AddMultipointNIC(item);

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
                                    //item.VCStatus = AESCryptography.DecryptAES(node["VCStatus"].ToString());
                                    item.VCStatus = "Confirmed";
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
                                        multipointNICDatabase.InsertMultipointNIClist(insertintotodayvc);
                                        insertintotodayvc = "";
                                    }
                                }
                                multipointNICDatabase.InsertMultipointNIClist(insertintotodayvc);
                                responsecode = 200;
                              
                            }
                            else
                            {
                                responsecode = 300;
                                
                            }
                        }
                    }                  
                }
                Loading_activity.IsVisible = false;
                return responsecode;
               

            }
            catch (Exception ey)
            {
                responsecode = 400;
                Loading_activity.IsVisible = false;
                await DisplayAlert("Exception", ey.Message, "OK");
                return 400;
            }
        }

        async Task GetPointToPointVc(string selecteddate)
        {
            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("pleasewait");

            try
            {
                var client = new HttpClient();
                string apiurl = $"{App.pointtopoint_url}" +
                $"stateid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StateID))}" +
                $"&districtid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).DistrictID))}" +
                $"&frdt={HttpUtility.UrlEncode(AESCryptography.EncryptAES(selecteddate))}" ;
                var responce = await client.GetAsync(apiurl);

                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);
                    pointToPointDatabase.DeletePointToPoint();
                    foreach (var pair in parsed)
                    {
                        if (pair.Key == "Point2PointList")
                        {
                            var nodes = pair.Value;
                            if (nodes.Count() > 0)
                            {
                                var item = new PointToPoint();
                                item.Startingtime = "All";
                                pointToPointDatabase.AddPointToPoint(item);

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
                                    //item.VCStatus = AESCryptography.DecryptAES(node["VCStatus"].ToString());
                                    item.VCStatus = "Confirmed";
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
                                        pointToPointDatabase.InsertPointToPointlist(insertintotodayvc);
                                        insertintotodayvc = "";
                                    }
                                }
                                pointToPointDatabase.InsertPointToPointlist(insertintotodayvc);
                                responsecode = 200;
                            }
                            else
                            {
                                responsecode = 300;
                            }
                        }
                    }
                }
                Loading_activity.IsVisible = false;

            }
            catch (Exception ey)
            {
                responsecode = 400;
                Loading_activity.IsVisible = false;
                await DisplayAlert("Exception", ey.Message, "OK");
                return;
            }
        }

        async Task<int> GetCICVc(string selecteddate)
        {
            int responsecode = 400;
            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("pleasewait");

            try
            {

                var client = new HttpClient();
                string apiurl = $"{App.cicvc_url}" +
                $"stateid={HttpUtility.UrlEncode(App.SavedUserPreferList.ElementAt(0).StateID)}" +
                $"&districtid={HttpUtility.UrlEncode(App.SavedUserPreferList.ElementAt(0).DistrictID)}" +
                $"&frdt={HttpUtility.UrlEncode(selecteddate)}";
                var responce = await client.GetAsync(apiurl);

                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);

                    cicVcDatabase.DeleteCicVc();

                    foreach (var pair in parsed)
                    {
                        if (pair.Key == "CICVcList")
                        {
                            var nodes = pair.Value;
                            if (nodes.Count() > 0)
                            {
                                var item = new CicVc();

                                item.Startingtime = "All";
                                cicVcDatabase.AddCicVc(item);

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
                                    //item.VCStatus = "Confirmed";
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
                                    //item.VcStartDateTime = Formattedvcstartdatetime.ToString();
                                    //item.VcEndDateTime = Formattedvcenddatetime.ToString();

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
                                        cicVcDatabase.InsertCicVclist(insertintotodayvc);
                                        insertintotodayvc = "";
                                    }
                                }
                                cicVcDatabase.InsertCicVclist(insertintotodayvc);
                                responsecode = 200;
                            }
                            else
                            {
                                responsecode = 300;
                            }
                        }
                    }                   
                }
                Loading_activity.IsVisible = false;
                return responsecode;
            }
            catch (Exception ey)
            {
                responsecode = 400;
                Loading_activity.IsVisible = false;
                await DisplayAlert("Exception", ey.Message, "OK");
                return responsecode;
            }
        }
    }
}