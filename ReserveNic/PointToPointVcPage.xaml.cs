using System;
using System.Collections.Generic;
using System.Linq;


using NICVC.Model;

namespace NICVC.ReserveNic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PointToPointVcPage : ContentPage
    {
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        List<SaveUserPreferences> SavedUserPreferList;
        PointToPointDatabase pointToPointDatabase;
        List<PointToPoint> pointToPointlist;
        string dateofvc;
        public PointToPointVcPage(string selectedate)
        {
            InitializeComponent();
            dateofvc = selectedate;
            saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
            pointToPointDatabase = new PointToPointDatabase();

            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");
            lbl_user_header1.Text = App.GetLabelByKey("ptp") + " - " + dateofvc;
            SavedUserPreferList = saveUserPreferencesDatabase.GetSaveUserPreferences("select * from SaveUserPreferences").ToList();
            string stateid = SavedUserPreferList.ElementAt(0).StateName;
            string district = SavedUserPreferList.ElementAt(0).DistrictName;
            string studio = SavedUserPreferList.ElementAt(0).StudioName;

            if (studio != null && studio.Length > 0)
            {
                lbl_user_header.Text = stateid + " - " + district + " - " + studio;
            }
            else if (district != null && district.Length > 0)
            {
                lbl_user_header.Text = stateid + " - " + district;
            }
            else
            {
                lbl_user_header.Text = stateid;
            }

            lbl_vcstatus.Text = App.GetLabelByKey("vcstatus");
            lbl_startingtime.Text = App.GetLabelByKey("vcstarttime");


            var vcstatuslisteng = new List<string>();
            vcstatuslisteng.Add(App.GetLabelByKey("All"));
            vcstatuslisteng.Add(App.GetLabelByKey("Completed"));
            vcstatuslisteng.Add(App.GetLabelByKey("Ongoing"));
            vcstatuslisteng.Add(App.GetLabelByKey("upcoming"));


            picker_vcstatus.Title = App.GetLabelByKey("vcstatus");
            picker_vcstatus.ItemsSource = vcstatuslisteng;

            listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPointByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from PointToPoint where Startingtime !='All'  group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
            //listView_multipointvc.ItemsSource = pointToPointlist;

            picker_vcstatus.SelectedIndex = 0;

            pointToPointlist = pointToPointDatabase.GetPointToPoint("select distinct Startingtime from PointToPoint order by time(startingtime)").ToList();
            picker_sort.ItemsSource = pointToPointlist;
            picker_sort.Title = App.GetLabelByKey("selstarttime");
            picker_sort.ItemDisplayBinding = new Binding("Startingtime");
            picker_sort.SelectedIndex = 0;

        }

        private void listView_multipointvc_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var currentRecord = e.Item as PointToPoint;
           var starttimevc = currentRecord.Startingtime.ToString();
            var vcstatus = picker_vcstatus.SelectedItem.ToString();
            Navigation.PushAsync(new PointToPointVcDetailsPage(dateofvc,starttimevc, vcstatus));
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            App.CurrentTabpageIndex = 1;
            await Navigation.PopToRootAsync();
        }

        private void picker_vcstatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            var vcstatus = picker_vcstatus.SelectedItem;

            string currenttime;

            pointToPointlist = new List<PointToPoint>();
            currenttime = string.Format("{0:HH:mm:ss}", DateTime.Now);
            listView_multipointvc.SelectedItem = null;

             if (picker_vcstatus.SelectedIndex == 0)
            {

                listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPointByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc  from PointToPoint " +
                     // $" where  VCStatus = 'Confirmed' and  " +
                     $" where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  and Startingtime !='All' " +
                    $" group by TIME(Startingtime) order by TIME(Startingtime) DESC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                pointToPointlist = pointToPointDatabase.GetPointToPoint($"select distinct Startingtime from PointToPoint ").ToList();

            }
            else if (picker_vcstatus.SelectedIndex == 1)
            {
                listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPointByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from PointToPoint " +

                    $" where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  " +
                    $"and TIME(VCEndTime) <= TIME('{currenttime}') and Startingtime !='All' group by Startingtime order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                pointToPointlist = pointToPointDatabase.GetPointToPoint($"select distinct Startingtime from PointToPoint   where TIME(VCEndTime) <= TIME('{currenttime}') union select 'All' as Startingtime order by Startingtime DESC").ToList();


            }
            else if (picker_vcstatus.SelectedIndex == 2)
            {
                listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPointByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from PointToPoint " +

                     $" where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  " +
                    $"and TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) and Startingtime !='All' group by (Startingtime) order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                pointToPointlist = pointToPointDatabase.GetPointToPoint($"select distinct Startingtime from PointToPoint  where TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) union select 'All' as Startingtime order by Startingtime DESC").ToList();

            }
            else if (picker_vcstatus.SelectedIndex == 3)
            {
                listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPointByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from PointToPoint " +

                     $" where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  " +
                    $" and TIME(Startingtime) >= TIME('{currenttime}')  and Startingtime !='All' group by TIME(Startingtime) order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                pointToPointlist = pointToPointDatabase.GetPointToPoint($"select distinct Startingtime from PointToPoint  where TIME(Startingtime) >= TIME('{currenttime}') union select 'All' as Startingtime order by Startingtime DESC").ToList();

            }

            picker_sort.ItemsSource = pointToPointlist;
            picker_sort.Title = "Select Starting Time";
            picker_sort.ItemDisplayBinding = new Binding("Startingtime");
            picker_sort.SelectedIndex = 0;
        }

        private void picker_sort_SelectedIndexChanged(object sender, EventArgs e)
        {
            PointToPoint selectedItem = picker_sort.SelectedItem as PointToPoint;

            if (picker_sort.SelectedIndex == -1)
            {
                return;
            }
            if (selectedItem != null)
            {
                string starttime = selectedItem.Startingtime;
                string vcstatus = picker_vcstatus.SelectedItem.ToString();
                var currenttime = string.Format("{0:HH:mm:ss}", DateTime.Now);

                  if (picker_vcstatus.SelectedIndex == 0)
                {
                    if (starttime.Equals("All"))
                    {
                        listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPointByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc  from PointToPoint  where Startingtime !='All' group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                    else
                    {
                        listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPointByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from PointToPoint  where time(Startingtime) =time('{starttime}') and Startingtime !='All' group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                }
                else if (picker_vcstatus.SelectedIndex == 1)
                {
                    if (starttime.Equals("All"))
                    {
                        // listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPoint($"SELECT  startingtime, count(*) as NoOfVc from PointToPoint  where and Startingtime !='All'group by Startingtime");
                        listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPointByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from PointToPoint   where TIME(VCEndTime) <= TIME('{currenttime}') and Startingtime !='All' group by Startingtime order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                    else
                    {
                        listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPointByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from PointToPoint   where TIME(VCEndTime) <= TIME('{currenttime}') and time(Startingtime) =time('{starttime}')  group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                }
                else if (picker_vcstatus.SelectedIndex == 2)
                {
                    if (starttime.Equals("All"))
                    {
                        listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPointByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from PointToPoint  where TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) and Startingtime !='All' group by Startingtime order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                    else
                    {
                        listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPointByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from PointToPoint where  TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) and time(Startingtime) =time('{starttime}')  group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                }
                else if (picker_vcstatus.SelectedIndex == 3)
                {
                    if (starttime.Equals("All"))
                    {
                        listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPointByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from PointToPoint  where TIME(Startingtime) >= TIME('{currenttime}')  and Startingtime !='All' group by Startingtime order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                    else
                    {
                        listView_multipointvc.ItemsSource = pointToPointDatabase.GetPointToPointByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from PointToPoint  where TIME(Startingtime) >= TIME('{currenttime}') and time(Startingtime) =time('{starttime}') group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                }

            }
        }
    }
}
