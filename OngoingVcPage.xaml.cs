using NICVC.Model;
using System;
using System.Collections.Generic;
using System.Linq;




namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OngoingVcPage : ContentPage
    {
        TodayVcDatabase todayVcDatabase;
        List<TodayVc> todayvclist;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        List<SaveUserPreferences> SavedUserPreferList;
        string districtname, studioname;
        string currenttime, dateofvc;

        public OngoingVcPage()
        {
            InitializeComponent();
            todayVcDatabase = new TodayVcDatabase();

            dateofvc = DateTime.Now.ToString("dd-MM-yyyy");
            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");      
            lbl_user_header.Text = App.GetLabelByKey("Ongoing") +" - " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            lbl_startingtime.Text = App.GetLabelByKey("selstarttime");

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
                      
          
            listView_ongoingvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from TodayVC " +
                $"where VCStatus = 'Confirmed' and TIME('{DateTime.Now.ToString("HH:mm:ss")}') between TIME(Startingtime) and TIME(VCEndTime) and Startingtime !='All' group by (Startingtime) order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });

            currenttime = string.Format("{0:HH:mm:ss}", DateTime.Now);
            todayvclist = todayVcDatabase.GetTodayVc($"select distinct Startingtime from todayVc where VCStatus = 'Confirmed' and TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) union select 'All' as Startingtime order by Startingtime DESC").ToList();
            picker_sort.ItemsSource = todayvclist;
            picker_sort.Title = App.GetLabelByKey("selstarttime");
            picker_sort.ItemDisplayBinding = new Binding("Startingtime");
            picker_sort.SelectedIndex = 0;

        }

        private void listView_ongoingvc_ItemTapped(object sender, EventArgs e)
        {
            var tappedEventArgs = e as TappedEventArgs;
            var currentRecord = tappedEventArgs?.Parameter as TodayVc;
            if (currentRecord == null)
            {
                // For CollectionView, get the binding context from the sender
                var grid = sender as Microsoft.Maui.Controls.Grid;
                currentRecord = grid?.BindingContext as TodayVc;
            }
            
            if (currentRecord != null)
            {
                App.starttimetodayvc = currentRecord.Startingtime.ToString();
                App.vcstatustodayvc = "Ongoing";
                Navigation.PushAsync(new TodayVcDetailsPage());
            }
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }


        private void picker_sort_SelectedIndexChanged(object sender, EventArgs e)
        {
            TodayVc selectedItem = picker_sort.SelectedItem as TodayVc;

            if (picker_sort.SelectedIndex == -1)
            {
                return;
            }
            if (selectedItem != null)
            {
                string starttime = selectedItem.Startingtime;

                if (starttime.Equals("All"))
                {
                    listView_ongoingvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from TodayVC" +
                        $" where VCStatus = 'Confirmed' " +
                        $" and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  " +
                        $"and TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) " +
                        $"and Startingtime !='All' group by Startingtime order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                }
                else
                {
                    listView_ongoingvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from TodayVC " +
                        $"where  VCStatus = 'Confirmed' " +
                        $" and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  " +
                        $"and TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) and time(Startingtime) =time('{starttime}')  " +
                        $"group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                }
            }
        }
    }
}