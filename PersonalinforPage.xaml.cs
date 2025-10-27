using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using NICVC.Model;


namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PersonalinforPage : ContentPage
    {
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        List<SaveUserPreferences> SavedUserPreferList;
        string districtname, studioname;

        PersonalInfoDatabase personalInfoDatabase;
        List<PersonalInfo> personalInfolist;
        string mobile, name, email, department, designation;
        public PersonalinforPage()
        {
            InitializeComponent();
            saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
            personalInfoDatabase = new PersonalInfoDatabase();
            
            // Set tab properties for when used as Feedback tab
            Title = "Feedback";
            IconImageSource = "ic_feedback1";
            
            // Hide the back button
            NavigationPage.SetHasBackButton(this, false);
        }

        protected override void OnAppearing()
        {
            // Set the header title
            Lbl_Header1.Text = App.GetLabelByKey("nicvdconf");
            Lbl_Header.Text = App.GetLabelByKey("personalinfo");

            lbl_mobile.Text = App.GetLabelByKey("mobileno");
            lbl_Name.Text = App.GetLabelByKey("name");
            lbl_department.Text = App.GetLabelByKey("department1");
            lbl_designation.Text = App.GetLabelByKey("designation");
            lbl_email.Text = App.GetLabelByKey("email");
            btn_cancel.Text = App.GetLabelByKey("cancel");
            btn_save.Text = App.GetLabelByKey("save");
            btn_update.Text = App.GetLabelByKey("update");

            SavedUserPreferList = saveUserPreferencesDatabase.GetSaveUserPreferences("select * from saveUserPreferences").ToList();
            string statename = SavedUserPreferList.ElementAt(0).StateName.ToString();

            try
            {
                districtname = SavedUserPreferList.ElementAt(0).DistrictName.ToString();

            }
            catch
            { districtname = ""; }
            try
            {
                studioname = SavedUserPreferList.ElementAt(0).StudioName.ToString();
            }
            catch
            { studioname = ""; }

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
            entry_mobile.Text = Preferences.Get("MobileNo", "");
            entry_name.Text = Preferences.Get("Full_Name", "");
            entry_email.Text = Preferences.Get("Email", "");

            entry_mobile.IsReadOnly = true;
            entry_name.IsReadOnly = true;
            entry_email.IsReadOnly = true;

            loadpersonalinfo();

        }
        void loadpersonalinfo()
        {
            personalInfolist = personalInfoDatabase.GetPersonalInfo("Select * from PersonalInfo ").ToList();

            if (personalInfolist.Any())
            {
                department = personalInfolist.ElementAt(0).Department;
                designation = personalInfolist.ElementAt(0).Designation;


                entry_department.Text = department;
                entry_designation.Text = designation;

                if (string.IsNullOrEmpty(designation))
                {
                    btn_save.IsVisible = true;
                    btn_update.IsVisible = false;
                }
                else
                {
                    btn_update.IsVisible = true;
                    btn_save.IsVisible = false;
                }
            }
            else
            {
                btn_save.IsVisible = true;
                btn_update.IsVisible = false;
            }
        }

        private async void btn_save_Clicked(object sender, EventArgs e)
        {

            if (await checkvalidation())
            {
                mobile = entry_mobile.Text;
                name = entry_name.Text;
                department = entry_department.Text;
                designation = entry_designation.Text;
                email = entry_email.Text;

                var item = new PersonalInfo();
                item.Mobile = mobile.ToString().Trim();
                item.Name = name.ToString().Trim();
                item.Department = department.ToString().Trim();
                item.Designation = designation.ToString().Trim();
                item.Email = email.ToString().Trim();

                personalInfoDatabase.AddPersonalInfo(item);
                
                // Also create default user preferences to avoid "incomplete input" errors
                SaveUserPreferencesDatabase saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
                var userPref = new SaveUserPreferences();
                userPref.StateID = "1";
                userPref.StateName = "Himachal Pradesh";
                userPref.DistrictID = "1";
                userPref.DistrictName = "Shimla";
                userPref.StudioID = "1";
                userPref.StudioName = "All Studios";
                userPref.StateChanged = "0";
                userPref.language = 0;
                saveUserPreferencesDatabase.AddSaveUserPreferences(userPref);
                
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("profilecreated"), App.GetLabelByKey("close"));
                
                // Recreate the TabbedPage to rebuild tabs with FeedbackPage
                App.CurrentTabpageIndex = 2; // Set to Feedback tab (index 2)
                
                // Recreate MainPage with proper NavigationPage styling (matching App.xaml.cs)
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

        private async void btn_update_Clicked(object sender, EventArgs e)
        {
            string query = $"update PersonalInfo set Mobile='{mobile}', Name='{name}',Department='{department}',Designation='{designation}',Email='{email}' ";
            personalInfoDatabase.customquery(query);
            await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("profileupdated"), App.GetLabelByKey("close"));
            // Application.Current.MainPage = new NavigationPage(new NICVCTabbedPage());
            await Navigation.PopAsync();

        }

        private async void btn_cancel_Clicked(object sender, EventArgs e)
        {
            // Check if this page is in a navigation stack
            if (Navigation != null && Navigation.NavigationStack.Count > 1)
            {
                // Page was navigated to, so pop it
                await Navigation.PopAsync();
            }
            else
            {
                // Page is part of a TabbedPage, switch to dashboard tab
                if (Parent is TabbedPage tabbedPage)
                {
                    tabbedPage.CurrentPage = tabbedPage.Children[0]; // Switch to dashboard (index 0)
                }
            }
        }

        async Task<bool> checkvalidation()
        {
            if (string.IsNullOrEmpty(entry_mobile.Text))
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("entmobileno"), App.GetLabelByKey("close"));
                return false;
            }

            if (entry_mobile.Text.Length < 10)
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("tendigitmobile"), App.GetLabelByKey("close"));
                return false;
            }

            if (!App.isNumeric(entry_mobile.Text))
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("numeric") + " " + App.GetLabelByKey("Mobile"), App.GetLabelByKey("close"));
                return false;
            }

            if (string.IsNullOrEmpty(entry_name.Text))
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("entname"), App.GetLabelByKey("close"));
                return false;
            }

            if (entry_name.Text.Length == 0)
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("correct_name"), App.GetLabelByKey("close"));
                return false;
            }

            if (!App.isAlphabetonly(entry_name.Text))
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("onlyalphabets") + " " + App.GetLabelByKey("name"), App.GetLabelByKey("close"));
                return false;
            }

            if (string.IsNullOrEmpty(entry_department.Text))
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("entdepartment1"), App.GetLabelByKey("close"));
                return false;
            }

            if (entry_department.Text.Length == 0)
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("correct_department"), App.GetLabelByKey("close"));
                return false;
            }

            if (!App.isAlphabetonly(entry_department.Text))
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("onlyalphabets") + " " + App.GetLabelByKey("department1"), App.GetLabelByKey("close"));
                return false;
            }


            if (string.IsNullOrEmpty(entry_designation.Text))
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("entdesignation"), App.GetLabelByKey("close"));
                return false;
            }

            if (entry_designation.Text.Length == 0)
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("correct_designation"), App.GetLabelByKey("close"));
                return false;
            }

            if (!App.isAlphabetonly(entry_designation.Text))
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("onlyalphabets") + " " + App.GetLabelByKey("designation"), App.GetLabelByKey("close"));
                return false;
            }

            if (string.IsNullOrEmpty(entry_email.Text))
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("entemail"), App.GetLabelByKey("close"));
                return false;
            }

            if (entry_email.Text.Length == 0)
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("correct_designation"), App.GetLabelByKey("close"));
                return false;
            }

            /*    if (!App.isemail(entry_email.Text))
                {
                    await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("correct_email"), App.GetLabelByKey("close"));
                    return false;
                }*/


            return true;
        }
    }
}