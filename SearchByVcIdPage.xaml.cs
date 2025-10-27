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
    public partial class SearchByVcIdPage : ContentPage
    {
        SearchByVcIdDatabase searchByVcIdDatabase;
        List<SearchByVcId> searchByVcIdlist;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        List<SaveUserPreferences> SavedUserPreferList;
        String districtname, studioname;
        public SearchByVcIdPage()
        {
            InitializeComponent();
            lbl_user_header.Text = App.GetLabelByKey("SearchVCID");
            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");
            saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
            

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



            searchByVcIdDatabase = new SearchByVcIdDatabase();

            entry_vcid.Placeholder = App.GetLabelByKey("entervcid");
            btn_search.Text = App.GetLabelByKey("search");
        }

        private async void btn_search_Clicked(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                await getsearchbyvcid();
            }
            else
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
            }
        }

        private async Task getsearchbyvcid()
        {
            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("loadvcdtls");
            try
            {
                var client = new HttpClient();
                string apiurl = $"{App.SearchByVcIdUrl}" +
                $"vcid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(entry_vcid.Text.ToString()))}";
                var responce = await client.GetAsync(apiurl);

                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);
                    searchByVcIdDatabase = new SearchByVcIdDatabase();
                    searchByVcIdDatabase.DeleteSearchByVcId();

                    foreach (var pair in parsed)
                    {

                        if (pair.Key == "NewDecSearchVcIdList")
                        {
                            var nodes = pair.Value;
                            if (Convert.ToInt32(nodes.Count()) == 0)
                            {
                                Loading_activity.IsVisible = false;
                                grid_vcdetails.IsVisible = false;
                                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("norecords"), App.GetLabelByKey("close"));
                            }

                            var item = new SearchByVcId();

                            foreach (var node in nodes)
                            {
                                item.vcid = AESCryptography.DecryptAES(node["vcid"].ToString());
                                item.vcdate = AESCryptography.DecryptAES(node["vcdate"].ToString());
                                item.VCStartTime = AESCryptography.DecryptAES(node["VCStartTime"].ToString());
                                item.Purpose = AESCryptography.DecryptAES(node["Purpose"].ToString());
                                item.participant = AESCryptography.DecryptAES(node["participant"].ToString());
                                item.LevelName = AESCryptography.DecryptAES(node["LevelName"].ToString());
                                item.DeptName = AESCryptography.DecryptAES(node["DeptName"].ToString());
                                item.MinistryName = AESCryptography.DecryptAES(node["MinistryName"].ToString());
                                item.VCStatus = AESCryptography.DecryptAES(node["VCStatus"].ToString());
                                item.VCEndTime = AESCryptography.DecryptAES(node["VCEndTime"].ToString());
                                item.StudioID = AESCryptography.DecryptAES(node["StudioID"].ToString());
                                item.participantsExt = AESCryptography.DecryptAES(node["participantsExt"].ToString());
                                item.webroom = AESCryptography.DecryptAES(node["webroom"].ToString());
                                item.hoststudio = AESCryptography.DecryptAES(node["hoststudio"].ToString());
                                item.RequestedBy = AESCryptography.DecryptAES(node["RequestedBy"].ToString());
                                item.VcCoordName = AESCryptography.DecryptAES(node["VcCoordName"].ToString());
                                item.VcCoordEmail = AESCryptography.DecryptAES(node["VcCoordEmail"].ToString());
                                item.VcCoordMobile = AESCryptography.DecryptAES(node["VcCoordMobile"].ToString());
                                item.VcCoordIpPhone = AESCryptography.DecryptAES(node["VcCoordIpPhone"].ToString());
                                item.VccoordLandline = AESCryptography.DecryptAES(node["VccoordLandline"].ToString());
                                item.mcuname = AESCryptography.DecryptAES(node["mcuname"].ToString());
                                item.mcuip = AESCryptography.DecryptAES(node["mcuip"].ToString());

                                searchByVcIdDatabase.AddSearchByVcId(item);
                            }
                        }
                    }
                }
                Loading_activity.IsVisible = false;
                grid_vcdetails.IsVisible = true;
                loadvcdetails();

            }
            catch (Exception ey)
            {
                Loading_activity.IsVisible = false;
                grid_vcdetails.IsVisible = false;
                await DisplayAlert(App.GetLabelByKey("Exception"), ey.Message, App.GetLabelByKey("close"));
                return;
            }
        }

        private void loadvcdetails()
        {
            lbl_vcid.Text = App.GetLabelByKey("vcid");
            lbl_vcdate.Text = App.GetLabelByKey("vcdate");
            lbl_VCStartTime.Text = App.GetLabelByKey("vcstarttime");
            lbl_Purpose.Text = App.GetLabelByKey("purpose");
            lbl_participant.Text = App.GetLabelByKey("vcstudioname");
            lbl_LevelName.Text = App.GetLabelByKey("level1");
            lbl_DeptName.Text = App.GetLabelByKey("department");
            lbl_MinistryName.Text = App.GetLabelByKey("ministry");
            lbl_VCStatus.Text = App.GetLabelByKey("vcstatus");
            lbl_VCEndTime.Text = App.GetLabelByKey("VCEndTime");
            lbl_participantsExt.Text = App.GetLabelByKey("participantsExt");
            lbl_webroom.Text = App.GetLabelByKey("webroom");
            lbl_hoststudio.Text = App.GetLabelByKey("hoststudio");
            lbl_RequestedBy.Text = App.GetLabelByKey("RequestedBy");
            lbl_VcCoordName.Text = App.GetLabelByKey("Name");
            lbl_VcCoordEmail.Text = App.GetLabelByKey("Email");
            lbl_VcCoordMobile.Text = App.GetLabelByKey("Mobile");
            lbl_VcCoordIpPhone.Text = App.GetLabelByKey("IpPhone");
            lbl_VccoordLandline.Text = App.GetLabelByKey("Landline");
            lbl_mcuname.Text = App.GetLabelByKey("mcuname");
            lbl_mcuip.Text = App.GetLabelByKey("mcuip");
            btn_takemethere.Text = "📍 " + App.GetLabelByKey("takemethere");
            btn_alertme.Text = "⏰ " + App.GetLabelByKey("alertme");
            btn_takemethere.IsVisible = false;
            btn_alertme.IsVisible = false;

            string query = $"Select * from searchByVcId";
            searchByVcIdlist = searchByVcIdDatabase.GetSearchByVcId(query).ToList();

            lbl_vcidvalue.Text = searchByVcIdlist.ElementAt(0).vcid.ToString();
            lbl_vcdatevalue.Text = searchByVcIdlist.ElementAt(0).vcdate.ToString();
            lbl_VCStartTimevalue.Text = searchByVcIdlist.ElementAt(0).VCStartTime.ToString();
            lbl_Purposevalue.Text = searchByVcIdlist.ElementAt(0).Purpose.ToString();
            lbl_participantvalue.Text = searchByVcIdlist.ElementAt(0).participant.ToString();
            lbl_LevelNamevalue.Text = searchByVcIdlist.ElementAt(0).LevelName.ToString();
            lbl_DeptNamevalue.Text = searchByVcIdlist.ElementAt(0).DeptName.ToString();
            lbl_MinistryNamevalue.Text = searchByVcIdlist.ElementAt(0).MinistryName.ToString();
            lbl_VCStatusvalue.Text = searchByVcIdlist.ElementAt(0).VCStatus.ToString();
            lbl_VCEndTimevalue.Text = searchByVcIdlist.ElementAt(0).VCEndTime.ToString();
            lbl_participantsExtvalue.Text = searchByVcIdlist.ElementAt(0).participantsExt.ToString();
            lbl_webroomvalue.Text = searchByVcIdlist.ElementAt(0).webroom.ToString();
            lbl_hoststudiovalue.Text = searchByVcIdlist.ElementAt(0).hoststudio.ToString();
            lbl_RequestedByvalue.Text = searchByVcIdlist.ElementAt(0).RequestedBy.ToString();
            lbl_VcCoordNamevalue.Text = searchByVcIdlist.ElementAt(0).VcCoordName.ToString();
            lbl_VcCoordEmailvalue.Text = searchByVcIdlist.ElementAt(0).VcCoordEmail.ToString();
            lbl_VcCoordMobilevalue.Text = searchByVcIdlist.ElementAt(0).VcCoordMobile.ToString();
            lbl_VcCoordIpPhonevalue.Text = searchByVcIdlist.ElementAt(0).VcCoordIpPhone.ToString();
            lbl_VccoordLandlinevalue.Text = searchByVcIdlist.ElementAt(0).VccoordLandline.ToString();
            lbl_mcunamevalue.Text = searchByVcIdlist.ElementAt(0).mcuname.ToString();
            lbl_mcuipvalue.Text = searchByVcIdlist.ElementAt(0).mcuip.ToString();

            CultureInfo provider = CultureInfo.InvariantCulture;
            string startdatetime = lbl_vcdatevalue.Text + " " + lbl_VCStartTimevalue.Text;
            string enddatetime = lbl_vcdatevalue.Text + " " + lbl_VCEndTimevalue.Text;
            string currentdatetime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            DateTime vcstartdatetime;
            DateTime vcenddatetime;
            DateTime currentdttm;

            try
            {
                vcstartdatetime = DateTime.ParseExact(startdatetime, "dd/MM/yyyy HH:mm:ss", provider);
                vcenddatetime = DateTime.ParseExact(enddatetime, "dd/MM/yyyy HH:mm:ss", provider);
            }
            catch
            {
                vcstartdatetime = DateTime.ParseExact(startdatetime, "dd-MM-yyyy HH:mm:ss", provider);
                vcenddatetime = DateTime.ParseExact(enddatetime, "dd-MM-yyyy HH:mm:ss", provider);
            }
            try
            {
                currentdttm = DateTime.ParseExact(currentdatetime, "dd/MM/yyyy HH:mm:ss", provider);
            }
            catch
            {
                currentdttm = DateTime.ParseExact(currentdatetime, "dd-MM-yyyy HH:mm:ss", provider);
            }

            if (vcstartdatetime.CompareTo(currentdttm) >= 0)
            {
                btn_alertme.IsVisible = true;
            }
            else
            {
                btn_alertme.IsVisible = false;
            }

            if (vcenddatetime.CompareTo(currentdttm) >= 0)
            {
                btn_takemethere.IsVisible = true;
            }
            else
            {
                btn_takemethere.IsVisible = false;
            }

        }


        private void btn_takemethere_Clicked(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            //string vcid = b.CommandParameter.ToString();
            Navigation.PushAsync(new TakeMeTherePage(lbl_vcidvalue.Text, "SearchByVcId"));
        }

        private async void btn_alertme_Clicked(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            string vcid = lbl_vcidvalue.Text.ToString();
            List<SearchByVcId> searchByVcIdlist;
            searchByVcIdlist = searchByVcIdDatabase.GetSearchByVcId($"Select * from SearchByVcId where vcid='{vcid}'").ToList();
            if (App.checkvcidinalerttable(vcid) == true)
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("alertalready"), App.GetLabelByKey("close"));
            }
            else
            {
                AlertableDatabase alertableDatabase = new AlertableDatabase();
                var item = new Alertable();
                item.VC_ID = searchByVcIdlist.ElementAt(0).vcid;
                var vcdt = searchByVcIdlist.ElementAt(0).vcdate;
                item.DateofVCtoshowonly = vcdt;
                DateTime dateofvc;
                try
                {
                    dateofvc = DateTime.ParseExact(vcdt, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                catch
                {
                    dateofvc = DateTime.ParseExact(vcdt, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }
                item.DateofVC = dateofvc.ToString("yyyy-MM-dd");
                item.Dept_Name = searchByVcIdlist.ElementAt(0).DeptName;
                item.Important = "";
                item.LevelName = searchByVcIdlist.ElementAt(0).LevelName;
                item.Org_Name = searchByVcIdlist.ElementAt(0).DeptName;
                item.Purpose = searchByVcIdlist.ElementAt(0).Purpose;
                item.RequestedBy = searchByVcIdlist.ElementAt(0).RequestedBy;
                item.Startingtime = searchByVcIdlist.ElementAt(0).VCStartTime;
                item.StudioID = searchByVcIdlist.ElementAt(0).StudioID;
                item.Studio_Name = searchByVcIdlist.ElementAt(0).participant;
                item.VCEndTime = searchByVcIdlist.ElementAt(0).VCEndTime;
                item.VCStatus = searchByVcIdlist.ElementAt(0).VCStatus;
                item.VcCoordEmail = searchByVcIdlist.ElementAt(0).VcCoordEmail;
                item.VcCoordIpPhone = searchByVcIdlist.ElementAt(0).VcCoordIpPhone;
                item.VcCoordMobile = searchByVcIdlist.ElementAt(0).VcCoordMobile;
                item.VcCoordName = searchByVcIdlist.ElementAt(0).VcCoordName;
                item.VccoordLandline = searchByVcIdlist.ElementAt(0).VccoordLandline;
                item.hoststudio = searchByVcIdlist.ElementAt(0).hoststudio;
                item.mcuip = searchByVcIdlist.ElementAt(0).mcuip;
                item.mcuname = searchByVcIdlist.ElementAt(0).mcuname;
                item.participantsExt = searchByVcIdlist.ElementAt(0).participantsExt;
                item.webroom = searchByVcIdlist.ElementAt(0).webroom;

                DateTime vcstartdate;
                try
                {
                    vcstartdate = DateTime.ParseExact(searchByVcIdlist.ElementAt(0).vcdate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                catch
                {
                    vcstartdate = DateTime.ParseExact(searchByVcIdlist.ElementAt(0).vcdate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                item.NotificationStartDate = vcstartdate.AddDays(-1).ToString("yyyy-MM-dd");
                item.Frequency = "1";

                alertableDatabase.AddAlertable(item);

                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("alertsave"), App.GetLabelByKey("close"));
                await Navigation.PopToRootAsync();
            }

        }
        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}