using System;
using System.Collections.Generic;
using System.Linq;



using NICVC.Model;

using System.Globalization;

namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSavedAlertDetailsPage : ContentPage
    {
        string vcdate, starttime;
        string districtname, studioname;
        string query;
        List<Alertable> itemsource;
        List<SaveUserPreferences> SavedUserPreferList;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        AlertableDatabase alertableDatabase;
        List<Alertable> alertablelist;
        public ViewSavedAlertDetailsPage(string dateofvc)
        {
            InitializeComponent();
            vcdate = dateofvc;
            alertableDatabase = new AlertableDatabase();
            saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();

            lbl_startingtime.Text = App.GetLabelByKey("vcstarttime");
            DateTime vcdatetoshow;
            try
            {
                vcdatetoshow = DateTime.ParseExact(vcdate, "yyyy/MM/dd", CultureInfo.InvariantCulture);
            }
            catch
            {
                vcdatetoshow = DateTime.ParseExact(vcdate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            lbl_user_header.Text = App.GetLabelByKey("viewalertvcdtls") + " - " + vcdatetoshow.ToString("dd-MM-yyyy");
            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");

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


            searchbar_schedulevcdetails.Placeholder = App.GetLabelByKey("search");
            lbl_search.Text = App.GetLabelByKey("searchtext");
            if (App.Language == 0)
            {

                listView_schedulevcdetails.IsVisible = true;
                listView_schedulevcdetailslocal.IsVisible = false;
                listView_schedulevcdetails.ItemsSource = loadalertVcdetails(null);
            }
            else
            {

                listView_schedulevcdetails.IsVisible = false;
                listView_schedulevcdetailslocal.IsVisible = true;
                listView_schedulevcdetailslocal.ItemsSource = loadalertVcdetails(null);
            }

            alertablelist = alertableDatabase.GetAlertable($"select distinct Startingtime from Alertable where VCStatus = 'Confirmed'  and substr(dateofvc,7)||substr(dateofvc,4,2)||substr(dateofvc,1,2)   = substr('{vcdate}', 7) || substr('{vcdate}', 4, 2) || substr('{vcdate}', 1, 2) or (Startingtime='All') order by Time(Startingtime) ASC").ToList();
            picker_sort.ItemsSource = alertablelist;
            picker_sort.Title = App.GetLabelByKey("selstarttime");
            picker_sort.ItemDisplayBinding = new Binding("Startingtime");
            picker_sort.SelectedIndex = 0;


        }

        private void picker_sort_SelectedIndexChanged(object sender, EventArgs e)
        {
            Alertable selectedItem = picker_sort.SelectedItem as Alertable;
            if (picker_sort.SelectedIndex == -1)
            {
                return;
            }
            if (selectedItem != null)
            {
                starttime = selectedItem.Startingtime;
                if (App.Language == 0)
                {
                    listView_schedulevcdetails.IsVisible = true;
                    listView_schedulevcdetailslocal.IsVisible = false;
                    listView_schedulevcdetails.ItemsSource = loadalertVcdetails(searchbar_schedulevcdetails.Text);
                }
                else
                {

                    listView_schedulevcdetails.IsVisible = false;
                    listView_schedulevcdetailslocal.IsVisible = true;
                    listView_schedulevcdetailslocal.ItemsSource = loadalertVcdetails(searchbar_schedulevcdetails.Text);
                }
                searchbar_schedulevcdetails.Text = "";
            }

        }

        public List<Alertable> loadalertVcdetails(string searched_text)
        {
            var currenttime = string.Format("{0:HH:mm:ss}", DateTime.Now);
            string buttonsvisibilitycases = $"(case when  TIME('{currenttime}') >= TIME(VCEndTime)  then 'false' else 'true' end ) takemetherevisibility " +

              $", (case when  date(DateofVC, '-2 days') >=  date('now') then 'true' else 'false' end ) frequencyvisiblity";

            /*  string searchedQueryAdd = $"VC_ID like '%{searched_text}%' or Startingtime like '%{searched_text}%' or VCEndTime like '%{searched_text}%' or Important like '%{searched_text}%' or " +
                  $"LevelName like '%{searched_text}%' or Dept_Name like '%{searched_text}%' or Org_Name like '%{searched_text}%' or Purpose like '%{searched_text}%' or RequestedBy like '%{searched_text}%' or Studio_Name like '%{searched_text}%' or " +
                  $"VCStatus like '%{searched_text}%' or VcCoordEmail like '%{searched_text}%' or VcCoordIpPhone like '%{searched_text}%' or VcCoordMobile like '%{searched_text}%' or VcCoordName like '%{searched_text}%' or " +
                  $"VccoordLandline like '%{searched_text}%' or hoststudio like '%{searched_text}%' or mcuip like '%{searched_text}%' or mcuname like '%{searched_text}%' or participantsExt like '%{searched_text}%' or webroom like '%{searched_text}%'";*/

            string[] columnsToSearch = new[]
                {
                    "VC_ID", "Startingtime", "VCEndTime", "Important", "LevelName",
                    "Dept_Name", "Org_Name", "Purpose", "RequestedBy", "Studio_Name",
                    "VCStatus", "VcCoordEmail", "VcCoordIpPhone", "VcCoordMobile", "VcCoordName",
                    "VccoordLandline", "hoststudio", "mcuip", "mcuname", "participantsExt", "webroom"
                };

            string searchCondition = string.Join(" or ", columnsToSearch.Select(column => $"{column} like '%{searched_text}%'"));

            string searchedQueryAdd = searchCondition;


            if (string.IsNullOrEmpty(starttime) || starttime.Equals("All"))
            {
                query = $"SELECT *, {buttonsvisibilitycases}  from Alertable where Startingtime !='All' and VCStatus = 'Confirmed'  and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{vcdate}', 7) || substr('{vcdate}', 4, 2) || substr('{vcdate}', 1, 2) and ({searchedQueryAdd})   order by substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) ASC";
            }
            else
            {
                query = $"SELECT *, {buttonsvisibilitycases}  from Alertable where Startingtime !='All' and VCStatus = 'Confirmed'  and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{vcdate}', 7) || substr('{vcdate}', 4, 2) || substr('{vcdate}', 1, 2) and  time(Startingtime)=time('{starttime}') and ({searchedQueryAdd})  order by substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) ASC";
            }


            itemsource = alertableDatabase.GetAlertable(query).ToList();
            return itemsource;

        }
        private void searchbar_schedulevcdetails_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (App.Language == 0)
            {
                listView_schedulevcdetails.IsVisible = true;
                listView_schedulevcdetailslocal.IsVisible = false;
                listView_schedulevcdetails.ItemsSource = loadalertVcdetails(searchbar_schedulevcdetails.Text);
            }
            else
            {

                listView_schedulevcdetails.IsVisible = false;
                listView_schedulevcdetailslocal.IsVisible = true;
                listView_schedulevcdetailslocal.ItemsSource = loadalertVcdetails(searchbar_schedulevcdetails.Text);
            }

        }


        private void img_map1_Clicked(object sender, EventArgs e)
        {
            ImageButton b = (ImageButton)sender;
            var vcid = b.CommandParameter.ToString();
            Navigation.PushAsync(new TakeMeTherePage(vcid, "Alerts"));
        }

        private async void img_delete_Clicked(object sender, EventArgs e)
        {
            ImageButton b = (ImageButton)sender;
            var vcid = b.CommandParameter.ToString();
            bool m = await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("deletevcid"), App.GetLabelByKey("delete"), App.GetLabelByKey("close"));
            if (m)
            {
                alertableDatabase.DeleteCustomAlertable(vcid);
                await Navigation.PopToRootAsync();
            }
        }

        private void img_frequency_Clicked(object sender, EventArgs e)
        {
            ImageButton b = (ImageButton)sender;
            var vcid = b.CommandParameter.ToString();
            Navigation.PushAsync(new ChangeFrequencyPage(vcid));
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            try
            {
                Label lbl_mobile = (Label)sender;
                string txt = lbl_mobile.Text;
                PhoneDialer.Open(txt);
            }
            catch (Exception ey)
            {
                DisplayAlert(App.GetLabelByKey("NICVC"), ey.Message.ToString(), App.GetLabelByKey("close"));
            }
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}
