using System;
using System.Collections.Generic;
using System.Linq;



using NICVC.Model;
using System.Globalization;

namespace NICVC.ReserveNic
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MultipointVcStateDetailsPage : ContentPage
    {
        List<MultipointState> itemsource;
        MultipointStateDatabase multipointStateDatabase;
        string vcstatus = App.vcstatusmultipointvc;
        string starttime = App.starttimemultipointvc;
        List<SaveUserPreferences> SavedUserPreferList;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        string districtname, studioname;
        string dateofvc;

        public MultipointVcStateDetailsPage(string vcdate)
        {
            InitializeComponent();
            multipointStateDatabase = new MultipointStateDatabase();
            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");
            dateofvc = vcdate;
            lbl_user_header.Text = App.GetLabelByKey(vcstatus) + " - " + dateofvc + " - " + App.starttimemultipointvc;
            

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

            searchbar_todayvcdetails.Placeholder = App.GetLabelByKey("search");
            lbl_search.Text = App.GetLabelByKey("searchtext");

            if (App.Language == 0)
            {
               
                listView_todayvcdetails.IsVisible = true;
                listView_todayvcdetailslocal.IsVisible = false;
                listView_todayvcdetails.ItemsSource = loadtodayvcdetails(null);
            }
            else
            {
               
                listView_todayvcdetails.IsVisible = false;
                listView_todayvcdetailslocal.IsVisible = true;
                listView_todayvcdetailslocal.ItemsSource = loadtodayvcdetails(null);
            }
        }


        public List<MultipointState> loadtodayvcdetails(string searched_text)
        {
            var currenttime = string.Format("{0:HH:mm:ss}", DateTime.Now);
            var currentdatetime = DateTime.Now.ToString("yyyy-MM-dd ") + currenttime;
            string buttonsvisibilitycases = $"(case when  DateTime('{currentdatetime}') >= DateTime(VcEndDateTime)  then 'false' else 'true' end ) takemetherevisibility , " +
                                            $"(case when   DateTime(VcStartDateTime) >= DateTime('{currentdatetime}') then 'true' else 'false' end) alertmevisibility ";
            //var currentdatetime = DateTime.Now;
            //string buttonsvisibilitycases = $"(case when  '{currentdatetime}' >= VcEndDateTime  then 'false' else 'true' end ) takemetherevisibility , " +
            //                                $"(case when VcStartDateTime > '{currentdatetime}'  then 'true' else 'false' end) alertmevisibility ";
            string searchedQueryAdd = $"VC_ID like '%{searched_text}%'  or Startingtime like '%{searched_text}%' or VCEndTime like '%{searched_text}%' or Important like '%{searched_text}%' or " +
                $"LevelName like '%{searched_text}%' or Dept_Name like '%{searched_text}%' or Org_Name like '%{searched_text}%' or Purpose like '%{searched_text}%' or RequestedBy like '%{searched_text}%' or Studio_Name like '%{searched_text}%' or " +
                $"VCStatus like '%{searched_text}%' or VcCoordEmail like '%{searched_text}%' or VcCoordIpPhone like '%{searched_text}%' or VcCoordMobile like '%{searched_text}%' or VcCoordName like '%{searched_text}%' or " +
                $"VccoordLandline like '%{searched_text}%' or hoststudio like '%{searched_text}%' or mcuip like '%{searched_text}%' or mcuname like '%{searched_text}%' or participantsExt like '%{searched_text}%' or webroom like '%{searched_text}%'";


            if (vcstatus.Contains("Completed"))
            {
                itemsource = multipointStateDatabase.GetMultipointState($"SELECT * , {buttonsvisibilitycases} from MultipointState where  " +
                    $" substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2)  " +
                    $" and TIME(VCEndTime) <= TIME('{currenttime}') and TIME(Startingtime) =TIME('{starttime}') " +
                    $" and ({searchedQueryAdd})  order by TIME(VCEndTime) ASC ").ToList();
            }
            else if (vcstatus.Contains("Ongoing"))
            {
                lbl_user_header.Text = App.GetLabelByKey(vcstatus) + " - " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                itemsource = multipointStateDatabase.GetMultipointState($"SELECT *, {buttonsvisibilitycases}  from MultipointState where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2) and TIME('{currenttime}') between TIME(Startingtime) and TIME(VCEndTime) and TIME(Startingtime)=TIME('{starttime}') " +
                    $" and ({searchedQueryAdd})  order by TIME(VCEndTime) ASC ").ToList();
            }
            else if (vcstatus.Contains("Upcoming"))
            {
                itemsource = multipointStateDatabase.GetMultipointState($"SELECT *, {buttonsvisibilitycases}  from MultipointState where substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2) and TIME(Startingtime) >= TIME('{currenttime}') and TIME(Startingtime) =TIME('{starttime}') " +
                    $" and  ({searchedQueryAdd})  order by TIME(VCEndTime) ASC ").ToList();
            }
            else
            {
                string query = $"SELECT *, {buttonsvisibilitycases} from MultipointState where  substr(dateofvc, 7) || substr(dateofvc, 4, 2) || substr(dateofvc, 1, 2) = substr('{dateofvc}', 7) || substr('{dateofvc}', 4, 2) || substr('{dateofvc}', 1, 2) and Startingtime !='All' and TIME(Startingtime) =TIME('{starttime}') and ({ searchedQueryAdd}) order by TIME(VCEndTime) ASC";
                itemsource = multipointStateDatabase.GetMultipointState(query).ToList();
            }
            return itemsource;

            // listView_todayvcdetailsdetails.ItemsSource = todayVcDatabase.GetTodayVc($"SELECT  * from TodayVC where Startingtime ='{ App.starttimetodayvc}'");
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

        private void btn_takemethere_Clicked(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            string vcid = b.CommandParameter.ToString();
            Navigation.PushAsync(new TakeMeTherePage(vcid, "MultipointState"));
        }

        private async void btn_alertme_Clicked(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            string vcid = b.CommandParameter.ToString();
            List<MultipointState> multipointStatelist;
            multipointStatelist = multipointStateDatabase.GetMultipointState($"Select * from MultipointState where VC_ID='{vcid}'").ToList();
            if (App.checkvcidinalerttable(vcid) == true)
            {
                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("alertalready"), App.GetLabelByKey("close"));
            }
            else
            {
                AlertableDatabase alertableDatabase = new AlertableDatabase();
                var item = new Alertable();
                item.VC_ID = multipointStatelist.ElementAt(0).VC_ID;
                var vcdt = multipointStatelist.ElementAt(0).DateofVC;
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
                item.Dept_Name = multipointStatelist.ElementAt(0).Dept_Name;
                item.Important = multipointStatelist.ElementAt(0).Important;
                item.LevelName = multipointStatelist.ElementAt(0).LevelName;
                item.Org_Name = multipointStatelist.ElementAt(0).Org_Name;
                item.Purpose = multipointStatelist.ElementAt(0).Purpose;
                item.RequestedBy = multipointStatelist.ElementAt(0).RequestedBy;
                item.Startingtime = multipointStatelist.ElementAt(0).Startingtime;
                item.StudioID = multipointStatelist.ElementAt(0).StudioID;
                item.Studio_Name = multipointStatelist.ElementAt(0).Studio_Name;
                item.VCEndTime = multipointStatelist.ElementAt(0).VCEndTime;
                item.VCStatus = multipointStatelist.ElementAt(0).VCStatus;
                item.VcCoordEmail = multipointStatelist.ElementAt(0).VcCoordEmail;
                item.VcCoordIpPhone = multipointStatelist.ElementAt(0).VcCoordIpPhone;
                item.VcCoordMobile = multipointStatelist.ElementAt(0).VcCoordMobile;
                item.VcCoordName = multipointStatelist.ElementAt(0).VcCoordName;
                item.VccoordLandline = multipointStatelist.ElementAt(0).VccoordLandline;
                item.hoststudio = multipointStatelist.ElementAt(0).hoststudio;
                item.mcuip = multipointStatelist.ElementAt(0).mcuip;
                item.mcuname = multipointStatelist.ElementAt(0).mcuname;
                item.participantsExt = multipointStatelist.ElementAt(0).participantsExt;
                item.webroom = multipointStatelist.ElementAt(0).webroom;

                DateTime vcstartdate;
                try
                {
                    vcstartdate = DateTime.ParseExact(multipointStatelist.ElementAt(0).DateofVC, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                catch
                {
                    vcstartdate = DateTime.ParseExact(multipointStatelist.ElementAt(0).DateofVC, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }

                item.NotificationStartDate = vcstartdate.AddDays(-1).ToString("yyyy-MM-dd");
                item.Frequency = "1";


                alertableDatabase.AddAlertable(item);

                await DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("alertsave"), App.GetLabelByKey("close"));
                //await Navigation.PushAsync(new ViewSavedAlertsPage());
                App.CurrentTabpageIndex = 0;
                await Navigation.PopToRootAsync();
                
            }
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (App.Language == 0)
            {

                listView_todayvcdetails.IsVisible = true;
                listView_todayvcdetailslocal.IsVisible = false;
                listView_todayvcdetails.ItemsSource = loadtodayvcdetails(searchbar_todayvcdetails.Text);
            }
            else
            {

                listView_todayvcdetails.IsVisible = false;
                listView_todayvcdetailslocal.IsVisible = true;
                listView_todayvcdetailslocal.ItemsSource = loadtodayvcdetails(searchbar_todayvcdetails.Text);
            }

        }
        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            App.CurrentTabpageIndex = 1;
            await Navigation.PopToRootAsync();
        }
    }
}
