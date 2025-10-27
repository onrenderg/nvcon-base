using System;
using System.Collections.Generic;
using System.Linq;



using NICVC.Model;

namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrentMonthVcDetailsPage : ContentPage
    {        
        CurrentMonthVcDatabase currentMonthVcDatabase;
        List<CurrentMonthVc> currentMonthVclist;
        List<CurrentMonthVc> itemsource;

        string dateofvc = App.vcdateschedulevc;
      
        List<SaveUserPreferences> SavedUserPreferList;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        string districtname, studioname;
        string starttime;
        string query;

        public CurrentMonthVcDetailsPage()
        {
            InitializeComponent();
            currentMonthVcDatabase = new CurrentMonthVcDatabase();
            
            lbl_startingtime.Text = App.GetLabelByKey("vcstarttime");
            lbl_user_header.Text = App.GetLabelByKey("schvcdtls") + " - " + dateofvc;
            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");
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

            searchbar_schedulevcdetails.Placeholder = App.GetLabelByKey("search");
            lbl_search.Text = App.GetLabelByKey("searchtext");
            if (App.Language == 0)
            {              
                listView_schedulevcdetails.IsVisible = true;
                listView_schedulevcdetailslocal.IsVisible = false;
                listView_schedulevcdetails.ItemsSource = loadscheduleVcdetails(null);
            }
            else
            {                
                listView_schedulevcdetails.IsVisible = false;
                listView_schedulevcdetailslocal.IsVisible = true;
                listView_schedulevcdetailslocal.ItemsSource = loadscheduleVcdetails(null);
            }

            currentMonthVclist = currentMonthVcDatabase.GetCurrentMonthVc($"select distinct Startingtime from CurrentMonthVc where VCStatus = 'Confirmed'  " +
                $"and substr(dateofvc,7)||substr(dateofvc,4,2)||substr(dateofvc,1,2)   = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2) or (Startingtime='All') order by Time(Startingtime) ASC").ToList();
            picker_sort.ItemsSource = currentMonthVclist;
            picker_sort.Title = App.GetLabelByKey("selstarttime");
            picker_sort.ItemDisplayBinding = new Binding("Startingtime");
            picker_sort.SelectedIndex = 0;


        }
        public List<CurrentMonthVc> loadscheduleVcdetails(string searched_text)
        {      
            string searchedQueryAdd = $"VC_ID like '%{searched_text}%' or Startingtime like '%{searched_text}%' or VCEndTime like '%{searched_text}%' or Important like '%{searched_text}%' or " +
                $"LevelName like '%{searched_text}%' or Dept_Name like '%{searched_text}%' or Org_Name like '%{searched_text}%' or Purpose like '%{searched_text}%' or RequestedBy like '%{searched_text}%' or Studio_Name like '%{searched_text}%' or " +
                $"VCStatus like '%{searched_text}%' or VcCoordEmail like '%{searched_text}%' or VcCoordIpPhone like '%{searched_text}%' or VcCoordMobile like '%{searched_text}%' or VcCoordName like '%{searched_text}%' or " +
                $"VccoordLandline like '%{searched_text}%' or hoststudio like '%{searched_text}%' or mcuip like '%{searched_text}%' or mcuname like '%{searched_text}%' or participantsExt like '%{searched_text}%' or webroom like '%{searched_text}%'";
            if (string.IsNullOrEmpty(starttime) || starttime.Equals("All"))
            {
                query = $"SELECT *  from CurrentMonthVc where Startingtime !='All' and VCStatus = 'Confirmed'  and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2) and ({searchedQueryAdd})   order by substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) ASC";
            }
            else
            {
                query = $"SELECT *  from CurrentMonthVc where Startingtime !='All' and VCStatus = 'Confirmed'  and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2) and  time(Startingtime)=time('{starttime}') and ({searchedQueryAdd})  order by substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) ASC";
            }

            itemsource = currentMonthVcDatabase.GetCurrentMonthVc(query).ToList();
            return itemsource;
        }


        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            try
            {
                Label lbl_mobile = (Label)sender;
                string txt = lbl_mobile.Text;
                PhoneDialer.Open(txt);
            }
            catch (Exception ey)
            {
               await DisplayAlert(App.GetLabelByKey("NICVC"), ey.Message.ToString(), App.GetLabelByKey("close"));
            }
        }


        private void searchbar_schedulevcdetails_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (App.Language == 0)
            {
                listView_schedulevcdetails.IsVisible = true;
                listView_schedulevcdetailslocal.IsVisible = false;
                listView_schedulevcdetails.ItemsSource = loadscheduleVcdetails(searchbar_schedulevcdetails.Text);
            }
            else
            {
                listView_schedulevcdetails.IsVisible = false;
                listView_schedulevcdetailslocal.IsVisible = true;
                listView_schedulevcdetailslocal.ItemsSource = loadscheduleVcdetails(searchbar_schedulevcdetails.Text);
            }
        }

        private void picker_sort_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentMonthVc selectedItem = picker_sort.SelectedItem as CurrentMonthVc;
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
                    listView_schedulevcdetails.ItemsSource = loadscheduleVcdetails(searchbar_schedulevcdetails.Text);
                }
                else
                {

                    listView_schedulevcdetails.IsVisible = false;
                    listView_schedulevcdetailslocal.IsVisible = true;
                    listView_schedulevcdetailslocal.ItemsSource = loadscheduleVcdetails(searchbar_schedulevcdetails.Text);
                }
                searchbar_schedulevcdetails.Text = "";
            }
        }
        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}