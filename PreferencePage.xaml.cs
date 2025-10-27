using Newtonsoft.Json.Linq;
using NICVC.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;





namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PreferencePage : ContentPage
    {

        DistrictMasterDatabase districtMasterDatabase;
        StudioMasterDatabase studioMasterDatabase;
        SaveUserPreferencesDatabase saveuserpreferencesDatabase;
        LanguageMasterDatabase languageMasterDatabase;

        string statename, districtname, studioName;
        string stateid, districtid, studioid;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        List<LanguageMaster> LanguageDropdown;

        StateMasterDatabase stateMasterDatabase;

        public PreferencePage()
        {
            InitializeComponent();
            stateMasterDatabase = new StateMasterDatabase();
            languageMasterDatabase = new LanguageMasterDatabase();
            saveuserpreferencesDatabase = new SaveUserPreferencesDatabase();
            Lbl_UserDetails.Text = Preferences.Get("DisplayName", "");

            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");

            App.StateMasterList = stateMasterDatabase.GetStateMaster("Select * from statemaster").ToList();
            if (!App.StateMasterList.Any())
            {
                var current = Connectivity.NetworkAccess;
                if (current == NetworkAccess.Internet)
                {
                    Dispatcher.Dispatch(async () =>
                    {
                        await GetState();
                    });
                }
                else
                {
                    DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
                }
            }

            lbl_title.Text = App.GetLabelByKey("selectstatedis");
            lbl_state.Text = App.GetLabelByKey("state");
            lbl_district.Text = App.GetLabelByKey("district");
            lbl_studio.Text = App.GetLabelByKey("studio");
            lbl_language.Text = App.GetLabelByKey("language");
            Btn_SavePrefer.Text = App.GetLabelByKey("submit");
            Btn_cancelPrefer.Text = App.GetLabelByKey("cancel");
            Picker_State.Title = App.GetLabelByKey("state");
            Picker_District.Title = App.GetLabelByKey("district");
            Picker_Studio.Title = App.GetLabelByKey("studio");
            Picker_language.Title = App.GetLabelByKey("language");
            LanguageDropdown = languageMasterDatabase.GetLanguageMaster($"select MultipleResourceKey,ResourceKey, (case when ({App.Language} = 0) then ResourceValue else LocalResourceValue end)ResourceValue from  LanguageMaster where MultipleResourceKey='language_drop_down'").ToList();
            Picker_language.ItemsSource = LanguageDropdown;
            Picker_language.ItemDisplayBinding = new Binding("ResourceValue");
            Picker_language.SelectedIndex = 0;


            saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
            Picker_State.ItemsSource = App.StateMasterList;
            Picker_State.Title = App.GetLabelByKey("state");
            Picker_State.ItemDisplayBinding = new Binding("StateName");

            Picker_District.Title = App.GetLabelByKey("district");
            Picker_District.ItemsSource = App.DistrictMasterList;
            Picker_District.ItemDisplayBinding = new Binding("DistrictName");
            Picker_Studio.Title = App.GetLabelByKey("studio");
            Picker_Studio.ItemsSource = App.StudioMasterList;
            Picker_Studio.ItemDisplayBinding = new Binding("StudioName");

            Picker_State.SelectedIndex = 0;

            try
            {
                Picker_State.SelectedIndex = App.StateMasterList.FindIndex(s => s.StateID == App.SavedUserPreferList.ElementAt(0).StateID);
                try
                {
                    Picker_District.SelectedIndex = App.DistrictMasterList.FindIndex(s => s.DistrictID == App.SavedUserPreferList.ElementAt(0).DistrictID);
                }
                catch { }
                try
                {
                    Picker_Studio.SelectedIndex = App.StudioMasterList.FindIndex(s => s.StudioID == App.SavedUserPreferList.ElementAt(0).StudioID);
                }
                catch { }

                Picker_language.SelectedIndex = App.Language;

            }
            catch
            {
            }

            App.SavedUserPreferList = saveUserPreferencesDatabase.GetSaveUserPreferences("select * from SaveUserPreferences").ToList();
            if (App.SavedUserPreferList.Any())
            {
                Btn_cancelPrefer.IsVisible = true;
            }
            else
            {
                Btn_cancelPrefer.IsVisible = false;
            }

        }

        private async void Picker_State_SelectedIndexChanged(object sender, EventArgs e)
        {
            Picker_State.Title = App.GetLabelByKey("state");
            if (Picker_State.SelectedIndex == -1)
            {
                return;
            }

            else
            {

                stateid = App.StateMasterList.ElementAt(Picker_State.SelectedIndex).StateID;
                statename = App.StateMasterList.ElementAt(Picker_State.SelectedIndex).StateName;

                if (statename.Contains("All"))
                {
                    Districtlayout.IsVisible = false;
                    Studiolayout.IsVisible = false;
                    Picker_District.SelectedIndex = -1;
                    Picker_Studio.SelectedIndex = -1;

                }
                else
                {
                    if (App.SavedUserPreferList.Any())
                    {
                        Districtlayout.IsVisible = true;
                        if (App.SavedUserPreferList.ElementAt(0).StateID != stateid)
                        {

                            var current = Connectivity.NetworkAccess;
                            if (current == NetworkAccess.Internet)
                            {
                                await GetDistrict(stateid);
                            }
                            else
                            {
                                await DisplayAlert("NICVC", "Their Seems To Be Problem With Your Internet Connection. Kindly Check Your Internet Connectivity", App.GetLabelByKey("close"));
                            }


                        }


                    }
                    else
                    {
                        var current = Connectivity.NetworkAccess;
                        if (current == NetworkAccess.Internet)
                        {
                            await GetDistrict(stateid);
                        }
                        else
                        {

                            await DisplayAlert("NICVC", "Their Seems To Be Problem With Your Internet Connection. Kindly Check Your Internet Connectivity", App.GetLabelByKey("close"));

                        }

                        Studiolayout.IsVisible = false;
                    }



                }
            }



        }

        private async void Picker_District_SelectedIndexChanged(object sender, EventArgs e)
        {
            Picker_District.Title = App.GetLabelByKey("district");


            // await DisplayAlert("Selected State", App.StateMasterList.ElementAt(Picker_State.SelectedIndex).StateID, App.GetLabelByKey("close"));
            if (Picker_District.SelectedIndex == -1)
            {
                districtid = "";
                districtname = "";
                return;
            }
            else
            {
                stateid = App.StateMasterList.ElementAt(Picker_State.SelectedIndex).StateID;
                districtid = App.DistrictMasterList.ElementAt(Picker_District.SelectedIndex).DistrictID;
                districtname = App.DistrictMasterList.ElementAt(Picker_District.SelectedIndex).DistrictName;

                if (districtname.Contains("All"))
                {
                    Studiolayout.IsVisible = false;
                    Picker_Studio.SelectedIndex = -1;
                    App.StudioMasterList = null;
                }
                else
                {
                    Studiolayout.IsVisible = true;
                    if (App.SavedUserPreferList.Any())
                    {
                        if (App.SavedUserPreferList.ElementAt(0).DistrictID != districtid)
                        {
                            var current = Connectivity.NetworkAccess;
                            if (current == NetworkAccess.Internet)
                            {
                                await GetStudio(stateid, districtid);
                            }
                            else
                            {

                                await DisplayAlert("NICVC", "Their Seems To Be Problem With Your Internet Connection. Kindly Check Your Internet Connectivity", App.GetLabelByKey("close"));

                            }
                        }

                    }
                    else
                    {

                        var current = Connectivity.NetworkAccess;
                        if (current == NetworkAccess.Internet)
                        {
                            await GetStudio(stateid, districtid);
                        }
                        else
                        {

                            await DisplayAlert("NICVC", "Their Seems To Be Problem With Your Internet Connection. Kindly Check Your Internet Connectivity", App.GetLabelByKey("close"));

                        }
                    }



                }
            }

        }

        private void Picker_Studio_SelectedIndexChanged(object sender, EventArgs e)
        {
            Picker_Studio.Title = App.GetLabelByKey("studio");

            if (Picker_Studio.SelectedIndex == -1)
            {
                studioid = "";
                studioName = "";
                return;
            }
            else
            {
                studioid = "";
                studioName = "";
                studioid = App.StudioMasterList.ElementAt(Picker_Studio.SelectedIndex).StudioID;
                studioName = App.StudioMasterList.ElementAt(Picker_Studio.SelectedIndex).StudioName;
            }


        }

        private void Picker_language_SelectedIndexChanged(object sender, EventArgs e)
        {
            Picker_language.Title = App.GetLabelByKey("language");
        }

        private async void Btn_SavePrefer_Clicked(object sender, EventArgs e)
        {
            if (await checkvalidations())
            {
                bool m = false;


                if (Picker_State.SelectedIndex != -1 && Picker_District.SelectedIndex != -1 && Picker_Studio.SelectedIndex != -1)
                {
                    m = await DisplayAlert(App.GetLabelByKey("confirmprefer"), "\n" + App.GetLabelByKey("state1") + " : " + App.StateMasterList.ElementAt(Picker_State.SelectedIndex).StateName +
                        "\n" + App.GetLabelByKey("district1") + " : " + App.DistrictMasterList.ElementAt(Picker_District.SelectedIndex).DistrictName +
                        "\n" + App.GetLabelByKey("studio1") + " : " + App.StudioMasterList.ElementAt(Picker_Studio.SelectedIndex).StudioName +
                        "\n" + App.GetLabelByKey("language1") + " : " + LanguageDropdown.ElementAt(Picker_language.SelectedIndex).ResourceValue,
                        App.GetLabelByKey("save"), App.GetLabelByKey("cancel"));
                }
                else if (Picker_State.SelectedIndex != -1 && Picker_District.SelectedIndex != -1)
                {
                    m = await DisplayAlert(App.GetLabelByKey("confirmprefer"), "\n" + App.GetLabelByKey("state1") + " : " + App.StateMasterList.ElementAt(Picker_State.SelectedIndex).StateName +
                        "\n" + App.GetLabelByKey("district1") + " : " + App.DistrictMasterList.ElementAt(Picker_District.SelectedIndex).DistrictName +
                        "\n" + App.GetLabelByKey("language1") + " : " + LanguageDropdown.ElementAt(Picker_language.SelectedIndex).ResourceValue,
                        App.GetLabelByKey("save"), App.GetLabelByKey("cancel"));
                }
                else if (Picker_State.SelectedIndex != -1)
                {
                    m = await DisplayAlert(App.GetLabelByKey("confirmprefer"), "\n" + App.GetLabelByKey("state1") + " : " + App.StateMasterList.ElementAt(Picker_State.SelectedIndex).StateName +
                        "\n" + App.GetLabelByKey("language1") + " : " + LanguageDropdown.ElementAt(Picker_language.SelectedIndex).ResourceValue,
                        App.GetLabelByKey("save"), App.GetLabelByKey("cancel"));
                }

                if (m)
                {
                    saveuserpreferencesDatabase.DeleteSaveUserPreferences();

                    App.SavedUserPreferList = null;
                    App.Language = 0;
                    App.MyLanguage = null;
                    var item = new SaveUserPreferences();
                    item.AppInstall = "Y";
                    item.StateID = stateid;
                    item.StateName = statename;
                    item.DistrictID = districtid;
                    item.DistrictName = districtname;
                    item.StudioID = studioid;
                    item.StudioName = studioName;
                    item.StateChanged = "Y";
                    item.language = Picker_language.SelectedIndex;
                    saveuserpreferencesDatabase.AddSaveUserPreferences(item);

                    languageMasterDatabase = new LanguageMasterDatabase();
                    App.Language = Picker_language.SelectedIndex;
                    App.MyLanguage = languageMasterDatabase.GetLanguageMaster($"select ResourceKey, (case when ({App.Language} = 0) then ResourceValue else LocalResourceValue end)ResourceValue from  LanguageMaster").ToList();
                    App.SavedUserPreferList = saveUserPreferencesDatabase.GetSaveUserPreferences("select * from SaveUserPreferences").ToList();
                    
                    // Recreate MainPage with TabbedPage to properly initialize the app
                    App.CurrentTabpageIndex = 0; // Set to Dashboard tab
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        Application.Current.MainPage = new NavigationPage(new NICVCTabbedPage());
                    }
                    else
                    {
                        Application.Current.MainPage = new NavigationPage(new NICVCTabbedPage())
                        {
                            BarBackgroundColor = Color.FromArgb("#2196f3"),
                            BarTextColor = Colors.WhiteSmoke
                        };
                    }

                }
            }
        }
        private void Btn_cancelPrefer_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private async Task<bool> checkvalidations()
        {
            if (Picker_State.SelectedIndex == -1)
            {
                await DisplayAlert(App.GetLabelByKey("state"), App.StateMasterList.ElementAt(Picker_State.SelectedIndex).StateID, App.GetLabelByKey("close"));
                return false;
            }
            return true;
        }

        async Task GetState()
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                Loading_activity.IsVisible = true;
                Lbl_PleaseWait.Text = App.GetLabelByKey("pleasewait");
                try
                {

                    var client = new HttpClient();
                    var responce = await client.GetAsync($"{App.StateUrl}");
               
                    string itemJson = await responce.Content.ReadAsStringAsync();

                    if (responce.IsSuccessStatusCode)
                    {
                        var result = await responce.Content.ReadAsStringAsync();
                        JObject parsed = JObject.Parse(result);
                        stateMasterDatabase = new StateMasterDatabase();
                        stateMasterDatabase.DeleteStateMaster();

                        foreach (var pair in parsed)
                        {

                            if (pair.Key == "StatesList")
                            {
                                var nodes = pair.Value;
                                var item = new StateMaster();
                                item.StateID = "0";
                                item.StateName = "All States";
                                stateMasterDatabase.AddStateMaster(item);
                                foreach (var node in nodes)
                                {

                                    item.StateID = AESCryptography.DecryptAES(node["StateID"].ToString());
                                    item.StateName = AESCryptography.DecryptAES(node["StateName"].ToString());
                                    stateMasterDatabase.AddStateMaster(item);
                                }
                            }
                        }

                    }
                    App.StateMasterList = stateMasterDatabase.GetStateMaster("select * from StateMaster order by StateName").ToList();
                    Loading_activity.IsVisible = false;
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        Application.Current.MainPage = new NavigationPage(new PreferencePage());
                    }
                    else
                    {
                        Application.Current.MainPage = new NavigationPage(new PreferencePage())
                        {
                            BarBackgroundColor = Color.FromArgb("#2196f3"),
                            BarTextColor = Colors.WhiteSmoke
                        };
                    }
                    
                }
                catch (Exception ey)
                {
                    Loading_activity.IsVisible = false;
                    await DisplayAlert(App.GetLabelByKey("Exception"), ey.Message, App.GetLabelByKey("close"));
                    return;
                }
            }
            else
            {

                Loading_activity.IsVisible = false;
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
                return;

            }

        }

        async Task GetDistrict(string stateid)
        {

            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("pleasewait");
            try
            {
                var client = new HttpClient();
                string url = $"{App.DistrictUrl}" +$"stateid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(stateid))}";
                var responce = await client.GetAsync(url);
                string itemJson = await responce.Content.ReadAsStringAsync();

                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);
                    districtMasterDatabase = new DistrictMasterDatabase();
                    districtMasterDatabase.DeleteDistrictMaster();

                    foreach (var pair in parsed)
                    {

                        if (pair.Key == "DistrictsList")
                        {
                            var nodes = pair.Value;
                            var item = new DistrictMaster();
                            item.StateID = "0";
                            item.DistrictID = "0";

                            item.DistrictName = "All Districts";
                            districtMasterDatabase.AddDistrictMaster(item);
                            foreach (var node in nodes)
                            {

                                item.StateID = AESCryptography.DecryptAES(node["StateID"].ToString());
                                item.DistrictID = AESCryptography.DecryptAES(node["DistrictID"].ToString());
                                item.DistrictName = AESCryptography.DecryptAES(node["DistrictName"].ToString());
                                districtMasterDatabase.AddDistrictMaster(item);
                            }
                        }
                    }
                    App.DistrictMasterList = districtMasterDatabase.GetDistrictMaster($"select * from DistrictMaster order by DistrictName").ToList();
                    //App.DistrictMasterList = districtMasterDatabase.GetDistrictMaster($"select * from DistrictMaster where (StateId = '{stateid}' order by DistrictName").ToList();
                    Picker_District.Title = App.GetLabelByKey("district");
                    Picker_District.ItemsSource = App.DistrictMasterList;
                    Picker_District.ItemDisplayBinding = new Binding("DistrictName");
                    Picker_District.SelectedIndex = 0;

                    Districtlayout.IsVisible = true;
                    Studiolayout.IsVisible = false;
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

        async Task GetStudio(string stateid, string districtid)
        {

            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("pleasewait");
            try
            {
              
                var client = new HttpClient();
                var url = $"{App.StudioListUrl}" +
                    $"stateid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(stateid))}" +
                    $"&DistrictID={HttpUtility.UrlEncode(AESCryptography.EncryptAES(districtid))}";

                var responce = await client.GetAsync(url);
                string itemJson = await responce.Content.ReadAsStringAsync();
                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);
                    studioMasterDatabase = new StudioMasterDatabase();
                    studioMasterDatabase.DeleteStudioMaster();

                    foreach (var pair in parsed)
                    {

                        if (pair.Key == "StudioNameList")
                        {
                            var nodes = pair.Value;
                            var item = new StudioMaster();
                            item.StateID = "0";
                            item.DistrictID = "0";
                            item.StudioID = "0";
                            item.StudioName = "All Studios";
                            studioMasterDatabase.AddStudioMaster(item);
                            foreach (var node in nodes)
                            {

                                item.StateID = AESCryptography.DecryptAES(node["StateID"].ToString());
                                item.DistrictID = AESCryptography.DecryptAES(node["DistrictID"].ToString());
                                item.StudioID = AESCryptography.DecryptAES(node["StudioID"].ToString());
                                item.StudioName = AESCryptography.DecryptAES(node["StudioName"].ToString());
                                studioMasterDatabase.AddStudioMaster(item);
                                Console.WriteLine("Studioid" + item.StudioID);
                            }
                        }
                    }
                    App.StudioMasterList = studioMasterDatabase.GetStudioMaster($"select * from StudioMaster order by StudioID").ToList();
                    Picker_Studio.Title = App.GetLabelByKey("selectstudio");
                    Picker_Studio.ItemsSource = App.StudioMasterList;
                    Picker_Studio.ItemDisplayBinding = new Binding("StudioName");
                    Picker_Studio.SelectedIndex = 0;
                }
                Loading_activity.IsVisible = false;
            }
            catch (Exception ey)
            {
                Loading_activity.IsVisible = false;
                await DisplayAlert(App.GetLabelByKey("NICVC"), ey.Message, App.GetLabelByKey("close"));
                return;
            }
        }
    }
}