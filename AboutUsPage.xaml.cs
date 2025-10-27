using System;

using NICVC.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutUsPage : ContentPage
    {
        public AboutUsPage()
        {
            InitializeComponent();
            title_aboutpage.Title = App.GetLabelByKey("more");
            VersionTracking.Track();
            var currentVersion = VersionTracking.CurrentVersion;

            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");
            lbl_appname.Text = App.GetLabelByKey("NICVC") + " " + App.GetLabelByKey("Version") + currentVersion;
            lbl_preferences.Text = App.GetLabelByKey("Preferences");
            lbl_Logout.Text = App.GetLabelByKey("logout");
            lbl_personlainfo.Text = App.GetLabelByKey("updatepersonalinfo");
            lbl_deleteprofile.Text = "Delete Profile";
            lbl_privacypolicy.Text = (App.Language == 0) ? "Privacy Policy" : "गोपनीयता नीति";
            lbl_deptheading.Text = App.GetLabelByKey("vcdivison");

            lbl_depttCall.Text = App.GetLabelByKey("niccontactinfo") + " - 1800111555";
            lbl_depttemail.Text = App.GetLabelByKey("Email") + " - vc-delhi[at]nic[dot]in";
            lbl_depttWebSite.Text = App.GetLabelByKey("nicwebsiteinfo") + " - http://vidcon.nic.in";

            PersonalInfoDatabase personalInfoDatabase = new PersonalInfoDatabase();
            List<PersonalInfo> personalInfolist = personalInfoDatabase.GetPersonalInfo("Select * from personalinfo").ToList();

            // Always show profile option, hide delete profile option
            // stack_personlainfo.IsVisible = true;
            
            if (personalInfolist.Any())
            {
                // Personal info exists - show update option
                lbl_personlainfo.Text = App.GetLabelByKey("updatepersonalinfo");
            }
            else
            {
                // No personal info - show create profile option
                lbl_personlainfo.Text = "Create Profile";
            }           
        }

        private void Deptt_Call(object sender, EventArgs e)
        {
            // Device.OpenUri(new Uri("tel: 01772621196"));
            // PhoneDialer.Open("1800111555");
            try
            {
                PhoneDialer.Open("1800111555");
            }
            catch (ArgumentNullException anEx)
            {
                // Number was null or white space
                DisplayAlert("NICVC", anEx.Message.ToString(), "Close");
            }
            catch (FeatureNotSupportedException ex)
            {
                // Phone Dialer is not supported on this device.
                DisplayAlert("NICVC", ex.Message.ToString(), "Close");
            }
            catch (Exception ex)
            {
                // Other error has occurred.
                DisplayAlert("NICVC", ex.Message.ToString(), "Close");
            }

        }

        private void deptt_WebSite(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("http://vidcon.nic.in"));
        }

        private void NIC_Call(object sender, EventArgs e)
        {
            //Device.OpenUri(new Uri("tel: 01772624045"));
            //  PhoneDialer.Open("01772624045");
            try
            {
                PhoneDialer.Open("01772624045");
            }
            catch (ArgumentNullException anEx)
            {
                // Number was null or white space
                DisplayAlert("NICVC", anEx.Message.ToString(), "Close");
            }
            catch (FeatureNotSupportedException ex)
            {
                // Phone Dialer is not supported on this device.
                DisplayAlert("NICVC", ex.Message.ToString(), "Close");
            }
            catch (Exception ex)
            {
                // Other error has occurred.
                DisplayAlert("NICVC", ex.Message.ToString(), "Close");
            }

        }

        private void NIC_Website(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("https://himachal.nic.in/en-IN/nichp.html"));
        }

        private void Deptt_email(object sender, EventArgs e)
        {

            string address = "vc-delhi@nic.in";
            Launcher.OpenAsync(new Uri($"mailto:{address}"));

        }

        private async void nic_email(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync("group1-hp@nic.in");
            if (Clipboard.HasText)
            {
                var text = await Clipboard.GetTextAsync();
                await DisplayAlert("Email", string.Format("Email Id ' {0} ' Copied ", text), "OK");
            }
        }

        private void Preferences(object sender, EventArgs e)
        {
            Navigation.PushAsync(new PreferencePage());
        }

        private void PersonalInfo_tapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(new PersonalinforPage());
        }

        private void PrivacyPolicy_tapped(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("https://mobileappshp.nic.in/assets/pdf/mobile-app-privacy-policy/nicvc.html"));
        }

        private async void DeleteProfile_tapped(object sender, EventArgs e)
        {
            var result = await DisplayAlert("Delete Profile", "Are you sure you want to delete your profile? This action cannot be undone.", "Delete", "Cancel");
            if (result)
            {
                try
                {
                    // Delete personal information from database
                    PersonalInfoDatabase personalInfoDatabase = new PersonalInfoDatabase();
                    personalInfoDatabase.customquery("DELETE FROM PersonalInfo");
                    
                    await DisplayAlert("Success", "Profile deleted successfully.", "OK");
                    
                    // Force complete app refresh to properly reinitialize everything
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        Application.Current.MainPage = new NavigationPage(new ParichayPage("Logout"));
                    }
                    else
                    {
                        Application.Current.MainPage = new NavigationPage(new ParichayPage("Logout"))
                        {
                            BarBackgroundColor = Color.FromArgb("#2196f3"),
                            BarTextColor = Colors.WhiteSmoke
                        };
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", "Failed to delete profile: " + ex.Message, "OK");
                }
            }
        }

        private async void Logout(object sender, EventArgs e)
        {

            string logoutmessage, btn1, btn2;
            btn1 = App.GetLabelByKey("logout");
            btn2 = App.GetLabelByKey("cancel");
            logoutmessage = App.GetLabelByKey("logoutmessage");

            var m = await DisplayAlert(App.getusername, logoutmessage, btn1, btn2);
            if (m)
            {
                UserLoginDatabase userlogindatabase;
                SaveUserPreferencesDatabase saveUserPreferencesDatabase;
                TodayVcDatabase todayVcDatabase;
                ScheduleVcDatabase scheduleVcDatabase;
                ImportantVcDatabase importantVcDatabase;
                SearchByVcIdDatabase searchByVcIdDatabase;
                AlertableDatabase alertableDatabase;
                DashboardDatabase dashboardDatabase = new DashboardDatabase();
                LatLongDatabase latLongDatabase = new LatLongDatabase();
                CurrentMonthVcDatabase currentMonthVcDatabase = new CurrentMonthVcDatabase();
                YearWiseVcDatabase yearWiseVcDatabase = new YearWiseVcDatabase();

                userlogindatabase = new UserLoginDatabase();
                saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
                todayVcDatabase = new TodayVcDatabase();
                scheduleVcDatabase = new ScheduleVcDatabase();
                importantVcDatabase = new ImportantVcDatabase();
                searchByVcIdDatabase = new SearchByVcIdDatabase();
                alertableDatabase = new AlertableDatabase();

                saveUserPreferencesDatabase.DeleteSaveUserPreferences();
                userlogindatabase.DeleteUserLogin();
                todayVcDatabase.DeleteTodayVc();
                scheduleVcDatabase.DeleteScheduleVc();
                importantVcDatabase.DeleteImportantVc();
                searchByVcIdDatabase.DeleteSearchByVcId();
                alertableDatabase.DeleteAlertable();
                dashboardDatabase.DeleteDashboard();
                latLongDatabase.DeleteLatLong();
                currentMonthVcDatabase.DeleteCurrentMonthVc();
                yearWiseVcDatabase.DeleteYearWiseVc();

                App.SavedUserPreferList = null;
                await Auth_Revoke();
                
                // Reset to login page
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    Application.Current.MainPage = new NavigationPage(new ParichayPage("Logout"));
                }
                else
                {
                    Application.Current.MainPage = new NavigationPage(new ParichayPage("Logout"))
                    {
                        BarBackgroundColor = Color.FromArgb("#2196f3"),
                        BarTextColor = Colors.WhiteSmoke
                    };
                }
            }

        }
        //revoke
        public async Task<int> Auth_Revoke()
        {
            string _tokenFromParichay = Microsoft.Maui.Storage.Preferences.Default.Get("access_token", "");
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                try
                {
                    var client = new HttpClient();
                    
                    client.DefaultRequestHeaders.Add("Authorization", _tokenFromParichay);
                    //awaited for API Response
                    HttpResponseMessage response = await client.GetAsync(App.Auth_revoke);
                    //API Response Received
                    var result = await response.Content.ReadAsStringAsync();
                    //await App.Current.MainPage.DisplayAlert("Logout", result, "OK");
                    UserLoginDatabase userlogindatabase = new UserLoginDatabase();
                    userlogindatabase.DeleteUserLogin();
                    return (int)response.StatusCode;

                }
                catch (Exception)
                {
                    await App.Current.MainPage.DisplayAlert("Exception", "Something went wrong. Please try again!", "OK");
                    return 500;
                }
            }
            else
            {
                //await App.Current.MainPage.DisplayAlert(App.AppName, App.NoInternet_, App.Btn_OK_);
                return 101;
            }
        }

        void Btn_CloseThisHelp_Clicked(System.Object sender, System.EventArgs e)
        {
            Stack_Carousel.IsVisible = false;
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                Application.Current.MainPage = new NavigationPage(new ParichayPage("Logout"));
                return;
            }
            Application.Current.MainPage = new NavigationPage(new ParichayPage("Logout"))
            {
                BarBackgroundColor = Color.FromArgb("#2196f3"),
                BarTextColor = Colors.WhiteSmoke
            };
        }
    }
}
