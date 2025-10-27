using Newtonsoft.Json.Linq;
using NICVC.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NICVC
{
    public partial class App : Application
    {
        public static int CurrentTabpageIndex = 0;
        UserLoginDatabase userlogindatabase;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        LanguageMasterDatabase languageMasterDatabase;
        //TodayVcDatabase todayVcDatabase;
        public static List<UserLogin> UserLoginValues;
        StateMasterDatabase stateMasterDatabase;
        DistrictMasterDatabase districtMasterDatabase;
        StudioMasterDatabase studioMasterDatabase;
        public static string getusername, getuserpwd;
        public static int Language = 0;
        public static List<LanguageMaster> MyLanguage;
        public static List<StateMaster> StateMasterList;
        public static List<DistrictMaster> DistrictMasterList;
        public static List<StudioMaster> StudioMasterList;
        public static List<SaveUserPreferences> SavedUserPreferList;
        AlertableDatabase alertableDatabase;
        public static string starttimetodayvc, vcstatustodayvc, vcdateschedulevc, vcdateimportantvc;
        public static string starttimemultipointvc, vcstatusmultipointvc, vcdatesmultipointvc;

        public static string StateUrl = "https://himachal.nic.in/NICVC/Parichay/v2/Service1.svc/StatesList?";
        public static string DistrictUrl = "https://himachal.nic.in/NICVC/Parichay/v2/Service1.svc/DistrictsList?";
        public static string StudioListUrl = "https://himachal.nic.in/NICVC/Parichay/v2/Service1.svc/StudioNameList?";
        public static string TodayVcUrl = "https://himachal.nic.in/NICVC/Parichay/v2/Service1.svc/NewDecTodayVcList?";
        public static string SearchByVcIdUrl = "https://himachal.nic.in/NICVC/Parichay/v2/Service1.svc/NewDecSearchVcIdList?";
        public static string Dashboardurl = "https://himachal.nic.in/NICVC/Parichay/v2/Service1.svc/Dashboard?";
        public static string latlong_url = "https://himachal.nic.in/NICVC/Parichay/v2/Service1.svc/LatLong?";
        public static string yearwisevcdetail_url = "https://himachal.nic.in/NICVC/Parichay/v2/Service1.svc/YearWiseVcDetails?";
        public static string savefeedback_url = "https://himachal.nic.in/NICVC/Parichay/v2/Service1.svc/savefeedback?";
        public static string currentmonthvcurl = "https://himachal.nic.in/NICVC/Parichay/v2/Service1.svc/NewDecTodayVcList?";
        public static string TodayVcUrlreserve = "https://himachal.nic.in/NICVC/Parichay/v2/ReserveNIC.svc/NewDecTodayVcList?";
        public static string newdec_multipointvcnichq_url = "https://himachal.nic.in/NICVC/Parichay/v2/ReserveNIC.svc/NewDecMultipointList?";
        public static string cicvc_url = "https://himachal.nic.in/NICVC/Parichay/v2/ReserveNIC.svc/CICVcList?";
        public static string pointtopoint_url = "https://himachal.nic.in/NICVC/Parichay/v2/ReserveNIC.svc/Point2PointList?";

        //Parichay Login URLSs       
        public static string AuthorizationUri = "https://parichay.nic.in/pnv1/oauth2/authorize?";
        public static string Redirectionuri = "https://mobileappshp.nic.in/Service1.svc/saveParichayAPIresponse";
        public static string tokenuri = "https://parichay.nic.in/pnv1/salt/api/oauth2/token?";
        public static string getuserdetails = "https://parichay.nic.in/pnv1/salt/api/oauth2/userdetails";
        public static string Auth_revoke = "https://parichay.nic.in/pnv1/salt/api/oauth2/revoke";
        public static string CodeVerifier = "khPbUjkiaiuaHsYpdaER3ZrYQ_Os1r_yuXjo5elSg-l1JLigZV6bkUsa6XqPY5oHX9F4d5DTx4shDptFl5ZiVhDKb9EMMYZr-9Cy985WFT-wSjIO0K3h2pBOhg0K354k";
        public static string CodeChallange = "HU539Hj9CGQkNOlbxuT5d3gId9mom_FJzG4T6fUFT54";
        public static string clientid = "6b7e1051d558f99eef419da88952d72d";
        public static string clientsecret = "7f0728f98140dd7dc081bf94ea562837";
        public static string scope = "USER_DETAILS";

        //END Parichay Auth//
        public App()
        {
            InitializeComponent();
            alertableDatabase = new AlertableDatabase();
            saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
            languageMasterDatabase = new LanguageMasterDatabase();
            stateMasterDatabase = new StateMasterDatabase();
            districtMasterDatabase = new DistrictMasterDatabase();
            studioMasterDatabase = new StudioMasterDatabase();
            userlogindatabase = new UserLoginDatabase();

            List<Alertable> alertablelist;
            alertablelist = alertableDatabase.GetAlertable("Select * from Alertable").ToList();
            if (!alertablelist.Any())
            {
                var item = new Alertable();
                item.Startingtime = "All";
                item.DateofVC = "All";
                item.DateofVCtoshowonly = "All";
                alertableDatabase.AddAlertable(item);
            }

            if (VersionTracking.IsFirstLaunchForCurrentVersion)
            {
                Prefixed();
            }

            SavedUserPreferList = new List<SaveUserPreferences>();
            DistrictMasterList = new List<DistrictMaster>();
            StudioMasterList = new List<StudioMaster>();

            //Prefixed();
            if (!languageMasterDatabase.GetLanguageMaster("Select * from languageMaster").Any())
            {
                Prefixed();
            }

            UserLoginValues = userlogindatabase.GetUserLogin("Select * from UserLogin").ToList();
            if (UserLoginValues.Count > 0)
            {
                getusername = UserLoginValues.ElementAt(0).UserName;
                getuserpwd = UserLoginValues.ElementAt(0).password;
                StateMasterList = stateMasterDatabase.GetStateMaster("select * from StateMaster").ToList();
                DistrictMasterList = districtMasterDatabase.GetDistrictMaster("select * from DistrictMaster").ToList();
                StudioMasterList = studioMasterDatabase.GetStudioMaster("select * from StudioMaster").ToList();
                SavedUserPreferList = saveUserPreferencesDatabase.GetSaveUserPreferences("select * from saveUserPreferences").ToList();
            }

            var isremember = userlogindatabase.GetUserLogin("select * from UserLogin").ToList();
            var languge = saveUserPreferencesDatabase.GetSaveUserPreferences("select * from saveUserPreferences").ToList();
            try
            {
                Language = languge.ElementAt(0).language;
            }
            catch
            {
                Language = 0;//1 for english
            }
            MyLanguage = languageMasterDatabase.GetLanguageMaster($"select MultipleResourceKey,ResourceKey, (case when ({Language} = 0) then ResourceValue else LocalResourceValue end)ResourceValue from  LanguageMaster").ToList();

            if (UserLoginValues.Count == 0)
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    MainPage = new NavigationPage(new ParichayPage());
                    return;
                }
                else
                {
                    MainPage = new NavigationPage(new ParichayPage())
                    {
                        BarBackgroundColor = Color.FromArgb("#2196f3"),
                        BarTextColor = Colors.WhiteSmoke
                    };
                }
            }
            else if (SavedUserPreferList.Count == 0)
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    MainPage = new NavigationPage(new ParichayPage());
                    return;
                }
                else
                {
                    MainPage = new NavigationPage(new ParichayPage())
                    {
                        BarBackgroundColor = Color.FromArgb("#2196f3"),
                        BarTextColor = Colors.WhiteSmoke
                    };
                }
            }
            else
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    MainPage = new NavigationPage(new NICVCTabbedPage());
                    return;
                }
                else
                {
                    MainPage = new NavigationPage(new NICVCTabbedPage())
                    {
                        BarBackgroundColor = Color.FromArgb("#2196f3"),
                        BarTextColor = Colors.WhiteSmoke
                    };
                }
            }
        }

        public static bool isAlphaNumeric(string strToCheck)
        {
            Regex rg = new Regex(@"^[a-zA-Z0-9\s,./]*$");
            return rg.IsMatch(strToCheck);
        }

        public static bool isemail(string strToCheck)
        {
            Regex rg = new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z");
            return rg.IsMatch(strToCheck);
        }

        public static bool isAlphabetonly(string strtocheck)
        {
            Regex rg = new Regex(@"^[a-zA-Z\s]+$");
            return rg.IsMatch(strtocheck);
        }

        public static bool isNumeric(string strToCheck)
        {
            Regex rg = new Regex("^[0-9]+$");
            return rg.IsMatch(strToCheck);
        }
        public static string GetLabelByKey(string Key)
        {
            string Lable_Name = "No Value";
            try
            {
                Lable_Name = MyLanguage.FindAll(s => s.ResourceKey == Key).ElementAt(0).ResourceValue;
            }
            catch
            {
                Lable_Name = Key;
            }
            return Lable_Name;
        }

        public static string GetLableByMultipleKey(string Key)
        {
            string Lable_Name = "No Value";
            try
            {
                Lable_Name = MyLanguage.FindAll(s => s.MultipleResourceKey == Key).ElementAt(0).ResourceValue;
            }
            catch
            {
                Lable_Name = Key;
            }
            return Lable_Name;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        void Prefixed()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Prefixed: Starting method");
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(PreferencePage)).Assembly;
                System.Diagnostics.Debug.WriteLine("Prefixed: Got assembly");
                
                Stream stream = assembly.GetManifestResourceStream("NICVC.languagejson.txt");
                System.Diagnostics.Debug.WriteLine($"Prefixed: Got stream - {(stream != null ? "Success" : "NULL")}");
                
                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine("Prefixed: Stream is null, skipping language initialization");
                    return;
                }
                
                string MyJson = "";
                using (var reader = new StreamReader(stream))
                {
                    MyJson = reader.ReadToEnd();
                }
                System.Diagnostics.Debug.WriteLine($"Prefixed: Read JSON, length: {MyJson.Length}");
                
                JObject parsed = JObject.Parse(MyJson);
                System.Diagnostics.Debug.WriteLine("Prefixed: Parsed JSON");
                
                // languageMasterDatabase = new LanguageMasterDatabase();
                languageMasterDatabase.DeleteLanguageMaster();
                System.Diagnostics.Debug.WriteLine("Prefixed: Deleted existing language data");

                foreach (var pair in parsed)
                {
                    if (pair.Key == "languagemaster")
                    {
                        var nodes = pair.Value;
                        var item = new LanguageMaster();
                        foreach (var node in nodes)
                        {
                            item.StateID = node["StateID"].ToString();
                            item.MultipleResourceKey = node["MultipleResourceKey"].ToString();
                            item.ResourceKey = node["ResourceKey"].ToString();
                            item.ResourceValue = node["ResourceValue"].ToString();
                            item.LocalResourceValue = node["LocalResourceValue"].ToString();
                            item.Sequence = node["Sequence"].ToString();

                            languageMasterDatabase.AddLanguageMaster(item);
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine("Prefixed: Completed successfully");
            }
            catch (Exception ey)
            {
                System.Diagnostics.Debug.WriteLine($"Prefixed Error: {ey.Message}");
                System.Diagnostics.Debug.WriteLine($"Prefixed Stack: {ey.StackTrace}");
                Current.MainPage.DisplayAlert("NICVC", ey.Message + "Failed to Load Default Data. Please Uninstall and then Re-install the App again", "OK");
            }
        }

        public static bool checkvcidinalerttable(string vcid)
        {
            bool flag = false;
            AlertableDatabase alertableDatabase = new AlertableDatabase();
            List<Alertable> alertablelist;
            alertablelist = alertableDatabase.GetAlertable($"Select * from Alertable where VC_ID='{vcid}'").ToList();
            if (alertablelist.Any())
            {
                flag = true;
            }
            return flag;
        }

        // Helper method to navigate to TabbedPage with consistent styling
        public static void NavigateToTabbedPage()
        {
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                Current.MainPage = new NavigationPage(new NICVCTabbedPage());
            }
            else
            {
                Current.MainPage = new NavigationPage(new NICVCTabbedPage())
                {
                    BarBackgroundColor = Color.FromArgb("#2196f3"),
                    BarTextColor = Colors.WhiteSmoke
                };
            }
        }

    }
}
