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
    public partial class TakeMeTherePage : ContentPage
    {
        TodayVcDatabase todayVcDatabase;
        List<TodayVc> todayvclistStudio;
        string mystudioID;
        LatLongDatabase latLongDatabase;
        List<LatLong> latLonglist;
        private string  Latitude, Longitude;
        List<SaveUserPreferences> SavedUserPreferList;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        string districtname, studioname;

        ScheduleVcDatabase scheduleVcDatabase;
        List<ScheduleVc> scheduleVclist;
        List<ScheduleVc> scheduleVclistStudio;

        SearchByVcIdDatabase searchByVcIdDatabase;
        List<SearchByVcId> searchByVcIdlist;
        List<SearchByVcId> searchByVcIdlistStudio;


        AlertableDatabase alertableDatabase;
        List<Alertable> alertablelist;
        List<Alertable> altertablelistStudio;

        MultipointStateDatabase multipointStateDatabase;
        List<MultipointState> multipointStatelist;
        List<MultipointState> multipointStatelistStudio;

        MultipointNICDatabase multipointNICDatabase;
        List<MultipointNIC> multipointNIClist;
        List<MultipointNIC> multipointNIClistStudio;

        PointToPointDatabase pointToPointDatabase;
        List<PointToPoint> pointToPointlist;
        List<PointToPoint> pointToPointlistStudio;

        CicVcDatabase cicVcDatabase;
        List<CicVc> cicvclist;
        List<CicVc> cicvclistStudio;


        string pagefrom;
        public TakeMeTherePage(string vcidfrompage, string pagefromback)
        {
            InitializeComponent();
            pagefrom = pagefromback;
            cicVcDatabase = new CicVcDatabase();
            pointToPointDatabase = new PointToPointDatabase();
            multipointNICDatabase = new MultipointNICDatabase();
            multipointStateDatabase = new MultipointStateDatabase();
            alertableDatabase = new AlertableDatabase();
            searchByVcIdDatabase = new SearchByVcIdDatabase();
            scheduleVcDatabase = new ScheduleVcDatabase();
            latLongDatabase = new LatLongDatabase();
            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");
            lbl_user_header.Text = App.GetLabelByKey(App.GetLabelByKey("takemethere"));

            todayVcDatabase = new TodayVcDatabase();
            List<TodayVc> todayvclist;
            todayvclistStudio = new List<TodayVc>();
            btn_gotomap.Text = App.GetLabelByKey("takemethere");
            lbl_selectstudio.Text = App.GetLabelByKey("selectstudio");
            btn_gotomap.IsVisible = false;


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


            string query;

            if (pagefrom.Equals("Todayvc"))
            {
                // string queryforpicker = "select StudioID, Studio_Name from TodayVc where VC_ID ='" + vcidfrompage + "' order by Studio_Name";
                query = "select StudioID, Studio_Name||','||participantsExt as Studio_Name from TodayVc where VC_ID ='" + vcidfrompage + "' order by Studio_Name";
                todayvclist = todayVcDatabase.GetTodayVc(query).ToList();
                string[] subsID = todayvclist.FirstOrDefault().StudioID.Split(',');
                string[] subsName = todayvclist.FirstOrDefault().Studio_Name.Split(',');
                try
                {
                    for (int i = 0; i < subsID.Count(); i++)
                    {
                        var items = new TodayVc();
                        items.StudioID = subsID[i];
                        items.Studio_Name = subsName[i];
                        todayvclistStudio.Add(items);
                    }
                    picker_studio.ItemsSource = todayvclistStudio;
                    picker_studio.ItemDisplayBinding = new Binding("Studio_Name");
                }
                catch { }

            }
            else if (pagefrom.Equals("ScheduleVc"))
            {
                query = "select StudioID, Studio_Name||','||participantsExt as Studio_Name from scheduleVc where VC_ID ='" + vcidfrompage + "' order by Studio_Name";
                scheduleVclist = scheduleVcDatabase.GetScheduleVc(query).ToList();
                string[] subsID = scheduleVclist.FirstOrDefault().StudioID.Split(',');
                string[] subsName = scheduleVclist.FirstOrDefault().Studio_Name.Split(',');
                scheduleVclistStudio = new List<ScheduleVc>();
                try
                {
                    for (int i = 0; i < subsID.Count(); i++)
                    {
                        var items = new ScheduleVc();
                        items.StudioID = subsID[i];
                        items.Studio_Name = subsName[i];
                        scheduleVclistStudio.Add(items);
                    }
                    picker_studio.ItemsSource = scheduleVclistStudio;
                    picker_studio.ItemDisplayBinding = new Binding("Studio_Name");
                }
                catch
                { }
            }
            else if (pagefrom.Equals("SearchByVcId"))
            {
                query = "select StudioID,  participant||','||participantsExt as participant from searchByVcId where vcid ='" + vcidfrompage + "' order by participant";
                searchByVcIdlist = searchByVcIdDatabase.GetSearchByVcId(query).ToList();
                string[] subsID = searchByVcIdlist.FirstOrDefault().StudioID.Split(',');
                string[] subsName = searchByVcIdlist.FirstOrDefault().participant.Split(',');
                searchByVcIdlistStudio = new List<SearchByVcId>();
                try
                {
                    for (int i = 0; i < subsID.Count(); i++)
                    {
                        var items = new SearchByVcId();
                        items.StudioID = subsID[i];
                        items.participant = subsName[i];
                        searchByVcIdlistStudio.Add(items);
                    }
                    picker_studio.ItemsSource = searchByVcIdlistStudio;
                    picker_studio.ItemDisplayBinding = new Binding("participant");
                }
                catch
                { }
            }
            else if (pagefrom.Equals("Alerts"))
            {
                query = "select StudioID, Studio_Name||','||participantsExt as Studio_Name from Alertable where VC_ID ='" + vcidfrompage + "' order by Studio_Name";
                alertablelist = alertableDatabase.GetAlertable(query).ToList();
                string[] subsID = alertablelist.FirstOrDefault().StudioID.Split(',');
                string[] subsName = alertablelist.FirstOrDefault().Studio_Name.Split(',');
                altertablelistStudio = new List<Alertable>();
                try
                {
                    for (int i = 0; i < subsID.Count(); i++)
                    {
                        var items = new Alertable();
                        items.StudioID = subsID[i];
                        items.Studio_Name = subsName[i];
                        altertablelistStudio.Add(items);
                    }
                    picker_studio.ItemsSource = altertablelistStudio;
                    picker_studio.ItemDisplayBinding = new Binding("Studio_Name");
                }
                catch
                { }
            }
            else if (pagefrom.Equals("MultipointState"))
            {
                query = "select StudioID, Studio_Name||','||participantsExt as Studio_Name from MultipointState where VC_ID ='" + vcidfrompage + "' order by Studio_Name";
                multipointStatelist = multipointStateDatabase.GetMultipointState(query).ToList();
                string[] subsID = multipointStatelist.FirstOrDefault().StudioID.Split(',');
                string[] subsName = multipointStatelist.FirstOrDefault().Studio_Name.Split(',');
                multipointStatelistStudio = new List<MultipointState>();
                try
                {
                    for (int i = 0; i < subsID.Count(); i++)
                    {
                        var items = new MultipointState();
                        items.StudioID = subsID[i];
                        items.Studio_Name = subsName[i];
                        multipointStatelistStudio.Add(items);
                    }
                    picker_studio.ItemsSource = multipointStatelistStudio;
                    picker_studio.ItemDisplayBinding = new Binding("Studio_Name");
                }
                catch { }
            }
            else if (pagefrom.Equals("MultipointNIC"))
            {
                query = "select StudioID, Studio_Name||','||participantsExt as Studio_Name from MultipointNic where VC_ID ='" + vcidfrompage + "' order by Studio_Name";
                multipointNIClist = multipointNICDatabase.GetMultipointNIC(query).ToList();
                string[] subsID = multipointNIClist.FirstOrDefault().StudioID.Split(',');
                string[] subsName = multipointNIClist.FirstOrDefault().Studio_Name.Split(',');
                multipointNIClistStudio = new List<MultipointNIC>();
                try
                {
                    for (int i = 0; i < subsID.Count(); i++)
                    {
                        var items = new MultipointNIC();
                        items.StudioID = subsID[i];
                        items.Studio_Name = subsName[i];
                        multipointNIClistStudio.Add(items);
                    }
                    picker_studio.ItemsSource = multipointNIClistStudio;
                    picker_studio.ItemDisplayBinding = new Binding("Studio_Name");
                }
                catch { }
            }
            else if (pagefrom.Equals("PointToPoint"))
            {
                query = "select StudioID, Studio_Name||','||participantsExt as Studio_Name from PointToPoint where VC_ID ='" + vcidfrompage + "' order by Studio_Name";
                pointToPointlist = pointToPointDatabase.GetPointToPoint(query).ToList();
                string[] subsID = pointToPointlist.FirstOrDefault().StudioID.Split(',');
                string[] subsName = pointToPointlist.FirstOrDefault().Studio_Name.Split(',');
                pointToPointlistStudio = new List<PointToPoint>();
                try
                {
                    for (int i = 0; i < subsID.Count(); i++)
                    {
                        var items = new PointToPoint();
                        items.StudioID = subsID[i];
                        items.Studio_Name = subsName[i];
                        pointToPointlistStudio.Add(items);
                    }
                    picker_studio.ItemsSource = pointToPointlistStudio;
                    picker_studio.ItemDisplayBinding = new Binding("Studio_Name");
                }
                catch { }
            }
            else if (pagefrom.Equals("CicVc"))
            {
                query = "select StudioID, Studio_Name||','||participantsExt as Studio_Name from CicVc where VC_ID ='" + vcidfrompage + "' order by Studio_Name";
                cicvclist = cicVcDatabase.GetCicVc(query).ToList();
                string[] subsID = cicvclist.FirstOrDefault().StudioID.Split(',');
                string[] subsName = cicvclist.FirstOrDefault().Studio_Name.Split(',');
                cicvclistStudio = new List<CicVc>();
                try
                {
                    for (int i = 0; i < subsID.Count(); i++)
                    {
                        var items = new CicVc();
                        items.StudioID = subsID[i];
                        items.Studio_Name = subsName[i];
                        cicvclistStudio.Add(items);
                    }
                    picker_studio.ItemsSource = cicvclistStudio;
                    picker_studio.ItemDisplayBinding = new Binding("Studio_Name");
                }
                catch { }
            }

            picker_studio.SelectedIndex = 0;
        }

        public async void picker_studio_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pagefrom.Equals("Todayvc"))
            {
                mystudioID = todayvclistStudio.ElementAt(picker_studio.SelectedIndex).StudioID.Trim();
            }
            else if (pagefrom.Equals("ScheduleVc"))
            {
                mystudioID = scheduleVclistStudio.ElementAt(picker_studio.SelectedIndex).StudioID.Trim();
            }
            else if (pagefrom.Equals("SearchByVcId"))
            {
                mystudioID = searchByVcIdlistStudio.ElementAt(picker_studio.SelectedIndex).StudioID.Trim();
            }
            else if (pagefrom.Equals("Alerts"))
            {
                mystudioID = altertablelistStudio.ElementAt(picker_studio.SelectedIndex).StudioID.Trim();
            }
            else if (pagefrom.Equals("MultipointState"))
            {
                mystudioID = multipointStatelistStudio.ElementAt(picker_studio.SelectedIndex).StudioID.Trim();
            }
            else if (pagefrom.Equals("MultipointNIC"))
            {
                mystudioID = multipointNIClistStudio.ElementAt(picker_studio.SelectedIndex).StudioID.Trim();
            }
            else if (pagefrom.Equals("PointToPoint"))
            {
                mystudioID = pointToPointlistStudio.ElementAt(picker_studio.SelectedIndex).StudioID.Trim();
            }
            else if (pagefrom.Equals("CicVc"))
            {
                mystudioID = cicvclistStudio.ElementAt(picker_studio.SelectedIndex).StudioID.Trim();
            }


            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                await LoadLatLong();
            }
            else
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            getlatlongparameters();
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                await Launcher.OpenAsync("http://maps.apple.com/?q=" + Latitude + "," + Longitude);
            }
            else if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                await Launcher.OpenAsync("https://www.google.com/maps/dir/Current+Location/" + Latitude + "," + Longitude);
            }

            /*  else if (Device.RuntimePlatform == Device.UWP)
              {
                  await Launcher.OpenAsync("bingmaps:?where=394 Pacific Ave San Francisco CA");
              }*/
        }


        async Task LoadLatLong()
        {
            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("pleasewait");
            try
            {
                var client = new HttpClient();
                string apiurl = $"{App.latlong_url}" +
                $"stateid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StateID))}" +
                $"&districtid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).DistrictID))}" +
                $"&StudioId={HttpUtility.UrlEncode(AESCryptography.EncryptAES(mystudioID))}";
                var responce = await client.GetAsync(apiurl);

                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);

                    latLongDatabase.DeleteLatLong();

                    foreach (var pair in parsed)
                    {
                        if (pair.Key == "LatLong")
                        {
                            var nodes = pair.Value;
                            var item = new LatLong();
                            foreach (var node in nodes)
                            {

                                item.StateId = AESCryptography.DecryptAES(node["StateId"].ToString());
                                item.DistrictId = AESCryptography.DecryptAES(node["DistrictId"].ToString());
                                item.StudioId = AESCryptography.DecryptAES(node["StudioId"].ToString());
                                item.StudioName = AESCryptography.DecryptAES(node["StudioName"].ToString());
                                item.Latitude = AESCryptography.DecryptAES(node["Latitude"].ToString());
                                item.Longitude = AESCryptography.DecryptAES(node["Longitude"].ToString());
                                item.Address = AESCryptography.DecryptAES(node["Address"].ToString());

                                latLongDatabase.AddLatLong(item);
                            }
                        }

                    }
                    latLonglist = latLongDatabase.GetLatLong($"Select * from LatLong where Trim(StudioId) ='{mystudioID.Trim()}'").ToList();
                    getlatlongparameters();


                    if (latLonglist.Any())
                    {
                        btn_gotomap.IsVisible = true;
                        lbl_nolatlong.IsVisible = false;
                    }
                    else
                    {
                        btn_gotomap.IsVisible = false;
                        lbl_nolatlong.IsVisible = true;
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

        void getlatlongparameters()
        {
            latLonglist = latLongDatabase.GetLatLong($"Select * from LatLong where Trim(StudioId) ='{mystudioID.Trim()}'").ToList();
            if (latLonglist.Any())
            {
                //StudioName = latLonglist.ElementAt(0).StudioName;
                Latitude = latLonglist.ElementAt(0).Latitude;
                Longitude = latLonglist.ElementAt(0).Longitude;
                //Address = latLonglist.ElementAt(0).Address;
            }
        }
    }
}