using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;



using NICVC.Model;

namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FeedbackPage : ContentPage
    {
        int rateing;
        TodayVcDatabase todayVcDatabase;
        List<TodayVc> todayVclist;
        string selectedVCid, mystudioID;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        List<SaveUserPreferences> SavedUserPreferList;
        string districtname, studioname;
        PersonalInfoDatabase personalInfoDatabase;
        List<PersonalInfo> personalInfolist;
        public FeedbackPage()
        {
            InitializeComponent();
            title_feedbackpage.Title = App.GetLabelByKey("Feedback");
            saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
            todayVcDatabase = new TodayVcDatabase();
            personalInfoDatabase = new PersonalInfoDatabase();
            Lbl_UserDetails.Text = Preferences.Get("DisplayName", "");
        }

        protected override void OnAppearing()
        {
            try
            {
                title_feedbackpage.Title = App.GetLabelByKey("Feedback");

                personalInfolist = personalInfoDatabase.GetPersonalInfo("Select * from personalinfo").ToList();

                // Personal info check is now handled at TabbedPage level
                Lbl_Header.Text = App.GetLabelByKey("feedbackidbased");
                btn_save.Text = App.GetLabelByKey("savefeedback");
                lbl_selvc.Text = App.GetLabelByKey("selectvc");
                lbl_selstudio.Text = App.GetLabelByKey("selectstudio");
                picker_vc.Title = App.GetLabelByKey("selectvc");
                picker_studio.Title = App.GetLabelByKey("selectstudio");
                lbl_taprating.Text = App.GetLabelByKey("SelRating");
                lbl_remarks.Text = App.GetLabelByKey("remarks");
                editor_remarks.Placeholder = App.GetLabelByKey("remarks");
                SavedUserPreferList = saveUserPreferencesDatabase.GetSaveUserPreferences("select * from saveUserPreferences").ToList();
                
                string statename = "";
                if (SavedUserPreferList.Any())
                {
                    statename = SavedUserPreferList.ElementAt(0).StateName.ToString();
                    
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
                }
                else
                {
                    // No user preferences found - set defaults
                    statename = "Default State";
                    districtname = "";
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

                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    Dispatcher.Dispatch(async () => await GetTodayVc());
                    loadtodayvc();
                }
                else
                {
                    // No internet - just load local data without showing popup
                    loadtodayvc();
                }
            }
            catch (Exception ex)
            {
                // Don't show popup on page load - just log the error
                System.Diagnostics.Debug.WriteLine("FeedbackPage Error: " + ex.Message);
            }
        }

        void loadtodayvc()
        {
            try
            {
                var currenttime = string.Format("{0:HH:mm:ss}", DateTime.Now);
                string query = "select * from todayvc where time(VCEndTime) <= '" + currenttime + "'";

                todayVclist = todayVcDatabase.GetTodayVc(query).ToList();
                picker_vc.ItemsSource = todayVclist;
                if (todayVclist.Any())
                {
                    picker_vc.ItemDisplayBinding = new Binding("Purpose");
                    picker_vc.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                // Handle database errors gracefully - just log, don't show popup
                System.Diagnostics.Debug.WriteLine("loadtodayvc Error: " + ex.Message);
            }
        }

        private void picker_vc_SelectedIndexChanged(object sender, EventArgs e)
        {
            TodayVc selectedItem = picker_vc.SelectedItem as TodayVc;

            if (picker_vc.SelectedIndex == -1)
            {
                return;
            }
            if (selectedItem != null)
            {
                selectedVCid = selectedItem.VC_ID;
                LoadStudio(selectedVCid);
            }
        }

        void LoadStudio(string vcid)
        {
            List<TodayVc> todayvclistStudio = new List<TodayVc>();
            string query = "select StudioID, Studio_Name||','||participantsExt as Studio_Name from TodayVc where VC_ID ='" + vcid + "' order by Studio_Name";
            todayVclist = todayVcDatabase.GetTodayVc(query).ToList();
            
            // Check if todayVclist has data before accessing it
            if (todayVclist.Any() && todayVclist.FirstOrDefault() != null)
            {
                var firstVc = todayVclist.FirstOrDefault();
                if (!string.IsNullOrEmpty(firstVc.StudioID) && !string.IsNullOrEmpty(firstVc.Studio_Name))
                {
                    string[] subsID = firstVc.StudioID.Split(',');
                    string[] subsName = firstVc.Studio_Name.Split(',');
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
                        picker_studio.SelectedIndex = 0;
                        mystudioID = todayvclistStudio.ElementAt(picker_studio.SelectedIndex).StudioID.Trim();
                    }
                    catch (Exception ex)
                    {
                        // Handle errors gracefully - just log
                        System.Diagnostics.Debug.WriteLine("LoadStudio Error: " + ex.Message);
                    }
                }
            }
        }

        void One(object sender, EventArgs e)

        {

            rateone.Source = "Starselected";

            ratetwo.Source = "Starunselected";

            ratethree.Source = "Starunselected";

            ratefour.Source = "Starunselected";

            ratefive.Source = "Starunselected";

            rateing = 1;

        }

        void Two(object sender, EventArgs e)

        {

            rateone.Source = "Starselected";

            ratetwo.Source = "Starselected";

            ratethree.Source = "Starunselected";

            ratefour.Source = "Starunselected";

            ratefive.Source = "Starunselected";

            rateing = 2;

        }

        void Three(object sender, EventArgs e)

        {

            rateone.Source = "Starselected";

            ratetwo.Source = "Starselected";

            ratethree.Source = "Starselected";

            ratefour.Source = "Starunselected";

            ratefive.Source = "Starunselected";

            rateing = 3;

        }

        void Four(object sender, EventArgs e)

        {

            rateone.Source = "Starselected";

            ratetwo.Source = "Starselected";

            ratethree.Source = "Starselected";

            ratefour.Source = "Starselected";

            ratefive.Source = "Starunselected";

            rateing = 4;

        }

        void Five(object sender, EventArgs e)

        {

            rateone.Source = "Starselected";

            ratetwo.Source = "Starselected";

            ratethree.Source = "Starselected";

            ratefour.Source = "Starselected";

            ratefive.Source = "Starselected";

            rateing = 5;

        }

        async Task<bool> checkvalidation()
        {
            if (picker_vc.ItemsSource == null || !((List<TodayVc>)picker_vc.ItemsSource).Any())
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), "No VC data available for feedback.", App.GetLabelByKey("close"));
                return false;
            }
            
            if (picker_studio.SelectedIndex == -1)
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), "Please select a VC first.", App.GetLabelByKey("close"));
                return false;
            }

            if (rateing <= 0)
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("SelRating"), App.GetLabelByKey("close"));
                return false;
            }

            if ((rateing <= 2) && string.IsNullOrEmpty(editor_remarks.Text))
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("remarks"), App.GetLabelByKey("close"));
                editor_remarks.Focus();
                return false;
            }
            if (rateing > 2 && !string.IsNullOrEmpty(editor_remarks.Text))
            {
                if (!App.isAlphaNumeric(editor_remarks.Text))
                {
                    await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("onlyalphabets") + " " + App.GetLabelByKey("remarks"), App.GetLabelByKey("close"));

                    return false;
                }
            }
            return true;
        }

        static public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(toEncode);
            string returnValue = Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        private async void btn_save_Clicked(object sender, EventArgs e)
        {
            if (await checkvalidation())
            {
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    await SaveFeedback();
                }
                else
                {
                    await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
                }
            }
        }

        async Task SaveFeedback()
        {
            string VQuality = "0.0";
            string SQuality = "0.0";
            var currenttime = string.Format("{0:HH:mm:ss}", DateTime.Now);
            PersonalInfoDatabase personalInfoDatabase = new PersonalInfoDatabase();
            List<PersonalInfo> personalInfolist;
            personalInfolist = personalInfoDatabase.GetPersonalInfo("Select * from PersonalInfo").ToList();
            string mobile = personalInfolist.ElementAt(0).Mobile;

            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("pleasewait");

            if (rateing > 2 && string.IsNullOrEmpty(editor_remarks.Text))
            {
                editor_remarks.Text = "NA";
            }
            try
            {
                var client = new HttpClient();
    

                string apiurl = $"{App.savefeedback_url}" +                              
                               $"stateid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StateID))}" +
                               $"&districtid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).DistrictID))}" +
                               $"&MobileNo={HttpUtility.UrlEncode(AESCryptography.EncryptAES(mobile))}" +
                               $"&studioid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(mystudioID))}" +
                               $"&VcId={HttpUtility.UrlEncode(AESCryptography.EncryptAES(selectedVCid))}" +
                               $"&VcQuality={HttpUtility.UrlEncode(AESCryptography.EncryptAES(VQuality))}" +
                               $"&StudioQuality={HttpUtility.UrlEncode(AESCryptography.EncryptAES(SQuality))}" +
                               $"&OverallExp={HttpUtility.UrlEncode(AESCryptography.EncryptAES(rateing.ToString()))}" +
                               $"&Time={HttpUtility.UrlEncode(AESCryptography.EncryptAES(currenttime))}" +
                               $"&ModifyDate={HttpUtility.UrlEncode(AESCryptography.EncryptAES(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")))}" +
                               $"&Remarks={HttpUtility.UrlEncode(AESCryptography.EncryptAES(editor_remarks.Text))}";

                var responce = await client.GetAsync(apiurl);

                if (responce.IsSuccessStatusCode)
                {

                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);
                    string statusMessage = "", status;

                    foreach (var pair in parsed)
                    {
                        if (pair.Key == "Feedback")
                        {
                            statusMessage = parsed["Feedback"]["message"].ToString();
                            status = parsed["Feedback"]["status"].ToString();

                            Loading_activity.IsVisible = false;
                            await Application.Current.MainPage.DisplayAlert(App.GetLabelByKey("NICVC"), statusMessage, App.GetLabelByKey("close"));
                            App.CurrentTabpageIndex = 2;
                            
                            // Navigate back to tabbed page instead of replacing MainPage
                            await Navigation.PopToRootAsync();
                        }
                    }
                }
                else
                {
                    Loading_activity.IsVisible = false;
                    await Application.Current.MainPage.DisplayAlert(App.GetLabelByKey("NICVC"), $"[{responce.StatusCode}]" + App.GetLabelByKey("noserver"), App.GetLabelByKey("close"));
                    return;
                }
                Loading_activity.IsVisible = false;
            }
            catch (OperationCanceledException)
            {
                await Application.Current.MainPage.DisplayAlert(App.GetLabelByKey("RequestTimeout"), App.GetLabelByKey("noserver"), App.GetLabelByKey("close"));
                Loading_activity.IsVisible = false;
            }
            catch (TimeoutException)
            {
                await Application.Current.MainPage.DisplayAlert(App.GetLabelByKey("RequestTimeout"), App.GetLabelByKey("noserver"), App.GetLabelByKey("close"));
                Loading_activity.IsVisible = false;
            }
            catch (SocketException)
            {
                await Application.Current.MainPage.DisplayAlert(App.GetLabelByKey("SocketClosed"), App.GetLabelByKey("noserver"), App.GetLabelByKey("close"));
                Loading_activity.IsVisible = false;
            }
            catch (Exception ey)
            {
                await Application.Current.MainPage.DisplayAlert(App.GetLabelByKey("Exception"), ey.Message, App.GetLabelByKey("close"));
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
                else
                {
                    Loading_activity.IsVisible = false;
                    await Application.Current.MainPage.DisplayAlert(App.GetLabelByKey("NICVC"), $"[{responce.StatusCode}]" + "Server Not Found", "Close");
                    return;
                }
                Loading_activity.IsVisible = false;

            }
            catch (OperationCanceledException)
            {
                await Application.Current.MainPage.DisplayAlert(App.GetLabelByKey("RequestTimeout"), App.GetLabelByKey("noserver"), App.GetLabelByKey("close"));
                Loading_activity.IsVisible = false;
            }
            catch (TimeoutException)
            {
                await Application.Current.MainPage.DisplayAlert(App.GetLabelByKey("RequestTimeout"), App.GetLabelByKey("noserver"), App.GetLabelByKey("close"));
                Loading_activity.IsVisible = false;
            }
            catch (SocketException)
            {
                await Application.Current.MainPage.DisplayAlert(App.GetLabelByKey("SocketClosed"), App.GetLabelByKey("noserver"), App.GetLabelByKey("close"));
                Loading_activity.IsVisible = false;
            }
            catch (Exception ey)
            {
                await Application.Current.MainPage.DisplayAlert(App.GetLabelByKey("Exception"), ey.Message, App.GetLabelByKey("close"));
                Loading_activity.IsVisible = false;
            }
        }
    }
}
