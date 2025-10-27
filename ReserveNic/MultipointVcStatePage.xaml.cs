using System;
using System.Collections.Generic;
using System.Linq;


using NICVC.Model;

namespace NICVC.ReserveNic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MultipointVcStatePage : ContentPage
    {
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        List<SaveUserPreferences> SavedUserPreferList;
        MultipointStateDatabase multipointStateDatabase;
        List<MultipointState> multipointStatelist;
        string dateofvc;
        string timequery;
        public MultipointVcStatePage(string selectedate, string mptype)
        {
            InitializeComponent();
            dateofvc = selectedate;
            // multipointtype = mptype;
            saveUserPreferencesDatabase = new SaveUserPreferencesDatabase();
            multipointStateDatabase = new MultipointStateDatabase();

            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");
            lbl_user_header1.Text = App.GetLabelByKey("multistate") + " - " + dateofvc;
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


            listView_multipointvc.ItemsSource = multipointStateDatabase.GetMultipointStateByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from MultipointState where Startingtime !='All'  group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });


            picker_vcstatus.SelectedIndex = 0;
            if (App.Language == 0)
            {
                timequery = $"select distinct Startingtime from MultipointState order by time(startingtime)";
            }
            else
            {
                timequery = $"select distinct replace (Startingtime,'All', '??')Startingtime from MultipointState order by time(startingtime)";
            }

            multipointStatelist = multipointStateDatabase.GetMultipointState(timequery).ToList();
            picker_sort.ItemsSource = multipointStatelist;
            picker_sort.Title = App.GetLabelByKey("selstarttime");
            picker_sort.ItemDisplayBinding = new Binding("Startingtime");
            picker_sort.SelectedIndex = 0;

        }

        private void listView_multipointvc_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var currentRecord = e.Item as MultipointState;
            App.starttimemultipointvc = currentRecord.Startingtime.ToString();
            App.vcstatusmultipointvc = picker_vcstatus.SelectedItem.ToString();
            Navigation.PushAsync(new MultipointVcStateDetailsPage(dateofvc));
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

            multipointStatelist = new List<MultipointState>();
            currenttime = string.Format("{0:HH:mm:ss}", DateTime.Now);
            listView_multipointvc.SelectedItem = null;

            if (picker_vcstatus.SelectedIndex == 0)
            {

                listView_multipointvc.ItemsSource = multipointStateDatabase.GetMultipointStateByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc  from MultipointState " +
                     $" where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  and Startingtime !='All' " +
                     $" group by TIME(Startingtime) order by TIME(Startingtime) DESC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                //multipointStatelist = multipointStateDatabase.GetMultipointState($"select distinct Startingtime from MultipointState ").ToList();
                if (App.Language == 0)
                {
                    timequery = $"select distinct Startingtime from MultipointState order by time(startingtime)";
                }
                else
                {
                    timequery = $"select distinct replace (Startingtime,'All', '??')Startingtime from MultipointState order by time(startingtime)";
                }
                multipointStatelist = multipointStateDatabase.GetMultipointState(timequery).ToList();
            }
            else if (picker_vcstatus.SelectedIndex == 1)

            {
                listView_multipointvc.ItemsSource = multipointStateDatabase.GetMultipointStateByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from MultipointState " +
                    $" where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  " +
                    $"and TIME(VCEndTime) <= TIME('{currenttime}') and Startingtime !='All' group by Startingtime order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                //multipointStatelist = multipointStateDatabase.GetMultipointState($"select distinct Startingtime from MultipointState   where TIME(VCEndTime) <= TIME('{currenttime}') union select 'All' as Startingtime order by Startingtime DESC").ToList();
                if (App.Language == 0)
                {
                    timequery = $"select distinct Startingtime from MultipointState where TIME(VCEndTime) <= TIME('{currenttime}') union select 'All' as Startingtime order by startingtime desc";
                }
                else
                {
                    timequery = $"select distinct replace (Startingtime,'All', '??')Startingtime from MultipointState where TIME(VCEndTime) <= TIME('{currenttime}') union select '??' as Startingtime order by startingtime desc";
                }
                multipointStatelist = multipointStateDatabase.GetMultipointState(timequery).ToList();


            }
            else if (picker_vcstatus.SelectedIndex == 2)

            {
                listView_multipointvc.ItemsSource = multipointStateDatabase.GetMultipointStateByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from MultipointState " +
                     $" where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  " +
                     $"and TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) and Startingtime !='All' group by (Startingtime) order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                //multipointStatelist = multipointStateDatabase.GetMultipointState($"select distinct Startingtime from MultipointState  where TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) union select 'All' as Startingtime order by Startingtime DESC").ToList();
                if (App.Language == 0)
                {
                    timequery = $"select distinct Startingtime from MultipointState where TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) union select 'All' as Startingtime order by startingtime desc";
                }
                else
                {
                    timequery = $"select distinct replace (Startingtime,'All', '??')Startingtime from MultipointState where TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) union select '??' as Startingtime order by startingtime desc";
                }
                multipointStatelist = multipointStateDatabase.GetMultipointState(timequery).ToList();

            }
            else if (picker_vcstatus.SelectedIndex == 3)

            {
                listView_multipointvc.ItemsSource = multipointStateDatabase.GetMultipointStateByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from MultipointState " +
                     $" where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  " +
                     $" and TIME(Startingtime) >= TIME('{currenttime}')  and Startingtime !='All' group by TIME(Startingtime) order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                // multipointStatelist = multipointStateDatabase.GetMultipointState($"select distinct Startingtime from MultipointState  where TIME(Startingtime) >= TIME('{currenttime}') union select 'All' as Startingtime order by Startingtime DESC").ToList();
                if (App.Language == 0)
                {
                    timequery = $"select distinct Startingtime from MultipointState where TIME(Startingtime) >= TIME('{currenttime}') union select 'All' as Startingtime order by startingtime desc";
                }
                else
                {
                    timequery = $"select distinct replace (Startingtime,'All', '??')Startingtime from MultipointState where TIME(Startingtime) >= TIME('{currenttime}') union select '??' as Startingtime order by startingtime desc";
                }
                multipointStatelist = multipointStateDatabase.GetMultipointState(timequery).ToList();

            }

            picker_sort.ItemsSource = multipointStatelist;
            picker_sort.Title = App.GetLabelByKey("selstarttime");
            picker_sort.ItemDisplayBinding = new Binding("Startingtime");
            picker_sort.SelectedIndex = 0;
        }

        private void picker_sort_SelectedIndexChanged(object sender, EventArgs e)
        {
            MultipointState selectedItem = picker_sort.SelectedItem as MultipointState;

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
                    if (picker_sort.SelectedIndex == 0)
                    {
                        listView_multipointvc.ItemsSource = multipointStateDatabase.GetMultipointStateByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc  from MultipointState  where Startingtime !='All' group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                    else
                    {
                        listView_multipointvc.ItemsSource = multipointStateDatabase.GetMultipointStateByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from MultipointState  where time(Startingtime) =time('{starttime}') and Startingtime !='All' group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                }
                else if (picker_vcstatus.SelectedIndex == 1)

                {
                    if (picker_sort.SelectedIndex == 0)
                    {
                        // listView_multipointvc.ItemsSource = MultipointStateDatabase.GetMultipointState($"SELECT  startingtime, count(*) as NoOfVc from MultipointState  where and Startingtime !='All'group by Startingtime");
                        listView_multipointvc.ItemsSource = multipointStateDatabase.GetMultipointStateByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from MultipointState   where TIME(VCEndTime) <= TIME('{currenttime}') and Startingtime !='All' group by Startingtime order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                    else
                    {
                        listView_multipointvc.ItemsSource = multipointStateDatabase.GetMultipointStateByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from MultipointState   where TIME(VCEndTime) <= TIME('{currenttime}') and time(Startingtime) =time('{starttime}')  group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                }
                else if (picker_vcstatus.SelectedIndex == 2)

                {
                    if (picker_sort.SelectedIndex == 0)
                    {
                        listView_multipointvc.ItemsSource = multipointStateDatabase.GetMultipointStateByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from MultipointState  where TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) and Startingtime !='All' group by Startingtime order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                    else
                    {
                        listView_multipointvc.ItemsSource = multipointStateDatabase.GetMultipointStateByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from MultipointState where  TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) and time(Startingtime) =time('{starttime}')  group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                }
                else if (picker_vcstatus.SelectedIndex == 3)

                {
                    if (picker_sort.SelectedIndex == 0)
                    {
                        listView_multipointvc.ItemsSource = multipointStateDatabase.GetMultipointStateByParameter($"SELECT (? || '\n\n' || ?) as  labelname, Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc,VCEndTime  from MultipointState  where TIME(Startingtime) >= TIME('{currenttime}')  and Startingtime !='All' group by Startingtime order by TIME(Startingtime) ASC", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                    else
                    {
                        listView_multipointvc.ItemsSource = multipointStateDatabase.GetMultipointStateByParameter($"SELECT (? || '\n\n' || ?) as labelname,  Startingtime, (startingtime||'\n\n'||count(*)) NoOfVc from MultipointState  where TIME(Startingtime) >= TIME('{currenttime}') and time(Startingtime) =time('{starttime}') group by Startingtime", new string[2] { App.GetLabelByKey("vcstarttime"), App.GetLabelByKey("noofvc") });
                    }
                }

            }
        }
    }
}
