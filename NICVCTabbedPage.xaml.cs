using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using NICVC.Model;

namespace NICVC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NICVCTabbedPage : Microsoft.Maui.Controls.TabbedPage
    {
        public NICVCTabbedPage()
        {
            InitializeComponent();
            Lbl_Header.Text = App.GetLabelByKey("nicvdconf");        

            On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
            Children.Add(new Dashboard_Page());
            Children.Add(new ReserveNicDashboardPage());
            
            // Check if personal info exists and add appropriate page
            var personalInfoDatabase = new NICVC.Model.PersonalInfoDatabase();
            var personalInfolist = personalInfoDatabase.GetPersonalInfo("Select * from personalinfo").ToList();
            
            if (!personalInfolist.Any())
            {
                Children.Add(new PersonalinforPage()); // Add PersonalinforPage directly as tab
            }
            else
            {
                Children.Add(new FeedbackPage()); // Add FeedbackPage as tab
            }
            
            Children.Add(new AboutUsPage());          
            CurrentPage = Children[App.CurrentTabpageIndex];

        }

    }
}