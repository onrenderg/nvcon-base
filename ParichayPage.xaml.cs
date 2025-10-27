using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NICVC.Model;
using Microsoft.Maui.Controls.Shapes;

namespace NICVC
{
    public partial class ParichayPage : ContentPage
    {
        string Action_Logout;
        public ParichayPage(string Logout = null)
        {
            InitializeComponent();
            Action_Logout = Logout;
            System.Diagnostics.Debug.WriteLine("ParichayPage: Starting initialization");
            if (string.IsNullOrEmpty(Action_Logout))
            {
                var url = $"https://parichay.nic.in/pnv1/oauth2/authorize?client_id={App.clientid}&redirect_uri={App.Redirectionuri}&scope={App.scope}&code_challenge={App.CodeChallange}&state={DateTime.Now.ToString("yyMMddHHmmsss")}&code_challenge_method=S256&response_type=code";
                System.Diagnostics.Debug.WriteLine($"ParichayPage: Loading URL: {url}");
                Parichay_browser.Source = url;
            }
            else
            {
                Dispatcher.Dispatch(async () =>
                {
                    await DisplayAlert("Logout", "Please Logout your Parichay account from here manually!", "OK");                    
                });
                Parichay_browser.Source = $"https://parichay.nic.in";
            }
        }

        void ToolbarItem_Clicked(System.Object sender, EventArgs e)
        {

        }

        async void Parichay_browser_Navigating(System.Object sender, WebNavigatingEventArgs e)
        {
            if (string.IsNullOrEmpty(Action_Logout))
            {
                if (e.Url.StartsWith(App.Redirectionuri))
                {
                    Uri uri = new Uri(e.Url);
                    string code_from_parichay = HttpUtility.ParseQueryString(uri.Query).Get("code");
                    string state_from_parichay = HttpUtility.ParseQueryString(uri.Query).Get("state");

                    Console.WriteLine(code_from_parichay);
                    Console.WriteLine(state_from_parichay);
                    e.Cancel = true;
                    await GetToken(code_from_parichay);
                    //Parichay_browser = new WebView();
                }
            }
            else
            {                
                if (e.Url.StartsWith("https://parichay.nic.in/Accounts/Services?service="))
                {
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        App.Current.MainPage = new NavigationPage(new ParichayPage());
                    }
                    else
                    {
                        App.Current.MainPage = new NavigationPage(new ParichayPage())
                        {
                            BarBackgroundColor = Color.FromArgb("#2196f3"),
                            BarTextColor = Colors.WhiteSmoke
                        };
                    }
                }
            }
            
        }

