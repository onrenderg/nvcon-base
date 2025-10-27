using Newtonsoft.Json.Linq;
using NICVC.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;




namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TodayVcPage : ContentPage
    {
        TodayVcDatabase todayVcDatabase;
        List<TodayVc> todayvclist;
        List<SaveUserPreferences> SavedUserPreferList;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        string districtname, studioname, dateofvc;


        public TodayVcPage()
        {
            InitializeComponent();
            todayVcDatabase = new TodayVcDatabase();
            todayvclist = new List<TodayVc>();
            dateofvc = DateTime.Now.ToString("dd-MM-yyyy");

            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");
            lbl_user_header.Text = App.GetLabelByKey("todayvc") + " - " + DateTime.Now.ToString("dd-MM-yyyy");

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

            lbl_vcstatus.Text = App.GetLabelByKey("vcstatus");
            lbl_startingtime.Text = App.GetLabelByKey("vcstarttime");

            picker_vcstatus.Title = App.GetLabelByKey("vcstatus");
            var vcstatuslisteng = new List<string>();
            vcstatuslisteng.Add("All");
            vcstatuslisteng.Add("Completed");
            vcstatuslisteng.Add("Ongoing");
            vcstatuslisteng.Add("Upcoming");
            picker_vcstatus.ItemsSource = vcstatuslisteng;

            listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from TodayVC where Startingtime !='All' and VCStatus = 'Confirmed' group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
            picker_vcstatus.SelectedIndex = 0;

            todayvclist = todayVcDatabase.GetTodayVc("select distinct Startingtime from todayVc order by time(startingtime)").ToList();
            picker_sort.ItemsSource = todayvclist;
            picker_sort.Title = App.GetLabelByKey("selstarttime");
            picker_sort.ItemDisplayBinding = new Binding("Startingtime");
            picker_sort.SelectedIndex = 0;
        }

        private void listView_todayvc_ItemTapped(object sender, EventArgs e)
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
                App.vcstatustodayvc = picker_vcstatus.SelectedItem.ToString();
                Navigation.PushAsync(new TodayVcDetailsPage());
            }
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }

        private void picker_vcstatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            var vcstatus = picker_vcstatus.SelectedItem;

            string currenttime;
            todayVcDatabase = new TodayVcDatabase();
            todayvclist = new List<TodayVc>();
            currenttime = string.Format("{0:HH:mm:ss}", DateTime.Now);
            listView_todayvc.SelectedItem = null;

            if (vcstatus.Equals("All"))
            {

                listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc  from TodayVC " +
                    $" where  VCStatus = 'Confirmed'  " +
                     $" and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  and Startingtime !='All' " +
                    $" group by TIME(Startingtime) order by TIME(Startingtime) DESC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                todayvclist = todayVcDatabase.GetTodayVc($"select distinct Startingtime from todayVc where VCStatus = 'Confirmed' union select 'All' as Startingtime order by Startingtime DESC").ToList();

            }
            else if (vcstatus.Equals("Completed"))
            {
                listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from TodayVC " +
                    $" where VCStatus = 'Confirmed'  " +
                    $" and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  " +
                    $"and TIME(VCEndTime) <= TIME('{currenttime}') and Startingtime !='All' group by Startingtime order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                todayvclist = todayVcDatabase.GetTodayVc($"select distinct Startingtime from todayVc where VCStatus = 'Confirmed'  and TIME(VCEndTime) <= TIME('{currenttime}') union select 'All' as Startingtime order by Startingtime DESC").ToList();


            }
            else if (vcstatus.Equals("Ongoing"))
            {
                listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from TodayVC " +
                    $"where VCStatus = 'Confirmed' " +
                     $" and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  " +
                    $"and TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) and Startingtime !='All' group by (Startingtime) order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                todayvclist = todayVcDatabase.GetTodayVc($"select distinct Startingtime from todayVc where VCStatus = 'Confirmed' and TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) union select 'All' as Startingtime order by Startingtime DESC").ToList();

            }
            else if (vcstatus.Equals("Upcoming"))
            {
                listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from TodayVC " +
                    $"where VCStatus = 'Confirmed'  " +
                     $" and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  " +
                    $" and TIME(Startingtime) >= TIME('{currenttime}')  and Startingtime !='All' group by TIME(Startingtime) order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                todayvclist = todayVcDatabase.GetTodayVc($"select distinct Startingtime from todayVc where VCStatus = 'Confirmed' and TIME(Startingtime) >= TIME('{currenttime}') union select 'All' as Startingtime order by Startingtime DESC").ToList();

            }

            picker_sort.ItemsSource = todayvclist;
            picker_sort.Title = "Select Starting Time";
            picker_sort.ItemDisplayBinding = new Binding("Startingtime");
            picker_sort.SelectedIndex = 0;

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
                string vcstatus = picker_vcstatus.SelectedItem.ToString();
                var currenttime = string.Format("{0:HH:mm:ss}", DateTime.Now);

                if (vcstatus.Contains("All"))
                {
                    if (starttime.Equals("All"))
                    {
                        listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc  from TodayVC where VCStatus = 'Confirmed' and Startingtime !='All' group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                    else
                    {
                        listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from TodayVC where VCStatus = 'Confirmed' and time(Startingtime) =time('{starttime}') and Startingtime !='All' group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                }
                else if (vcstatus.Contains("Completed"))
                {
                    if (starttime.Equals("All"))
                    {
                        // listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVc($"SELECT  startingtime, count(*) as NoOfVc from TodayVC  where and Startingtime !='All'group by Startingtime");
                        listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from TodayVC where VCStatus = 'Confirmed'  and TIME(VCEndTime) <= TIME('{currenttime}') and Startingtime !='All' group by Startingtime order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                    else
                    {
                        listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from TodayVC where VCStatus = 'Confirmed'  and TIME(VCEndTime) <= TIME('{currenttime}') and time(Startingtime) =time('{starttime}')  group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                }
                else if (vcstatus.Contains("Ongoing"))
                {
                    if (starttime.Equals("All"))
                    {
                        listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from TodayVC where VCStatus = 'Confirmed' and TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) and Startingtime !='All' group by Startingtime order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                    else
                    {
                        listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from TodayVC where  VCStatus = 'Confirmed' and TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) and time(Startingtime) =time('{starttime}')  group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                }
                else if (vcstatus.Contains("Upcoming"))
                {
                    if (starttime.Equals("All"))
                    {
                        listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from TodayVC where VCStatus = 'Confirmed' and TIME(Startingtime) >= TIME('{currenttime}')  and Startingtime !='All' group by Startingtime order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                    else
                    {
                        listView_todayvc.ItemsSource = todayVcDatabase.GetTodayVcByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from TodayVC where VCStatus = 'Confirmed' and TIME(Startingtime) >= TIME('{currenttime}') and time(Startingtime) =time('{starttime}') group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                }
            }
        }
    }
}