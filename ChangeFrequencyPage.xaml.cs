using System;
using System.Collections.Generic;
using System.Linq;


using NICVC.Model;
using System.Globalization;

namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChangeFrequencyPage : ContentPage
    {
        AlertableDatabase alertableDatabase;
        List<Alertable> alertablelist;
        string vcid;
        NotificationDaysDatabase notificationDaysDatabase;
        List<NotificationDays> notificationDaylist;
        List<SaveUserPreferences> SavedUserPreferList;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        string districtname, studioname;
        DateTime oldnotifydate, newnotificationdate;


        public ChangeFrequencyPage(string vcidfrompage)
        {
            InitializeComponent();
            vcid = vcidfrompage;
            saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
            notificationDaysDatabase = new NotificationDaysDatabase();
            alertableDatabase = new AlertableDatabase();

            lbl_user_header.Text = App.GetLabelByKey("changenotification") + vcid;
            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");
            lbl_selectday.Text = App.GetLabelByKey("alertdays");
            btn_cancel.Text = "❌ " + App.GetLabelByKey("cancel");
            btn_update.Text = "🔄 " + App.GetLabelByKey("update");


            string query = $"Select cast( (julianday(DateofVC)- julianday('now') ) as int ) as NumberOfdays from Alertable where VC_ID='{vcid}'";
            alertablelist = alertableDatabase.GetAlertable(query).ToList();
            int diff = int.Parse(alertablelist.ElementAt(0).NumberOfdays);
            notificationDaysDatabase.DeleteNotificationDays();
            for (int i = 1; i <= diff; i++)
            {
                var item = new NotificationDays();
                item.DaysId = i.ToString();
                item.NumberofDays = i.ToString();
                //Console.WriteLine("day=====" + i);
                notificationDaysDatabase.AddNotificationDays(item);
            }

            notificationDaylist = notificationDaysDatabase.GetNotificationDays("Select * from NotificationDays").ToList();

            picker_selectdays.ItemsSource = notificationDaylist;
            picker_selectdays.Title = App.GetLabelByKey("selectdays");
            picker_selectdays.ItemDisplayBinding = new Binding("NumberofDays");
            picker_selectdays.SelectedIndex = 0;


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
        }


        private void picker_selectdays_SelectedIndexChanged(object sender, EventArgs e)
        {
            NotificationDays selectedItem = picker_selectdays.SelectedItem as NotificationDays;

            if (picker_selectdays.SelectedIndex == -1)
            {
                return;
            }
            if (selectedItem != null)
            {
                int Daysid = int.Parse(selectedItem.DaysId);
                string query = $"Select * from Alertable where VC_ID='{vcid}'";
                alertablelist = alertableDatabase.GetAlertable(query).ToList();

                var vcdate = alertablelist.ElementAt(0).DateofVC;
                try
                {
                    oldnotifydate = DateTime.ParseExact(vcdate, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                }
                catch
                {
                    oldnotifydate = DateTime.ParseExact(vcdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                newnotificationdate = oldnotifydate.AddDays(-Daysid);
            }
        }

        private void btn_cancel_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private async void btn_update_Clicked(object sender, EventArgs e)
        {
            alertableDatabase.UpdateCustomAlertable(vcid, newnotificationdate.ToString("yyyy-MM-dd"));
            await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("notifydays"), App.GetLabelByKey("close"));
            await Navigation.PopAsync();
        }
        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}