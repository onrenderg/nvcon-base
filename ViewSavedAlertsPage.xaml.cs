using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using NICVC.Model;

namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSavedAlertsPage : ContentPage
    {
        List<SaveUserPreferences> SavedUserPreferList;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        string districtname, studioname;
        AlertableDatabase alertableDatabase;
        List<Alertable> alertablelist;
        public ViewSavedAlertsPage()
        {
            InitializeComponent();
            saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
            alertableDatabase = new AlertableDatabase();

            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");
            lbl_user_header.Text = App.GetLabelByKey("viewalert");
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

            //alertableDatabase.UpdateCustomqueryAlertable("update Alertable set DateofVCtoshowonly = 'All'");
            
            lbl_selectdate.Text = App.GetLabelByKey("selectdate");
            listView_alertvc.ItemsSource = alertableDatabase.GetAlertableByParameter($"SELECT (? || '\n\n' || ?) as  labelname, DateofVCtoshowonly,DateofVC, (DateofVCtoshowonly||'\n\n'||count(*)) NoOfVc from Alertable where VCStatus = 'Confirmed' and Startingtime <>'All' group by DateofVC order by substr(dateofvc,7)||substr(dateofvc,4,2)||substr(dateofvc,1,2)", new string[2] { App.GetLabelByKey("vcdate"), App.GetLabelByKey("noofvc") });
            //listView_alertvc.ItemsSource = alertableDatabase.GetAlertable($"SELECT DateofVCtoshowonly,DateofVC, count(*) as NoOfVc from Alertable where VCStatus = 'Confirmed' and Startingtime <> 'All' group by DateofVC order by substr(dateofvc,7)||substr(dateofvc,4,2)||substr(dateofvc,1,2)");
            //listView_alertvc.ItemsSource = alertableDatabase.GetAlertableByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from Alertable where Startingtime !='All' and VCStatus = 'Confirmed' group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
            alertablelist = alertableDatabase.GetAlertable($"Select distinct DateofVCtoshowonly from Alertable  ").ToList();

            picker_selectdate.ItemsSource = alertablelist;
            picker_selectdate.Title = App.GetLabelByKey("selectdate");
            picker_selectdate.ItemDisplayBinding = new Binding("DateofVCtoshowonly");
            picker_selectdate.SelectedIndex = 0;



        }
        private void picker_sort_SelectedIndexChanged(object sender, EventArgs e)
        {
            Alertable selectedItem = picker_selectdate.SelectedItem as Alertable;

            if (picker_selectdate.SelectedIndex == -1)
            {
                return;
            }
            if (selectedItem != null)
            {
                string startdate = selectedItem.DateofVCtoshowonly;
               
                var currenttime = string.Format("{0:HH:mm:ss}", DateTime.Now);

                /*
                    if (startdate.Equals("All"))
                    {
                        listView_alertvc.ItemsSource = alertableDatabase.GetAlertable($"SELECT DateofVCtoshowonly,DateofVC, count(*) as NoOfVc from Alertable where VCStatus = 'Confirmed' and Startingtime <> 'All' group by DateofVCtoshowonly order by substr(DateofVCtoshowonly,7)||substr(DateofVCtoshowonly,4,2)||substr(DateofVCtoshowonly,1,2)");
                    }
                    else
                    {
                        listView_alertvc.ItemsSource = alertableDatabase.GetAlertable($"SELECT DateofVCtoshowonly, DateofVC, count(*) as NoOfVc from Alertable where VCStatus = 'Confirmed'  and substr(DateofVCtoshowonly,7)||substr(DateofVCtoshowonly,4,2)||substr(DateofVCtoshowonly,1,2)   = substr('{startdate}', 7) || substr('{startdate}', 4, 2) || substr('{startdate}', 1, 2) group by DateofVCtoshowonly order by substr(DateofVCtoshowonly,7)||substr(DateofVCtoshowonly,4,2)||substr(DateofVCtoshowonly,1,2)");
                    }*/

                if (startdate.Equals("All"))
                {
                    listView_alertvc.ItemsSource = alertableDatabase.GetAlertableByParameter($"SELECT (? || '\n\n' || ?) as labelname,  DateofVCtoshowonly,DateofVC, (DateofVCtoshowonly||'\n\n'||count(*)) NoOfVc  from Alertable where VCStatus = 'Confirmed' and Startingtime <> 'All' group by DateofVCtoshowonly order by substr(DateofVCtoshowonly,7)||substr(DateofVCtoshowonly,4,2)||substr(DateofVCtoshowonly,1,2)", new string[2] { App.GetLabelByKey("vcdate"), App.GetLabelByKey("noofvc") });
                }
                else
                {
                    listView_alertvc.ItemsSource = alertableDatabase.GetAlertableByParameter($"SELECT (? || '\n\n' || ?) as labelname,  DateofVCtoshowonly, DateofVC, (DateofVCtoshowonly||'\n\n'||count(*)) NoOfVc from Alertable where VCStatus = 'Confirmed' and substr(DateofVCtoshowonly,7)||substr(DateofVCtoshowonly,4,2)||substr(DateofVCtoshowonly,1,2)   = substr('{startdate}', 7) || substr('{startdate}', 4, 2) || substr('{startdate}', 1, 2) and Startingtime !='All' group by DateofVCtoshowonly order by substr(DateofVCtoshowonly,7)||substr(DateofVCtoshowonly,4,2)||substr(DateofVCtoshowonly,1,2)", new string[2] { App.GetLabelByKey("vcdate"), App.GetLabelByKey("noofvc") });
                }

            }
        }

        private void listView_alertvc_ItemTapped(object sender, EventArgs e)
        {
            var tappedEventArgs = e as TappedEventArgs;
            var currentRecord = tappedEventArgs?.Parameter as Alertable;
            if (currentRecord == null)
            {
                // For CollectionView, get the binding context from the sender
                var grid = sender as Microsoft.Maui.Controls.Grid;
                currentRecord = grid?.BindingContext as Alertable;
            }
            
            if (currentRecord != null)
            {
                string dateofvc = currentRecord.DateofVC.ToString();           
                Navigation.PushAsync(new ViewSavedAlertDetailsPage(dateofvc));
            }
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}
