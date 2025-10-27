using NICVC.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;




namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImportantVcDetailsPage : ContentPage
    {
        List<TodayVc> itemsource;
        TodayVcDatabase todayVcDatabase;
        string dateofvc = App.vcdateimportantvc;

        List<SaveUserPreferences> SavedUserPreferList;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        string districtname, studioname;

        string starttime;
        string query;

        public ImportantVcDetailsPage(string vcstarttime)
        {
            InitializeComponent();
            todayVcDatabase = new TodayVcDatabase();
            
            starttime = vcstarttime;
            lbl_user_header.Text = App.GetLabelByKey("impvcdtls") + " - " + dateofvc;
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
                listView_ImportantVcdetails.IsVisible = true;
                listView_ImportantVcdetailslocal.IsVisible = false;
                listView_ImportantVcdetails.ItemsSource = loadImportantVcdetails(null);
            }
            else
            {
                listView_ImportantVcdetails.IsVisible = false;
                listView_ImportantVcdetailslocal.IsVisible = true;
                listView_ImportantVcdetailslocal.ItemsSource = loadImportantVcdetails(null);
            }
            

        }
        public List<TodayVc> loadImportantVcdetails(string searched_text)
        {
            var currenttime = string.Format("{0:HH:mm:ss}", DateTime.Now);                    
            var currentdatetime = DateTime.Now.ToString("yyyy-MM-dd ") + currenttime;

            string buttonsvisibilitycases = $"(case when  DateTime('{currentdatetime}') >= DateTime(VcEndDateTime)  then 'false' else 'true' end ) takemetherevisibility , " +
                                           $"(case when   DateTime(VcStartDateTime) >= DateTime('{currentdatetime}') then 'true' else 'false' end) alertmevisibility ";

            string searchedQueryAdd = $"VC_ID like '%{searched_text}%' or DateofVC like '%{searched_text}%' or Startingtime like '%{searched_text}%' or VCEndTime like '%{searched_text}%' or Important like '%{searched_text}%' or " +
                $"LevelName like '%{searched_text}%' or Dept_Name like '%{searched_text}%' or Org_Name like '%{searched_text}%' or Purpose like '%{searched_text}%' or RequestedBy like '%{searched_text}%' or Studio_Name like '%{searched_text}%' or " +
                $"VCStatus like '%{searched_text}%' or VcCoordEmail like '%{searched_text}%' or VcCoordIpPhone like '%{searched_text}%' or VcCoordMobile like '%{searched_text}%' or VcCoordName like '%{searched_text}%' or " +
                $"VccoordLandline like '%{searched_text}%' or hoststudio like '%{searched_text}%' or mcuip like '%{searched_text}%' or mcuname like '%{searched_text}%' or participantsExt like '%{searched_text}%' or webroom like '%{searched_text}%'";

            if (string.IsNullOrEmpty(starttime) || starttime.Equals("All"))
            {
                query = $"SELECT *, {buttonsvisibilitycases}  from Todayvc where VCStatus = 'Confirmed' and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2) and Important='Y' and time(Startingtime) =time('{starttime}')  and ({searchedQueryAdd})  order by TIME(VCEndTime) ASC ";
              
            }
            else
            {
                query = $"SELECT *, {buttonsvisibilitycases}  from Todayvc where Startingtime !='All' and VCStatus = 'Confirmed'  and Important='Y' and substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2) and  time(Startingtime)=time('{starttime}') and ({searchedQueryAdd}) order by substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2)  ASC";
            }

            itemsource = todayVcDatabase.GetTodayVc(query).ToList();
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

        private void btn_takemethere_Clicked(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            string vcid = b.CommandParameter.ToString();
            Navigation.PushAsync(new TakeMeTherePage(vcid, "Todayvc"));
        }

        private async void btn_alertme_Clicked(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            string vcid = b.CommandParameter.ToString();
            List<TodayVc> todayVclist;
            todayVclist = todayVcDatabase.GetTodayVc($"Select * from todayvc where VC_ID='{vcid}'").ToList();
            if (App.checkvcidinalerttable(vcid) == true)
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("alertalready"), App.GetLabelByKey("close"));
            }
            else
            {
                AlertableDatabase alertableDatabase = new AlertableDatabase();
                var item = new Alertable();
                item.VC_ID = todayVclist.ElementAt(0).VC_ID;
                var vcdt = todayVclist.ElementAt(0).DateofVC;
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
                item.Dept_Name = todayVclist.ElementAt(0).Dept_Name;
                item.Important = todayVclist.ElementAt(0).Important;
                item.LevelName = todayVclist.ElementAt(0).LevelName;
                item.Org_Name = todayVclist.ElementAt(0).Org_Name;
                item.Purpose = todayVclist.ElementAt(0).Purpose;
                item.RequestedBy = todayVclist.ElementAt(0).RequestedBy;
                item.Startingtime = todayVclist.ElementAt(0).Startingtime;
                item.StudioID = todayVclist.ElementAt(0).StudioID;
                item.Studio_Name = todayVclist.ElementAt(0).Studio_Name;
                item.VCEndTime = todayVclist.ElementAt(0).VCEndTime;
                item.VCStatus = todayVclist.ElementAt(0).VCStatus;
                item.VcCoordEmail = todayVclist.ElementAt(0).VcCoordEmail;
                item.VcCoordIpPhone = todayVclist.ElementAt(0).VcCoordIpPhone;
                item.VcCoordMobile = todayVclist.ElementAt(0).VcCoordMobile;
                item.VcCoordName = todayVclist.ElementAt(0).VcCoordName;
                item.VccoordLandline = todayVclist.ElementAt(0).VccoordLandline;
                item.hoststudio = todayVclist.ElementAt(0).hoststudio;
                item.mcuip = todayVclist.ElementAt(0).mcuip;
                item.mcuname = todayVclist.ElementAt(0).mcuname;
                item.participantsExt = todayVclist.ElementAt(0).participantsExt;
                item.webroom = todayVclist.ElementAt(0).webroom;

                DateTime vcstartdate;
                try
                {
                    vcstartdate = DateTime.ParseExact(todayVclist.ElementAt(0).DateofVC, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                catch
                {
                    vcstartdate = DateTime.ParseExact(todayVclist.ElementAt(0).DateofVC, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                item.NotificationStartDate = vcstartdate.AddDays(-1).ToString("yyyy-MM-dd");
                item.Frequency = "1";


                alertableDatabase.AddAlertable(item);

                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("alertsave"), App.GetLabelByKey("close"));
                //await Navigation.PushAsync(new ViewSavedAlertsPage());
                await Navigation.PopToRootAsync();
            }
        }

        private void searchbar_schedulevcdetails_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (App.Language == 0)
            {
                listView_ImportantVcdetails.IsVisible = true;
                listView_ImportantVcdetailslocal.IsVisible = false;
                listView_ImportantVcdetails.ItemsSource = loadImportantVcdetails(searchbar_schedulevcdetails.Text);
            }
            else
            {

                listView_ImportantVcdetails.IsVisible = false;
                listView_ImportantVcdetailslocal.IsVisible = true;
                listView_ImportantVcdetailslocal.ItemsSource = loadImportantVcdetails(searchbar_schedulevcdetails.Text);
            }

        }
        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }

    }
}