        public async Task<int> GetToken(string _CodeParichayReturned)
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                try
                {
                    var client = new HttpClient();
                    
                    //request parameters as Json
                    string jsonData = JsonConvert.SerializeObject(new
                    {
                        code = _CodeParichayReturned,
                        code_verifier = App.CodeVerifier,
                        redirect_uri = App.Redirectionuri,
                        client_id = App.clientid,
                        grant_type = "authorization_code",
                        client_secret = App.clientsecret
                    });
                    Console.WriteLine(jsonData);
                    //Send Json with request
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    //End Json request

                    //awaited for API Response
                    HttpResponseMessage response = await client.PostAsync(App.tokenuri, content);
                    //API Response Received
                    if ((int)response.StatusCode == 200)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var parsed = JObject.Parse(result);
                        //Console.WriteLine("access_token : " + parsed["access_token"].ToString());
                        Preferences.Set("access_token", parsed["access_token"].ToString());
                       // Console.WriteLine("Complete Return : " + result);
                        await GetProfile(parsed["access_token"].ToString());
                        return (int)response.StatusCode;

                    } else
                    {
                        await DisplayAlert("Exception", ((int)response.StatusCode).ToString(), "Close");
                        if (DeviceInfo.Platform == DevicePlatform.iOS)
                        {
                            Application.Current.MainPage = new NavigationPage(new ParichayPage());
                        }
                        else
                        {
                            Application.Current.MainPage = new NavigationPage(new ParichayPage())
                            {
                                BarBackgroundColor = Color.FromArgb("#2196f3"),
                                BarTextColor = Colors.WhiteSmoke
                            };
                        }
                        return -1;
                    }
                    

                }
                catch (Exception)
                {
                    await App.Current.MainPage.DisplayAlert("Exception", "Something went wrong. Please try again!", "OK");
                    return 500;
                }
            }
            else
            {
                //await App.Current.MainPage.DisplayAlert(App.AppName, App.NoInternet_, App.Btn_OK_);
                return 101;
            }
        }

        public async Task<int> GetProfile(string _tokenFromParichay)
        {
            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                try
                {
                    var client = new HttpClient();
                    ////Added Bearer Auth to client
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("access_token", _tokenFromParichay);
                    //End basic Auth to client

                    client.DefaultRequestHeaders.Add("Authorization", _tokenFromParichay);
                    //awaited for API Response
                    HttpResponseMessage response = await client.GetAsync(App.getuserdetails.ToString());
                    //API Response Received
                    var result = await response.Content.ReadAsStringAsync();
                    //await DisplayAlert("Profile", result, "OK");
                    var parsed = JObject.Parse(result);
                    UserLoginDatabase userlogindatabase = new UserLoginDatabase();
                    userlogindatabase.DeleteUserLogin();
                    var item = new UserLogin();

                    //{ "MobileNo":"+918219211012","departmentName":"hp_users","loginId":"paramjeet.singh11@nic.in","FirstName":"Paramjeet","brwId":"vUs0c6gS6uL1Ayx4ZnbW5HDF7p4l3HEZ","sessionId":"QVhCwIck0oawGFHKG7x6Uq2WaCkKIL9h","LastName":"Singh","parichayId":"paramjeet.singh11","ua":"Mozilla/5.0(Linux;Android11;sdk_gphone_x86_armBuild/RSR1.201013.001;wv)AppleWebKit/537.36(KHTML,likeGecko)Version/4.0Chrome/83.0.4103.106MobileSafari/537.36","userName":"paramjeet.singh11"}
                    item.Email = parsed["loginId"].ToString();
                 
                    item.UserId = parsed["parichayId"].ToString();
                    item.UserName = parsed["userName"].ToString();
                    item.isremembered = "Y";
                    Preferences.Set("F_Name", parsed["FirstName"].ToString());
                    Preferences.Set("L_Name", parsed["LastName"].ToString());
                    Preferences.Set("DisplayName", $"{parsed["FirstName"].ToString()} {parsed["LastName"].ToString()}\n{item.Email}\n{parsed["MobileNo"].ToString()}");
                    Preferences.Set("LoginMode", "SSO");

                    Preferences.Set("Full_Name", $"{parsed["FirstName"].ToString()} {parsed["LastName"].ToString()}");
                    Preferences.Set("Email", item.Email);
                    try
                    {
                        Preferences.Set("MobileNo", (parsed["MobileNo"].ToString() ?? "").Replace("+91",""));
                    }
                    catch (Exception)
                    {

                    }
                    
                    userlogindatabase.AddUserLogin(item);
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        Application.Current.MainPage = new NavigationPage(new PreferencePage());
                     
                    }
                    else
                    {
                        App.Current.MainPage = new NavigationPage(new PreferencePage())
                        {
                            BarBackgroundColor = Color.FromArgb("#2196f3"),
                            BarTextColor = Colors.WhiteSmoke
                        };
                    }
                    
                    //await App.Current.MainPage.DisplayAlert(App.AppName, parsed["Message"].ToString(), App.Btn_OK_);
                    /*if ((int)response.StatusCode != 200)
                    {

                        await App.Current.MainPage.DisplayAlert(App.AppName, parsed["Message"].ToString(), App.Btn_OK_);
                    }*/
                    return (int)response.StatusCode;

                }
                catch (Exception)
                {
                    await App.Current.MainPage.DisplayAlert("Exception", "Something went wrong. Please try again!", "OK");
                    return 500;
                }
            }
            else
            {
                //await App.Current.MainPage.DisplayAlert(App.AppName, App.NoInternet_, App.Btn_OK_);
                return 101;
            }
        }

        

    }
}
