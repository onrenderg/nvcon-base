using NICVC.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;




namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScheduleVcDetailsPage : ContentPage
    {
        string query;
        List<ScheduleVc> itemsource;
        ScheduleVcDatabase scheduleVcDatabase;
        string dateofvc = App.vcdateschedulevc;
        List<ScheduleVc> schedulevclist;
        List<SaveUserPreferences> SavedUserPreferList;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        string districtname, studioname;
        string starttime;
       

        public ScheduleVcDetailsPage()
        {
            InitializeComponent();
            scheduleVcDatabase = new ScheduleVcDatabase();
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

            schedulevclist = scheduleVcDatabase.GetScheduleVc($"select distinct Startingtime from scheduleVc where VCStatus = 'Confirmed'  and substr(dateofvc,7)||substr(dateofvc,4,2)||substr(dateofvc,1,2)   = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2) or (Startingtime='All') order by Time(Startingtime) ASC").ToList();
            picker_sort.ItemsSource = schedulevclist;
            picker_sort.Title = App.GetLabelByKey("selstarttime"); 
            picker_sort.ItemDisplayBinding = new Binding("Startingtime");
            picker_sort.SelectedIndex = 0;


        }
        public List<ScheduleVc> loadscheduleVcdetails(string searched_text)
        {
            var currenttime = string.Format("{0:HH:mm:ss}", DateTime.Now);
            var currentdatetime = DateTime.Now.ToString("yyyy-MM-dd ") + currenttime;

            string buttonsvisibilitycases = $"(case when  DateTime('{currentdatetime}') >= DateTime(VcEndDateTime)  then 'false' else 'true' end ) takemetherevisibility , " +
                                           $"(case when   DateTime(VcStartDateTime) >= DateTime('{currentdatetime}') then 'true' else 'false' end) alertmevisibility ";

            string searchedQueryAdd = $"VC_ID like '%{searched_text}%' or Startingtime like '%{searched_text}%' or VCEndTime like '%{searched_text}%' or Important like '%{searched_text}%' or " +
                $"LevelName like '%{searched_text}%' or Dept_Name like '%{searched_text}%' or Org_Name like '%{searched_text}%' or Purpose like '%{searched_text}%' or RequestedBy like '%{searched_text}%' or Studio_Name like '%{searched_text}%' or " +
                $"VCStatus like '%{searched_text}%' or VcCoordEmail like '%{searched_text}%' or VcCoordIpPhone like '%{searched_text}%' or VcCoordMobile like '%{searched_text}%' or VcCoordName like '%{searched_text}%' or " +
                $"VccoordLandline like '%{searched_text}%' or hoststudio like '%{searched_text}%' or mcuip like '%{searched_text}%' or mcuname like '%{searched_text}%' or participantsExt like '%{searched_text}%' or webroom like '%{searched_text}%'";
            if (string.IsNullOrEmpty(starttime) || starttime.Equals("All"))
            {
                query = $"SELECT *, {buttonsvisibilitycases}  from scheduleVc where Startingtime !='All' and VCStatus = 'Confirmed'  and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2) and ({searchedQueryAdd})   order by substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) ASC";
            }
            else
            {
                query = $"SELECT *, {buttonsvisibilitycases}  from scheduleVc where Startingtime !='All' and VCStatus = 'Confirmed'  and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2) and  time(Startingtime)=time('{starttime}') and ({searchedQueryAdd})  order by substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) ASC";
            }

            itemsource = scheduleVcDatabase.GetScheduleVc(query).ToList();
            return itemsource;
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            try
            {
                Label lbl_mobile = (Label)sender;
                string txt = lbl_mobile.Text;
                PhoneDialer.Open(txt);
            }catch(Exception ey)
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

        private void btn_takemethere_Clicked(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            string vcid = b.CommandParameter.ToString();
            Navigation.PushAsync(new TakeMeTherePage(vcid, "ScheduleVc"));
        }

        private async void btn_alertme_Clicked(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            string vcid = b.CommandParameter.ToString();
            List<ScheduleVc> scheduleVclist;
            scheduleVclist = scheduleVcDatabase.GetScheduleVc($"Select * from scheduleVc where VC_ID='{vcid}'").ToList();
            if (App.checkvcidinalerttable(vcid) == true)
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("alertalready"), App.GetLabelByKey("close"));
            }
            else
            {
                AlertableDatabase alertableDatabase = new AlertableDatabase();
                var item = new Alertable();
                item.VC_ID = scheduleVclist.ElementAt(0).VC_ID;
                var vcdt = scheduleVclist.ElementAt(0).DateofVC;
                item.DateofVCtoshowonly = vcdt;
                DateTime dateofvc;
                try
                {
                    dateofvc = DateTime.ParseExact(vcdt, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                catch
                {
                    dateofvc = DateTime.ParseExact(vcdt, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }
                item.DateofVC = dateofvc.ToString("yyyy-MM-dd");
                item.Dept_Name = scheduleVclist.ElementAt(0).Dept_Name;
                item.Important = scheduleVclist.ElementAt(0).Important;
                item.LevelName = scheduleVclist.ElementAt(0).LevelName;
                item.Org_Name = scheduleVclist.ElementAt(0).Org_Name;
                item.Purpose = scheduleVclist.ElementAt(0).Purpose;
                item.RequestedBy = scheduleVclist.ElementAt(0).RequestedBy;
                item.Startingtime = scheduleVclist.ElementAt(0).Startingtime;
                item.StudioID = scheduleVclist.ElementAt(0).StudioID;
                item.Studio_Name = scheduleVclist.ElementAt(0).Studio_Name;
                item.VCEndTime = scheduleVclist.ElementAt(0).VCEndTime;
                item.VCStatus = scheduleVclist.ElementAt(0).VCStatus;
                item.VcCoordEmail = scheduleVclist.ElementAt(0).VcCoordEmail;
                item.VcCoordIpPhone = scheduleVclist.ElementAt(0).VcCoordIpPhone;
                item.VcCoordMobile = scheduleVclist.ElementAt(0).VcCoordMobile;
                item.VcCoordName = scheduleVclist.ElementAt(0).VcCoordName;
                item.VccoordLandline = scheduleVclist.ElementAt(0).VccoordLandline;
                item.hoststudio = scheduleVclist.ElementAt(0).hoststudio;
                item.mcuip = scheduleVclist.ElementAt(0).mcuip;
                item.mcuname = scheduleVclist.ElementAt(0).mcuname;
                item.participantsExt = scheduleVclist.ElementAt(0).participantsExt;
                item.webroom = scheduleVclist.ElementAt(0).webroom;

                DateTime vcstartdate;
                try
                {
                    vcstartdate = DateTime.ParseExact(scheduleVclist.ElementAt(0).DateofVC, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                catch
                {
                    vcstartdate = DateTime.ParseExact(scheduleVclist.ElementAt(0).DateofVC, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                item.NotificationStartDate = vcstartdate.AddDays(-1).ToString("yyyy-MM-dd");
                item.Frequency = "1";

                alertableDatabase.AddAlertable(item);
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("alertsave"), App.GetLabelByKey("close"));

                await Navigation.PopToRootAsync();
            }
        }

        private void picker_sort_SelectedIndexChanged(object sender, EventArgs e)
        {
            ScheduleVc selectedItem = picker_sort.SelectedItem as ScheduleVc;
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
