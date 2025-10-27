using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;



using Newtonsoft.Json.Linq;
using System.Web;
using NICVC.Model;


namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VcInYearPage : ContentPage
    {
        List<SaveUserPreferences> SavedUserPreferList;
        SaveUserPreferencesDatabase saveUserPreferencesDatabase;
        string districtname, studioname;
        YearWiseVcDatabase yearWiseVcDatabase;
        List<YearWiseVc> yearWiseVclist;
        public VcInYearPage()
        {
            InitializeComponent();
            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");
            lbl_user_header.Text = App.GetLabelByKey("YearWise");
            lbl_month.Text = App.GetLabelByKey("MonthName");
            lbl_totalvc.Text = App.GetLabelByKey("TotalVc");
            lbl_totalhrs.Text = App.GetLabelByKey("TotalHours");
            yearWiseVcDatabase = new YearWiseVcDatabase();
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
            
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                Dispatcher.Dispatch(async () => await GetYearVc());
            }
            else
            {
                yearWiseVclist = yearWiseVcDatabase.GetYearWiseVc("Select * from YearWiseVc").ToList();
                if (yearWiseVclist.Any())
                {
                    listView_yearvc.ItemsSource = yearWiseVclist;
                }
                else
                {
                     DisplayAlert(App.GetLabelByKey("NICVC"), App.GetLabelByKey("nointernet"), App.GetLabelByKey("close"));
                }
            }

        }

        async Task GetYearVc()
        {

            Loading_activity.IsVisible = true;
            Lbl_PleaseWait.Text = App.GetLabelByKey("pleasewait");
            
            try
            {

                var client = new HttpClient();
                string apiurl = $"{App.yearwisevcdetail_url}" +
                $"stateid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StateID))}" +
                $"&districtid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).DistrictID))}" +
                $"&studioid={HttpUtility.UrlEncode(AESCryptography.EncryptAES(App.SavedUserPreferList.ElementAt(0).StudioID))}";
                var responce = await client.GetAsync(apiurl);

                //string itemJson = await responce.Content.ReadAsStringAsync();

                if (responce.IsSuccessStatusCode)
                {
                    var result = await responce.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(result);

                    yearWiseVcDatabase.DeleteYearWiseVc();

                    foreach (var pair in parsed)
                    {

                        if (pair.Key == "YearWiseVcDetails")
                        {
                            var nodes = pair.Value;
                            var item = new YearWiseVc();

                            foreach (var node in nodes)
                            {

                                item.MonthName = AESCryptography.DecryptAES(node["MonthName"].ToString());
                                item.TotalHours = AESCryptography.DecryptAES(node["TotalHours"].ToString());
                                item.TotalVc = AESCryptography.DecryptAES(node["TotalVc"].ToString());


                                yearWiseVcDatabase.AddYearWiseVc(item);
                            }
                        }
                    }
                }
                Loading_activity.IsVisible = false;
                yearWiseVclist = yearWiseVcDatabase.GetYearWiseVc("Select * from YearWiseVc").ToList();
                listView_yearvc.ItemsSource = yearWiseVclist;
            }
            catch (Exception ey)
            {
                Loading_activity.IsVisible = false;
                await DisplayAlert(App.GetLabelByKey("Exception"), ey.Message, App.GetLabelByKey("close"));
                return;
            }
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }
    }
}
